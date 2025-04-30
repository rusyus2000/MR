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

        // GET /api/items
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> GetAll(
            [FromQuery] string? domain,
            [FromQuery] string? division,
            [FromQuery] string? serviceLine,
            [FromQuery] string? dataSource,
            [FromQuery] string? assetType,
            [FromQuery] bool? phi,
            [FromQuery] string? q)
        {
            var query = _db.Items.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(i =>
                    EF.Functions.Like(i.Title, $"%{q}%") ||
                    EF.Functions.Like(i.Description, $"%{q}%"));
            }
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
                    PrivacyPhi = i.PrivacyPhi
                })
                .ToListAsync();

            return Ok(items);
        }

        // GET /api/items/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ItemDto>> GetById(int id)
        {
            var i = await _db.Items.FindAsync(id);
            if (i == null) return NotFound();

            return Ok(new ItemDto
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
                PrivacyPhi = i.PrivacyPhi
            });
        }

        // POST /api/items
        [HttpPost]
        public async Task<ActionResult<ItemDto>> Create(CreateItemDto dto)
        {
            var i = new Item
            {
                Title = dto.Title,
                Description = dto.Description,
                Url = dto.Url,
                AssetTypes = dto.AssetTypes,
                Domain = dto.Domain,
                Division = dto.Division,
                ServiceLine = dto.ServiceLine,
                DataSource = dto.DataSource,
                PrivacyPhi = dto.PrivacyPhi
            };
            _db.Items.Add(i);
            await _db.SaveChangesAsync();

            var result = new ItemDto
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
                PrivacyPhi = i.PrivacyPhi
            };
            return CreatedAtAction(nameof(GetById), new { id = i.Id }, result);
        }

        // PUT /api/items/{id}
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Update(int id, CreateItemDto dto)
        {
            var i = await _db.Items.FindAsync(id);
            if (i == null) return NotFound();

            i.Title = dto.Title;
            i.Description = dto.Description;
            i.Url = dto.Url;
            i.AssetTypes = dto.AssetTypes;
            i.Domain = dto.Domain;
            i.Division = dto.Division;
            i.ServiceLine = dto.ServiceLine;
            i.DataSource = dto.DataSource;
            i.PrivacyPhi = dto.PrivacyPhi;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/items/{id}
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var i = await _db.Items.FindAsync(id);
            if (i == null) return NotFound();
            _db.Items.Remove(i);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
