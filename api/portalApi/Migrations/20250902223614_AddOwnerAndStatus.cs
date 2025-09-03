using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerAndStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Owners",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_OwnerId",
                table: "Items",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_StatusId",
                table: "Items",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_StatusId",
                table: "Items",
                column: "StatusId",
                principalTable: "LookupValues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Owners_OwnerId",
                table: "Items",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_StatusId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Owners_OwnerId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "Owners");

            migrationBuilder.DropIndex(
                name: "IX_Items_OwnerId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_StatusId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Items");
        }
    }
}
