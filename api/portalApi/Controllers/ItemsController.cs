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
     [FromQuery] bool? phi)
        {
            var user = CurrentUser;
            var query = _db.Items.AsQueryable();

            // Filtering only (no search query)
            if (!string.IsNullOrWhiteSpace(domain))
                query = query.Where(i => i.Domain == domain);
            if (!string.IsNullOrWhiteSpace(division))
                query = query.Where(i => i.Division == division);
            if (!string.IsNullOrWhiteSpace(serviceLine))
                query = query.Where(i => i.ServiceLine == serviceLine);
            if (!string.IsNullOrWhiteSpace(dataSource))
                query = query.Where(i => i.DataSource == dataSource);
            if (!string.IsNullOrWhiteSpace(assetType))
                query = query.Where(i => i.AssetTypesCsv != null && i.AssetTypesCsv.Contains(assetType));
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
                AssetTypes = i.AssetTypes,
                Tags = i.Tags,
                Domain = i.Domain,
                Division = i.Division,
                ServiceLine = i.ServiceLine,
                DataSource = i.DataSource,
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
            var i = await _db.Items.FindAsync(id);
            if (i == null) return NotFound();
            // record that the current user opened this asset
            try
            {
                var user = CurrentUser;
                if (user != null)
                {
                    _db.UserAssetOpenHistories.Add(new UserAssetOpenHistory
                    {
                        UserId = user.Id,
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
                AssetTypes = i.AssetTypes,
                Tags = i.Tags,
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
        public async Task<IActionResult> Create([FromForm] CreateItemDto dto)
        {
            var i = new Item
            {
                Title = dto.Title,
                Description = dto.Description,
                Url = dto.Url,
                AssetTypes = dto.AssetTypes,
                Tags = dto.Tags,
                Domain = dto.Domain,
                Division = dto.Division,
                ServiceLine = dto.ServiceLine,
                DataSource = dto.DataSource,
                PrivacyPhi = dto.PrivacyPhi,
                DateAdded = DateTime.UtcNow
            };

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
            i.AssetTypes = dto.AssetTypes;
            i.Tags = dto.Tags;
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
