using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMustDo2025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This migration is designed to be safe to apply on environments that may already have
            // some of these schema changes (e.g., manual DBA updates on QA/DEV).
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Employee]', N'U') IS NULL
BEGIN
    IF OBJECT_ID(N'[dbo].[Owners]', N'U') IS NOT NULL
        EXEC sp_rename N'[dbo].[Owners]', N'Employee';
    ELSE
        CREATE TABLE [dbo].[Employee](
            [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Employee] PRIMARY KEY,
            [Name] nvarchar(max) NOT NULL,
            [Email] nvarchar(max) NOT NULL
        );
END
");

            // Ensure Item person FKs target Employee (drop old Owners FKs if present).
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[dbo].[Items]', N'U') IS NOT NULL
BEGIN
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Items_Owners_OwnerId')
        ALTER TABLE [dbo].[Items] DROP CONSTRAINT [FK_Items_Owners_OwnerId];
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Items_Owners_ExecutiveSponsorId')
        ALTER TABLE [dbo].[Items] DROP CONSTRAINT [FK_Items_Owners_ExecutiveSponsorId];

    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Items_Employee_OwnerId')
        ALTER TABLE [dbo].[Items] WITH CHECK ADD CONSTRAINT [FK_Items_Employee_OwnerId] FOREIGN KEY ([OwnerId]) REFERENCES [dbo].[Employee]([Id]);
    IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Items_Employee_ExecutiveSponsorId')
        ALTER TABLE [dbo].[Items] WITH CHECK ADD CONSTRAINT [FK_Items_Employee_ExecutiveSponsorId] FOREIGN KEY ([ExecutiveSponsorId]) REFERENCES [dbo].[Employee]([Id]);
END
");

            // Ensure ProductImpactCategoryId exists (do not repurpose MustDo2025Id).
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Items', N'ProductImpactCategoryId') IS NULL
    ALTER TABLE [dbo].[Items] ADD [ProductImpactCategoryId] int NULL;

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Items_ProductImpactCategoryId' AND object_id = OBJECT_ID(N'[dbo].[Items]')
)
    CREATE INDEX [IX_Items_ProductImpactCategoryId] ON [dbo].[Items]([ProductImpactCategoryId]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Items_LookupValues_ProductImpactCategoryId')
    ALTER TABLE [dbo].[Items] WITH CHECK ADD CONSTRAINT [FK_Items_LookupValues_ProductImpactCategoryId]
        FOREIGN KEY ([ProductImpactCategoryId]) REFERENCES [dbo].[LookupValues]([Id]);
");

            // Drop MustDo2025Id from Items and remove lookup values.
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Items', N'MustDo2025Id') IS NOT NULL
BEGIN
    -- Drop FK(s) that reference MustDo2025Id (name may vary)
    DECLARE @dropFkSql nvarchar(max) = N'';
    SELECT @dropFkSql = @dropFkSql + N'ALTER TABLE [dbo].[Items] DROP CONSTRAINT ' + QUOTENAME(fk.name) + N';' + CHAR(10)
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    JOIN sys.columns c ON c.object_id = fkc.parent_object_id AND c.column_id = fkc.parent_column_id
    WHERE fk.parent_object_id = OBJECT_ID(N'[dbo].[Items]') AND c.name = N'MustDo2025Id';
    IF (@dropFkSql <> N'') EXEC sp_executesql @dropFkSql;

    -- Drop any non-constraint indexes that include MustDo2025Id (name may vary)
    DECLARE @dropIdxSql nvarchar(max) = N'';
    SELECT @dropIdxSql = @dropIdxSql + N'DROP INDEX ' + QUOTENAME(i.name) + N' ON [dbo].[Items];' + CHAR(10)
    FROM sys.indexes i
    JOIN sys.index_columns ic ON ic.object_id = i.object_id AND ic.index_id = i.index_id
    JOIN sys.columns c ON c.object_id = ic.object_id AND c.column_id = ic.column_id
    WHERE i.object_id = OBJECT_ID(N'[dbo].[Items]')
      AND c.name = N'MustDo2025Id'
      AND i.is_primary_key = 0
      AND i.is_unique_constraint = 0;
    IF (@dropIdxSql <> N'') EXEC sp_executesql @dropIdxSql;

    ALTER TABLE [dbo].[Items] DROP COLUMN [MustDo2025Id];
END

DELETE FROM [dbo].[LookupValues] WHERE [Type] = N'MustDo2025';
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Best-effort restore of the MustDo2025 lookup and column. This does not attempt to
            // revert Employee-related changes, which may be shared by other migrations/environments.
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Items', N'MustDo2025Id') IS NULL
BEGIN
    ALTER TABLE [dbo].[Items] ADD [MustDo2025Id] int NULL;
END

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Items_MustDo2025Id' AND object_id = OBJECT_ID(N'[dbo].[Items]')
)
    CREATE INDEX [IX_Items_MustDo2025Id] ON [dbo].[Items]([MustDo2025Id]);

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Items_LookupValues_MustDo2025Id')
    ALTER TABLE [dbo].[Items] WITH CHECK ADD CONSTRAINT [FK_Items_LookupValues_MustDo2025Id]
        FOREIGN KEY ([MustDo2025Id]) REFERENCES [dbo].[LookupValues]([Id]);

IF NOT EXISTS (SELECT 1 FROM [dbo].[LookupValues] WHERE [Type] = N'MustDo2025' AND [Value] = N'Yes')
    INSERT INTO [dbo].[LookupValues]([Type],[Value]) VALUES (N'MustDo2025', N'Yes');
IF NOT EXISTS (SELECT 1 FROM [dbo].[LookupValues] WHERE [Type] = N'MustDo2025' AND [Value] = N'No')
    INSERT INTO [dbo].[LookupValues]([Type],[Value]) VALUES (N'MustDo2025', N'No');
");
        }
    }
}
