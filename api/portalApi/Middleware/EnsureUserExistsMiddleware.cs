using System;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.Models;

namespace portalApi.Middleware
{
    public class EnsureUserExistsMiddleware
    {
        private readonly RequestDelegate _next;

        public EnsureUserExistsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var upn = context.User.Identity?.Name?.Split('\\').Last();
                if (!string.IsNullOrWhiteSpace(upn))
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.UserPrincipalName == upn);

                    if (user == null)
                    {
                        var claimsPrincipal = context.User;
                        string email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
                                        ?? claimsPrincipal.FindFirst("email")?.Value ?? string.Empty;
                        string fullName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value
                                        ?? claimsPrincipal.FindFirst("displayname")?.Value ?? string.Empty;

                        user = new User
                        {
                            UserPrincipalName = upn,
                            DisplayName = fullName,
                            Email = email,
                            UserType = "User",
                            CreatedAt = DateTime.UtcNow
                        };

                        db.Users.Add(user);
                        await db.SaveChangesAsync();
                    }

                    // UI now populates profile via /api/users/profile; no server-side external calls here

                    var isDevAdmin = string.Equals(upn, "adzhiey", StringComparison.OrdinalIgnoreCase);
                    var desiredType = isDevAdmin ? "Admin" : "User";
                    if (!string.Equals(user.UserType, desiredType, StringComparison.Ordinal))
                    {
                        user.UserType = desiredType;
                        db.Users.Update(user);
                        await db.SaveChangesAsync();
                    }

                    context.Items["AppUser"] = user;

                    try
                    {
                        db.UserLoginHistories.Add(new UserLoginHistory
                        {
                            UserId = user.Id,
                            LoggedInAt = DateTime.UtcNow
                        });
                        await db.SaveChangesAsync();
                    }
                    catch { }
                }
            }

            await _next(context);
        }

        // Profile is enriched by the UI via /api/users/profile
    }
}
