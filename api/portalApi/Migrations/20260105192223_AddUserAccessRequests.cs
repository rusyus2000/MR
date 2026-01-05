using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAccessRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccessRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccessRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAccessRequests_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAccessRequests_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessRequests_ItemId",
                table: "UserAccessRequests",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAccessRequests_UserId_ItemId",
                table: "UserAccessRequests",
                columns: new[] { "UserId", "ItemId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccessRequests");
        }
    }
}
