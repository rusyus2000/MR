using Microsoft.AspNetCore.Mvc;
using SutterAnalyticsApi.Data;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/users")] 
    public class UsersController : MpBaseController
    {
        // Simple identity probe for the UI
        [HttpGet("me")]
        public ActionResult<object> Me()
        {
            var u = CurrentUser;
            if (u == null) return Unauthorized();
            return Ok(new { u.Id, u.UserPrincipalName, u.DisplayName, u.Email, u.UserType });
        }

        public class UserProfileDto
        {
            public string? DisplayName { get; set; }
            public string? Email { get; set; }
            public string? NetworkId { get; set; }
        }

        // Update current user's profile (from client-sourced intranet API)
        [HttpPost("profile")]
        public async Task<IActionResult> UpdateProfile([FromForm] UserProfileDto dto, [FromServices] Data.AppDbContext db, [FromServices] Microsoft.Extensions.Caching.Memory.IMemoryCache cache)
        {
            var u = CurrentUser;
            if (u == null) return Unauthorized();
            bool changed = false;
            if (!string.IsNullOrWhiteSpace(dto.DisplayName) && dto.DisplayName != u.DisplayName)
            {
                u.DisplayName = dto.DisplayName;
                changed = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != u.Email)
            {
                u.Email = dto.Email;
                changed = true;
            }
            if (!string.IsNullOrWhiteSpace(dto.NetworkId) && dto.NetworkId != u.UserPrincipalName)
            {
                u.UserPrincipalName = dto.NetworkId;
                changed = true;
            }
            if (changed)
            {
                db.Users.Update(u);
                await db.SaveChangesAsync();
                // Invalidate per-UPN cache
                var upn = u.UserPrincipalName;
                if (!string.IsNullOrWhiteSpace(upn))
                {
                    cache.Remove($"user:{upn}");
                }
            }
            return NoContent();
        }
    }
}
