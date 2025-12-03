using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SutterAnalyticsApi.Data;

namespace portalApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20251202134500_AddNewItemLookupFields")]
    public partial class AddNewItemLookupFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add nullable FK columns
            migrationBuilder.AddColumn<int>(name: "PotentialToConsolidateId", table: "Items", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "PotentialToAutomateId", table: "Items", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "SponsorBusinessValueId", table: "Items", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "MustDo2025Id", table: "Items", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "DevelopmentEffortId", table: "Items", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "EstimatedDevHoursId", table: "Items", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "ResourcesDevelopmentId", table: "Items", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "ResourcesAnalystsId", table: "Items", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "ResourcesPlatformId", table: "Items", type: "int", nullable: true);
            migrationBuilder.AddColumn<int>(name: "ResourcesDataEngineeringId", table: "Items", type: "int", nullable: true);

            // Indexes
            migrationBuilder.CreateIndex(name: "IX_Items_PotentialToConsolidateId", table: "Items", column: "PotentialToConsolidateId");
            migrationBuilder.CreateIndex(name: "IX_Items_PotentialToAutomateId", table: "Items", column: "PotentialToAutomateId");
            migrationBuilder.CreateIndex(name: "IX_Items_SponsorBusinessValueId", table: "Items", column: "SponsorBusinessValueId");
            migrationBuilder.CreateIndex(name: "IX_Items_MustDo2025Id", table: "Items", column: "MustDo2025Id");
            migrationBuilder.CreateIndex(name: "IX_Items_DevelopmentEffortId", table: "Items", column: "DevelopmentEffortId");
            migrationBuilder.CreateIndex(name: "IX_Items_EstimatedDevHoursId", table: "Items", column: "EstimatedDevHoursId");
            migrationBuilder.CreateIndex(name: "IX_Items_ResourcesDevelopmentId", table: "Items", column: "ResourcesDevelopmentId");
            migrationBuilder.CreateIndex(name: "IX_Items_ResourcesAnalystsId", table: "Items", column: "ResourcesAnalystsId");
            migrationBuilder.CreateIndex(name: "IX_Items_ResourcesPlatformId", table: "Items", column: "ResourcesPlatformId");
            migrationBuilder.CreateIndex(name: "IX_Items_ResourcesDataEngineeringId", table: "Items", column: "ResourcesDataEngineeringId");

            // FKs
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_PotentialToConsolidateId", "Items", "PotentialToConsolidateId", "LookupValues", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_PotentialToAutomateId", "Items", "PotentialToAutomateId", "LookupValues", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_SponsorBusinessValueId", "Items", "SponsorBusinessValueId", "LookupValues", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_MustDo2025Id", "Items", "MustDo2025Id", "LookupValues", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_DevelopmentEffortId", "Items", "DevelopmentEffortId", "LookupValues", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_EstimatedDevHoursId", "Items", "EstimatedDevHoursId", "LookupValues", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_ResourcesDevelopmentId", "Items", "ResourcesDevelopmentId", "LookupValues", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_ResourcesAnalystsId", "Items", "ResourcesAnalystsId", "LookupValues", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_ResourcesPlatformId", "Items", "ResourcesPlatformId", "LookupValues", principalColumn: "Id");
            migrationBuilder.AddForeignKey("FK_Items_LookupValues_ResourcesDataEngineeringId", "Items", "ResourcesDataEngineeringId", "LookupValues", principalColumn: "Id");

            // Seed the new lookup types & values
            void Seed(string type, string[] values)
            {
                foreach (var v in values)
                {
                    migrationBuilder.Sql($@"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = '{type}' AND [Value] = '{v}')
BEGIN INSERT INTO LookupValues([Type],[Value]) VALUES('{type}','{v}'); END;");
                }
            }

            Seed("PotentialToConsolidate", new[] { "Yes", "No" });
            Seed("PotentialToAutomate", new[] { "Yes", "No" });
            Seed("SponsorBusinessValue", new[] { "Low", "Medium", "High" });
            Seed("MustDo2025", new[] { "Yes", "No" });
            Seed("DevelopmentEffort", new[] { "Low", "Medium", "High" });
            Seed("EstimatedDevHours", new[] { "40", "100", "200", "300", "500", "1000" });
            Seed("ResourcesDevelopment", new[] { "TBD" });
            Seed("ResourcesAnalysts", new[] { "TBD" });
            Seed("ResourcesPlatform", new[] { "TBD" });
            Seed("ResourcesDataEngineering", new[] { "TBD" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FKs & indexes then columns
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_PotentialToConsolidateId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_PotentialToAutomateId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_SponsorBusinessValueId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_MustDo2025Id", "Items");
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_DevelopmentEffortId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_EstimatedDevHoursId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_ResourcesDevelopmentId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_ResourcesAnalystsId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_ResourcesPlatformId", "Items");
            migrationBuilder.DropForeignKey("FK_Items_LookupValues_ResourcesDataEngineeringId", "Items");

            migrationBuilder.DropIndex("IX_Items_PotentialToConsolidateId", "Items");
            migrationBuilder.DropIndex("IX_Items_PotentialToAutomateId", "Items");
            migrationBuilder.DropIndex("IX_Items_SponsorBusinessValueId", "Items");
            migrationBuilder.DropIndex("IX_Items_MustDo2025Id", "Items");
            migrationBuilder.DropIndex("IX_Items_DevelopmentEffortId", "Items");
            migrationBuilder.DropIndex("IX_Items_EstimatedDevHoursId", "Items");
            migrationBuilder.DropIndex("IX_Items_ResourcesDevelopmentId", "Items");
            migrationBuilder.DropIndex("IX_Items_ResourcesAnalystsId", "Items");
            migrationBuilder.DropIndex("IX_Items_ResourcesPlatformId", "Items");
            migrationBuilder.DropIndex("IX_Items_ResourcesDataEngineeringId", "Items");

            migrationBuilder.DropColumn("PotentialToConsolidateId", "Items");
            migrationBuilder.DropColumn("PotentialToAutomateId", "Items");
            migrationBuilder.DropColumn("SponsorBusinessValueId", "Items");
            migrationBuilder.DropColumn("MustDo2025Id", "Items");
            migrationBuilder.DropColumn("DevelopmentEffortId", "Items");
            migrationBuilder.DropColumn("EstimatedDevHoursId", "Items");
            migrationBuilder.DropColumn("ResourcesDevelopmentId", "Items");
            migrationBuilder.DropColumn("ResourcesAnalystsId", "Items");
            migrationBuilder.DropColumn("ResourcesPlatformId", "Items");
            migrationBuilder.DropColumn("ResourcesDataEngineeringId", "Items");
        }
    }
}

