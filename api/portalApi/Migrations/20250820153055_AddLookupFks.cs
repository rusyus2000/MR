using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddLookupFks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetTypeId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DataSourceId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DivisionId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DomainId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceLineId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_AssetTypeId",
                table: "Items",
                column: "AssetTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DataSourceId",
                table: "Items",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DivisionId",
                table: "Items",
                column: "DivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_DomainId",
                table: "Items",
                column: "DomainId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_ServiceLineId",
                table: "Items",
                column: "ServiceLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_AssetTypeId",
                table: "Items",
                column: "AssetTypeId",
                principalTable: "LookupValues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_DataSourceId",
                table: "Items",
                column: "DataSourceId",
                principalTable: "LookupValues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_DivisionId",
                table: "Items",
                column: "DivisionId",
                principalTable: "LookupValues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_DomainId",
                table: "Items",
                column: "DomainId",
                principalTable: "LookupValues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_ServiceLineId",
                table: "Items",
                column: "ServiceLineId",
                principalTable: "LookupValues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_AssetTypeId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_DataSourceId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_DivisionId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_DomainId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_ServiceLineId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_AssetTypeId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_DataSourceId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_DivisionId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_DomainId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ServiceLineId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "AssetTypeId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DataSourceId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DomainId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ServiceLineId",
                table: "Items");
        }
    }
}
