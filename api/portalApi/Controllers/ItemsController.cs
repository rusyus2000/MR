using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public ItemsController(AppDbContext db) => _db = db;

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
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll(
     [FromQuery] int? top,
     [FromQuery] string? domain,
     [FromQuery] string? division,
     [FromQuery] string? serviceLine,
     [FromQuery] string? dataSource,
     [FromQuery] string? assetType,
     [FromQuery] int? assetTypeId,
     // ID-based filters (comma-separated ids)
     [FromQuery] string? domainIds,
     [FromQuery] string? divisionIds,
     [FromQuery] string? serviceLineIds,
     [FromQuery] string? dataSourceIds,
     [FromQuery] string? assetTypeIds,
     [FromQuery] bool? phi)
        {
            var user = CurrentUser;
            var query = _db.Items
                .Include(i => i.ItemTags)
                    .ThenInclude(it => it.Tag)
                .Include(i => i.AssetType)
                .Include(i => i.DomainLookup)
                .Include(i => i.DivisionLookup)
                .Include(i => i.ServiceLineLookup)
                .Include(i => i.DataSourceLookup)
                .Include(i => i.Owner)
                .Include(i => i.StatusLookup)
                .AsQueryable();

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

            var serviceLineIdList = ParseIds(serviceLineIds);
            if (serviceLineIdList.Any())
                query = query.Where(i => i.ServiceLineId.HasValue && serviceLineIdList.Contains(i.ServiceLineId.Value));

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
            if (!string.IsNullOrWhiteSpace(serviceLine))
            {
                var lv = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "ServiceLine" && l.Value == serviceLine);
                if (lv != null) query = query.Where(i => i.ServiceLineId == lv.Id);
                else return Ok(new List<ItemDto>());
            }
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

            var list = await query.Select(i => new ItemDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Url = i.Url,
                Tags = i.ItemTags.Select(it => it.Tag.Value).ToList(),
                AssetTypeId = i.AssetTypeId,
                AssetTypeName = i.AssetType != null ? i.AssetType.Value : null,
                Featured = i.Featured,
                Domain = i.DomainLookup != null ? i.DomainLookup.Value : null,
                Division = i.DivisionLookup != null ? i.DivisionLookup.Value : null,
                ServiceLine = i.ServiceLineLookup != null ? i.ServiceLineLookup.Value : null,
                DataSource = i.DataSourceLookup != null ? i.DataSourceLookup.Value : null,
                DomainId = i.DomainId,
                DivisionId = i.DivisionId,
                ServiceLineId = i.ServiceLineId,
                DataSourceId = i.DataSourceId,
                StatusId = i.StatusId,
                Status = i.StatusLookup != null ? i.StatusLookup.Value : null,
                OwnerId = i.OwnerId,
                OwnerName = i.Owner != null ? i.Owner.Name : null,
                OwnerEmail = i.Owner != null ? i.Owner.Email : null,
                PrivacyPhi = i.PrivacyPhi,
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
                .Include(it => it.ServiceLineLookup)
                .Include(it => it.DataSourceLookup)
                .Include(it => it.Owner)
                .Include(it => it.StatusLookup)
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

            return Ok(new ItemDto
            {
                Id = i.Id,
                Title = i.Title,
                Description = i.Description,
                Url = i.Url,
                Tags = i.ItemTags.Select(it => it.Tag.Value).ToList(),
                AssetTypeId = i.AssetTypeId,
                AssetTypeName = i.AssetType != null ? i.AssetType.Value : null,
                Domain = i.DomainLookup != null ? i.DomainLookup.Value : null,
                Division = i.DivisionLookup != null ? i.DivisionLookup.Value : null,
                ServiceLine = i.ServiceLineLookup != null ? i.ServiceLineLookup.Value : null,
                DataSource = i.DataSourceLookup != null ? i.DataSourceLookup.Value : null,
                StatusId = i.StatusId,
                Status = i.StatusLookup != null ? i.StatusLookup.Value : null,
                OwnerId = i.OwnerId,
                OwnerName = i.Owner != null ? i.Owner.Name : null,
                OwnerEmail = i.Owner != null ? i.Owner.Email : null,
                PrivacyPhi = i.PrivacyPhi,
                DateAdded = i.DateAdded,
                Featured = i.Featured
            });
        }

        // POST /api/items
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateItemDto dto)
        {
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
                ServiceLineId = null,
                DataSourceId = null,
                PrivacyPhi = dto.PrivacyPhi,
                DateAdded = DateTime.UtcNow,
                Featured = dto.Featured
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

            if (dto.ServiceLineId.HasValue)
            {
                var lookup = await _db.LookupValues.FindAsync(dto.ServiceLineId.Value);
                if (lookup != null) { i.ServiceLineId = lookup.Id; }
            }
            else if (!string.IsNullOrWhiteSpace(dto.ServiceLine))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "ServiceLine" && l.Value == dto.ServiceLine);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "ServiceLine", Value = dto.ServiceLine };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.ServiceLineId = lookup.Id;
            }

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

            // Call FastAPI /rebuild-index endpoint
            using (var http = new HttpClient()) 
                {
                // Replace with your FastAPI base URL as appropriate!
                var apiBaseUrl = "http://localhost:8000"; // e.g., if running locally on port 8000
                var response = await http.PostAsync($"{apiBaseUrl}/rebuild-index", null);
                // Optionally: check response.IsSuccessStatusCode and handle errors/logging
            }

            return Ok(new { id = i.Id });
        }

        // PUT /api/items/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateItemDto dto)
        {
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
            // Map domain/division/serviceLine/dataSource to lookup ids when provided
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

            if (dto.ServiceLineId.HasValue)
                i.ServiceLineId = dto.ServiceLineId;
            else if (!string.IsNullOrWhiteSpace(dto.ServiceLine))
            {
                var lookup = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "ServiceLine" && l.Value == dto.ServiceLine);
                if (lookup == null)
                {
                    lookup = new LookupValue { Type = "ServiceLine", Value = dto.ServiceLine };
                    _db.LookupValues.Add(lookup);
                    await _db.SaveChangesAsync();
                }
                i.ServiceLineId = lookup.Id;
            }

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

            // Status update (admin only)
            var currentUser = CurrentUser;
            if (currentUser?.UserType == "Admin" && dto.StatusId.HasValue)
            {
                var st = await _db.LookupValues.FindAsync(dto.StatusId.Value);
                if (st != null) i.StatusId = st.Id;
            }
            i.PrivacyPhi = dto.PrivacyPhi;
            i.Featured = dto.Featured;
            // keep original DateAdded

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/items/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var i = await _db.Items.FindAsync(id);
            if (i == null) return NotFound();
            _db.Items.Remove(i);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
