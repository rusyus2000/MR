using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.DTOs;
using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public ItemsController(AppDbContext db) => _db = db;

        // GET /api/items?top=10&domain=...
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll(
            [FromQuery] int? top,
            [FromQuery] string? domain,
            [FromQuery] string? division,
            [FromQuery] string? serviceLine,
            [FromQuery] string? dataSource,
            [FromQuery] string? assetType,
            [FromQuery] bool? phi,
            [FromQuery] string? q)
        {
            var query = _db.Items.AsQueryable();

            // full-text
            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(i =>
                    EF.Functions.Like(i.Title, $"%{q}%") ||
                    EF.Functions.Like(i.Description, $"%{q}%"));
            }
            // facets
            if (!string.IsNullOrWhiteSpace(domain))
                query = query.Where(i => i.Domain == domain);
            if (!string.IsNullOrWhiteSpace(division))
                query = query.Where(i => i.Division == division);
            if (!string.IsNullOrWhiteSpace(serviceLine))
                query = query.Where(i => i.ServiceLine == serviceLine);
            if (!string.IsNullOrWhiteSpace(dataSource))
                query = query.Where(i => i.DataSource == dataSource);
            if (!string.IsNullOrWhiteSpace(assetType))
                query = query.Where(i => i.AssetTypesCsv.Contains(assetType));
            if (phi.HasValue)
                query = query.Where(i => i.PrivacyPhi == phi.Value);

            // ordering & limiting for landing page
            if (top.HasValue)
            {
                query = query.OrderByDescending(i => i.DateAdded)
                             .Take(top.Value);
            }

            var items = await query
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Title = i.Title,
                    Description = i.Description,
                    Url = i.Url,
                    AssetTypes = i.AssetTypes,
                    Domain = i.Domain,
                    Division = i.Division,
                    ServiceLine = i.ServiceLine,
                    DataSource = i.DataSource,
                    PrivacyPhi = i.PrivacyPhi,
                    DateAdded = i.DateAdded
                })
                .ToListAsync();

            return Ok(items);
        }

        // ... other actions unchanged ...
    }
}
