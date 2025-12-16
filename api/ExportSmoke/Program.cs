using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SutterAnalyticsApi.Controllers;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.Models;

// Minimal smoke test that seeds an in-memory DB, invokes ItemsController.Export, and writes CSV

class DummyFactory : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => new HttpClient();
}

static class Seed
{
    public static void Run(AppDbContext db)
    {
        var domain = new LookupValue { Type = "Domain", Value = "Finance" };
        var division = new LookupValue { Type = "Division", Value = "North" };
        var serviceLine = new LookupValue { Type = "ServiceLine", Value = "SL-A" };
        var dataSource = new LookupValue { Type = "DataSource", Value = "EDW" };
        var assetType = new LookupValue { Type = "AssetType", Value = "Dashboard" };
        var status = new LookupValue { Type = "Status", Value = "Published" };
        var operatingEntity = new LookupValue { Type = "OperatingEntity", Value = "Entity-1" };
        var refresh = new LookupValue { Type = "RefreshFrequency", Value = "Daily" };
        var dc1 = new LookupValue { Type = "DataConsumer", Value = "Analysts" };

        var optCon = new LookupValue { Type = "PotentialToConsolidate", Value = "High" };
        var optAuto = new LookupValue { Type = "PotentialToAutomate", Value = "Medium" };
        var sponsorVal = new LookupValue { Type = "SponsorBusinessValue", Value = "High" };
        var mustDo = new LookupValue { Type = "MustDo2025", Value = "Yes" };
        var devEffort = new LookupValue { Type = "DevelopmentEffort", Value = "Small" };
        var estHours = new LookupValue { Type = "EstimatedDevHours", Value = "80" };
        var resDev = new LookupValue { Type = "ResourcesDevelopment", Value = "2" };
        var resAnalysts = new LookupValue { Type = "ResourcesAnalysts", Value = "1" };
        var resPlatform = new LookupValue { Type = "ResourcesPlatform", Value = "1" };
        var resDE = new LookupValue { Type = "ResourcesDataEngineering", Value = "0" };

        db.LookupValues.AddRange(domain, division, serviceLine, dataSource, assetType, status, operatingEntity, refresh,
            dc1, optCon, optAuto, sponsorVal, mustDo, devEffort, estHours, resDev, resAnalysts, resPlatform, resDE);

        var owner = new Owner { Name = "Owner One", Email = "owner@example.com" };
        var sponsor = new Owner { Name = "Exec Sponsor", Email = "exec@example.com" };
        db.Owners.AddRange(owner, sponsor);

        var tagA = new Tag { Value = "Finance" };
        var tagB = new Tag { Value = "QBR" };
        db.Tags.AddRange(tagA, tagB);

        var item = new Item
        {
            Title = "Revenue Dashboard",
            Description = "KPIs for revenue trends",
            Url = "http://example/revenue",
            AssetType = assetType,
            AssetTypeId = null, // navigation populated
            DomainLookup = domain,
            DivisionLookup = division,
            ServiceLineLookup = serviceLine,
            DataSourceLookup = dataSource,
            StatusLookup = status,
            Owner = owner,
            ExecutiveSponsor = sponsor,
            OperatingEntityLookup = operatingEntity,
            RefreshFrequencyLookup = refresh,
            PrivacyPhi = true,
            PrivacyPii = false,
            HasRls = true,
            DateAdded = DateTime.UtcNow.Date.AddDays(-3),
            LastModifiedDate = DateTime.UtcNow.Date,
            Featured = true,
            ProductGroup = "Finance",
            ProductStatusNotes = "Stable",
            DataConsumersText = "Finance Managers",
            TechDeliveryManager = "Tech Lead",
            RegulatoryComplianceContractual = "SOX",
            BiPlatform = "Power BI",
            DbServer = "sql01",
            DbDataMart = "EDW_Finance",
            DatabaseTable = "FactRevenue",
            SourceRep = "GitHub",
            DataSecurityClassification = "Internal",
            AccessGroupName = "BI-Users",
            AccessGroupDn = "CN=BI-Users,OU=Groups,DC=example,DC=com",
            AutomationClassification = "Manual",
            UserVisibilityString = "Public",
            UserVisibilityNumber = "1",
            EpicSecurityGroupTag = "EPIC-1",
            KeepLongTerm = "Yes",
            PotentialToConsolidate = optCon,
            PotentialToAutomate = optAuto,
            SponsorBusinessValue = sponsorVal,
            MustDo2025 = mustDo,
            DevelopmentEffortLookup = devEffort,
            EstimatedDevHoursLookup = estHours,
            ResourcesDevelopmentLookup = resDev,
            ResourcesAnalystsLookup = resAnalysts,
            ResourcesPlatformLookup = resPlatform,
            ResourcesDataEngineeringLookup = resDE
        };

        db.Items.Add(item);
        db.SaveChanges();

        // Tags
        db.ItemTags.Add(new ItemTag { ItemId = item.Id, Tag = tagA });
        db.ItemTags.Add(new ItemTag { ItemId = item.Id, Tag = tagB });

        // Data Consumers (lookup relation)
        db.ItemDataConsumers.Add(new ItemDataConsumer { ItemId = item.Id, DataConsumerId = dc1.Id });
        db.SaveChanges();
    }
}

class Program
{
    static async Task<int> Main()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("export-test")
            .Options;

        using var db = new AppDbContext(options);
        Seed.Run(db);

        var cfg = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var controller = new ItemsController(db, cfg, new DummyFactory());

        var httpContext = new DefaultHttpContext();
        httpContext.Items["AppUser"] = new User { Id = 1, UserPrincipalName = "tester", UserType = "Admin" };
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = await controller.Export();
        if (result is FileContentResult fcr)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "export_test.csv");
            await File.WriteAllBytesAsync(path, fcr.FileContents);
            Console.WriteLine($"Wrote: {path}");
            var preview = Encoding.UTF8.GetString(fcr.FileContents).Split('\n').Take(5);
            Console.WriteLine("--- CSV Preview (first lines) ---");
            foreach (var line in preview) Console.WriteLine(line);
            return 0;
        }
        Console.Error.WriteLine("Export did not return FileContentResult");
        return 1;
    }
}

