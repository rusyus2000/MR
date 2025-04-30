using System.Linq;
using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Data
{
    public class SampleDataSeeder
    {
        private readonly AppDbContext _db;

        public SampleDataSeeder(AppDbContext db)
        {
            _db = db;
        }

        public void Seed()
        {
            if (_db.Items.Any()) return;

            // 1) Seed lookup values
            var lookups = new[]
            {
                // Domains
                new LookupValue { Type = "Domain", Value = "Access to Care" },
                new LookupValue { Type = "Domain", Value = "Quality" },
                new LookupValue { Type = "Domain", Value = "Strategy" },
                new LookupValue { Type = "Domain", Value = "People & Workforce" },
                // Divisions
                new LookupValue { Type = "Division", Value = "Greater Central Valley" },
                new LookupValue { Type = "Division", Value = "Greater East Bay" },
                new LookupValue { Type = "Division", Value = "Greater Sacramento" },
                new LookupValue { Type = "Division", Value = "Greater San Francisco" },
                // Service Lines
                new LookupValue { Type = "ServiceLine", Value = "Primary Care" },
                new LookupValue { Type = "ServiceLine", Value = "Cardiology" },
                new LookupValue { Type = "ServiceLine", Value = "Behavioral Health" },
                new LookupValue { Type = "ServiceLine", Value = "Hospital" },
                new LookupValue { Type = "ServiceLine", Value = "Oncology" },
                new LookupValue { Type = "ServiceLine", Value = "Orthopedics" },
                // Data Sources
                new LookupValue { Type = "DataSource", Value = "Power BI" },
                new LookupValue { Type = "DataSource", Value = "Epic" },
                new LookupValue { Type = "DataSource", Value = "Tableau" },
                new LookupValue { Type = "DataSource", Value = "Web-Based" },
                // Asset Types
                new LookupValue { Type = "AssetType", Value = "Dashboard" },
                new LookupValue { Type = "AssetType", Value = "Report" },
                new LookupValue { Type = "AssetType", Value = "Application" },
                new LookupValue { Type = "AssetType", Value = "Data Model" },
                new LookupValue { Type = "AssetType", Value = "Featured" },
            };
            _db.LookupValues.AddRange(lookups);

            // 2) Seed 25 sample items
            var samples = Enumerable.Range(1, 25).Select(i => new Item
            {
                Id = i,
                Title = $"Sample Asset {i}",
                Description = $"This is a description for Sample Asset {i}. It covers multiple scenarios for testing.",
                Url = $"https://example.com/resource/{i}",
                AssetTypesCsv = i % 5 == 0 ? "Dashboard,Featured" : (i % 4 == 0 ? "Dashboard" : i % 3 == 0 ? "Report" : "Application"),
                Domain = lookups.First(l => l.Type == "Domain" && l.Id % 4 + 1 == (i % 4) + 1).Value,
                Division = lookups.First(l => l.Type == "Division" && l.Id % 4 + 5 == (i % 4) + 5).Value,
                ServiceLine = lookups.First(l => l.Type == "ServiceLine" && l.Id % 6 + 9 == (i % 6) + 9).Value,
                DataSource = lookups.First(l => l.Type == "DataSource" && l.Id % 4 + 13 == (i % 4) + 13).Value,
                PrivacyPhi = (i % 2 == 0)
            }).ToArray();
            _db.Items.AddRange(samples);

            _db.SaveChanges();
        }
    }
}
