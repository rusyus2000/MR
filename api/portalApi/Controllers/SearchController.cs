using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.DTOs;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly AppDbContext _db;
        public SearchController(AppDbContext db) => _db = db;

        // GET /api/search?q=term
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query parameter 'q' is required.");

            var results = await _db.Items
                .Where(i =>
                    EF.Functions.Like(i.Title, $"%{q}%") ||
                    EF.Functions.Like(i.Description, $"%{q}%"))
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

            return Ok(results);
        }
    }
}
