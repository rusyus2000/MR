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
            int cPhi = idx("PHI");
            int cDateAdded = idx("Date Added");
            int cFeatured = idx("Featured");
            int cTags = idx("Tags");

            if (cTitle < 0 || cUrl < 0)
                return BadRequest("Missing required columns");

            var dataRows = rows.Skip(1).Where(r => r.Length > 1).ToList();

            // Prefetch existing items minimal data
            var existing = await _db.Items
                .AsNoTracking()
                .Include(i => i.AssetType)
                .Include(i => i.DomainLookup)
                .Include(i => i.DivisionLookup)
                .Include(i => i.ServiceLineLookup)
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
                    ServiceLine = i.ServiceLineLookup != null ? i.ServiceLineLookup.Value : string.Empty,
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

            // Prefetch lookup values for strict validation (case-insensitive)
            var lookups = await _db.LookupValues.AsNoTracking().ToListAsync();
            var luByType = lookups
                .GroupBy(l => l.Type, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Value).ToHashSet(StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);

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
                    var rawPhi = cPhi < r.Length ? r[cPhi] : null;
                    var rawDateAdded = cDateAdded < r.Length ? r[cDateAdded] : null;
                    var rawFeatured = cFeatured < r.Length ? r[cFeatured] : null;
                    var rawTags = cTags < r.Length ? r[cTags] : null;

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
                        PrivacyPhi = Normalize(rawPhi).Equals("true", StringComparison.OrdinalIgnoreCase) ? "true" : "false",
                        DateAdded = Normalize(rawDateAdded),
                        Featured = Normalize(rawFeatured).Equals("true", StringComparison.OrdinalIgnoreCase) ? "true" : "false",
                        Tags = NormalizeTags(rawTags)
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
                                 && string.IsNullOrEmpty(row.DateAdded)
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
                    else if (!ok("ServiceLine", row.ServiceLine)) missing = "ServiceLine";
                    else if (!ok("DataSource", row.DataSource)) missing = "DataSource";
                    else if (!ok("Status", row.Status)) missing = "Status";
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
                        row.ServiceLine,
                        row.DataSource,
                        row.Status,
                        row.OwnerName,
                        row.OwnerEmail,
                        row.PrivacyPhi,
                        row.DateAdded,
                        row.Featured,
                        row.Tags
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
                                Normalize(exById.ServiceLine),
                                Normalize(exById.DataSource),
                                Normalize(exById.Status),
                                Normalize(exById.OwnerName),
                                Normalize(exById.OwnerEmail),
                                exById.PrivacyPhi ? "true" : "false",
                                exById.DateAdded.ToString("yyyy-MM-dd"),
                                exById.Featured ? "true" : "false",
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
                var id = luAll.FirstOrDefault(l => l.Type.Equals(type, StringComparison.OrdinalIgnoreCase)
                                                && l.Value.Equals(value, StringComparison.OrdinalIgnoreCase))?.Id;
                return id;
            }

            // Owner helper
            async Task<int?> ResolveOwnerAsync(string ownerName, string ownerEmail)
            {
                ownerEmail = ownerEmail?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(ownerEmail)) return null;
                var existing = await _db.Owners.AsNoTracking().FirstOrDefaultAsync(o => o.Email == ownerEmail);
                if (existing != null) return existing.Id;
                if (string.IsNullOrEmpty(ownerName)) return null;
                var o = new Owner { Name = ownerName.Trim(), Email = ownerEmail };
                _db.Owners.Add(o);
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
                    int? atId = L("AssetType", at), dmId = L("Domain", dm), dvId = L("Division", dv), slId = L("ServiceLine", sl), dsId = L("DataSource", ds), stId = L("Status", st);
                    if ((atId == null && !string.IsNullOrEmpty(at)) || (dmId == null && !string.IsNullOrEmpty(dm)) ||
                        (dvId == null && !string.IsNullOrEmpty(dv)) || (slId == null && !string.IsNullOrEmpty(sl)) ||
                        (dsId == null && !string.IsNullOrEmpty(ds)) || (stId == null && !string.IsNullOrEmpty(st)))
                    {
                        skipped++; skippedList.Add(new { index = idx, reason = "unknown lookup value" }); continue;
                    }

                    string title = row.GetProperty("Title").GetString() ?? string.Empty;
                    string desc = row.GetProperty("Description").GetString() ?? string.Empty;
                    string url = row.GetProperty("Url").GetString() ?? string.Empty;
                    bool phi = (row.GetProperty("PrivacyPhi").GetString() ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                    bool featured = (row.GetProperty("Featured").GetString() ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                    string dateStr = row.GetProperty("DateAdded").GetString() ?? string.Empty;
                    DateTime dateAdded = DateTime.UtcNow;
                    if (!string.IsNullOrEmpty(dateStr)) DateTime.TryParseExact(dateStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out dateAdded);
                    string ownerName = row.GetProperty("OwnerName").GetString() ?? string.Empty;
                    string ownerEmail = row.GetProperty("OwnerEmail").GetString() ?? string.Empty;
                    string tagsStr = row.GetProperty("Tags").GetString() ?? string.Empty;

                    int? ownerId = await ResolveOwnerAsync(ownerName, ownerEmail);

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
                        item.ServiceLineId = slId;
                        item.DataSourceId = dsId;
                        item.StatusId = stId;
                        item.OwnerId = ownerId;
                        item.PrivacyPhi = phi;
                        item.DateAdded = dateAdded;
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
                            ServiceLineId = slId,
                            DataSourceId = dsId,
                            StatusId = stId,
                            OwnerId = ownerId,
                            PrivacyPhi = phi,
                            DateAdded = dateAdded,
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
