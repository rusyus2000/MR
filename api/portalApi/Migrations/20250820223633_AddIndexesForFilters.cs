using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserPrincipalName",
                table: "Users",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Tags",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "LookupValues",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "LookupValues",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserPrincipalName",
                table: "Users",
                column: "UserPrincipalName");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Value",
                table: "Tags",
                column: "Value");

            migrationBuilder.CreateIndex(
                name: "IX_LookupValues_Type_Value",
                table: "LookupValues",
                columns: new[] { "Type", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_Items_DateAdded",
                table: "Items",
                column: "DateAdded");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Featured",
                table: "Items",
                column: "Featured");

            migrationBuilder.CreateIndex(
                name: "IX_Items_PrivacyPhi",
                table: "Items",
                column: "PrivacyPhi");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_UserPrincipalName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tags_Value",
                table: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_LookupValues_Type_Value",
                table: "LookupValues");

            migrationBuilder.DropIndex(
                name: "IX_Items_DateAdded",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_Featured",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_PrivacyPhi",
                table: "Items");

            migrationBuilder.AlterColumn<string>(
                name: "UserPrincipalName",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "LookupValues",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "LookupValues",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
