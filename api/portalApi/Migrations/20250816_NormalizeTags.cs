using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    [Microsoft.EntityFrameworkCore.Infrastructure.DbContextAttribute(typeof(SutterAnalyticsApi.Data.AppDbContext))]
    [Migration("20250816_NormalizeTags")]
    public partial class NormalizeTags : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemTags",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    TagId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemTags", x => new { x.ItemId, x.TagId });
                    table.ForeignKey(
                        name: "FK_ItemTags_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ItemTags_TagId",
                table: "ItemTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Value",
                table: "Tags",
                column: "Value",
                unique: true);

            // Migrate existing tags from TagsCsv into Tags and ItemTags
            // Note: uses STRING_SPLIT which does not guarantee order.
            migrationBuilder.Sql(@"
                INSERT INTO Tags(Value)
                SELECT DISTINCT LTRIM(RTRIM(s.value))
                FROM Items
                CROSS APPLY STRING_SPLIT(Items.TagsCsv, ',') s
                WHERE Items.TagsCsv IS NOT NULL AND Items.TagsCsv <> ''
                    AND LTRIM(RTRIM(s.value)) <> ''
                    AND NOT EXISTS(SELECT 1 FROM Tags t WHERE t.Value = LTRIM(RTRIM(s.value)));

                INSERT INTO ItemTags(ItemId, TagId)
                SELECT i.Id, t.Id
                FROM Items i
                CROSS APPLY STRING_SPLIT(i.TagsCsv, ',') s
                JOIN Tags t ON t.Value = LTRIM(RTRIM(s.value))
                WHERE i.TagsCsv IS NOT NULL AND i.TagsCsv <> '' AND LTRIM(RTRIM(s.value)) <> ''
                AND NOT EXISTS(SELECT 1 FROM ItemTags it WHERE it.ItemId = i.Id AND it.TagId = t.Id);

                -- Drop the old TagsCsv column now that tags are normalized
                ALTER TABLE Items DROP COLUMN TagsCsv;
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // add TagsCsv back
            migrationBuilder.AddColumn<string>(
                name: "TagsCsv",
                table: "Items",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // rebuild TagsCsv from ItemTags
            migrationBuilder.Sql(@"
                UPDATE Items
                SET TagsCsv = ISNULL( (
                    SELECT STUFF((
                        SELECT ',' + t.Value
                        FROM ItemTags it
                        JOIN Tags t ON t.Id = it.TagId
                        WHERE it.ItemId = Items.Id
                        FOR XML PATH(''), TYPE
                    ).value('.', 'nvarchar(max)'),1,1,'')
                ), '')
            ");

            migrationBuilder.DropTable(
                name: "ItemTags");

            migrationBuilder.DropTable(
                name: "Tags");
        }
    }
}
