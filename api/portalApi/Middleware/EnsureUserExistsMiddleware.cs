using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.Models;
using SutterAnalyticsApi.Options;

namespace portalApi.Middleware
{
    public class EnsureUserExistsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly UserCacheOptions _options;
        private readonly string[] _adminUpns;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public EnsureUserExistsMiddleware(RequestDelegate next, IMemoryCache cache, IOptions<UserCacheOptions> options, IOptions<AdminOptions> adminOptions)
        {
            _next = next;
            _cache = cache;
            _options = options?.Value ?? new UserCacheOptions();
            var csv = adminOptions?.Value?.UpnsCsv ?? string.Empty;
            _adminUpns = (csv ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(s => s.ToLowerInvariant())
                .Distinct()
                .ToArray();
        }

        public async Task InvokeAsync(HttpContext context, AppDbContext db)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var upn = context.User.Identity?.Name?.Split('\\').Last();
                if (!string.IsNullOrWhiteSpace(upn))
                {
                    var cacheKey = $"user:{upn}";
                    if (!_cache.TryGetValue<User>(cacheKey, out var user))
                    {
                        var sem = _locks.GetOrAdd(upn, _ => new SemaphoreSlim(1, 1));
                        await sem.WaitAsync();
                        try
                        {
                            // Double-check after acquiring lock
                            if (!_cache.TryGetValue<User>(cacheKey, out user))
                            {
                                // Lightweight, no-tracking read
                                user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.UserPrincipalName == upn);

                                if (user == null)
                                {
                                    var claimsPrincipal = context.User;
                                    string email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
                                                    ?? claimsPrincipal.FindFirst("email")?.Value ?? string.Empty;
                                    string fullName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value
                                                    ?? claimsPrincipal.FindFirst("displayname")?.Value ?? string.Empty;

                                    var newUser = new User
                                    {
                                        UserPrincipalName = upn,
                                        DisplayName = fullName,
                                        Email = email,
                                        UserType = "User",
                                        CreatedAt = DateTime.UtcNow
                                    };

                                    db.Users.Add(newUser);
                                    try
                                    {
                                        await db.SaveChangesAsync();
                                        user = newUser;
                                    }
                                    catch (DbUpdateException)
                                    {
                                        // In case of concurrent create or unique constraint, re-fetch existing
                                        user = await db.Users.AsNoTracking()
                                            .FirstOrDefaultAsync(u => u.UserPrincipalName == upn);
                                    }
                                }

                                if (user != null)
                                {
                                    var entryOptions = new MemoryCacheEntryOptions
                                    {
                                        SlidingExpiration = TimeSpan.FromMinutes(Math.Max(1, _options.SlidingMinutes))
                                    };
                                    _cache.Set(cacheKey, user, entryOptions);
                                }
                            }
                        }
                        finally
                        {
                            sem.Release();
                            _ = _locks.TryRemove(upn, out _);
                        }
                    }

                    if (user != null)
                    {
                        // UI now populates profile via /api/users/profile; no server-side external calls here

                        var isAdminConfigured = _adminUpns.Any() && _adminUpns.Contains(upn.ToLowerInvariant());
                        var desiredType = isAdminConfigured ? "Admin" : "User";
                        if (!string.Equals(user.UserType, desiredType, StringComparison.Ordinal))
                        {
                            // Update role in DB (and refresh cache)
                            var tracked = await db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
                            if (tracked != null && !string.Equals(tracked.UserType, desiredType, StringComparison.Ordinal))
                            {
                                tracked.UserType = desiredType;
                                db.Users.Update(tracked);
                                await db.SaveChangesAsync();
                                // update cached copy
                                user.UserType = desiredType;
                                _cache.Set($"user:{upn}", user, new MemoryCacheEntryOptions
                                {
                                    SlidingExpiration = TimeSpan.FromMinutes(Math.Max(1, _options.SlidingMinutes))
                                });
                            }
                        }

                        context.Items["AppUser"] = user;

                        try
                        {
                            // Throttle login history entries per user
                            var loginKey = $"login:{upn}";
                            if (!_cache.TryGetValue(loginKey, out _))
                            {
                                db.UserLoginHistories.Add(new UserLoginHistory
                                {
                                    UserId = user.Id,
                                    LoggedInAt = DateTime.UtcNow
                                });
                                await db.SaveChangesAsync();
                                _cache.Set(loginKey, true, new MemoryCacheEntryOptions
                                {
                                    SlidingExpiration = TimeSpan.FromMinutes(Math.Max(1, _options.LoginHistoryThrottleMinutes))
                                });
                            }
                        }
                        catch { }
                    }
                }
            }

            await _next(context);
        }

        // Profile is enriched by the UI via /api/users/profile
    }
}
