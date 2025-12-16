using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.DTOs;
using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : MpBaseController
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;
        public ItemsController(AppDbContext db, IConfiguration config, IHttpClientFactory httpFactory)
        {
            _db = db;
            _config = config;
            _httpFactory = httpFactory;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var user = CurrentUser;

        //    var favoriteIds = await _db.UserFavorites
        //        .Where(f => f.UserId == user.Id)
        //        .Select(f => f.ItemId)
        //        .ToListAsync();

        //    var items = await _db.Items
        //        .Select(item => new
        //        {
        //            item.Id,
        //            item.Title,
        //            item.Description,
        //            item.DateAdded,
        //            IsFavorite = favoriteIds.Contains(item.Id)
        //        })
        //        .ToListAsync();

        //    return Ok(items);
        //}


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemListDto>>> GetAll(
     [FromQuery] int? top,
     [FromQuery] string? domain,
     [FromQuery] string? division,
     // ServiceLine removed
     [FromQuery] string? dataSource,
     [FromQuery] string? assetType,
     [FromQuery] int? assetTypeId,
     // ID-based filters (comma-separated ids)
     [FromQuery] string? domainIds,
     [FromQuery] string? divisionIds,
     // ServiceLine removed
     [FromQuery] string? dataSourceIds,
     [FromQuery] string? assetTypeIds,
     [FromQuery] bool? phi)
        {
            var user = CurrentUser;
            var query = _db.Items.AsQueryable();

            // Restrict visibility: non-admins see only Published items
            var isAdmin = user?.UserType == "Admin";
            if (!isAdmin)
            {
                var published = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Status" && l.Value == "Published");
                if (published != null)
                {
                    query = query.Where(i => !i.StatusId.HasValue || i.StatusId == published.Id);
                }
            }

            // Helper to parse comma-separated id lists into integers
            List<int> ParseIds(string? ids)
            {
                if (string.IsNullOrWhiteSpace(ids)) return new List<int>();
                return ids.Split(',', StringSplitOptions.RemoveEmptyEntries)
                          .Select(s => { bool ok = int.TryParse(s.Trim(), out var v); return (ok, v); })
                          .Where(t => t.ok)
                          .Select(t => t.v)
                          .ToList();
            }

            // Apply ID-based filters first (preferred). These expect lookup FK ids.
            var domainIdList = ParseIds(domainIds);
            if (domainIdList.Any())
                query = query.Where(i => i.DomainId.HasValue && domainIdList.Contains(i.DomainId.Value));

            var divisionIdList = ParseIds(divisionIds);
            if (divisionIdList.Any())
                query = query.Where(i => i.DivisionId.HasValue && divisionIdList.Contains(i.DivisionId.Value));

            // ServiceLine removed

            var dataSourceIdList = ParseIds(dataSourceIds);
            if (dataSourceIdList.Any())
                query = query.Where(i => i.DataSourceId.HasValue && dataSourceIdList.Contains(i.DataSourceId.Value));

            var assetTypeIdList = ParseIds(assetTypeIds);
            if (assetTypeIdList.Any())
                query = query.Where(i => i.AssetTypeId.HasValue && assetTypeIdList.Contains(i.AssetTypeId.Value));


            // Filtering only (no search query)
            if (!string.IsNullOrWhiteSpace(domain))
            {
                var lv = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Domain" && l.Value == domain);
                if (lv != null) query = query.Where(i => i.DomainId == lv.Id);
                else return Ok(new List<ItemDto>()); // no matching lookup -> empty result
            }
            if (!string.IsNullOrWhiteSpace(division))
            {
                var lv = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Division" && l.Value == division);
                if (lv != null) query = query.Where(i => i.DivisionId == lv.Id);
                else return Ok(new List<ItemDto>());
            }
            // ServiceLine filtering removed
            if (!string.IsNullOrWhiteSpace(dataSource))
            {
                var lv = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "DataSource" && l.Value == dataSource);
                if (lv != null) query = query.Where(i => i.DataSourceId == lv.Id);
                else return Ok(new List<ItemDto>());
            }
            // AssetType filtering: accept numeric id or comma-separated names.
            if (assetTypeId.HasValue)
            {
                query = query.Where(i => i.AssetTypeId.HasValue && i.AssetTypeId == assetTypeId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(assetType))
            {
                var names = assetType.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                if (names.Count == 0) { /* no-op */ }
                else
                {
                    var lvs = await _db.LookupValues.Where(l => l.Type == "AssetType" && names.Contains(l.Value)).Select(l => l.Id).ToListAsync();
                    if (!lvs.Any()) return Ok(new List<ItemDto>());
                    query = query.Where(i => i.AssetTypeId.HasValue && lvs.Contains(i.AssetTypeId.Value));
                }
            }
            if (phi.HasValue)
                query = query.Where(i => i.PrivacyPhi == phi.Value);

            if (top.HasValue)
                query = query.OrderByDescending(i => i.DateAdded).Take(top.Value);

            // Get user's favorite item IDs
            var favoriteIds = await _db.UserFavorites
                .Where(f => f.UserId == user.Id)
                .Select(f => f.ItemId)
                .ToListAsync();

            var list = await query.Select(i => new ItemListDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Url = i.Url,
                AssetTypeId = i.AssetTypeId,
                AssetTypeName = i.AssetType != null ? i.AssetType.Value : null,
                Featured = i.Featured,
                DomainId = i.DomainId,
                DivisionId = i.DivisionId,
                // ServiceLine removed
                DataSourceId = i.DataSourceId,
                PrivacyPhi = i.PrivacyPhi,
                PrivacyPii = i.PrivacyPii,
                HasRls = i.HasRls,
                OperatingEntityId = i.OperatingEntityId,
                OperatingEntity = i.OperatingEntityLookup != null ? i.OperatingEntityLookup.Value : null,
                RefreshFrequencyId = i.RefreshFrequencyId,
                RefreshFrequency = i.RefreshFrequencyLookup != null ? i.RefreshFrequencyLookup.Value : null,
                LastModifiedDate = i.LastModifiedDate,
                DateAdded = i.DateAdded,
                IsFavorite = favoriteIds.Contains(i.Id)
            }).ToListAsync();

            return Ok(list);
        }


        // GET /api/items/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ItemDto>> GetById(int id)
        {
            var i = await _db.Items
                .Include(it => it.ItemTags)
                    .ThenInclude(it => it.Tag)
                .Include(it => it.AssetType)
                .Include(it => it.DomainLookup)
                .Include(it => it.DivisionLookup)
                // ServiceLine removed
                .Include(it => it.DataSourceLookup)
                .Include(it => it.Owner)
                .Include(it => it.ExecutiveSponsor)
                .Include(it => it.StatusLookup)
                .Include(it => it.OperatingEntityLookup)
                .Include(it => it.RefreshFrequencyLookup)
                .Include(it => it.ProductImpactCategory)
                // Include optional lookup navigations so display values are populated
                .Include(it => it.PotentialToConsolidate)
                .Include(it => it.PotentialToAutomate)
                .Include(it => it.SponsorBusinessValue)
                // MustDo2025 removed
                .Include(it => it.DevelopmentEffortLookup)
                .Include(it => it.EstimatedDevHoursLookup)
                .Include(it => it.ResourcesDevelopmentLookup)
                .Include(it => it.ResourcesAnalystsLookup)
                .Include(it => it.ResourcesPlatformLookup)
                .Include(it => it.ResourcesDataEngineeringLookup)
                .FirstOrDefaultAsync(it => it.Id == id);
            if (i == null) return NotFound();
            // Non-admins cannot view non-Published items
            var user = CurrentUser;
            var isAdmin = user?.UserType == "Admin";
            if (!isAdmin)
            {
                var published = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Status" && l.Value == "Published");
                if (published != null && i.StatusId.HasValue && i.StatusId.Value != published.Id)
                {
                    return NotFound();
                }
            }
            // record that the current user opened this asset
            try
            {
                var u = CurrentUser;
                if (u != null)
                {
                    _db.UserAssetOpenHistories.Add(new UserAssetOpenHistory
                    {
                        UserId = u.Id,
                        ItemId = i.Id,
                        OpenedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }
            }
            catch
            {
                // Ignore logging errors
            }

            string DisplayBool(bool? v) => v.HasValue ? (v.Value ? "Yes" : "No") : "Missing Data";

            return Ok(new ItemDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Url = i.Url,
                Tags = i.ItemTags.Select(it => it.Tag.Value).ToList(),
                AssetTypeId = i.AssetTypeId,
                AssetTypeName = i.AssetType != null ? i.AssetType.Value : null,
                DomainId = i.DomainId,
                DivisionId = i.DivisionId,
                // ServiceLine removed
                DataSourceId = i.DataSourceId,
                Domain = i.DomainLookup != null ? i.DomainLookup.Value : null,
                Division = i.DivisionLookup != null ? i.DivisionLookup.Value : null,
                // ServiceLine removed
                DataSource = i.DataSourceLookup != null ? i.DataSourceLookup.Value : null,
                StatusId = i.StatusId,
                Status = i.StatusLookup != null ? i.StatusLookup.Value : null,
                OwnerId = i.OwnerId,
                OwnerName = i.Owner != null ? i.Owner.Name : null,
                OwnerEmail = i.Owner != null ? i.Owner.Email : null,
                ExecutiveSponsorId = i.ExecutiveSponsorId,
                ExecutiveSponsorName = i.ExecutiveSponsor != null ? i.ExecutiveSponsor.Name : null,
                ExecutiveSponsorEmail = i.ExecutiveSponsor != null ? i.ExecutiveSponsor.Email : null,
                OperatingEntityId = i.OperatingEntityId,
                OperatingEntity = i.OperatingEntityLookup != null ? i.OperatingEntityLookup.Value : null,
                RefreshFrequencyId = i.RefreshFrequencyId,
                RefreshFrequency = i.RefreshFrequencyLookup != null ? i.RefreshFrequencyLookup.Value : null,
                DataConsumers = i.DataConsumers,
                PrivacyPhi = i.PrivacyPhi,
                PrivacyPii = i.PrivacyPii,
                HasRls = i.HasRls,
                // New optional lookup fields (ids + display)
                PotentialToConsolidateId = i.PotentialToConsolidateId,
                PotentialToConsolidate = i.PotentialToConsolidate != null ? i.PotentialToConsolidate.Value : null,
                PotentialToAutomateId = i.PotentialToAutomateId,
                PotentialToAutomate = i.PotentialToAutomate != null ? i.PotentialToAutomate.Value : null,
                SponsorBusinessValueId = i.SponsorBusinessValueId,
                SponsorBusinessValue = i.SponsorBusinessValue != null ? i.SponsorBusinessValue.Value : null,
                // MustDo2025 removed
                DevelopmentEffortId = i.DevelopmentEffortId,
                DevelopmentEffort = i.DevelopmentEffortLookup != null ? i.DevelopmentEffortLookup.Value : null,
                EstimatedDevHoursId = i.EstimatedDevHoursId,
                EstimatedDevHours = i.EstimatedDevHoursLookup != null ? i.EstimatedDevHoursLookup.Value : null,
                ResourcesDevelopmentId = i.ResourcesDevelopmentId,
                ResourcesDevelopment = i.ResourcesDevelopmentLookup != null ? i.ResourcesDevelopmentLookup.Value : null,
                ResourcesAnalystsId = i.ResourcesAnalystsId,
                ResourcesAnalysts = i.ResourcesAnalystsLookup != null ? i.ResourcesAnalystsLookup.Value : null,
                ResourcesPlatformId = i.ResourcesPlatformId,
                ResourcesPlatform = i.ResourcesPlatformLookup != null ? i.ResourcesPlatformLookup.Value : null,
                ResourcesDataEngineeringId = i.ResourcesDataEngineeringId,
                ResourcesDataEngineering = i.ResourcesDataEngineeringLookup != null ? i.ResourcesDataEngineeringLookup.Value : null,
                ProductImpactCategoryId = i.ProductImpactCategoryId,
                ProductImpactCategory = i.ProductImpactCategory != null ? i.ProductImpactCategory.Value : null,
                // Additional free-text fields
                ProductGroup = i.ProductGroup,
                ProductStatusNotes = i.ProductStatusNotes,
                // DataConsumers already mapped
                TechDeliveryManager = i.TechDeliveryManager,
                RegulatoryComplianceContractual = i.RegulatoryComplianceContractual,
                BiPlatform = i.BiPlatform,
                DbServer = i.DbServer,
                DbDataMart = i.DbDataMart,
                DatabaseTable = i.DatabaseTable,
                SourceRep = i.SourceRep,
                DataSecurityClassification = i.DataSecurityClassification,
                AccessGroupName = i.AccessGroupName,
                AccessGroupDn = i.AccessGroupDn,
                AutomationClassification = i.AutomationClassification,
                UserVisibilityString = i.UserVisibilityString,
                UserVisibilityNumber = i.UserVisibilityNumber,
                EpicSecurityGroupTag = i.EpicSecurityGroupTag,
                KeepLongTerm = i.KeepLongTerm,
                PrivacyPhiDisplay = DisplayBool(i.PrivacyPhi),
                PrivacyPiiDisplay = DisplayBool(i.PrivacyPii),
                HasRlsDisplay = DisplayBool(i.HasRls),
                Dependencies = i.Dependencies,
                DefaultAdGroupNames = i.DefaultAdGroupNames,
                LastModifiedDate = i.LastModifiedDate,
                DateAdded = i.DateAdded,
                Featured = i.Featured,
                FeaturedDisplay = DisplayBool(i.Featured)
            });
        }

        public class ExportRequest { public List<int> Ids { get; set; } = new(); }

        // Admin-only export of items as CSV (values, not IDs)
        [HttpPost("export")]
        public async Task<IActionResult> Export()
        {
            var user = CurrentUser;
            if (user?.UserType != "Admin") return Forbid();
            List<int>? idsToExport = null;

            // Try FormData ids first (avoids preflight)
            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                var ids = new List<int>();
                foreach (var v in form["ids"]) { if (int.TryParse(v, out var id)) ids.Add(id); }
                if (ids.Count > 0) idsToExport = ids;
            }
            else if (Request.ContentLength.GetValueOrDefault() > 0 &&
                     (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                try
                {
                    using var sr = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
                    var json = await sr.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var dto = JsonSerializer.Deserialize<ExportRequest>(json);
                        if (dto?.Ids != null && dto.Ids.Count > 0) idsToExport = dto.Ids;
                    }
                }
                catch { }
            }
            // If no IDs provided in any format, export ALL items

            var query = _db.Items.AsNoTracking();
            if (idsToExport != null)
            {
                query = query.Where(i => idsToExport.Contains(i.Id));
            }

            var rows = await query
                .Select(i => new
                {
                    // Required first
                    i.Id,
                    i.Title,
                    i.Description,
                    i.Url,

                    // Existing optional metadata
                    AssetType = i.AssetType != null ? i.AssetType.Value : null,
                    Domain = i.DomainLookup != null ? i.DomainLookup.Value : null,
                    Division = i.DivisionLookup != null ? i.DivisionLookup.Value : null,
                    // ServiceLine removed
                    DataSource = i.DataSourceLookup != null ? i.DataSourceLookup.Value : null,
                    Status = i.StatusLookup != null ? i.StatusLookup.Value : null,
                    OwnerName = i.Owner != null ? i.Owner.Name : null,
                    OwnerEmail = i.Owner != null ? i.Owner.Email : null,
                    ExecutiveSponsorName = i.ExecutiveSponsor != null ? i.ExecutiveSponsor.Name : null,
                    ExecutiveSponsorEmail = i.ExecutiveSponsor != null ? i.ExecutiveSponsor.Email : null,
                    OperatingEntity = i.OperatingEntityLookup != null ? i.OperatingEntityLookup.Value : null,
                    RefreshFrequency = i.RefreshFrequencyLookup != null ? i.RefreshFrequencyLookup.Value : null,
                    i.PrivacyPhi,
                    i.PrivacyPii,
                    i.HasRls,
                    i.LastModifiedDate,
                    i.DateAdded,
                    i.Featured,
                    Tags = i.ItemTags.Select(t => t.Tag.Value),
                    // DataConsumers removed from export projection (use text field later)
                    i.Dependencies,
                    i.DefaultAdGroupNames,

                    // New optional free-text fields
                    i.ProductGroup,
                    i.ProductStatusNotes,
                    DataConsumers = i.DataConsumers,
                    i.TechDeliveryManager,
                    i.RegulatoryComplianceContractual,
                    i.BiPlatform,
                    i.DbServer,
                    i.DbDataMart,
                    i.DatabaseTable,
                    i.SourceRep,
                    i.DataSecurityClassification,
                    i.AccessGroupName,
                    i.AccessGroupDn,
                    i.AutomationClassification,
                    i.UserVisibilityString,
                    i.UserVisibilityNumber,
                    i.EpicSecurityGroupTag,
                    i.KeepLongTerm,

                    // New optional lookup fields (export display values)
                    PotentialToConsolidate = i.PotentialToConsolidate != null ? i.PotentialToConsolidate.Value : null,
                    PotentialToAutomate = i.PotentialToAutomate != null ? i.PotentialToAutomate.Value : null,
                    SponsorBusinessValue = i.SponsorBusinessValue != null ? i.SponsorBusinessValue.Value : null,
                    // MustDo2025 removed
                    DevelopmentEffort = i.DevelopmentEffortLookup != null ? i.DevelopmentEffortLookup.Value : null,
                    EstimatedDevHours = i.EstimatedDevHoursLookup != null ? i.EstimatedDevHoursLookup.Value : null,
                    ResourcesDevelopment = i.ResourcesDevelopmentLookup != null ? i.ResourcesDevelopmentLookup.Value : null,
                    ResourcesAnalysts = i.ResourcesAnalystsLookup != null ? i.ResourcesAnalystsLookup.Value : null,
                    ResourcesPlatform = i.ResourcesPlatformLookup != null ? i.ResourcesPlatformLookup.Value : null,
                    ResourcesDataEngineering = i.ResourcesDataEngineeringLookup != null ? i.ResourcesDataEngineeringLookup.Value : null,
                    ProductImpactCategory = i.ProductImpactCategory != null ? i.ProductImpactCategory.Value : null
                })
                .ToListAsync();

            string CsvEscape(string s)
            {
                if (s == null) return string.Empty;
                var needsQuote = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
                var t = s.Replace("\"", "\"\"");
                return needsQuote ? $"\"{t}\"" : t;
            }
            string BoolOut(bool? b)
            {
                if (!b.HasValue) return "Missing Data";
                return b.Value ? "true" : "false";
            }

            var sb = new StringBuilder();
            // Header: custom order provided by requestor
            sb.AppendLine(string.Join(",", new[] {
                "Domain",
                "Product Group",
                "Title",
                "Description",
                "Status",
                "Product Status Notes",
                "Division",
                "Operating Entity",
                "Executive Sponsor Name",
                "Data Consumers",
                "Owner Name",
                "Owner Email",
                "Tech Delivery Mgr",
                "Regulatory/Compliance/Contractual",
                "Asset Type",
                "Featured",
                "BI Platform",
                "Url",
                "DB Server",
                "DB/Data Mart",
                "Database Table",
                "Source Rep",
                "dataSecurityClassification",
                "accessGroupName",
                "accessGroupDN",
                "Data Source",
                "Automation Classification",
                "user_visibility_string",
                "user_visibility_number",
                "Default AD Group Names",
                "PHI",
                "PII",
                "Has RLS",
                "Epic Security Group tag",
                "Refresh Frequency",
                "Keep Long Term",
                "Potential to Consolidate",
                "Potential to Automate",
                "Last Modified Date",
                "Business Value by executive sponsor",
                // 2025 Must Do removed
                "Development Effort",
                "Estimated development hours",
                "Resources - Development",
                "Resources - Analysts",
                "Resources - Platform",
                "Resources - Data Engineering",
                "Product Impact Category",
                // Extra fields appended at the end (unchanged labels)
                "Id",
                // Service Line removed
                "Tags",
                "Dependencies",
                "Executive Sponsor Email"
            }));
            foreach (var r in rows)
            {
                var tags = string.Join("; ", r.Tags);
                var consumers = r.DataConsumers ?? string.Empty;
                var line = string.Join(",", new[]
                {
                    // Domain *
                    CsvEscape(r.Domain ?? string.Empty),
                    // Product Group
                    CsvEscape(r.ProductGroup ?? string.Empty),
                    // Product Name * (Title)
                    CsvEscape(r.Title ?? string.Empty),
                    // Product Description and Purpose * (Description)
                    CsvEscape(r.Description ?? string.Empty),
                    // Product Status (Status)
                    CsvEscape(r.Status ?? string.Empty),
                    // Product Status Notes
                    CsvEscape(r.ProductStatusNotes ?? string.Empty),
                    // Division *
                    CsvEscape(r.Division ?? string.Empty),
                    // Operating Entity *
                    CsvEscape(r.OperatingEntity ?? string.Empty),
                    // Executive Sponsor * (name only)
                    CsvEscape(r.ExecutiveSponsorName ?? string.Empty),
                    // Data Consumers * (semicolon separated)
                    CsvEscape(consumers),
                    // D&A Product Owner * (Owner Name)
                    CsvEscape(r.OwnerName ?? string.Empty),
                    // D&A Product Owner Email *
                    CsvEscape(r.OwnerEmail ?? string.Empty),
                    // Tech Delivery Mgr
                    CsvEscape(r.TechDeliveryManager ?? string.Empty),
                    // Regulatory/Compliance/Contractual
                    CsvEscape(r.RegulatoryComplianceContractual ?? string.Empty),
                    // Asset Type *
                    CsvEscape(r.AssetType ?? string.Empty),
                    // Featured
                    BoolOut(r.Featured),
                    // BI Platform *
                    CsvEscape(r.BiPlatform ?? string.Empty),
                    // Location/URL * (Url)
                    CsvEscape(r.Url ?? string.Empty),
                    // DB Server
                    CsvEscape(r.DbServer ?? string.Empty),
                    // DB/Data Mart
                    CsvEscape(r.DbDataMart ?? string.Empty),
                    // Database Table
                    CsvEscape(r.DatabaseTable ?? string.Empty),
                    // Source Repo (maps to SourceRep)
                    CsvEscape(r.SourceRep ?? string.Empty),
                    // dataSecurityClassification
                    CsvEscape(r.DataSecurityClassification ?? string.Empty),
                    // accessGroupName
                    CsvEscape(r.AccessGroupName ?? string.Empty),
                    // accessGroupDN
                    CsvEscape(r.AccessGroupDn ?? string.Empty),
                    // Data Source *
                    CsvEscape(r.DataSource ?? string.Empty),
                    // Automation Classification
                    CsvEscape(r.AutomationClassification ?? string.Empty),
                    // user_visibility_string
                    CsvEscape(r.UserVisibilityString ?? string.Empty),
                    // user_visibility_number
                    CsvEscape(r.UserVisibilityNumber ?? string.Empty),
                    // Default AD Group Names
                    CsvEscape(r.DefaultAdGroupNames ?? string.Empty),
                    // Has PHI Flag *
                    BoolOut(r.PrivacyPhi),
                    // Has PII Flag *
                    BoolOut(r.PrivacyPii),
                    // Has RLS Flag *
                    BoolOut(r.HasRls),
                    // Epic Security Group tag
                    CsvEscape(r.EpicSecurityGroupTag ?? string.Empty),
                    // Refresh Frequency *
                    CsvEscape(r.RefreshFrequency ?? string.Empty),
                    // Keep (maps to KeepLongTerm)
                    CsvEscape(r.KeepLongTerm ?? string.Empty),
                    // Potential to Consolidate
                    CsvEscape(r.PotentialToConsolidate ?? string.Empty),
                    // Potential to Automate
                    CsvEscape(r.PotentialToAutomate ?? string.Empty),
                    // Last Date Modified for this row * (yyyy-MM-dd)
                    CsvEscape(r.LastModifiedDate.HasValue ? r.LastModifiedDate.Value.ToString("yyyy-MM-dd") : string.Empty),
                    // Business Value rated by the executive sponsor
                    CsvEscape(r.SponsorBusinessValue ?? string.Empty),
                    // 2025 Must Do removed
                    // Development Effort
                    CsvEscape(r.DevelopmentEffort ?? string.Empty),
                    // Estimated 2025 development hours
                    CsvEscape(r.EstimatedDevHours ?? string.Empty),
                    // Resources - Development
                    CsvEscape(r.ResourcesDevelopment ?? string.Empty),
                    // Resources - Analysts
                    CsvEscape(r.ResourcesAnalysts ?? string.Empty),
                    // Resources - Platform
                    CsvEscape(r.ResourcesPlatform ?? string.Empty),
                    // Resources - Data Engineering
                    CsvEscape(r.ResourcesDataEngineering ?? string.Empty),
                    // Product Impact Category
                    CsvEscape(r.ProductImpactCategory ?? string.Empty),
                    // Extra fields appended at the end
                    CsvEscape(r.Id.ToString()),
                    // Service Line removed
                    CsvEscape(tags),
                    CsvEscape(r.Dependencies ?? string.Empty),
                    CsvEscape(r.ExecutiveSponsorEmail ?? string.Empty)
                });
                sb.AppendLine(line);
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "reports-export.csv");
        }

        // POST /api/items
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateItemDto dto)
        {
            // Admin-only create
            if (CurrentUser?.UserType != "Admin")
                return Forbid();
            var i = new Item
            {
                Title = dto.Title,
                Description = dto.Description,
                Url = dto.Url,
                // single AssetTypeId is used
                AssetTypeId = dto.AssetTypeId,
                // tags will be attached via ItemTags below
                DomainId = null,
                DivisionId = null,
                DataSourceId = null,
                PrivacyPhi = dto.PrivacyPhi,
                PrivacyPii = dto.PrivacyPii,
                HasRls = dto.HasRls,
                DateAdded = DateTime.UtcNow,
                Featured = dto.Featured,
                Dependencies = string.IsNullOrWhiteSpace(dto.Dependencies) ? null : dto.Dependencies,
                DefaultAdGroupNames = string.IsNullOrWhiteSpace(dto.DefaultAdGroupNames) ? null : dto.DefaultAdGroupNames,
                LastModifiedDate = dto.LastModifiedDate
            };

            // Attach tags: find existing Tag entities or create new ones
            if (dto.Tags != null && dto.Tags.Any())
            {
                foreach (var t in dto.Tags.Select(tt => tt?.Trim()).Where(tt => !string.IsNullOrWhiteSpace(tt)))
                {
                    var tag = await _db.Tags.FirstOrDefaultAsync(x => x.Value == t);
                    if (tag == null)
                    {
                        tag = new Tag { Value = t };
                        _db.Tags.Add(tag);
                    }
                    i.ItemTags.Add(new ItemTag { Item = i, Tag = tag });
                }
            }

            // If lookup IDs provided, populate string fields for compatibility
            // Map lookup ids or text to LookupValue FKs. If caller provided only text
            // (e.g., legacy clients), try to find an existing lookup and create one
            // if necessary, then assign the corresponding FK id.
            if (dto.DomainId.HasValue)
            {
                var lookup = await _db.LookupValues.FindAsync(dto.DomainId.Value);
                if (lookup != null) { i.DomainId = lookup.Id; }
            }
            else if (!string.IsNullOrWhiteSpace(dto.Domain))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Domain" && l.Value == dto.Domain);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "Domain", Value = dto.Domain };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.DomainId = lookup.Id;
            }

            if (dto.DivisionId.HasValue)
            {
                var lookup = await _db.LookupValues.FindAsync(dto.DivisionId.Value);
                if (lookup != null) { i.DivisionId = lookup.Id; }
            }
            else if (!string.IsNullOrWhiteSpace(dto.Division))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Division" && l.Value == dto.Division);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "Division", Value = dto.Division };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.DivisionId = lookup.Id;
            }

            // ServiceLine removed

            if (dto.DataSourceId.HasValue)
            {
                var lookup = await _db.LookupValues.FindAsync(dto.DataSourceId.Value);
                if (lookup != null) { i.DataSourceId = lookup.Id; }
            }
            else if (!string.IsNullOrWhiteSpace(dto.DataSource))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "DataSource" && l.Value == dto.DataSource);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "DataSource", Value = dto.DataSource };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.DataSourceId = lookup.Id;
            }

            if (dto.AssetTypeId.HasValue)
            {
                var at = await _db.LookupValues.FindAsync(dto.AssetTypeId.Value);
                // nothing else required; AssetTypeId is stored
            }

            // Set featured flag
            i.Featured = dto.Featured;

            // Owner assignment
            if (dto.OwnerId.HasValue)
            {
                var owner = await _db.Owners.FindAsync(dto.OwnerId.Value);
                if (owner != null) i.OwnerId = owner.Id;
            }
            else if (!string.IsNullOrWhiteSpace(dto.OwnerEmail) || !string.IsNullOrWhiteSpace(dto.OwnerName))
            {
                var email = (dto.OwnerEmail ?? string.Empty).Trim();
                var name = (dto.OwnerName ?? string.Empty).Trim();
                var existing = !string.IsNullOrEmpty(email)
                    ? await _db.Owners.FirstOrDefaultAsync(o => o.Email == email)
                    : await _db.Owners.FirstOrDefaultAsync(o => o.Name == name);
                if (existing == null)
                {
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                    {
                        return BadRequest("Both OwnerName and OwnerEmail are required to create a new owner.");
                    }
                    existing = new Owner { Name = name, Email = email };
                    _db.Owners.Add(existing);
                    await _db.SaveChangesAsync();
                }
                i.OwnerId = existing.Id;
            }

            // Executive Sponsor assignment
            if (dto.ExecutiveSponsorId.HasValue)
            {
                var s = await _db.Owners.FindAsync(dto.ExecutiveSponsorId.Value);
                if (s != null) i.ExecutiveSponsorId = s.Id;
            }
            else if (!string.IsNullOrWhiteSpace(dto.ExecutiveSponsorEmail) || !string.IsNullOrWhiteSpace(dto.ExecutiveSponsorName))
            {
                var email = (dto.ExecutiveSponsorEmail ?? string.Empty).Trim();
                var name = (dto.ExecutiveSponsorName ?? string.Empty).Trim();
                var existing = !string.IsNullOrEmpty(email)
                    ? await _db.Owners.FirstOrDefaultAsync(o => o.Email == email)
                    : await _db.Owners.FirstOrDefaultAsync(o => o.Name == name);
                if (existing == null)
                {
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                    {
                        return BadRequest("Both ExecutiveSponsorName and ExecutiveSponsorEmail are required to create a new sponsor.");
                    }
                    existing = new Owner { Name = name, Email = email };
                    _db.Owners.Add(existing);
                    await _db.SaveChangesAsync();
                }
                i.ExecutiveSponsorId = existing.Id;
            }

            // Operating Entity
            if (dto.OperatingEntityId.HasValue)
            {
                var lookup = await _db.LookupValues.FindAsync(dto.OperatingEntityId.Value);
                if (lookup != null) i.OperatingEntityId = lookup.Id;
            }
            else if (!string.IsNullOrWhiteSpace(dto.OperatingEntity))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "OperatingEntity" && l.Value == dto.OperatingEntity);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "OperatingEntity", Value = dto.OperatingEntity };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.OperatingEntityId = lookup.Id;
            }

            // Refresh Frequency
            if (dto.RefreshFrequencyId.HasValue)
            {
                var lookup = await _db.LookupValues.FindAsync(dto.RefreshFrequencyId.Value);
                if (lookup != null) i.RefreshFrequencyId = lookup.Id;
            }
            else if (!string.IsNullOrWhiteSpace(dto.RefreshFrequency))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "RefreshFrequency" && l.Value == dto.RefreshFrequency);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "RefreshFrequency", Value = dto.RefreshFrequency };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.RefreshFrequencyId = lookup.Id;
            }

            // Data Consumers: simple free text
            i.DataConsumers = string.IsNullOrWhiteSpace(dto.DataConsumers) ? null : dto.DataConsumers.Trim();

            // Product Impact Category
            if (dto.ProductImpactCategoryId.HasValue)
                i.ProductImpactCategoryId = dto.ProductImpactCategoryId;
            else if (!string.IsNullOrWhiteSpace(dto.ProductImpactCategory))
            {
                var pic = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "ProductImpactCategory" && l.Value == dto.ProductImpactCategory);
                if (pic == null)
                {
                    pic = new LookupValue { Type = "ProductImpactCategory", Value = dto.ProductImpactCategory };
                    _db.LookupValues.Add(pic);
                    await _db.SaveChangesAsync();
                }
                i.ProductImpactCategoryId = pic.Id;
            }

            // Status assignment: default to Published; admins may override via dto.StatusId
            var currentUser = CurrentUser;
            var admin = currentUser?.UserType == "Admin";
            var publishedLv = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Status" && l.Value == "Published");
            if (admin && dto.StatusId.HasValue)
            {
                var st = await _db.LookupValues.FindAsync(dto.StatusId.Value);
                if (st != null) i.StatusId = st.Id;
                else if (publishedLv != null) i.StatusId = publishedLv.Id;
            }
            else if (publishedLv != null)
            {
                i.StatusId = publishedLv.Id;
            }

            _db.Items.Add(i);
            await _db.SaveChangesAsync();

            // Call AI index service: create index entry for this item
            try { await _httpFactory.CreateClient("SearchApi").PostAsync($"items/{i.Id}", null); } catch { /* swallow */ }

            return Ok(new { id = i.Id });
        }

        // PUT /api/items/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromForm] CreateItemDto dto)
        {
            // Admin-only update
            if (CurrentUser?.UserType != "Admin")
                return Forbid();
            var i = await _db.Items.FindAsync(id);
            if (i == null) return NotFound();

            i.Title = dto.Title;
            i.Description = dto.Description;
            i.Url = dto.Url;
            // asset types are now single-valued; update AssetTypeId/Featured below
            // Update tags: remove existing item-tags and reattach
            // Load existing ItemTags
            var existingTags = await _db.ItemTags.Where(it => it.ItemId == i.Id).ToListAsync();
            if (existingTags.Any())
            {
                _db.ItemTags.RemoveRange(existingTags);
            }
            if (dto.Tags != null && dto.Tags.Any())
            {
                foreach (var t in dto.Tags.Select(tt => tt?.Trim()).Where(tt => !string.IsNullOrWhiteSpace(tt)))
                {
                    var tag = await _db.Tags.FirstOrDefaultAsync(x => x.Value == t);
                    if (tag == null)
                    {
                        tag = new Tag { Value = t };
                        _db.Tags.Add(tag);
                    }
                    _db.ItemTags.Add(new ItemTag { Item = i, Tag = tag });
                }
            }
            if (dto.AssetTypeId.HasValue) i.AssetTypeId = dto.AssetTypeId;
            // Map domain/division/dataSource to lookup ids when provided
            if (dto.DomainId.HasValue)
                i.DomainId = dto.DomainId;
            else if (!string.IsNullOrWhiteSpace(dto.Domain))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Domain" && l.Value == dto.Domain);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "Domain", Value = dto.Domain };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.DomainId = lookup.Id;
            }

            if (dto.DivisionId.HasValue)
                i.DivisionId = dto.DivisionId;
            else if (!string.IsNullOrWhiteSpace(dto.Division))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Division" && l.Value == dto.Division);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "Division", Value = dto.Division };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.DivisionId = lookup.Id;
            }

            // ServiceLine removed

            if (dto.DataSourceId.HasValue)
                i.DataSourceId = dto.DataSourceId;
            else if (!string.IsNullOrWhiteSpace(dto.DataSource))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "DataSource" && l.Value == dto.DataSource);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "DataSource", Value = dto.DataSource };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.DataSourceId = lookup.Id;
            }
            // Owner update
            if (dto.OwnerId.HasValue)
            {
                var owner = await _db.Owners.FindAsync(dto.OwnerId.Value);
                if (owner != null) i.OwnerId = owner.Id;
            }
            else if (!string.IsNullOrWhiteSpace(dto.OwnerEmail) || !string.IsNullOrWhiteSpace(dto.OwnerName))
            {
                var email = (dto.OwnerEmail ?? string.Empty).Trim();
                var name = (dto.OwnerName ?? string.Empty).Trim();
                var existing = !string.IsNullOrEmpty(email)
                    ? await _db.Owners.FirstOrDefaultAsync(o => o.Email == email)
                    : await _db.Owners.FirstOrDefaultAsync(o => o.Name == name);
                if (existing == null)
                {
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                    {
                        return BadRequest("Both OwnerName and OwnerEmail are required to create a new owner.");
                    }
                    existing = new Owner { Name = name, Email = email };
                    _db.Owners.Add(existing);
                    await _db.SaveChangesAsync();
                }
                i.OwnerId = existing.Id;
            }

            // Executive Sponsor update
            if (dto.ExecutiveSponsorId.HasValue)
            {
                var s = await _db.Owners.FindAsync(dto.ExecutiveSponsorId.Value);
                if (s != null) i.ExecutiveSponsorId = s.Id;
            }
            else if (!string.IsNullOrWhiteSpace(dto.ExecutiveSponsorEmail) || !string.IsNullOrWhiteSpace(dto.ExecutiveSponsorName))
            {
                var email = (dto.ExecutiveSponsorEmail ?? string.Empty).Trim();
                var name = (dto.ExecutiveSponsorName ?? string.Empty).Trim();
                var existing = !string.IsNullOrEmpty(email)
                    ? await _db.Owners.FirstOrDefaultAsync(o => o.Email == email)
                    : await _db.Owners.FirstOrDefaultAsync(o => o.Name == name);
                if (existing == null)
                {
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                    {
                        return BadRequest("Both ExecutiveSponsorName and ExecutiveSponsorEmail are required to create a new sponsor.");
                    }
                    existing = new Owner { Name = name, Email = email };
                    _db.Owners.Add(existing);
                    await _db.SaveChangesAsync();
                }
                i.ExecutiveSponsorId = existing.Id;
            }

            // Operating Entity update
            if (dto.OperatingEntityId.HasValue)
                i.OperatingEntityId = dto.OperatingEntityId;
            else if (!string.IsNullOrWhiteSpace(dto.OperatingEntity))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "OperatingEntity" && l.Value == dto.OperatingEntity);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "OperatingEntity", Value = dto.OperatingEntity };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.OperatingEntityId = lookup.Id;
            }

            // Refresh Frequency update
            if (dto.RefreshFrequencyId.HasValue)
                i.RefreshFrequencyId = dto.RefreshFrequencyId;
            else if (!string.IsNullOrWhiteSpace(dto.RefreshFrequency))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "RefreshFrequency" && l.Value == dto.RefreshFrequency);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "RefreshFrequency", Value = dto.RefreshFrequency };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.RefreshFrequencyId = lookup.Id;
            }

            // Data Consumers update: simple free text
            i.DataConsumers = string.IsNullOrWhiteSpace(dto.DataConsumers) ? null : dto.DataConsumers.Trim();

            // Product Impact Category
            if (dto.ProductImpactCategoryId.HasValue)
                i.ProductImpactCategoryId = dto.ProductImpactCategoryId;
            else if (!string.IsNullOrWhiteSpace(dto.ProductImpactCategory))
            {
                var pic = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "ProductImpactCategory" && l.Value == dto.ProductImpactCategory);
                if (pic == null)
                {
                    pic = new LookupValue { Type = "ProductImpactCategory", Value = dto.ProductImpactCategory };
                    _db.LookupValues.Add(pic);
                    await _db.SaveChangesAsync();
                }
                i.ProductImpactCategoryId = pic.Id;
            }

            // Status update (admin only)
            var currentUser = CurrentUser;
            if (currentUser?.UserType == "Admin" && dto.StatusId.HasValue)
            {
                var st = await _db.LookupValues.FindAsync(dto.StatusId.Value);
                if (st != null) i.StatusId = st.Id;
            }
            i.PrivacyPhi = dto.PrivacyPhi;
            i.PrivacyPii = dto.PrivacyPii;
            i.HasRls = dto.HasRls;
            i.Dependencies = string.IsNullOrWhiteSpace(dto.Dependencies) ? null : dto.Dependencies;
            i.DefaultAdGroupNames = string.IsNullOrWhiteSpace(dto.DefaultAdGroupNames) ? null : dto.DefaultAdGroupNames;
            i.LastModifiedDate = dto.LastModifiedDate;
            i.Featured = dto.Featured;
            // keep original DateAdded

            await _db.SaveChangesAsync();

            // Call AI index service: update index entry for this item (soft delete + add internally)
            try { await _httpFactory.CreateClient("SearchApi").PutAsync($"items/{i.Id}", null); } catch { /* swallow */ }
            return NoContent();
        }

        // POST /api/items/{id}/edit  — dev-friendly alternative to avoid CORS preflight under IIS Express
        [HttpPost("{id:int}/edit")]
        public async Task<IActionResult> EditCompat(int id, [FromForm] CreateItemDto dto)
        {
            // Reuse Update logic
            return await Update(id, dto);
        }

        // DELETE /api/items/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Admin-only delete
            if (CurrentUser?.UserType != "Admin")
                return Forbid();
            var i = await _db.Items.FindAsync(id);
            if (i == null) return NotFound();
            _db.Items.Remove(i);
            await _db.SaveChangesAsync();

            // Call AI index service: delete index entry for this item
            try { await _httpFactory.CreateClient("SearchApi").DeleteAsync($"items/{id}"); } catch { /* swallow */ }
            return NoContent();
        }
    }
}
