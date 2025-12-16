using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    public partial class RemoveUserVisibilityNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserVisibilityNumber",
                table: "Items");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserVisibilityNumber",
                table: "Items",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}

