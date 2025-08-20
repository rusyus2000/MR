// Data/SampleDataSeeder.cs (simplified)
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
                new LookupValue { Type = "ServiceLine", Value = "Behavioral Health" },
                new LookupValue { Type = "ServiceLine", Value = "Cardiology" },
                new LookupValue { Type = "ServiceLine", Value = "Hospital" },
                new LookupValue { Type = "ServiceLine", Value = "Oncology" },
                new LookupValue { Type = "ServiceLine", Value = "Primary Care" },
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
            };

            _db.LookupValues.AddRange(lookups);
            _db.SaveChanges();

            var all = _db.LookupValues.ToList();
            int L(string type, string value) => all.First(l => l.Type == type && l.Value == value).Id;

            var now = DateTime.UtcNow;

            var samples = new[]
            {
                new Item { Title = "Patient Flow Dashboard", Description = "Visualizes patient wait times.", Url = "https://dashboard.healthorg.com/patient-flow", AssetTypeId = L("AssetType","Dashboard"), DomainId = L("Domain","Access to Care"), DivisionId = L("Division","Greater Central Valley"), ServiceLineId = L("ServiceLine","Hospital"), DataSourceId = L("DataSource","Power BI"), PrivacyPhi = true, DateAdded = now.AddDays(-1) },
                new Item { Title = "Leadership KPI Dashboard", Description = "Executive-level dashboard.", Url = "https://dashboard.healthorg.com/leadership-kpi", AssetTypeId = L("AssetType","Dashboard"), Featured = true, DomainId = L("Domain","Quality"), DivisionId = L("Division","Greater East Bay"), ServiceLineId = L("ServiceLine","Cardiology"), DataSourceId = L("DataSource","Epic"), PrivacyPhi = false, DateAdded = now.AddDays(-3) }
            };

            _db.Items.AddRange(samples);
            _db.SaveChanges();
        }
    }
}
