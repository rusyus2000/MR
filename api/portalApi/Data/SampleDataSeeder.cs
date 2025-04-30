// Data/SampleDataSeeder.cs
using System;
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
            _db.SaveChanges();

            // Reload lookupValues from DB so they have IDs
            var allLookups = _db.LookupValues.ToList();

            // 2) Seed 25 sample items with DateAdded descending
            var now = DateTime.UtcNow;
            var samples = Enumerable.Range(1, 25).Select(i => new Item
            {
                Id = i,
                Title = $"Sample Asset {i}",
                Description = $"This is a description for Sample Asset {i}. It covers multiple scenarios for testing.",
                Url = $"https://example.com/resource/{i}",
                AssetTypesCsv = i % 5 == 0
                    ? "Dashboard,Featured"
                    : (i % 4 == 0
                        ? "Dashboard"
                        : i % 3 == 0
                            ? "Report"
                            : "Application"),
                Domain = allLookups
                             .First(l => l.Type == "Domain" &&
                                         allLookups.IndexOf(l) % 4 == (i - 1) % 4)
                             .Value,
                Division = allLookups
                               .First(l => l.Type == "Division" &&
                                           allLookups.IndexOf(l) % 4 == (i - 1) % 4)
                               .Value,
                ServiceLine = allLookups
                                  .First(l => l.Type == "ServiceLine" &&
                                              allLookups.IndexOf(l) % 6 == (i - 1) % 6)
                                  .Value,
                DataSource = allLookups
                                 .First(l => l.Type == "DataSource" &&
                                             allLookups.IndexOf(l) % 4 == (i - 1) % 4)
                                 .Value,
                PrivacyPhi = (i % 2 == 0),
                DateAdded = now.AddDays(-i)
            }).ToArray();

            _db.Items.AddRange(samples);
            _db.SaveChanges();
        }
    }
}
