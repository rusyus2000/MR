using Microsoft.EntityFrameworkCore.Migrations;

namespace portalApi.Migrations
{
    public partial class RemoveServiceLineAndPromoteOperatingEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = 'ServiceLine')
BEGIN
    INSERT INTO LookupValues([Type],[Value])
    SELECT 'OperatingEntity', sl.[Value]
    FROM LookupValues sl
    LEFT JOIN LookupValues oe ON oe.[Type] = 'OperatingEntity' AND oe.[Value] = sl.[Value]
    WHERE sl.[Type] = 'ServiceLine' AND oe.Id IS NULL;
END

UPDATE i
SET i.OperatingEntityId = oe.Id
FROM Items i
JOIN LookupValues sl ON sl.Id = i.ServiceLineId AND sl.[Type] = 'ServiceLine'
JOIN LookupValues oe ON oe.[Type] = 'OperatingEntity' AND oe.[Value] = sl.[Value]
WHERE i.OperatingEntityId IS NULL;

IF COL_LENGTH('Items','ServiceLineId') IS NOT NULL
BEGIN
    DECLARE @fk sysname;
    SELECT @fk = fk.name
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
    JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
    WHERE fk.parent_object_id = OBJECT_ID('Items') AND c.name = 'ServiceLineId';
    IF @fk IS NOT NULL EXEC('ALTER TABLE [Items] DROP CONSTRAINT [' + @fk + ']');
    IF EXISTS(SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Items') AND name = 'IX_Items_ServiceLineId')
        DROP INDEX [IX_Items_ServiceLineId] ON [Items];
    ALTER TABLE [Items] DROP COLUMN [ServiceLineId];
END

DELETE FROM LookupValues WHERE [Type] = 'ServiceLine';


            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Irreversible
        }
    }
}

