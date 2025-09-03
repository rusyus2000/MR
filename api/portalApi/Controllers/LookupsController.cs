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

        // GET /api/lookups/{type}/counts
        [HttpGet("{type}/counts")]
        public async Task<ActionResult<IEnumerable<object>>> GetByTypeWithCounts(string type)
        {
            var typeLower = type.ToLower();

            var lookups = await _db.LookupValues
                .Where(l => l.Type.ToLower() == typeLower)
                .ToListAsync();

            if (!lookups.Any()) return NotFound($"No lookup values for type '{type}'");

            var result = new List<object>();
            foreach (var lv in lookups)
            {
                int count = 0;
                switch (lv.Type)
                {
                    case "AssetType":
                        count = await _db.Items.CountAsync(i => i.AssetTypeId == lv.Id);
                        break;
                    case "Domain":
                        count = await _db.Items.CountAsync(i => i.DomainId == lv.Id);
                        break;
                    case "Division":
                        count = await _db.Items.CountAsync(i => i.DivisionId == lv.Id);
                        break;
                    case "ServiceLine":
                        count = await _db.Items.CountAsync(i => i.ServiceLineId == lv.Id);
                        break;
                    case "DataSource":
                        count = await _db.Items.CountAsync(i => i.DataSourceId == lv.Id);
                        break;
                    case "Status":
                        count = await _db.Items.CountAsync(i => i.StatusId == lv.Id);
                        break;
                    default:
                        count = 0; break;
                }

                result.Add(new { Id = lv.Id, Value = lv.Value, Count = count });
            }

            return Ok(result);
        }

        // GET /api/lookups/bulk?types=AssetType,Domain,Division,ServiceLine,DataSource,Status
        // Returns an object keyed by type with arrays of { id, value }
        [HttpGet("bulk")]
        public async Task<ActionResult<object>> GetBulk([FromQuery] string? types)
        {
            var defaultTypes = new[] { "AssetType", "Domain", "Division", "ServiceLine", "DataSource", "Status" };
            var typeSet = string.IsNullOrWhiteSpace(types)
                ? defaultTypes
                : types.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var lowerSet = typeSet.Select(t => t.ToLower()).ToHashSet();

            var all = await _db.LookupValues
                .Where(l => lowerSet.Contains(l.Type.ToLower()))
                .ToListAsync();

            var result = new Dictionary<string, IEnumerable<LookupDto>>(StringComparer.OrdinalIgnoreCase);
            foreach (var t in typeSet)
            {
                var list = all
                    .Where(l => l.Type.Equals(t, StringComparison.OrdinalIgnoreCase))
                    .Select(l => new LookupDto { Id = l.Id, Value = l.Value })
                    .ToList();
                result[t] = list;
            }

            return Ok(result);
        }
    }
}
