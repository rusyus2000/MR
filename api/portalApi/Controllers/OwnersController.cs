using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OwnersController : ControllerBase
    {
        private readonly AppDbContext _db;
        public OwnersController(AppDbContext db) => _db = db;

        // GET /api/owners?search=term
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Search([FromQuery] string? search, [FromQuery] int top = 10)
        {
            var q = _db.Employees.AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                q = q.Where(o => o.Name.ToLower().Contains(s) || o.Email.ToLower().Contains(s));
            }
            var list = await q
                .OrderBy(o => o.Name)
                .Take(Math.Clamp(top, 1, 50))
                .Select(o => new { o.Id, o.Name, o.Email })
                .ToListAsync();
            return Ok(list);
        }

        // GET /api/owners/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var o = await _db.Employees.FindAsync(id);
            if (o == null) return NotFound();
            return Ok(new { o.Id, o.Name, o.Email });
        }
    }
}
