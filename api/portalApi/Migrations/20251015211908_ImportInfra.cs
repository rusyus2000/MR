using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class ImportInfra : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ContentHash",
                table: "Items",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Items",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ImportJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Token = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DatasetKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Total = table.Column<int>(type: "int", nullable: false),
                    ToCreate = table.Column<int>(type: "int", nullable: false),
                    ToUpdate = table.Column<int>(type: "int", nullable: false),
                    Unchanged = table.Column<int>(type: "int", nullable: false),
                    Conflicts = table.Column<int>(type: "int", nullable: false),
                    Errors = table.Column<int>(type: "int", nullable: false),
                    PreviewJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImportJobs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImportJobs_Token",
                table: "ImportJobs",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImportJobs");

            migrationBuilder.DropColumn(
                name: "ContentHash",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Items");
        }
    }
}
