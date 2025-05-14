using Microsoft.AspNetCore.Mvc;
using SutterAnalyticsApi.Models;
namespace SutterAnalyticsApi.Controllers
{
    public class MpBaseController : ControllerBase
    {
        protected User CurrentUser => HttpContext.Items["AppUser"] as User;
    }
}