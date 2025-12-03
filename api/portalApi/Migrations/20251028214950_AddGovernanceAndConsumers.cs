using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class AddGovernanceAndConsumers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultAdGroupNames",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Dependencies",
                table: "Items",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExecutiveSponsorId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasRls",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedDate",
                table: "Items",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OperatingEntityId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrivacyPii",
                table: "Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RefreshFrequencyId",
                table: "Items",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ItemDataConsumers",
                columns: table => new
                {
                    ItemId = table.Column<int>(type: "int", nullable: false),
                    DataConsumerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemDataConsumers", x => new { x.ItemId, x.DataConsumerId });
                    table.ForeignKey(
                        name: "FK_ItemDataConsumers_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ItemDataConsumers_LookupValues_DataConsumerId",
                        column: x => x.DataConsumerId,
                        principalTable: "LookupValues",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Items_ExecutiveSponsorId",
                table: "Items",
                column: "ExecutiveSponsorId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_HasRls",
                table: "Items",
                column: "HasRls");

            migrationBuilder.CreateIndex(
                name: "IX_Items_OperatingEntityId",
                table: "Items",
                column: "OperatingEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_PrivacyPii",
                table: "Items",
                column: "PrivacyPii");

            migrationBuilder.CreateIndex(
                name: "IX_Items_RefreshFrequencyId",
                table: "Items",
                column: "RefreshFrequencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ItemDataConsumers_DataConsumerId",
                table: "ItemDataConsumers",
                column: "DataConsumerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_OperatingEntityId",
                table: "Items",
                column: "OperatingEntityId",
                principalTable: "LookupValues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_LookupValues_RefreshFrequencyId",
                table: "Items",
                column: "RefreshFrequencyId",
                principalTable: "LookupValues",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Owners_ExecutiveSponsorId",
                table: "Items",
                column: "ExecutiveSponsorId",
                principalTable: "Owners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_OperatingEntityId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_LookupValues_RefreshFrequencyId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Owners_ExecutiveSponsorId",
                table: "Items");

            migrationBuilder.DropTable(
                name: "ItemDataConsumers");

            migrationBuilder.DropIndex(
                name: "IX_Items_ExecutiveSponsorId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_HasRls",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_OperatingEntityId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_PrivacyPii",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_RefreshFrequencyId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "DefaultAdGroupNames",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "Dependencies",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "ExecutiveSponsorId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "HasRls",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "LastModifiedDate",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "OperatingEntityId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "PrivacyPii",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "RefreshFrequencyId",
                table: "Items");
        }
    }
}
