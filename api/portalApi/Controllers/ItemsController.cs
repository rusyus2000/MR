using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        // GET /api/items?…&q=…
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

            if (!string.IsNullOrWhiteSpace(q))
            {
                var pattern = $"%{q}%";
                query = query.Where(i =>
                       EF.Functions.Like(i.Title, pattern)
                    || EF.Functions.Like(i.Description, pattern)
                    || EF.Functions.Like(i.Url, pattern)
                    || EF.Functions.Like(i.Domain, pattern)
                    || EF.Functions.Like(i.Division, pattern)
                    || EF.Functions.Like(i.ServiceLine, pattern)
                    || EF.Functions.Like(i.DataSource, pattern)
                );
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

            if (top.HasValue)
                query = query.OrderByDescending(i => i.DateAdded).Take(top.Value);

            var list = await query.Select(i => new ItemDto
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
            }).ToListAsync();

            return Ok(list);
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
                PrivacyPhi = i.PrivacyPhi,
                DateAdded = i.DateAdded
            });
        }

        // POST /api/items
        [HttpPost]
        public async Task<ActionResult<ItemDto>> Create([FromBody] CreateItemDto dto)
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
                PrivacyPhi = dto.PrivacyPhi,
                DateAdded = DateTime.UtcNow
            };
            _db.Items.Add(i);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = i.Id }, new ItemDto
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
            });
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
            i.AssetTypes = dto.AssetTypes;
            i.Domain = dto.Domain;
            i.Division = dto.Division;
            i.ServiceLine = dto.ServiceLine;
            i.DataSource = dto.DataSource;
            i.PrivacyPhi = dto.PrivacyPhi;
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
