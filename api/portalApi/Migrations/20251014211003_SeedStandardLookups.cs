using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace portalApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedStandardLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            static string Esc(string s) => s.Replace("'", "''");
            void Upsert(string type, string value)
            {
                var sql = $"IF NOT EXISTS (SELECT 1 FROM LookupValues WHERE [Type] = '{Esc(type)}' AND [Value] = '{Esc(value)}')\n" +
                          $"    INSERT INTO LookupValues([Type],[Value]) VALUES('{Esc(type)}','{Esc(value)}');";
                migrationBuilder.Sql(sql);
            }

            // STATUS
            Upsert("Status", "Active - Legacy");
            Upsert("Status", "Active - Approved Standard in Prod");
            Upsert("Status", "Active - Legal Retained");
            Upsert("Status", "Decommissioned");
            Upsert("Status", "Not Started");
            Upsert("Status", "On-Hold");
            Upsert("Status", "Pending Decommision");
            Upsert("Status", "Unknown");
            Upsert("Status", "Released to Production");
            Upsert("Status", "Acquisition or in Development");
            Upsert("Status", "In Pilot");
            Upsert("Status", "In Review");
            Upsert("Status", "In Development");

            // DOMAIN
            Upsert("Domain", "Access");
            Upsert("Domain", "Business Operations (e.g. Digital, Marketing, Legal)");
            Upsert("Domain", "Clinical Operations");
            Upsert("Domain", "Division");
            Upsert("Domain", "Finance");
            Upsert("Domain", "Foundation");
            Upsert("Domain", "Growth & Strategy");
            Upsert("Domain", "Hospital");
            Upsert("Domain", "Independent Practice Association (IPA)");
            Upsert("Domain", "Insured Products");
            Upsert("Domain", "Medical Group");
            Upsert("Domain", "Patient Experience");
            Upsert("Domain", "People & Workforce");
            Upsert("Domain", "Quality ");
            Upsert("Domain", "Service Lines");

            // ASSETTYPE
            Upsert("AssetType", "API");
            Upsert("AssetType", "Application");
            Upsert("AssetType", "Code");
            Upsert("AssetType", "Dashboard");
            Upsert("AssetType", "Data Extract");
            Upsert("AssetType", "ETL Data Pipeline");
            Upsert("AssetType", "ML Model");
            Upsert("AssetType", "Patient Registry");
            Upsert("AssetType", "Report");

            // DIVISION
            Upsert("Division", "Greater Central Coast");
            Upsert("Division", "Greater Central Valley");
            Upsert("Division", "Greater East Bay");
            Upsert("Division", "Greater Sacramento");
            Upsert("Division", "Greater San Francisco (including North Bay)");
            Upsert("Division", "Greater Silicon Valley");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
