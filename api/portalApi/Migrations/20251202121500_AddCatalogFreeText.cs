using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogFreeText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProductGroup",
                table: "Items",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductStatusNotes",
                table: "Items",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataConsumersText",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TechDeliveryManager",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegulatoryComplianceContractual",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BiPlatform",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DbServer",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DbDataMart",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatabaseTable",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceRep",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DataSecurityClassification",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessGroupName",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccessGroupDn",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AutomationClassification",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserVisibilityString",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserVisibilityNumber",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EpicSecurityGroupTag",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeepLongTerm",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductGroup",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ProductStatusNotes",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DataConsumersText",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "TechDeliveryManager",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "RegulatoryComplianceContractual",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "BiPlatform",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DbServer",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DbDataMart",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DatabaseTable",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "SourceRep",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DataSecurityClassification",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AccessGroupName",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AccessGroupDn",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AutomationClassification",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UserVisibilityString",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UserVisibilityNumber",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "EpicSecurityGroupTag",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "KeepLongTerm",
                table: "Items");
        }
    }
}

