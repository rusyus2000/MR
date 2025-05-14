namespace portalApi.Middleware
{
    using Microsoft.EntityFrameworkCore;
    using SutterAnalyticsApi.Data;
    using SutterAnalyticsApi.Models;
    using System.Security.Claims;

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
                var upn = context.User.Identity.Name?.Split('\\').Last();
                if (!string.IsNullOrWhiteSpace(upn))
                {
                    var user = await db.Users.FirstOrDefaultAsync(u => u.UserPrincipalName == upn);

                    if (user == null)
                    {
                        // Attempt to get email/full name from claims
                        var claimsPrincipal = context.User;
                        string email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value
                                    ?? claimsPrincipal.FindFirst("email")?.Value ?? "";
                        string fullName = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value
                                        ?? claimsPrincipal.Identity?.Name?.Split('\\').Last() ?? upn;

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

                    // Make user available to the rest of the request
                    context.Items["AppUser"] = user;
                }
            }

            await _next(context);
        }
    }

}
