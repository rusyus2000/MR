using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.DTOs;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/lookups")]
    public class LookupsController : ControllerBase
    {
        private readonly AppDbContext _db;
        public LookupsController(AppDbContext db) => _db = db;

        // GET /api/lookups/{type}
        [HttpGet("{type}")]
        public async Task<ActionResult<IEnumerable<LookupDto>>> GetByType(string type)
        {
            var typeLower = type.ToLower();

            var list = await _db.LookupValues
                .Where(l => l.Type.ToLower() == typeLower)
                .Select(l => new LookupDto { Id = l.Id, Value = l.Value })
                .ToListAsync();

            if (!list.Any()) return NotFound($"No lookup values for type '{type}'");
            return Ok(list);
        }
    }
}
