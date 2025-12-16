using System.Data.SqlClient;
using System.Text.RegularExpressions;

static string GetConnectionString(bool useDev = false)
{
    // Resolve repo root relative to this project's directory
    var baseDir = AppContext.BaseDirectory;
    // bin/Debug/net9.0 -> project dir
    var projDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
    var path = Path.Combine(projDir, "..", "..", "api", "portalApi", "appsettings.json");
    path = Path.GetFullPath(path);
    var lines = File.ReadAllLines(path);
    string? found = null;
    foreach (var raw in lines)
    {
        var line = raw.Trim();
        bool isComment = line.StartsWith("//");
        if (useDev && !isComment) continue;
        if (!useDev && isComment) continue;
        var m = Regex.Match(line, "\"DefaultConnection\"\\s*:\\s*\"(?<cs>[^\"]+)\"");
        if (m.Success) found = m.Groups["cs"].Value; // take the last matching occurrence in the chosen set
    }
    if (string.IsNullOrEmpty(found)) throw new Exception("DefaultConnection not found in appsettings.json");
    return found;
}

bool useDev = args.Length > 0 && args[0].Equals("dev", StringComparison.OrdinalIgnoreCase);
string? overrideDb = null;
foreach (var a in args)
{
    if (a.StartsWith("db=", StringComparison.OrdinalIgnoreCase))
    {
        overrideDb = a.Substring(3);
    }
}
var connStr = GetConnectionString(useDev);
if (!string.IsNullOrWhiteSpace(overrideDb))
{
    // Replace Database or Initial Catalog in the connection string
    connStr = System.Text.RegularExpressions.Regex.Replace(connStr, @"(?i)(Database|Initial Catalog)\s*=\s*[^;]+", m =>
    {
        var key = m.Groups[1].Value;
        return $"{key}=" + overrideDb;
    });
}
Console.WriteLine("Connecting to DB...");
using var conn = new SqlConnection(connStr);
await conn.OpenAsync();

// Print server and database for clarity
using (var info = conn.CreateCommand())
{
    info.CommandText = "SELECT @@SERVERNAME, DB_NAME()";
    using var rr = await info.ExecuteReaderAsync();
    if (await rr.ReadAsync())
    {
        Console.WriteLine($"Server: {rr.GetString(0)} | Database: {rr.GetString(1)}");
    }
}

string sql = @"
BEGIN TRY
    IF OBJECT_ID(N'[dbo].[ItemDataConsumers]', N'U') IS NOT NULL
        DROP TABLE [dbo].[ItemDataConsumers];

    IF COL_LENGTH('Items','DataConsumers') IS NULL
        ALTER TABLE [Items] ADD [DataConsumers] NVARCHAR(100) NULL;

    IF COL_LENGTH('Items','DataConsumersText') IS NOT NULL
    BEGIN
        EXEC('UPDATE [Items] SET [DataConsumers] = COALESCE(NULLIF([DataConsumers],''''), [DataConsumersText]);');
        EXEC('ALTER TABLE [Items] DROP COLUMN [DataConsumersText];');
    END

    DELETE FROM [LookupValues] WHERE [Type] = 'DataConsumer';
END TRY
BEGIN CATCH
    DECLARE @msg nvarchar(4000) = ERROR_MESSAGE();
    THROW 50000, @msg, 1;
END CATCH;";

using var cmd = conn.CreateCommand();
cmd.CommandText = sql;
cmd.CommandTimeout = 120;
await cmd.ExecuteNonQueryAsync();
Console.WriteLine("Schema patch applied.");

// List related tables to verify state
using (var listCmd = conn.CreateCommand())
{
    listCmd.CommandText = @"SELECT name FROM sys.tables WHERE name LIKE '%DataConsum%' ORDER BY name;";
    using var r = await listCmd.ExecuteReaderAsync();
Console.WriteLine("Tables containing 'DataConsum':");
    while (await r.ReadAsync())
    {
        Console.WriteLine(" - " + r.GetString(0));
    }
}

// Apply Service Line -> Operating Entity consolidation
Console.WriteLine("Applying Service Line → Operating Entity migration...");
string sql2 = @"
BEGIN TRY
    -- Ensure OperatingEntityId exists
    IF COL_LENGTH('Items','OperatingEntityId') IS NULL
        ALTER TABLE [Items] ADD [OperatingEntityId] INT NULL;

    -- Copy ServiceLine values into OperatingEntity where missing
    IF EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ServiceLine')
    BEGIN
        INSERT INTO LookupValues([Type],[Value])
        SELECT 'OperatingEntity', sl.[Value]
        FROM LookupValues sl
        LEFT JOIN LookupValues oe ON oe.[Type] = 'OperatingEntity' AND oe.[Value] = sl.[Value]
        WHERE sl.[Type] = 'ServiceLine' AND oe.Id IS NULL;
    END

    -- Map Items: if OperatingEntityId is NULL but ServiceLineId present, map by value
    IF COL_LENGTH('Items','ServiceLineId') IS NOT NULL
    BEGIN
        EXEC('UPDATE i
              SET i.OperatingEntityId = oe.Id
              FROM Items i
              JOIN LookupValues sl ON sl.Id = i.ServiceLineId AND sl.[Type] = ''ServiceLine''
              JOIN LookupValues oe ON oe.[Type] = ''OperatingEntity'' AND oe.[Value] = sl.[Value]
              WHERE i.OperatingEntityId IS NULL;');

        -- Drop FK/index and column
        DECLARE @fk2 sysname;
        SELECT @fk2 = fk.name
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
        JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
        WHERE fk.parent_object_id = OBJECT_ID('Items') AND c.name = 'ServiceLineId';
        IF @fk2 IS NOT NULL EXEC('ALTER TABLE [Items] DROP CONSTRAINT [' + @fk2 + ']');
        IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Items') AND name = 'IX_Items_ServiceLineId')
            EXEC('DROP INDEX [IX_Items_ServiceLineId] ON [Items]');
        EXEC('ALTER TABLE [Items] DROP COLUMN [ServiceLineId]');
    END

    -- Remove ServiceLine lookup rows
    DELETE FROM LookupValues WHERE [Type] = 'ServiceLine';
END TRY
BEGIN CATCH
    DECLARE @msg2 nvarchar(4000) = ERROR_MESSAGE();
    THROW 50001, @msg2, 1;
END CATCH;";
using (var cmd2 = conn.CreateCommand())
{
    cmd2.CommandText = sql2;
    cmd2.CommandTimeout = 120;
    await cmd2.ExecuteNonQueryAsync();
}
Console.WriteLine("Service Line migration applied.");

// Remove MustDo2025 lookup and column
Console.WriteLine("Removing MustDo2025 (column + lookups)…");
string sql3 = @"
BEGIN TRY
    -- Drop FK and index then column if present
    IF COL_LENGTH('Items','MustDo2025Id') IS NOT NULL
    BEGIN
        DECLARE @fk3 sysname;
        SELECT @fk3 = fk.name
        FROM sys.foreign_keys fk
        JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
        JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
        WHERE fk.parent_object_id = OBJECT_ID('Items') AND c.name = 'MustDo2025Id';
        IF @fk3 IS NOT NULL EXEC('ALTER TABLE [Items] DROP CONSTRAINT [' + @fk3 + ']');
        IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Items') AND name = 'IX_Items_MustDo2025Id')
            EXEC('DROP INDEX [IX_Items_MustDo2025Id] ON [Items]');
        EXEC('ALTER TABLE [Items] DROP COLUMN [MustDo2025Id]');
    END

    -- Delete lookup rows
    DELETE FROM LookupValues WHERE [Type] = 'MustDo2025';
END TRY
BEGIN CATCH
    DECLARE @msg3 nvarchar(4000) = ERROR_MESSAGE();
    THROW 50002, @msg3, 1;
END CATCH;";

using (var cmd3 = conn.CreateCommand())
{
    cmd3.CommandText = sql3;
    cmd3.CommandTimeout = 120;
    await cmd3.ExecuteNonQueryAsync();
}
Console.WriteLine("MustDo2025 removed.");

// Seed Product Impact Category lookups and add column
Console.WriteLine("Ensuring ProductImpactCategory (lookup + column)…");
string sql4 = @"
BEGIN TRY
    IF COL_LENGTH('Items','ProductImpactCategoryId') IS NULL
        ALTER TABLE [Items] ADD [ProductImpactCategoryId] INT NULL;

    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ProductImpactCategory' AND [Value] = 'Strategic')
        INSERT INTO LookupValues([Type],[Value]) VALUES('ProductImpactCategory','Strategic');
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ProductImpactCategory' AND [Value] = 'Enhancements')
        INSERT INTO LookupValues([Type],[Value]) VALUES('ProductImpactCategory','Enhancements');
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ProductImpactCategory' AND [Value] = 'KTLO')
        INSERT INTO LookupValues([Type],[Value]) VALUES('ProductImpactCategory','KTLO');

    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Items_ProductImpactCategoryId' AND object_id = OBJECT_ID('Items'))
        CREATE INDEX [IX_Items_ProductImpactCategoryId] ON [Items]([ProductImpactCategoryId]);
END TRY
BEGIN CATCH
    DECLARE @msg4 nvarchar(4000) = ERROR_MESSAGE();
    THROW 50003, @msg4, 1;
END CATCH;";

using (var cmd4 = conn.CreateCommand())
{
    cmd4.CommandText = sql4;
    cmd4.CommandTimeout = 120;
    await cmd4.ExecuteNonQueryAsync();
}
Console.WriteLine("ProductImpactCategory ensured.");

// Rename Owners table to Employee (singular) if needed and backfill missing person FKs
Console.WriteLine("Ensuring Employee table and backfilling person references...");
string sqlEmp = @"
BEGIN TRY
    -- Rename table Owners -> Employee if Employee does not exist
    IF OBJECT_ID(N'[dbo].[Employee]', N'U') IS NULL AND OBJECT_ID(N'[dbo].[Owners]', N'U') IS NOT NULL
        EXEC sp_rename 'dbo.Owners', 'Employee';

    -- Backfill Item.OwnerId and Item.ExecutiveSponsorId to 11 when NULL
    IF COL_LENGTH('Items','OwnerId') IS NOT NULL
        UPDATE Items SET OwnerId = 11 WHERE OwnerId IS NULL;
    IF COL_LENGTH('Items','ExecutiveSponsorId') IS NOT NULL
        UPDATE Items SET ExecutiveSponsorId = 11 WHERE ExecutiveSponsorId IS NULL;
END TRY
BEGIN CATCH
    DECLARE @msgEmp nvarchar(4000) = ERROR_MESSAGE();
    THROW 50005, @msgEmp, 1;
END CATCH;";
using (var cmdEmp = conn.CreateCommand())
{
    cmdEmp.CommandText = sqlEmp;
    cmdEmp.CommandTimeout = 120;
    await cmdEmp.ExecuteNonQueryAsync();
}
Console.WriteLine("Employee table ensured and person FKs backfilled.");

// Backfill required fields with 'Missing Data' and ensure required lookup FKs point to 'Missing Data'
Console.WriteLine("Backfilling required fields to 'Missing Data' where missing...");
string sql5 = @"
BEGIN TRY
    -- Ensure 'Missing Data' lookup exists for required types
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Missing Data')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Domain','Missing Data');
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'Division' AND [Value] = 'Missing Data')
        INSERT INTO LookupValues([Type],[Value]) VALUES('Division','Missing Data');
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'OperatingEntity' AND [Value] = 'Missing Data')
        INSERT INTO LookupValues([Type],[Value]) VALUES('OperatingEntity','Missing Data');
    IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Missing Data')
        INSERT INTO LookupValues([Type],[Value]) VALUES('RefreshFrequency','Missing Data');

    DECLARE @domainMissingId INT = (SELECT TOP 1 Id FROM LookupValues WHERE [Type] = 'Domain' AND [Value] = 'Missing Data');
    DECLARE @divisionMissingId INT = (SELECT TOP 1 Id FROM LookupValues WHERE [Type] = 'Division' AND [Value] = 'Missing Data');
    DECLARE @opMissingId INT = (SELECT TOP 1 Id FROM LookupValues WHERE [Type] = 'OperatingEntity' AND [Value] = 'Missing Data');
    DECLARE @rfMissingId INT = (SELECT TOP 1 Id FROM LookupValues WHERE [Type] = 'RefreshFrequency' AND [Value] = 'Missing Data');

    -- Backfill required lookup foreign keys when NULL
    UPDATE i SET i.DomainId = @domainMissingId FROM Items i WHERE i.DomainId IS NULL;
    UPDATE i SET i.DivisionId = @divisionMissingId FROM Items i WHERE i.DivisionId IS NULL;
    UPDATE i SET i.OperatingEntityId = @opMissingId FROM Items i WHERE i.OperatingEntityId IS NULL;
    UPDATE i SET i.RefreshFrequencyId = @rfMissingId FROM Items i WHERE i.RefreshFrequencyId IS NULL;

    -- Backfill required text fields when NULL/empty
    UPDATE Items SET Description = 'Missing Data' WHERE (Description IS NULL OR LTRIM(RTRIM(Description)) = '');
    UPDATE Items SET DataConsumers = 'Missing Data' WHERE (DataConsumers IS NULL OR LTRIM(RTRIM(DataConsumers)) = '');
    UPDATE Items SET BiPlatform = 'Missing Data' WHERE (BiPlatform IS NULL OR LTRIM(RTRIM(BiPlatform)) = '');
    UPDATE Items SET Dependencies = 'Missing Data' WHERE (Dependencies IS NULL OR LTRIM(RTRIM(Dependencies)) = '');
    UPDATE Items SET DefaultAdGroupNames = 'Missing Data' WHERE (DefaultAdGroupNames IS NULL OR LTRIM(RTRIM(DefaultAdGroupNames)) = '');
END TRY
BEGIN CATCH
    DECLARE @msg5 nvarchar(4000) = ERROR_MESSAGE();
    THROW 50004, @msg5, 1;
END CATCH;";
using (var cmd5 = conn.CreateCommand())
{
    cmd5.CommandText = sql5;
    cmd5.CommandTimeout = 180;
    await cmd5.ExecuteNonQueryAsync();
}
Console.WriteLine("Backfill complete.");
