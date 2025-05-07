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

            var lookups = new[]
            {
                new LookupValue { Type = "Domain", Value = "Access to Care" },
                new LookupValue { Type = "Domain", Value = "Quality" },
                new LookupValue { Type = "Domain", Value = "Strategy" },
                new LookupValue { Type = "Domain", Value = "People & Workforce" },

                new LookupValue { Type = "Division", Value = "Greater Central Valley" },
                new LookupValue { Type = "Division", Value = "Greater East Bay" },
                new LookupValue { Type = "Division", Value = "Greater Sacramento" },
                new LookupValue { Type = "Division", Value = "Greater San Francisco" },

                new LookupValue { Type = "ServiceLine", Value = "Primary Care" },
                new LookupValue { Type = "ServiceLine", Value = "Cardiology" },
                new LookupValue { Type = "ServiceLine", Value = "Behavioral Health" },
                new LookupValue { Type = "ServiceLine", Value = "Hospital" },
                new LookupValue { Type = "ServiceLine", Value = "Oncology" },
                new LookupValue { Type = "ServiceLine", Value = "Orthopedics" },

                new LookupValue { Type = "DataSource", Value = "Power BI" },
                new LookupValue { Type = "DataSource", Value = "Epic" },
                new LookupValue { Type = "DataSource", Value = "Tableau" },
                new LookupValue { Type = "DataSource", Value = "Web-Based" },

                new LookupValue { Type = "AssetType", Value = "Dashboard" },
                new LookupValue { Type = "AssetType", Value = "Report" },
                new LookupValue { Type = "AssetType", Value = "Application" },
                new LookupValue { Type = "AssetType", Value = "Data Model" },
                new LookupValue { Type = "AssetType", Value = "Featured" },
            };
            _db.LookupValues.AddRange(lookups);
            _db.SaveChanges();

            var allLookups = _db.LookupValues.ToList();
            var now = DateTime.UtcNow;

            var samples = new[]
            {
                new Item { Title = "Patient Flow Dashboard", Description = "Visualizes patient wait times, throughput metrics, and bottlenecks across multiple departments including emergency, surgical, and urgent care clinics, providing real-time insight for operations managers.", Url = "https://dashboard.healthorg.com/patient-flow", AssetTypesCsv = "Dashboard", Domain = "Access to Care", Division = "Greater Central Valley", ServiceLine = "Hospital", DataSource = "Power BI", PrivacyPhi = true, DateAdded = now.AddDays(-1) },
                new Item { Title = "Leadership KPI Dashboard", Description = "Executive-level dashboard summarizing metrics such as financial performance, patient satisfaction, strategic project progress, and operational throughput, tailored for senior health system leadership.", Url = "https://dashboard.healthorg.com/leadership-kpi", AssetTypesCsv = "Dashboard,Featured", Domain = "Strategy", Division = "Greater Sacramento", ServiceLine = "Oncology", DataSource = "Tableau", PrivacyPhi = false, DateAdded = now.AddDays(-3) },
                new Item { Title = "Chronic Disease Management Model", Description = "Comprehensive data model that captures patient journeys for chronic conditions such as diabetes, CHF, and COPD. Enables advanced analytics for risk stratification and outcomes-based reimbursement.", Url = "https://datamodel.healthorg.com/chronic", AssetTypesCsv = "Data Model", Domain = "Quality", Division = "Greater East Bay", ServiceLine = "Cardiology", DataSource = "Epic", PrivacyPhi = false, DateAdded = now.AddDays(-6) },
                new Item { Title = "ED Wait Times Overview", Description = "Real-time dashboard of emergency department wait times.", Url = "https://dashboard.healthorg.com/ed-wait", AssetTypesCsv = "Dashboard", Domain = "Access to Care", Division = "Greater Sacramento", ServiceLine = "Hospital", DataSource = "Tableau", PrivacyPhi = true, DateAdded = now.AddDays(-7) },
                new Item { Title = "Monthly Provider Scorecards", Description = "Scorecards for evaluating provider performance.", Url = "https://reports.healthorg.com/scorecards", AssetTypesCsv = "Report", Domain = "Quality", Division = "Greater San Francisco", ServiceLine = "Primary Care", DataSource = "Power BI", PrivacyPhi = true, DateAdded = now.AddDays(-8) },
                new Item { Title = "Surgical Volume Tracker", Description = "An interactive dashboard that tracks surgical volume across service lines, comparing scheduled vs. completed procedures, cancellation rates, and time-to-operating-room efficiency for each site.", Url = "https://apps.healthorg.com/surgery-tracker", AssetTypesCsv = "Application", Domain = "Access to Care", Division = "Greater Central Valley", ServiceLine = "Orthopedics", DataSource = "Web-Based", PrivacyPhi = true, DateAdded = now.AddDays(-5) },
                new Item { Title = "Care Gap Closure App", Description = "Tool to track and close care gaps by provider.", Url = "https://apps.healthorg.com/care-gap", AssetTypesCsv = "Application", Domain = "Strategy", Division = "Greater Central Valley", ServiceLine = "Oncology", DataSource = "Web-Based", PrivacyPhi = true, DateAdded = now.AddDays(-9) },
                new Item { Title = "Nursing Staffing Analysis", Description = "Dashboard to analyze RN coverage vs. demand.", Url = "https://dashboard.healthorg.com/nursing", AssetTypesCsv = "Dashboard", Domain = "People & Workforce", Division = "Greater East Bay", ServiceLine = "Hospital", DataSource = "Epic", PrivacyPhi = false, DateAdded = now.AddDays(-10) },
                new Item { Title = "Clinical Quality Metrics Report", Description = "Monthly quality metrics segmented by provider, facility, and region. This report includes HEDIS measures, medication compliance, screening adherence, and adverse events over time.", Url = "https://reports.healthorg.com/quality-metrics", AssetTypesCsv = "Report", Domain = "Quality", Division = "Greater East Bay", ServiceLine = "Primary Care", DataSource = "Epic", PrivacyPhi = false, DateAdded = now.AddDays(-2) },
                new Item { Title = "Ambulatory Utilization Report", Description = "Monthly visit volume by clinic and provider.", Url = "https://reports.healthorg.com/utilization", AssetTypesCsv = "Report", Domain = "Access to Care", Division = "Greater Sacramento", ServiceLine = "Primary Care", DataSource = "Power BI", PrivacyPhi = false, DateAdded = now.AddDays(-11) },
                new Item { Title = "PHI Risk Assessment Dashboard", Description = "Tracks exposure and access of PHI data.", Url = "https://dashboard.healthorg.com/phi-risk", AssetTypesCsv = "Dashboard,Featured", Domain = "Quality", Division = "Greater San Francisco", ServiceLine = "Behavioral Health", DataSource = "Tableau", PrivacyPhi = true, DateAdded = now.AddDays(-12) },
                new Item { Title = "Patient Satisfaction Insights", Description = "Visual report of satisfaction scores by department.", Url = "https://reports.healthorg.com/satisfaction", AssetTypesCsv = "Report", Domain = "Strategy", Division = "Greater Central Valley", ServiceLine = "Hospital", DataSource = "Epic", PrivacyPhi = false, DateAdded = now.AddDays(-13) },
                new Item { Title = "Oncology Pathway App", Description = "Guided decision app for oncology treatment options.", Url = "https://apps.healthorg.com/oncology-pathway", AssetTypesCsv = "Application", Domain = "Quality", Division = "Greater East Bay", ServiceLine = "Oncology", DataSource = "Web-Based", PrivacyPhi = true, DateAdded = now.AddDays(-14) },
                new Item { Title = "Workforce Diversity Report", Description = "Analytics on diversity and inclusion metrics.", Url = "https://reports.healthorg.com/diversity", AssetTypesCsv = "Report", Domain = "People & Workforce", Division = "Greater Sacramento", ServiceLine = "Behavioral Health", DataSource = "Power BI", PrivacyPhi = false, DateAdded = now.AddDays(-15) },
                new Item { Title = "Cardiology Referral Tracker", Description = "Track cardiology referrals and outcomes.", Url = "https://dashboard.healthorg.com/cardio-referral", AssetTypesCsv = "Dashboard", Domain = "Access to Care", Division = "Greater San Francisco", ServiceLine = "Cardiology", DataSource = "Epic", PrivacyPhi = true, DateAdded = now.AddDays(-16) },
                new Item { Title = "Hospital Readmission Analyzer", Description = "Tool to analyze hospital readmission drivers.", Url = "https://apps.healthorg.com/readmissions", AssetTypesCsv = "Application", Domain = "Quality", Division = "Greater Central Valley", ServiceLine = "Hospital", DataSource = "Tableau", PrivacyPhi = true, DateAdded = now.AddDays(-17) },
                new Item { Title = "Employee Retention Analysis", Description = "Workforce turnover and retention metrics broken down by region, role, tenure, and department. Includes predictive modeling for high-risk exits based on historical patterns and survey sentiment.", Url = "https://analytics.healthorg.com/retention", AssetTypesCsv = "Dashboard", Domain = "People & Workforce", Division = "Greater San Francisco", ServiceLine = "Behavioral Health", DataSource = "Power BI", PrivacyPhi = false, DateAdded = now.AddDays(-4) },
                new Item { Title = "Telehealth Adoption Trends", Description = "Dashboard of virtual visit trends by site.", Url = "https://dashboard.healthorg.com/telehealth", AssetTypesCsv = "Dashboard", Domain = "Strategy", Division = "Greater East Bay", ServiceLine = "Primary Care", DataSource = "Power BI", PrivacyPhi = false, DateAdded = now.AddDays(-18) },
                new Item { Title = "Oncology Treatment Metrics", Description = "Monthly outcomes for oncology care plans.", Url = "https://reports.healthorg.com/oncology-metrics", AssetTypesCsv = "Report", Domain = "Quality", Division = "Greater Sacramento", ServiceLine = "Oncology", DataSource = "Epic", PrivacyPhi = true, DateAdded = now.AddDays(-19) },
                new Item { Title = "Health Equity Index", Description = "Index measuring disparities in access and outcomes.", Url = "https://dashboard.healthorg.com/equity-index", AssetTypesCsv = "Dashboard,Featured", Domain = "People & Workforce", Division = "Greater San Francisco", ServiceLine = "Primary Care", DataSource = "Tableau", PrivacyPhi = false, DateAdded = now.AddDays(-20) },
                new Item { Title = "Emergency Ops Tracker", Description = "Real-time app for emergency command center.", Url = "https://apps.healthorg.com/emergency-ops", AssetTypesCsv = "Application", Domain = "Strategy", Division = "Greater Central Valley", ServiceLine = "Hospital", DataSource = "Web-Based", PrivacyPhi = true, DateAdded = now.AddDays(-21) },
                new Item { Title = "Orthopedic Waitlist Dashboard", Description = "Track orthopedic surgery scheduling by site.", Url = "https://dashboard.healthorg.com/ortho-waitlist", AssetTypesCsv = "Dashboard", Domain = "Access to Care", Division = "Greater East Bay", ServiceLine = "Orthopedics", DataSource = "Epic", PrivacyPhi = false, DateAdded = now.AddDays(-22) },
                new Item { Title = "Clinical Trials Participation App", Description = "App tracking clinical trial participation.", Url = "https://apps.healthorg.com/trials", AssetTypesCsv = "Application", Domain = "Quality", Division = "Greater Sacramento", ServiceLine = "Oncology", DataSource = "Web-Based", PrivacyPhi = true, DateAdded = now.AddDays(-23) },
                new Item { Title = "Finance Dashboard", Description = "Budget vs. actuals for regional leadership.", Url = "https://dashboard.healthorg.com/finance", AssetTypesCsv = "Dashboard", Domain = "Strategy", Division = "Greater San Francisco", ServiceLine = "Hospital", DataSource = "Power BI", PrivacyPhi = false, DateAdded = now.AddDays(-24) },
                new Item { Title = "Medication Reconciliation Tracker", Description = "Track medication reconciliation compliance.", Url = "https://apps.healthorg.com/meds", AssetTypesCsv = "Application", Domain = "Quality", Division = "Greater Central Valley", ServiceLine = "Primary Care", DataSource = "Epic", PrivacyPhi = true, DateAdded = now.AddDays(-25) },
            };

            _db.Items.AddRange(samples);
            _db.SaveChanges();
        }
    }
}
