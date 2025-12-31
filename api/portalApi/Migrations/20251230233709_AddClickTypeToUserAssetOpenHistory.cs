using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddClickTypeToUserAssetOpenHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[UserAssetOpenHistories]') IS NOT NULL
BEGIN
    DELETE FROM [UserAssetOpenHistories];
    IF COL_LENGTH('UserAssetOpenHistories','ClickType') IS NULL
        ALTER TABLE [UserAssetOpenHistories]
        ADD [ClickType] nvarchar(max) NOT NULL
            CONSTRAINT [DF_UserAssetOpenHistories_ClickType] DEFAULT ('Details');
END
ELSE IF OBJECT_ID(N'[UserAssetOpenHistory]') IS NOT NULL
BEGIN
    DELETE FROM [UserAssetOpenHistory];
    IF COL_LENGTH('UserAssetOpenHistory','ClickType') IS NULL
        ALTER TABLE [UserAssetOpenHistory]
        ADD [ClickType] nvarchar(max) NOT NULL
            CONSTRAINT [DF_UserAssetOpenHistory_ClickType] DEFAULT ('Details');
END
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[UserAssetOpenHistories]') IS NOT NULL AND COL_LENGTH('UserAssetOpenHistories','ClickType') IS NOT NULL
BEGIN
    DECLARE @df1 nvarchar(200);
    SELECT @df1 = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[UserAssetOpenHistories]') AND c.name = 'ClickType';
    IF @df1 IS NOT NULL EXEC('ALTER TABLE [UserAssetOpenHistories] DROP CONSTRAINT [' + @df1 + ']');
    ALTER TABLE [UserAssetOpenHistories] DROP COLUMN [ClickType];
END
ELSE IF OBJECT_ID(N'[UserAssetOpenHistory]') IS NOT NULL AND COL_LENGTH('UserAssetOpenHistory','ClickType') IS NOT NULL
BEGIN
    DECLARE @df2 nvarchar(200);
    SELECT @df2 = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'[UserAssetOpenHistory]') AND c.name = 'ClickType';
    IF @df2 IS NOT NULL EXEC('ALTER TABLE [UserAssetOpenHistory] DROP CONSTRAINT [' + @df2 + ']');
    ALTER TABLE [UserAssetOpenHistory] DROP COLUMN [ClickType];
END
");
        }
    }
}
