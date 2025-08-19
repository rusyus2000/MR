using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class FinalNormalizeTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop dependent index before altering column
            migrationBuilder.DropIndex(
                name: "IX_Tags_Value",
                table: "Tags");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);

            // Foreign keys already created in previous migration; no-op here.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // foreign keys were created earlier; nothing to drop here.

            // Recreate the index with previous definition
            migrationBuilder.CreateIndex(
                name: "IX_Tags_Value",
                table: "Tags",
                column: "Value",
                unique: true,
                filter: "[Value] IS NOT NULL");

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "Tags",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
