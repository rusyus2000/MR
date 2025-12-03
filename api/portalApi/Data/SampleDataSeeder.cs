// Data/SampleDataSeeder.cs
using System;
using System.Collections.Generic;
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

        public void Seed(int count = 100)
        {
            if (_db.Items.Any()) return;

            EnsureLookups();
            EnsureOwners();

            var all = _db.LookupValues.ToList();
            int L(string type, string value) => all.First(l => l.Type == type && l.Value == value).Id;

            var domains = all.Where(x => x.Type == "Domain").Select(x => x.Value).ToArray();
            var divisions = all.Where(x => x.Type == "Division").Select(x => x.Value).ToArray();
            var serviceLines = all.Where(x => x.Type == "ServiceLine").Select(x => x.Value).ToArray();
            var dataSources = all.Where(x => x.Type == "DataSource").Select(x => x.Value).ToArray();
            var assetTypes = all.Where(x => x.Type == "AssetType").Select(x => x.Value).ToArray();
            var publishedId = L("Status", "Published");

            var rnd = new Random(42);

            string[] topics = new[]
            {
                "ED Throughput", "Readmission Rates", "Surgical Outcomes", "Patient Satisfaction",
                "Sepsis Alerts", "Telehealth Utilization", "No-Show Rates", "Staffing Levels",
                "OR Utilization", "Length of Stay", "Discharge Planning", "Care Transitions",
                "Revenue Cycle", "Denials Management", "Payer Mix", "Case Mix Index",
                "Referral Leakage", "Access Scheduling", "HEDIS Measures", "Chronic Care",
                "Cancer Registry", "Cardiology Outcomes", "Behavioral Health Access", "Ambulatory Quality",
                "Population Health", "COVID-19 Trends", "Flu Surveillance", "Immunization Coverage",
                "Medication Safety", "Antibiotic Stewardship", "Falls Prevention", "Pressure Ulcers",
                "Net Promoter Score", "Provider Productivity", "Clinic Utilization", "Financial Forecast",
                "Supply Chain", "Lab Turnaround", "Imaging Wait Times", "MRI Utilization",
                "Bed Management", "Transfer Center", "Call Center KPIs", "Auth Turnaround",
                "Claim Lag", "Bad Debt", "Charity Care", "Out-of-Network",
                "Care Gaps", "Risk Adjustment", "ACO Quality", "Bundled Payments",
                "Readiness Dashboard", "Infection Control", "Hand Hygiene", "Vaccination Compliance",
                "Care Coordination", "Social Determinants", "Equity Measures", "Language Services",
                "Workforce Diversity", "Recruitment Pipeline", "Onboarding", "Training Compliance",
            };

            string[] adjectives = new[] { "Dashboard", "Report", "Explorer", "Monitor", "Insights", "Tracker" };
            string[] tagPool = new[] { "Epic", "Power BI", "Tableau", "Finance", "Quality", "Operations", "Clinical", "Access", "ED", "Inpatient", "Ambulatory", "HCC", "ACO", "Sepsis", "Telehealth", "LOS", "Readmissions" };

            var items = new List<Item>();
            var now = DateTime.UtcNow;

            for (int i = 0; i < count; i++)
            {
                var topic = topics[i % topics.Length];
                var adj = adjectives[rnd.Next(adjectives.Length)];
                var at = assetTypes[rnd.Next(assetTypes.Length)];
                var title = $"{topic} {adj}";
                var desc = BuildDescription(topic);
                var url = $"https://analytics.sutterhealth.org/{title.ToLower().Replace(' ', '-')}-{i + 1}";

                var item = new Item
                {
                    Title = title,
                    Description = desc,
                    Url = url,
                    AssetTypeId = L("AssetType", at),
                    DomainId = L("Domain", domains[rnd.Next(domains.Length)]),
                    DivisionId = L("Division", divisions[rnd.Next(divisions.Length)]),
                    ServiceLineId = L("ServiceLine", serviceLines[rnd.Next(serviceLines.Length)]),
                    DataSourceId = L("DataSource", dataSources[rnd.Next(dataSources.Length)]),
                    StatusId = publishedId,
                    PrivacyPhi = rnd.NextDouble() < 0.3,
                    Featured = rnd.NextDouble() < 0.15,
                    DateAdded = now.AddDays(-rnd.Next(0, 120))
                };

                // Tags (0-3 random tags)
                var tagCount = rnd.Next(0, 4);
                var selected = tagPool.OrderBy(_ => rnd.Next()).Take(tagCount).ToArray();
                // Add contextual tags from selected domain/service line
                selected = selected
                    .Concat(new[] { item.ServiceLineId.HasValue ? serviceLines[(Array.IndexOf(serviceLines, serviceLines.FirstOrDefault(sl => L("ServiceLine", sl) == item.ServiceLineId)) + serviceLines.Length) % serviceLines.Length] : null })
                    .Concat(new[] { item.DomainId.HasValue ? domains[(Array.IndexOf(domains, domains.FirstOrDefault(d => L("Domain", d) == item.DomainId)) + domains.Length) % domains.Length] : null })
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .Take(4)
                    .ToArray();
                foreach (var t in selected)
                {
                    var tag = _db.Tags.FirstOrDefault(x => x.Value == t);
                    if (tag == null)
                    {
                        tag = new Tag { Value = t };
                        _db.Tags.Add(tag);
                        _db.SaveChanges();
                    }
                    item.ItemTags.Add(new ItemTag { Item = item, Tag = tag });
                }

                // Assign an owner (reuse among 5 seeded owners)
                var owner = _db.Owners.OrderBy(_ => Guid.NewGuid()).First();
                item.OwnerId = owner.Id;

                items.Add(item);
            }

            _db.Items.AddRange(items);
            _db.SaveChanges();
        }

        private void EnsureLookups()
        {
            void Ensure(string type, IEnumerable<string> values)
            {
                foreach (var v in values)
                {
                    if (!_db.LookupValues.Any(l => l.Type == type && l.Value == v))
                    {
                        _db.LookupValues.Add(new LookupValue { Type = type, Value = v });
                    }
                }
                _db.SaveChanges();
            }

            Ensure("Domain", new[] { "Access to Care", "Quality", "Strategy", "People & Workforce", "Missing Data" });
            Ensure("Division", new[] { "Greater Central Valley", "Greater East Bay", "Greater Sacramento", "Greater San Francisco" });
            Ensure("ServiceLine", new[] { "Behavioral Health", "Cardiology", "Hospital", "Oncology", "Primary Care", "Orthopedics", "Missing Data" });
            Ensure("DataSource", new[] { "Power BI", "Tableau", "Web-Based","SSRS","Epic Reports","SSIS","Epic Dashboards" });
            Ensure("AssetType", new[] { "Dashboard", "Report", "Application", "Data Model" });
            Ensure("Status", new[] { "Published", "Offline" });
            Ensure("OperatingEntity", new[] { "Unknown" });
            Ensure("RefreshFrequency", new[] { "Daily", "Weekly", "Biweekly", "2x/Month", "Monthly", "Quarterly", "Annually" });
            Ensure("DataConsumer", new[] { "Analysts", "Leaders", "Clinicians" });
        }

        private void EnsureOwners()
        {
            var owners = new (string Name, string Email)[]
            {
                ("Clinical Operations", "clinical.ops@sutterhealth.org"),
                ("Revenue Cycle", "rev.cycle@sutterhealth.org"),
                ("Population Health", "pop.health@sutterhealth.org"),
                ("Ambulatory Quality", "ambulatory.quality@sutterhealth.org"),
                ("Inpatient Services", "inpatient.services@sutterhealth.org"),
            };
            foreach (var (name, email) in owners)
            {
                if (!_db.Owners.Any(o => o.Email == email))
                {
                    _db.Owners.Add(new Owner { Name = name, Email = email });
                }
            }
            _db.SaveChanges();
        }

        private static string BuildDescription(string topic)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ED Throughput"] = "Tracks emergency department arrivals, door-to-doc, LWBS, and boarding times to identify bottlenecks.",
                ["Readmission Rates"] = "Monitors 7/30-day readmissions by service line and payer to drive targeted interventions.",
                ["Surgical Outcomes"] = "Summarizes surgical volumes, complications, and mortality trends for perioperative committees.",
                ["Patient Satisfaction"] = "Aggregates CAHPS and NPS across settings to highlight experience improvement opportunities.",
                ["Sepsis Alerts"] = "Real-time sepsis flags, bundle compliance, and mortality to support rapid response.",
                ["Telehealth Utilization"] = "Virtual visit volume, adoption, and access metrics across regions and specialties.",
                ["No-Show Rates"] = "Identifies missed appointments by clinic/provider with actionable rebooking insights.",
                ["Staffing Levels"] = "Compares staffing to demand, overtime, and agency usage to optimize labor costs.",
                ["OR Utilization"] = "Block utilization, turnover, first-case on-time starts, and case duration outliers.",
                ["Length of Stay"] = "Observed vs expected LOS trends with drivers by diagnosis and service line.",
                ["Discharge Planning"] = "Discharge barriers, placement delays, and weekend discharge readiness metrics.",
                ["Care Transitions"] = "Post-acute referrals, acceptance, and 7-day follow-up completion rates.",
                ["Revenue Cycle"] = "Gross/net revenue trends, AR aging, DNFB, and clean claim rates by payer.",
                ["Denials Management"] = "Denial reasons, avoidable categories, overturn rates, and financial impact.",
                ["Payer Mix"] = "Payer distribution and shifts over time by geography and service line.",
                ["Case Mix Index"] = "CMI trends and impact on reimbursement across entities and specialties.",
                ["Referral Leakage"] = "Out-of-network referral patterns and opportunities to retain care in system.",
                ["Access Scheduling"] = "New patient access, third-next-available, and template optimization insights.",
                ["HEDIS Measures"] = "Measure gaps, compliance, and trends for quality improvement programs.",
                ["Chronic Care"] = "Disease registry metrics (DM, CHF, COPD) and care gap closure progress.",
                ["Population Health"] = "Risk stratification, PMPM costs, utilization, and quality scorecards.",
                ["Flu Surveillance"] = "Flu positivity, vaccination uptake, and regional seasonal trends.",
                ["Immunization Coverage"] = "Child and adult immunization rates with disparities and outreach targets.",
                ["Medication Safety"] = "High-alert medication compliance and adverse drug event reporting.",
                ["Antibiotic Stewardship"] = "Antibiotic days of therapy, resistance patterns, and guideline adherence.",
                ["Falls Prevention"] = "Fall rates, injury severity, and unit compliance with prevention bundles.",
                ["Provider Productivity"] = "wRVUs, visits per clinic session, and panel size management.",
                ["Clinic Utilization"] = "Template fill rates, no-show impact, and appointment type mix.",
                ["Financial Forecast"] = "Run-rate projections, variance analysis, and margin sensitivity scenarios.",
                ["Lab Turnaround"] = "Lab order-to-result turnaround times and outliers by test type.",
                ["Imaging Wait Times"] = "Modality scheduling lead times and access improvement recommendations.",
                ["Bed Management"] = "Census, admissions/discharges, and predicted bed demand for capacity.",
                ["Call Center KPIs"] = "Service level, abandon rates, and call resolution by queue.",
                ["Care Gaps"] = "Open care gaps by population segment with prioritized outreach lists.",
                ["Risk Adjustment"] = "HCC capture, suspect conditions, and RAF optimization opportunities.",
                ["ACO Quality"] = "ACO measure performance with benchmarks and improvement trajectories.",
                ["Bundled Payments"] = "Episode costs, readmits, and PAC utilization for bundle performance.",
                ["Infection Control"] = "HAI rates (CLABSI, CAUTI, SSI) and hand hygiene compliance.",
                ["Hand Hygiene"] = "Observation compliance and unit trends in hand hygiene.",
                ["Vaccination Compliance"] = "Employee and patient vaccination rates vs targets.",
                ["Care Coordination"] = "Cross-setting communication and closed-loop referral metrics.",
                ["Social Determinants"] = "SDOH screening, resource referrals, and outcomes.",
                ["Equity Measures"] = "Performance by race/ethnicity/language to identify disparities.",
                ["Language Services"] = "Interpreter utilization and language access compliance.",
                ["Workforce Diversity"] = "Diversity, hiring, and retention metrics across roles.",
                ["Training Compliance"] = "Mandatory training completion rates and overdue tracking.",
            };

            return map.TryGetValue(topic, out var text)
                ? text
                : $"Key performance indicators for {topic.ToLower()} across settings.";
        }
    }
}
