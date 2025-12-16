using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServiceLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_ServiceLineId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_ServiceLineId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ServiceLineId",
                table: "Items");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceLineId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_ServiceLineId",
                table: "Items",
                column: "ServiceLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_ServiceLineId",
                table: "Items",
                column: "ServiceLineId",
                principalTable: "LookupValues",
                principalColumn: "Id");
        }
    }
}
