using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class SwapBiPlatformAndDataSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_DataSourceId",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "DataSourceId",
                table: "Items",
                newName: "BiPlatformId");

            migrationBuilder.RenameColumn(
                name: "BiPlatform",
                table: "Items",
                newName: "DataSource");

            migrationBuilder.RenameIndex(
                name: "IX_Items_DataSourceId",
                table: "Items",
                newName: "IX_Items_BiPlatformId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_BiPlatformId",
                table: "Items",
                column: "BiPlatformId",
                principalTable: "LookupValues",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_BiPlatformId",
                table: "Items");

            migrationBuilder.RenameColumn(
                name: "DataSource",
                table: "Items",
                newName: "BiPlatform");

            migrationBuilder.RenameColumn(
                name: "BiPlatformId",
                table: "Items",
                newName: "DataSourceId");

            migrationBuilder.RenameIndex(
                name: "IX_Items_BiPlatformId",
                table: "Items",
                newName: "IX_Items_DataSourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_DataSourceId",
                table: "Items",
                column: "DataSourceId",
                principalTable: "LookupValues",
                principalColumn: "Id");
        }
    }
}
