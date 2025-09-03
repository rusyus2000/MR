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
    }
}

