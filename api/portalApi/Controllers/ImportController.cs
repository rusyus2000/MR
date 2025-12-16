using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/imports")]
        public class ImportController : MpBaseController
        {
        private readonly AppDbContext _db;
        private readonly IHttpClientFactory _httpFactory;
        public ImportController(AppDbContext db, IHttpClientFactory httpFactory)
        {
            _db = db;
            _httpFactory = httpFactory;
        }

        private static string? NullIfEmpty(string? s)
        {
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        public class PreviewResult
        {
            public string Token { get; set; } = string.Empty;
            public int Total { get; set; }
            public int ToCreate { get; set; }
            public int ToUpdate { get; set; }
            public int Unchanged { get; set; }
            public int Conflicts { get; set; }
            public int Errors { get; set; }
            public List<object> ConflictsList { get; set; } = new();
            public List<object> ErrorsList { get; set; } = new();
        }

        private static string Normalize(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return s.Trim();
        }

        private static string NormalizeUrl(string? s) => Normalize(s).ToLowerInvariant();

        private static string NormalizeTags(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var parts = s.Split(';')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrEmpty(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase);
            return string.Join("; ", parts);
        }

        // Compute SHA-256 by streaming each field with a 4-byte big-endian length prefix,
        // avoiding large string allocations and eliminating delimiter ambiguity.
        private static byte[] ComputeHashFromFields(params string?[] fields)
        {
            using var ih = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            Span<byte> len = stackalloc byte[4];
            foreach (var f in fields)
            {
                var s = f ?? string.Empty;
                var bytes = Encoding.UTF8.GetBytes(s);
                len[0] = (byte)((bytes.Length >> 24) & 0xFF);
                len[1] = (byte)((bytes.Length >> 16) & 0xFF);
                len[2] = (byte)((bytes.Length >> 8) & 0xFF);
                len[3] = (byte)(bytes.Length & 0xFF);
                ih.AppendData(len);
                ih.AppendData(bytes);
            }
            return ih.GetHashAndReset();
        }

        private static List<string[]> ParseCsv(System.IO.Stream fileStream)
        {
            var rows = new List<string[]>();
            using var sr = new System.IO.StreamReader(fileStream, Encoding.UTF8, true, 1024, leaveOpen: true);
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                rows.Add(ParseCsvLine(line));
            }
            return rows;
        }

        private static string[] ParseCsvLine(string line)
        {
            var list = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == ',')
                    {
                        list.Add(sb.ToString()); sb.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }
            list.Add(sb.ToString());
            return list.ToArray();
        }

        // POST /api/imports/preview (multipart form: file, source, datasetKey, mode)
        [HttpPost("preview")]
        public async Task<ActionResult<PreviewResult>> Preview()
        {
            var user = CurrentUser;
            if (user?.UserType != "Admin") return Forbid();

            if (!Request.HasFormContentType) return BadRequest("Expected multipart form upload");
            var form = await Request.ReadFormAsync();
            var file = form.Files["file"];
            if (file == null || file.Length == 0) return BadRequest("Missing file");
            var source = form["source"].FirstOrDefault();
            var datasetKey = form["datasetKey"].FirstOrDefault();
            var mode = form["mode"].FirstOrDefault() ?? "upsert";

            // Parse CSV
            List<string[]> rows;
            using (var s = file.OpenReadStream())
            {
                rows = ParseCsv(s);
            }
            if (rows.Count == 0) return BadRequest("Empty file");

            // Expect header
            var header = rows[0];
            // Map columns by name
            int idx(string name)
            {
                for (int i = 0; i < header.Length; i++)
                    if (string.Equals(header[i]?.Trim(), name, StringComparison.OrdinalIgnoreCase))
                        return i;
                return -1;
            }

            int cId = idx("Id");
            int cTitle = idx("Title");
            int cDesc = idx("Description");
            int cUrl = idx("Url");
            int cAssetType = idx("Asset Type");
            int cDomain = idx("Domain");
            int cDivision = idx("Division");
            int cServiceLine = idx("Service Line");
            int cDataSource = idx("Data Source");
            int cStatus = idx("Status");
            int cOwnerName = idx("Owner Name");
            int cOwnerEmail = idx("Owner Email");
            int cExecName = idx("Executive Sponsor Name");
            int cExecEmail = idx("Executive Sponsor Email");
            int cOperatingEntity = idx("Operating Entity");
            int cRefreshFrequency = idx("Refresh Frequency");
            int cPhi = idx("PHI");
            int cPii = idx("PII");
            int cHasRls = idx("Has RLS");
            int cLastModified = idx("Last Modified Date");
            // Date Added removed from import format (set server-side)
            int cFeatured = idx("Featured");
            int cTags = idx("Tags");
            int cDataConsumers = idx("Data Consumers");
            int cDependencies = idx("Dependencies");
            int cDefaultAdGroups = idx("Default AD Group Names");
            // Free-text extras
            int cProductGroup = idx("Product Group");
            int cProductStatusNotes = idx("Product Status Notes");
            int cTechDeliveryManager = idx("Tech Delivery Mgr");
            int cRegulatoryCompliance = idx("Regulatory/Compliance/Contractual");
            int cBiPlatform = idx("BI Platform");
            int cDbServer = idx("DB Server");
            int cDbDataMart = idx("DB/Data Mart");
            int cDatabaseTable = idx("Database Table");
            int cSourceRep = idx("Source Rep");
            int cDataSecurityClassification = idx("dataSecurityClassification");
            int cAccessGroupName = idx("accessGroupName");
            int cAccessGroupDn = idx("accessGroupDN");
            int cAutomationClassification = idx("Automation Classification");
            int cUserVisibilityString = idx("user_visibility_string");
            int cUserVisibilityNumber = idx("user_visibility_number");
            int cEpicSecurityGroupTag = idx("Epic Security Group tag");
            int cKeepLongTerm = idx("Keep Long Term");
            // Optional lookup extras
            int cPotentialToConsolidate = idx("Potential to Consolidate");
            int cPotentialToAutomate = idx("Potential to Automate");
            int cSponsorBusinessValue = idx("Business Value by executive sponsor");
            // 2025 Must Do removed
            int cDevelopmentEffort = idx("Development Effort");
            int cEstimatedDevHours = idx("Estimated development hours");
            int cResourcesDevelopment = idx("Resources - Development");
            int cResourcesAnalysts = idx("Resources - Analysts");
            int cResourcesPlatform = idx("Resources - Platform");
            int cResourcesDataEngineering = idx("Resources - Data Engineering");

            if (cTitle < 0 || cUrl < 0)
                return BadRequest("Missing required columns");

            var dataRows = rows.Skip(1).Where(r => r.Length > 1).ToList();

            // Prefetch existing items minimal data
            var existing = await _db.Items
                .AsNoTracking()
                .Include(i => i.AssetType)
                .Include(i => i.DomainLookup)
                .Include(i => i.DivisionLookup)
                // ServiceLine removed
                .Include(i => i.DataSourceLookup)
                .Include(i => i.StatusLookup)
                .Include(i => i.Owner)
                .Include(i => i.ItemTags).ThenInclude(it => it.Tag)
                .Select(i => new
                {
                    i.Id,
                    i.Url,
                    i.Title,
                    Description = i.Description ?? string.Empty,
                    AssetType = i.AssetType != null ? i.AssetType.Value : string.Empty,
                    Domain = i.DomainLookup != null ? i.DomainLookup.Value : string.Empty,
                    Division = i.DivisionLookup != null ? i.DivisionLookup.Value : string.Empty,
                    OperatingEntity = i.OperatingEntityLookup != null ? i.OperatingEntityLookup.Value : string.Empty,
                    // ServiceLine removed
                    DataSource = i.DataSourceLookup != null ? i.DataSourceLookup.Value : string.Empty,
                    Status = i.StatusLookup != null ? i.StatusLookup.Value : string.Empty,
                    OwnerName = i.Owner != null ? i.Owner.Name : string.Empty,
                    OwnerEmail = i.Owner != null ? i.Owner.Email : string.Empty,
                    i.PrivacyPhi,
                    i.DateAdded,
                    i.Featured,
                    Tags = i.ItemTags.Select(it => it.Tag.Value),
                    i.ContentHash
                })
                .ToListAsync();

            var byId = existing.ToDictionary(x => x.Id, x => x);
            string keyOf(string url, string title, string domain, string at) =>
                $"{NormalizeUrl(url)}|{Normalize(title).ToLowerInvariant()}|{Normalize(domain).ToLowerInvariant()}|{Normalize(at).ToLowerInvariant()}";
            var byKey = existing.GroupBy(x => keyOf(x.Url, x.Title, x.Domain, x.AssetType))
                                .ToDictionary(g => g.Key, g => g.ToList());

            // Prefetch lookup values for strict validation (case-insensitive),
            // normalizing both Type and Value to avoid mismatch from stray spaces
            var lookups = await _db.LookupValues.AsNoTracking().ToListAsync();
            var luByType = lookups
                .GroupBy(l => Normalize(l.Type), StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => Normalize(x.Value)).ToHashSet(StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase);

            int toCreate = 0, toUpdate = 0, unchanged = 0, conflicts = 0, errors = 0, skippedBlank = 0;
            var conflictsList = new List<object>();
            var errorsList = new List<object>();

            var previewRows = new List<object>(dataRows.Count);

            int rowIdx = 0;
            foreach (var r in dataRows)
            {
                try
                {
                    rowIdx++;
                    // Read raw values
                    var rawTitle = cTitle < r.Length ? r[cTitle] : null;
                    var rawDesc = cDesc < r.Length ? r[cDesc] : null;
                    var rawUrl = cUrl < r.Length ? r[cUrl] : null;
                    var rawAssetType = cAssetType < r.Length ? r[cAssetType] : null;
                    var rawDomain = cDomain < r.Length ? r[cDomain] : null;
                    var rawDivision = cDivision < r.Length ? r[cDivision] : null;
                    var rawServiceLine = cServiceLine < r.Length ? r[cServiceLine] : null;
                    var rawDataSource = cDataSource < r.Length ? r[cDataSource] : null;
                    var rawStatus = cStatus < r.Length ? r[cStatus] : null;
                    var rawOwnerName = cOwnerName < r.Length ? r[cOwnerName] : null;
                    var rawOwnerEmail = cOwnerEmail < r.Length ? r[cOwnerEmail] : null;
                    var rawExecName = cExecName >= 0 && cExecName < r.Length ? r[cExecName] : null;
                    var rawExecEmail = cExecEmail >= 0 && cExecEmail < r.Length ? r[cExecEmail] : null;
                    var rawPhi = cPhi < r.Length ? r[cPhi] : null;
                    var rawPii = cPii >= 0 && cPii < r.Length ? r[cPii] : null;
                    var rawHasRls = cHasRls >= 0 && cHasRls < r.Length ? r[cHasRls] : null;
                    var rawLastModified = cLastModified >= 0 && cLastModified < r.Length ? r[cLastModified] : null;
                    // Date Added removed (handled server-side)
                    var rawFeatured = cFeatured < r.Length ? r[cFeatured] : null;
                    var rawTags = cTags < r.Length ? r[cTags] : null;
                    var rawOperatingEntity = cOperatingEntity >= 0 && cOperatingEntity < r.Length ? r[cOperatingEntity] : null;
                    var rawRefreshFrequency = cRefreshFrequency >= 0 && cRefreshFrequency < r.Length ? r[cRefreshFrequency] : null;
                    var rawDataConsumers = cDataConsumers >= 0 && cDataConsumers < r.Length ? r[cDataConsumers] : null;
                    var rawDependencies = cDependencies >= 0 && cDependencies < r.Length ? r[cDependencies] : null;
                    var rawDefaultAdGroups = cDefaultAdGroups >= 0 && cDefaultAdGroups < r.Length ? r[cDefaultAdGroups] : null;
                    // Free-text extras
                    var rawProductGroup = cProductGroup >= 0 && cProductGroup < r.Length ? r[cProductGroup] : null;
                    var rawProductStatusNotes = cProductStatusNotes >= 0 && cProductStatusNotes < r.Length ? r[cProductStatusNotes] : null;
                    var rawTechDeliveryManager = cTechDeliveryManager >= 0 && cTechDeliveryManager < r.Length ? r[cTechDeliveryManager] : null;
                    var rawRegulatoryCompliance = cRegulatoryCompliance >= 0 && cRegulatoryCompliance < r.Length ? r[cRegulatoryCompliance] : null;
                    var rawBiPlatform = cBiPlatform >= 0 && cBiPlatform < r.Length ? r[cBiPlatform] : null;
                    var rawDbServer = cDbServer >= 0 && cDbServer < r.Length ? r[cDbServer] : null;
                    var rawDbDataMart = cDbDataMart >= 0 && cDbDataMart < r.Length ? r[cDbDataMart] : null;
                    var rawDatabaseTable = cDatabaseTable >= 0 && cDatabaseTable < r.Length ? r[cDatabaseTable] : null;
                    var rawSourceRep = cSourceRep >= 0 && cSourceRep < r.Length ? r[cSourceRep] : null;
                    var rawDataSecurityClassification = cDataSecurityClassification >= 0 && cDataSecurityClassification < r.Length ? r[cDataSecurityClassification] : null;
                    var rawAccessGroupName = cAccessGroupName >= 0 && cAccessGroupName < r.Length ? r[cAccessGroupName] : null;
                    var rawAccessGroupDn = cAccessGroupDn >= 0 && cAccessGroupDn < r.Length ? r[cAccessGroupDn] : null;
                    var rawAutomationClassification = cAutomationClassification >= 0 && cAutomationClassification < r.Length ? r[cAutomationClassification] : null;
                    var rawUserVisibilityString = cUserVisibilityString >= 0 && cUserVisibilityString < r.Length ? r[cUserVisibilityString] : null;
                    var rawUserVisibilityNumber = cUserVisibilityNumber >= 0 && cUserVisibilityNumber < r.Length ? r[cUserVisibilityNumber] : null;
                    var rawEpicSecurityGroupTag = cEpicSecurityGroupTag >= 0 && cEpicSecurityGroupTag < r.Length ? r[cEpicSecurityGroupTag] : null;
                    var rawKeepLongTerm = cKeepLongTerm >= 0 && cKeepLongTerm < r.Length ? r[cKeepLongTerm] : null;
                    // Optional lookup extras
                    var rawPotentialToConsolidate = cPotentialToConsolidate >= 0 && cPotentialToConsolidate < r.Length ? r[cPotentialToConsolidate] : null;
                    var rawPotentialToAutomate = cPotentialToAutomate >= 0 && cPotentialToAutomate < r.Length ? r[cPotentialToAutomate] : null;
                    var rawSponsorBusinessValue = cSponsorBusinessValue >= 0 && cSponsorBusinessValue < r.Length ? r[cSponsorBusinessValue] : null;
                    // MustDo2025 removed
                    var rawDevelopmentEffort = cDevelopmentEffort >= 0 && cDevelopmentEffort < r.Length ? r[cDevelopmentEffort] : null;
                    var rawEstimatedDevHours = cEstimatedDevHours >= 0 && cEstimatedDevHours < r.Length ? r[cEstimatedDevHours] : null;
                    var rawResourcesDevelopment = cResourcesDevelopment >= 0 && cResourcesDevelopment < r.Length ? r[cResourcesDevelopment] : null;
                    var rawResourcesAnalysts = cResourcesAnalysts >= 0 && cResourcesAnalysts < r.Length ? r[cResourcesAnalysts] : null;
                    var rawResourcesPlatform = cResourcesPlatform >= 0 && cResourcesPlatform < r.Length ? r[cResourcesPlatform] : null;
                    var rawResourcesDataEngineering = cResourcesDataEngineering >= 0 && cResourcesDataEngineering < r.Length ? r[cResourcesDataEngineering] : null;

                    // Normalize strings
                    var row = new
                    {
                        Id = (cId >= 0 && cId < r.Length && int.TryParse(r[cId], out var rid)) ? rid : (int?)null,
                        Title = Normalize(rawTitle),
                        Description = Normalize(rawDesc),
                        Url = NormalizeUrl(rawUrl),
                        AssetType = Normalize(rawAssetType),
                        Domain = Normalize(rawDomain),
                        Division = Normalize(rawDivision),
                        ServiceLine = Normalize(rawServiceLine),
                        DataSource = Normalize(rawDataSource),
                        Status = Normalize(rawStatus),
                        OwnerName = Normalize(rawOwnerName),
                        OwnerEmail = Normalize(rawOwnerEmail),
                        ExecutiveSponsorName = Normalize(rawExecName),
                        ExecutiveSponsorEmail = Normalize(rawExecEmail),
                        PrivacyPhi = Normalize(rawPhi).Equals("true", StringComparison.OrdinalIgnoreCase) ? "true" : "false",
                        PrivacyPii = Normalize(rawPii).Equals("true", StringComparison.OrdinalIgnoreCase) ? "true" : "false",
                        HasRls = Normalize(rawHasRls).Equals("true", StringComparison.OrdinalIgnoreCase) ? "true" : "false",
                        LastModifiedDate = Normalize(rawLastModified),
                        // Date Added removed from import row
                        Featured = Normalize(rawFeatured).Equals("true", StringComparison.OrdinalIgnoreCase) ? "true" : "false",
                        Tags = NormalizeTags(rawTags),
                        DataConsumers = Normalize(rawDataConsumers),
                        Dependencies = Normalize(rawDependencies),
                        DefaultAdGroupNames = Normalize(rawDefaultAdGroups),
                        OperatingEntity = Normalize(rawOperatingEntity),
                        RefreshFrequency = Normalize(rawRefreshFrequency),
                        // Free-text extras
                        ProductGroup = Normalize(rawProductGroup),
                        ProductStatusNotes = Normalize(rawProductStatusNotes),
                        // DataConsumers is a free-text field
                        TechDeliveryManager = Normalize(rawTechDeliveryManager),
                        RegulatoryComplianceContractual = Normalize(rawRegulatoryCompliance),
                        BiPlatform = Normalize(rawBiPlatform),
                        DbServer = Normalize(rawDbServer),
                        DbDataMart = Normalize(rawDbDataMart),
                        DatabaseTable = Normalize(rawDatabaseTable),
                        SourceRep = Normalize(rawSourceRep),
                        DataSecurityClassification = Normalize(rawDataSecurityClassification),
                        AccessGroupName = Normalize(rawAccessGroupName),
                        AccessGroupDn = Normalize(rawAccessGroupDn),
                        AutomationClassification = Normalize(rawAutomationClassification),
                        UserVisibilityString = Normalize(rawUserVisibilityString),
                        UserVisibilityNumber = Normalize(rawUserVisibilityNumber),
                        EpicSecurityGroupTag = Normalize(rawEpicSecurityGroupTag),
                        KeepLongTerm = Normalize(rawKeepLongTerm),
                        // Lookup extras
                        PotentialToConsolidate = Normalize(rawPotentialToConsolidate),
                        PotentialToAutomate = Normalize(rawPotentialToAutomate),
                        SponsorBusinessValue = Normalize(rawSponsorBusinessValue),
                        // MustDo2025 removed
                        DevelopmentEffort = Normalize(rawDevelopmentEffort),
                        EstimatedDevHours = Normalize(rawEstimatedDevHours),
                        ResourcesDevelopment = Normalize(rawResourcesDevelopment),
                        ResourcesAnalysts = Normalize(rawResourcesAnalysts),
                        ResourcesPlatform = Normalize(rawResourcesPlatform),
                        ResourcesDataEngineering = Normalize(rawResourcesDataEngineering)
                    };

                    // Skip completely blank rows (e.g., leftover commas in CSV)
                    bool allEmpty = string.IsNullOrEmpty(row.Title)
                                 && string.IsNullOrEmpty(row.Description)
                                 && string.IsNullOrEmpty(row.Url)
                                 && string.IsNullOrEmpty(row.AssetType)
                                 && string.IsNullOrEmpty(row.Domain)
                                 && string.IsNullOrEmpty(row.Division)
                                 && string.IsNullOrEmpty(row.ServiceLine)
                                 && string.IsNullOrEmpty(row.DataSource)
                                 && string.IsNullOrEmpty(row.Status)
                                 && string.IsNullOrEmpty(row.OwnerName)
                                 && string.IsNullOrEmpty(row.OwnerEmail)
                                 && string.IsNullOrEmpty(Normalize(rawPhi))
                                 // Date Added removed from blank row detection
                                 && string.IsNullOrEmpty(Normalize(rawFeatured))
                                 && string.IsNullOrEmpty(row.Tags);
                    if (allEmpty)
                    {
                        skippedBlank++; // Do not count toward totals; ignore silently
                        continue;
                    }

                    // Strict lookup validation: all named lookups must exist
                    string missing = string.Empty;
                    bool ok(string type, string val) => string.IsNullOrEmpty(val) || (luByType.TryGetValue(type, out var set) && set.Contains(val));
                    if (!ok("AssetType", row.AssetType)) missing = "AssetType";
                    else if (!ok("Domain", row.Domain)) missing = "Domain";
                    else if (!ok("Division", row.Division)) missing = "Division";
                    // ServiceLine removed from validation
                    else if (!ok("DataSource", row.DataSource)) missing = "DataSource";
                    else if (!ok("Status", row.Status)) missing = "Status";
                    else if (!ok("OperatingEntity", row.OperatingEntity)) missing = "OperatingEntity";
                    else if (!ok("RefreshFrequency", row.RefreshFrequency)) missing = "RefreshFrequency";
                    if (!string.IsNullOrEmpty(missing))
                    {
                        errors++;
                        errorsList.Add(new { index = rowIdx, reason = $"Unknown {missing}" });
                        previewRows.Add(new { index = rowIdx, action = "error", reason = $"Unknown {missing}" });
                        continue;
                    }

                    var hash = ComputeHashFromFields(
                        row.Title,
                        row.Description,
                        row.Url,
                        row.AssetType,
                        row.Domain,
                        row.Division,
                        // ServiceLine removed
                        row.DataSource,
                        row.Status,
                        row.OwnerName,
                        row.OwnerEmail,
                        row.PrivacyPhi,
                        row.PrivacyPii,
                        row.HasRls,
                        row.LastModifiedDate,
                        row.Featured,
                        row.Tags,
                        row.DataConsumers,
                        row.Dependencies,
                        row.DefaultAdGroupNames,
                        row.OperatingEntity,
                        row.RefreshFrequency,
                        row.ProductGroup,
                        row.ProductStatusNotes,
                        row.TechDeliveryManager,
                        row.RegulatoryComplianceContractual,
                        row.BiPlatform,
                        row.DbServer,
                        row.DbDataMart,
                        row.DatabaseTable,
                        row.SourceRep,
                        row.DataSecurityClassification,
                        row.AccessGroupName,
                        row.AccessGroupDn,
                        row.AutomationClassification,
                        row.UserVisibilityString,
                        row.UserVisibilityNumber,
                        row.EpicSecurityGroupTag,
                        row.KeepLongTerm,
                        row.PotentialToConsolidate,
                        row.PotentialToAutomate,
                        row.SponsorBusinessValue,
                        // MustDo2025 removed
                        row.DevelopmentEffort,
                        row.EstimatedDevHours,
                        row.ResourcesDevelopment,
                        row.ResourcesAnalysts,
                        row.ResourcesPlatform,
                        row.ResourcesDataEngineering
                    );

                    string k = keyOf(row.Url, row.Title, row.Domain, row.AssetType);
                    if (row.Id.HasValue && byId.TryGetValue(row.Id.Value, out var exById))
                    {
                        bool same;
                        if (exById.ContentHash != null)
                        {
                            same = exById.ContentHash.SequenceEqual(hash);
                        }
                        else
                        {
                            // Fallback: compute canonical from existing DB values when hash is missing
                            var exTagsNorm = NormalizeTags(string.Join("; ", (exById.Tags ?? Enumerable.Empty<string>()).Where(t => !string.IsNullOrWhiteSpace(t))));
                            var exHash = ComputeHashFromFields(
                                Normalize(exById.Title),
                                Normalize(exById.Description),
                                NormalizeUrl(exById.Url),
                                Normalize(exById.AssetType),
                                Normalize(exById.Domain),
                                Normalize(exById.Division),
                                Normalize(exById.OperatingEntity),
                                Normalize(exById.DataSource),
                                Normalize(exById.Status),
                                Normalize(exById.OwnerName),
                                Normalize(exById.OwnerEmail),
                                (exById.PrivacyPhi == true) ? "true" : "false",
                        // Date Added excluded from hash comparison
                                (exById.Featured == true) ? "true" : "false",
                                exTagsNorm
                            );
                            same = exHash.SequenceEqual(hash);
                        }
                        if (same) { unchanged++; previewRows.Add(new { index = rowIdx, row, hash = Convert.ToHexString(hash), targetId = row.Id, action = "unchanged" }); }
                        else { toUpdate++; previewRows.Add(new { index = rowIdx, row, hash = Convert.ToHexString(hash), targetId = row.Id, action = "update" }); }
                    }
                    else
                    {
                        if (byKey.TryGetValue(k, out var matches))
                        {
                            // For rows without Id that match existing by composite key, always require a decision
                            // (Create new vs Overwrite existing), even if there is a single match.
                            var cands = matches.Select(m => new { m.Id, m.Title, m.Url, m.AssetType, m.Domain }).ToList();
                            conflicts++;
                            conflictsList.Add(new { index = rowIdx, row, candidates = cands });
                            previewRows.Add(new { index = rowIdx, row, hash = Convert.ToHexString(hash), action = "conflict" });
                        }
                        else
                        {
                            toCreate++;
                            previewRows.Add(new { index = rowIdx, row, hash = Convert.ToHexString(hash), action = "create" });
                        }
                    }
                }
                catch
                {
                    errors++;
                    errorsList.Add(new { index = rowIdx, reason = "Parse error" });
                }
            }

            var job = new ImportJob
            {
                Source = source,
                DatasetKey = datasetKey,
                UploadedBy = user?.UserPrincipalName,
                FileName = form["file"].FirstOrDefault(),
                Mode = mode,
                Total = dataRows.Count - skippedBlank,
                ToCreate = toCreate,
                ToUpdate = toUpdate,
                Unchanged = unchanged,
                Conflicts = conflicts,
                Errors = errors,
                PreviewJson = JsonSerializer.Serialize(new { rows = previewRows, conflicts = conflictsList, errors = errorsList })
            };
            _db.ImportJobs.Add(job);
            await _db.SaveChangesAsync();

            return Ok(new PreviewResult
            {
                Token = job.Token,
                Total = job.Total,
                ToCreate = job.ToCreate,
                ToUpdate = job.ToUpdate,
                Unchanged = job.Unchanged,
                Conflicts = job.Conflicts,
                Errors = job.Errors,
                ConflictsList = conflictsList,
                ErrorsList = errorsList
            });
        }

        [HttpPost("commit")]
        public async Task<ActionResult<object>> Commit()
        {
            var user = CurrentUser;
            if (user?.UserType != "Admin") return Forbid();
            string token = string.Empty;
            JsonElement? decisionsPayload = null;

            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync();
                token = form["token"].FirstOrDefault() ?? string.Empty;
                var decisionsJson = form["decisions"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(decisionsJson))
                {
                    var doc = JsonDocument.Parse(decisionsJson);
                    decisionsPayload = doc.RootElement.Clone();
                }
            }
            else if (Request.ContentLength.GetValueOrDefault() > 0 &&
                     (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false))
            {
                using var sr = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
                var json = await sr.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("token", out var tEl)) token = tEl.GetString() ?? string.Empty;
                    if (doc.RootElement.TryGetProperty("decisions", out var dEl)) decisionsPayload = dEl.Clone();
                }
            }

            if (string.IsNullOrWhiteSpace(token)) return BadRequest("Missing token");

            var job = await _db.ImportJobs.FirstOrDefaultAsync(j => j.Token == token);
            if (job == null) return NotFound("Job");

            var previewDoc = JsonDocument.Parse(job.PreviewJson);
            var rows = previewDoc.RootElement.GetProperty("rows");
            var conflictsJson = previewDoc.RootElement.TryGetProperty("conflicts", out var cj) ? cj : default;
            var errorsJson = previewDoc.RootElement.TryGetProperty("errors", out var ej) ? ej : default;

            // Build decisions map
            var decisionsMap = new Dictionary<int, (string action, int? targetId)>();
            if (decisionsPayload.HasValue && decisionsPayload.Value.ValueKind == JsonValueKind.Array)
            {
                foreach (var jd in decisionsPayload.Value.EnumerateArray())
                {
                    if (jd.ValueKind != JsonValueKind.Object) continue;
                    if (jd.TryGetProperty("rowIndex", out var rpi) && jd.TryGetProperty("action", out var api))
                    {
                        int idx = rpi.GetInt32();
                        string act = api.GetString() ?? "skip";
                        int? tid = jd.TryGetProperty("targetId", out var tpi) && tpi.ValueKind == JsonValueKind.Number ? tpi.GetInt32() : (int?)null;
                        decisionsMap[idx] = (act, tid);
                    }
                }
            }

            // Lookup maps
            var luAll = await _db.LookupValues.AsNoTracking().ToListAsync();
            int? L(string type, string value)
            {
                if (string.IsNullOrEmpty(value)) return null;
                var tnorm = Normalize(type);
                var vnorm = Normalize(value);
                var id = luAll.FirstOrDefault(l => Normalize(l.Type).Equals(tnorm, StringComparison.OrdinalIgnoreCase)
                                                && Normalize(l.Value).Equals(vnorm, StringComparison.OrdinalIgnoreCase))?.Id;
                return id;
            }

            // Missing Data helpers
            var missingCache = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            async Task<int> MissingLookupIdAsync(string type)
            {
                if (missingCache.TryGetValue(type, out var id)) return id;
                var lv = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == type && l.Value == "Missing Data");
                if (lv == null)
                {
                    lv = new LookupValue { Type = type, Value = "Missing Data" };
                    _db.LookupValues.Add(lv);
                    await _db.SaveChangesAsync();
                }
                missingCache[type] = lv.Id;
                return lv.Id;
            }

            async Task<int> MissingOwnerIdAsync()
            {
                // Use existing Missing Data employee (ID 11)
                return 11;
            }

            // Person helper (Owner/Executive Sponsor): find by email; if not exists, create; if email missing, use Missing Data person
            async Task<int> ResolvePersonAsync(string personName, string personEmail)
            {
                personEmail = personEmail?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(personEmail))
                {
                    return await MissingOwnerIdAsync();
                }
                var existing = await _db.Employees.AsNoTracking().FirstOrDefaultAsync(o => o.Email == personEmail);
                if (existing != null) return existing.Id;
                var name = string.IsNullOrWhiteSpace(personName) ? personEmail : personName.Trim();
                var o = new Employee { Name = name, Email = personEmail };
                _db.Employees.Add(o);
                await _db.SaveChangesAsync();
                return o.Id;
            }

            // Process
            int created = 0, updated = 0, skipped = 0, unchanged = 0;
            var skippedList = new List<object>();

            // Disable auto detect changes for batching
            var prevDetect = _db.ChangeTracker.AutoDetectChangesEnabled;
            _db.ChangeTracker.AutoDetectChangesEnabled = false;
            try
            {
                int batch = 0;
                foreach (var el in rows.EnumerateArray())
                {
                    int idx = el.GetProperty("index").GetInt32();
                    string action = el.GetProperty("action").GetString() ?? "";
                    if (action == "error") { skipped++; skippedList.Add(new { index = idx, reason = el.TryGetProperty("reason", out var rr) ? rr.GetString() : "error" }); continue; }
                    if (action == "unchanged") { unchanged++; continue; }
                    if (action == "conflict")
                    {
                        if (!decisionsMap.TryGetValue(idx, out var dec)) { skipped++; skippedList.Add(new { index = idx, reason = "unresolved conflict" }); continue; }
                        action = dec.action;
                        if (action == "update" && !dec.targetId.HasValue) { skipped++; skippedList.Add(new { index = idx, reason = "no target for update" }); continue; }
                    }

                    var row = el.GetProperty("row");
                    // Re-validate lookups strictly
                    string at = row.GetProperty("AssetType").GetString() ?? string.Empty;
                    string dm = row.GetProperty("Domain").GetString() ?? string.Empty;
                    string dv = row.GetProperty("Division").GetString() ?? string.Empty;
                    string sl = row.GetProperty("ServiceLine").GetString() ?? string.Empty;
                    string ds = row.GetProperty("DataSource").GetString() ?? string.Empty;
                    string st = row.GetProperty("Status").GetString() ?? string.Empty;
                    string oe = row.TryGetProperty("OperatingEntity", out var oeEl) ? (oeEl.GetString() ?? string.Empty) : string.Empty;
                    string rf = row.TryGetProperty("RefreshFrequency", out var rfEl) ? (rfEl.GetString() ?? string.Empty) : string.Empty;
                    // Optional lookup extras
                    string plc = row.TryGetProperty("PotentialToConsolidate", out var plcEl) ? (plcEl.GetString() ?? string.Empty) : string.Empty;
                    string pla = row.TryGetProperty("PotentialToAutomate", out var plaEl) ? (plaEl.GetString() ?? string.Empty) : string.Empty;
                    string sbv = row.TryGetProperty("SponsorBusinessValue", out var sbvEl) ? (sbvEl.GetString() ?? string.Empty) : string.Empty;
                    // MustDo2025 removed
                    string devEff = row.TryGetProperty("DevelopmentEffort", out var deEl) ? (deEl.GetString() ?? string.Empty) : string.Empty;
                    string estHrs = row.TryGetProperty("EstimatedDevHours", out var edhEl) ? (edhEl.GetString() ?? string.Empty) : string.Empty;
                    string resDev = row.TryGetProperty("ResourcesDevelopment", out var rdevEl) ? (rdevEl.GetString() ?? string.Empty) : string.Empty;
                    string resAna = row.TryGetProperty("ResourcesAnalysts", out var ranaEl) ? (ranaEl.GetString() ?? string.Empty) : string.Empty;
                    string resPlat = row.TryGetProperty("ResourcesPlatform", out var rplEl) ? (rplEl.GetString() ?? string.Empty) : string.Empty;
                    string resDE = row.TryGetProperty("ResourcesDataEngineering", out var rdeEl) ? (rdeEl.GetString() ?? string.Empty) : string.Empty;
                    int? atId = L("AssetType", at), dmId = L("Domain", dm), dvId = L("Division", dv), dsId = L("DataSource", ds), stId = L("Status", st);
                    int? oeId = L("OperatingEntity", oe), rfId = L("RefreshFrequency", rf);
                    if (oeId == null && !string.IsNullOrWhiteSpace(sl))
                    {
                        oeId = await EnsureLookupAsync("OperatingEntity", sl, null);
                    }
                    int? plcId = L("PotentialToConsolidate", plc), plaId = L("PotentialToAutomate", pla), sbvId = L("SponsorBusinessValue", sbv);
                    int? devEffId = L("DevelopmentEffort", devEff), estHrsId = L("EstimatedDevHours", estHrs), resDevId = L("ResourcesDevelopment", resDev), resAnaId = L("ResourcesAnalysts", resAna), resPlatId = L("ResourcesPlatform", resPlat), resDEId = L("ResourcesDataEngineering", resDE);
                    // Create optional lookup values on-the-fly when not found but provided
                    async Task<int?> EnsureLookupAsync(string type, string value, int? id)
                    {
                        if (!string.IsNullOrWhiteSpace(value) && id == null)
                        {
                            var lv = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == type && l.Value == value);
                            if (lv == null)
                            {
                                lv = new LookupValue { Type = type, Value = value };
                                _db.LookupValues.Add(lv);
                                await _db.SaveChangesAsync();
                            }
                            return lv.Id;
                        }
                        return id;
                    }
                    plcId = await EnsureLookupAsync("PotentialToConsolidate", plc, plcId);
                    plaId = await EnsureLookupAsync("PotentialToAutomate", pla, plaId);
                    sbvId = await EnsureLookupAsync("SponsorBusinessValue", sbv, sbvId);
                    // MustDo2025 removed
                    devEffId = await EnsureLookupAsync("DevelopmentEffort", devEff, devEffId);
                    estHrsId = await EnsureLookupAsync("EstimatedDevHours", estHrs, estHrsId);
                    resDevId = await EnsureLookupAsync("ResourcesDevelopment", resDev, resDevId);
                    resAnaId = await EnsureLookupAsync("ResourcesAnalysts", resAna, resAnaId);
                    resPlatId = await EnsureLookupAsync("ResourcesPlatform", resPlat, resPlatId);
                    resDEId = await EnsureLookupAsync("ResourcesDataEngineering", resDE, resDEId);
                    if ((atId == null && !string.IsNullOrEmpty(at)) || (dmId == null && !string.IsNullOrEmpty(dm)) ||
                        (dvId == null && !string.IsNullOrEmpty(dv)) ||
                        (dsId == null && !string.IsNullOrEmpty(ds)) || (stId == null && !string.IsNullOrEmpty(st)) ||
                        (oeId == null && !string.IsNullOrEmpty(oe)) || (rfId == null && !string.IsNullOrEmpty(rf)))
                    {
                        skipped++; skippedList.Add(new { index = idx, reason = "unknown lookup value" }); continue;
                    }

                    // Default required lookups to 'Missing Data' when not provided
                    if (string.IsNullOrEmpty(at)) atId = await MissingLookupIdAsync("AssetType");
                    if (string.IsNullOrEmpty(dm)) dmId = await MissingLookupIdAsync("Domain");
                    if (string.IsNullOrEmpty(dv)) dvId = await MissingLookupIdAsync("Division");
                    // ServiceLine removed
                    if (string.IsNullOrEmpty(ds)) dsId = await MissingLookupIdAsync("DataSource");
                    if (string.IsNullOrEmpty(st)) stId = await MissingLookupIdAsync("Status");
                    if (string.IsNullOrEmpty(oe)) oeId = await MissingLookupIdAsync("OperatingEntity");
                    if (string.IsNullOrEmpty(rf)) rfId = await MissingLookupIdAsync("RefreshFrequency");

                    string title = row.GetProperty("Title").GetString() ?? string.Empty;
                    string desc = row.GetProperty("Description").GetString() ?? string.Empty;
                    string url = row.GetProperty("Url").GetString() ?? string.Empty;
                    bool phi = (row.GetProperty("PrivacyPhi").GetString() ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                    bool pii = (row.TryGetProperty("PrivacyPii", out var ppi) ? (ppi.GetString() ?? "false") : "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                    bool hasRls = (row.TryGetProperty("HasRls", out var hr) ? (hr.GetString() ?? "false") : "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                    bool featured = (row.GetProperty("Featured").GetString() ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                    // DateAdded provided by server at create time; not read from import
                    DateTime dateAdded = DateTime.UtcNow;
                    DateTime? lastModified = null;
                    var lmStr = row.TryGetProperty("LastModifiedDate", out var lmd) ? (lmd.GetString() ?? string.Empty) : string.Empty;
                    if (!string.IsNullOrEmpty(lmStr))
                    {
                        if (DateTime.TryParse(lmStr, out var lm)) lastModified = lm;
                    }
                    string ownerName = row.GetProperty("OwnerName").GetString() ?? string.Empty;
                    string ownerEmail = row.GetProperty("OwnerEmail").GetString() ?? string.Empty;
                    string execName = row.TryGetProperty("ExecutiveSponsorName", out var exn) ? (exn.GetString() ?? string.Empty) : string.Empty;
                    string execEmail = row.TryGetProperty("ExecutiveSponsorEmail", out var exe) ? (exe.GetString() ?? string.Empty) : string.Empty;
                    string tagsStr = row.GetProperty("Tags").GetString() ?? string.Empty;
                    string dataConsumersStr = row.TryGetProperty("DataConsumers", out var dcs) ? (dcs.GetString() ?? string.Empty) : string.Empty;
                    string dependencies = row.TryGetProperty("Dependencies", out var dep) ? (dep.GetString() ?? string.Empty) : string.Empty;
                    string defaultAdGroups = row.TryGetProperty("DefaultAdGroupNames", out var dag) ? (dag.GetString() ?? string.Empty) : string.Empty;
                    // Free-text extras
                    string productGroup = row.TryGetProperty("ProductGroup", out var pg) ? (pg.GetString() ?? string.Empty) : string.Empty;
                    string productStatusNotes = row.TryGetProperty("ProductStatusNotes", out var psn) ? (psn.GetString() ?? string.Empty) : string.Empty;
                    string dataConsumersText = row.TryGetProperty("DataConsumers", out var dct) ? (dct.GetString() ?? string.Empty) : string.Empty;
                    string techDeliveryManager = row.TryGetProperty("TechDeliveryManager", out var tdm) ? (tdm.GetString() ?? string.Empty) : string.Empty;
                    string regulatoryComplianceContractual = row.TryGetProperty("RegulatoryComplianceContractual", out var rcc) ? (rcc.GetString() ?? string.Empty) : string.Empty;
                    string biPlatform = row.TryGetProperty("BiPlatform", out var bip) ? (bip.GetString() ?? string.Empty) : string.Empty;
                    string dbServer = row.TryGetProperty("DbServer", out var dbs) ? (dbs.GetString() ?? string.Empty) : string.Empty;
                    string dbDataMart = row.TryGetProperty("DbDataMart", out var dbm) ? (dbm.GetString() ?? string.Empty) : string.Empty;
                    string databaseTable = row.TryGetProperty("DatabaseTable", out var dbt) ? (dbt.GetString() ?? string.Empty) : string.Empty;
                    string sourceRep = row.TryGetProperty("SourceRep", out var srp) ? (srp.GetString() ?? string.Empty) : string.Empty;
                    string dataSecurityClassification = row.TryGetProperty("DataSecurityClassification", out var dsc) ? (dsc.GetString() ?? string.Empty) : string.Empty;
                    string accessGroupName = row.TryGetProperty("AccessGroupName", out var agn) ? (agn.GetString() ?? string.Empty) : string.Empty;
                    string accessGroupDn = row.TryGetProperty("AccessGroupDn", out var agd) ? (agd.GetString() ?? string.Empty) : string.Empty;
                    string automationClassification = row.TryGetProperty("AutomationClassification", out var ac) ? (ac.GetString() ?? string.Empty) : string.Empty;
                    string userVisibilityString = row.TryGetProperty("UserVisibilityString", out var uvs) ? (uvs.GetString() ?? string.Empty) : string.Empty;
                    string userVisibilityNumber = row.TryGetProperty("UserVisibilityNumber", out var uvn) ? (uvn.GetString() ?? string.Empty) : string.Empty;
                    string epicSecurityGroupTag = row.TryGetProperty("EpicSecurityGroupTag", out var esg) ? (esg.GetString() ?? string.Empty) : string.Empty;
                    string keepLongTerm = row.TryGetProperty("KeepLongTerm", out var klt) ? (klt.GetString() ?? string.Empty) : string.Empty;

                    int ownerId = await ResolvePersonAsync(ownerName, ownerEmail);
                    int execId = await ResolvePersonAsync(execName, execEmail);

                    // Compute hash
                    var hash = ComputeHashFromFields(
                        title,
                        desc,
                        url,
                        at,
                        dm,
                        dv,
                        sl,
                        ds,
                        st,
                        ownerName,
                        ownerEmail,
                        phi ? "true" : "false",
                        dateAdded.ToString("yyyy-MM-dd"),
                        featured ? "true" : "false",
                        tagsStr
                    );

                    if (action == "update")
                    {
                        int targetId = el.TryGetProperty("targetId", out var tidEl) ? tidEl.GetInt32() : (decisionsMap.TryGetValue(idx, out var dec) && dec.targetId.HasValue ? dec.targetId.Value : 0);
                        if (targetId <= 0) { skipped++; skippedList.Add(new { index = idx, reason = "invalid update target" }); continue; }
                        var item = await _db.Items.FirstOrDefaultAsync(i => i.Id == targetId);
                        if (item == null) { skipped++; skippedList.Add(new { index = idx, reason = "target not found" }); continue; }

                        // Update fields
                        item.Title = title;
                        item.Description = desc;
                        item.Url = url;
                        item.AssetTypeId = atId;
                        item.DomainId = dmId;
                        item.DivisionId = dvId;
                        // Map ServiceLine value to Operating Entity when OperatingEntity not provided
                        if (oeId == null && !string.IsNullOrWhiteSpace(sl))
                        {
                            oeId = await EnsureLookupAsync("OperatingEntity", sl, null);
                        }
                        item.DataSourceId = dsId;
                        item.StatusId = stId;
                        item.OwnerId = ownerId;
                        item.ExecutiveSponsorId = execId;
                        item.OperatingEntityId = oeId;
                        item.RefreshFrequencyId = rfId;
                        item.PrivacyPhi = phi;
                        item.PrivacyPii = pii;
                        item.HasRls = hasRls;
                        item.LastModifiedDate = lastModified;
                        item.Dependencies = NullIfEmpty(dependencies);
                        item.DefaultAdGroupNames = NullIfEmpty(defaultAdGroups);
                        item.ProductGroup = NullIfEmpty(productGroup);
                        item.ProductStatusNotes = NullIfEmpty(productStatusNotes);
                        item.DataConsumers = NullIfEmpty(dataConsumersText);
                        item.TechDeliveryManager = NullIfEmpty(techDeliveryManager);
                        item.RegulatoryComplianceContractual = NullIfEmpty(regulatoryComplianceContractual);
                        item.BiPlatform = NullIfEmpty(biPlatform);
                        item.DbServer = NullIfEmpty(dbServer);
                        item.DbDataMart = NullIfEmpty(dbDataMart);
                        item.DatabaseTable = NullIfEmpty(databaseTable);
                        item.SourceRep = NullIfEmpty(sourceRep);
                        item.DataSecurityClassification = NullIfEmpty(dataSecurityClassification);
                        item.AccessGroupName = NullIfEmpty(accessGroupName);
                        item.AccessGroupDn = NullIfEmpty(accessGroupDn);
                        item.AutomationClassification = NullIfEmpty(automationClassification);
                        item.UserVisibilityString = NullIfEmpty(userVisibilityString);
                        item.UserVisibilityNumber = NullIfEmpty(userVisibilityNumber);
                        item.EpicSecurityGroupTag = NullIfEmpty(epicSecurityGroupTag);
                        item.KeepLongTerm = NullIfEmpty(keepLongTerm);
                        // Optional lookups
                        item.PotentialToConsolidateId = plcId;
                        item.PotentialToAutomateId = plaId;
                        item.SponsorBusinessValueId = sbvId;
                        // MustDo2025 removed
                        item.DevelopmentEffortId = devEffId;
                        item.EstimatedDevHoursId = estHrsId;
                        item.ResourcesDevelopmentId = resDevId;
                        item.ResourcesAnalystsId = resAnaId;
                        item.ResourcesPlatformId = resPlatId;
                        item.ResourcesDataEngineeringId = resDEId;
                        // Keep existing DateAdded on updates
                        item.Featured = featured;
                        item.ContentHash = hash;
                        item.UpdatedAt = DateTime.UtcNow;
                        item.UpdatedBy = user.UserPrincipalName;

                        // Replace tags
                        await _db.ItemTags.Where(it => it.ItemId == item.Id).ExecuteDeleteAsync();
                        if (!string.IsNullOrEmpty(tagsStr))
                        {
                            foreach (var tagVal in tagsStr.Split(';').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)))
                            {
                                var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Value == tagVal);
                                if (tag == null) { tag = new Tag { Value = tagVal }; _db.Tags.Add(tag); await _db.SaveChangesAsync(); }
                                _db.ItemTags.Add(new ItemTag { ItemId = item.Id, TagId = tag.Id });
                            }
                        }
                        updated++; batch++;
                    }
                    else if (action == "create")
                    {
                        var item = new Item
                        {
                            Title = title,
                            Description = desc,
                            Url = url,
                            AssetTypeId = atId,
                            DomainId = dmId,
                            DivisionId = dvId,
                            DataSourceId = dsId,
                            StatusId = stId,
                            OwnerId = ownerId,
                            ExecutiveSponsorId = execId,
                            OperatingEntityId = oeId,
                            RefreshFrequencyId = rfId,
                            PrivacyPhi = phi,
                            PrivacyPii = pii,
                            HasRls = hasRls,
                            LastModifiedDate = lastModified,
                            Dependencies = NullIfEmpty(dependencies),
                            DefaultAdGroupNames = NullIfEmpty(defaultAdGroups),
                            ProductGroup = NullIfEmpty(productGroup),
                            ProductStatusNotes = NullIfEmpty(productStatusNotes),
                            DataConsumers = NullIfEmpty(dataConsumersText),
                            TechDeliveryManager = NullIfEmpty(techDeliveryManager),
                            RegulatoryComplianceContractual = NullIfEmpty(regulatoryComplianceContractual),
                            BiPlatform = NullIfEmpty(biPlatform),
                            DbServer = NullIfEmpty(dbServer),
                            DbDataMart = NullIfEmpty(dbDataMart),
                            DatabaseTable = NullIfEmpty(databaseTable),
                            SourceRep = NullIfEmpty(sourceRep),
                            DataSecurityClassification = NullIfEmpty(dataSecurityClassification),
                            AccessGroupName = NullIfEmpty(accessGroupName),
                            AccessGroupDn = NullIfEmpty(accessGroupDn),
                            AutomationClassification = NullIfEmpty(automationClassification),
                            UserVisibilityString = NullIfEmpty(userVisibilityString),
                            UserVisibilityNumber = NullIfEmpty(userVisibilityNumber),
                            EpicSecurityGroupTag = NullIfEmpty(epicSecurityGroupTag),
                            KeepLongTerm = NullIfEmpty(keepLongTerm),
                            PotentialToConsolidateId = plcId,
                            PotentialToAutomateId = plaId,
                            SponsorBusinessValueId = sbvId,
                            // MustDo2025 removed
                            DevelopmentEffortId = devEffId,
                            EstimatedDevHoursId = estHrsId,
                            ResourcesDevelopmentId = resDevId,
                            ResourcesAnalystsId = resAnaId,
                            ResourcesPlatformId = resPlatId,
                            ResourcesDataEngineeringId = resDEId,
                            DateAdded = DateTime.UtcNow,
                            Featured = featured,
                            ContentHash = hash,
                            UpdatedAt = DateTime.UtcNow,
                            UpdatedBy = user.UserPrincipalName
                        };
                        _db.Items.Add(item);
                        await _db.SaveChangesAsync();
                        if (!string.IsNullOrEmpty(tagsStr))
                        {
                            foreach (var tagVal in tagsStr.Split(';').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)))
                            {
                                var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Value == tagVal);
                                if (tag == null) { tag = new Tag { Value = tagVal }; _db.Tags.Add(tag); await _db.SaveChangesAsync(); }
                                _db.ItemTags.Add(new ItemTag { ItemId = item.Id, TagId = tag.Id });
                            }
                        }
                        // Data Consumers stored as free-text (semicolon-separated)
                        created++; batch++;
                    }
                    else
                    {
                        skipped++; skippedList.Add(new { index = idx, reason = "skipped" });
                    }

                    if (batch >= 300)
                    {
                        await _db.SaveChangesAsync();
                        batch = 0;
                    }
                }

                if (_db.ChangeTracker.HasChanges())
                {
                    await _db.SaveChangesAsync();
                }
            }
            finally
            {
                _db.ChangeTracker.AutoDetectChangesEnabled = prevDetect;
            }

            job.Status = "completed";
            await _db.SaveChangesAsync();

            return Ok(new
            {
                summary = new { created, updated, unchanged, skipped },
                notImported = skippedList
            });
        }

        // Trigger a full search index rebuild on the external Search API
        [HttpPost("rebuild-index")]
        public async Task<IActionResult> RebuildIndex()
        {
            try
            {
                var client = _httpFactory.CreateClient("SearchApi");
                // Rebuild can take longer; override default timeout
                client.Timeout = TimeSpan.FromMinutes(2);
                var res = await client.PostAsync("rebuild-index", content: null);
                if (!res.IsSuccessStatusCode)
                {
                    return StatusCode((int)res.StatusCode, await res.Content.ReadAsStringAsync());
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Rebuild failed: {ex.Message}");
            }
        }
    }
}
