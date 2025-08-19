using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/useractions")]
    public class UserActionsController : MpBaseController

    {
        private readonly AppDbContext _db;

        public UserActionsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            var user = CurrentUser;
            if (user == null) return Unauthorized();

            var favIds = await _db.UserFavorites
                .Where(f => f.UserId == user.Id)
                .Select(f => f.ItemId)
                .ToListAsync();

            return Ok(favIds);
        }

        // POST /api/useractions/togglefavorite/{itemId}
        [HttpPost("togglefavorite/{itemId:int}")]
        public async Task<IActionResult> ToggleFavorite(int itemId)
        {
            // TODO: Replace this with real user lookup (e.g., from JWT or HttpContext)
            var user = CurrentUser;
            if (user == null) return Unauthorized("User not found");

            var existing = await _db.UserFavorites
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ItemId == itemId);

            if (existing != null)
            {
                _db.UserFavorites.Remove(existing);
            }
            else
            {
                // Confirm item exists
                var item = await _db.Items.FindAsync(itemId);
                if (item == null) return NotFound("Item not found");

                _db.UserFavorites.Add(new UserFavorite
                {
                    UserId = user.Id,
                    ItemId = itemId
                });
            }

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // POST /api/useractions/recordopen/{itemId}
        [HttpPost("recordopen/{itemId:int}")]
        public async Task<IActionResult> RecordOpen(int itemId)
        {
            var user = CurrentUser;
            if (user == null) return Unauthorized("User not found");

            var item = await _db.Items.FindAsync(itemId);
            if (item == null) return NotFound("Item not found");

            _db.UserAssetOpenHistories.Add(new UserAssetOpenHistory
            {
                UserId = user.Id,
                ItemId = itemId,
                OpenedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return NoContent();
        }

    }
}
