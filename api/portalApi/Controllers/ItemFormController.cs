using Microsoft.AspNetCore.Mvc;
using SutterAnalyticsApi.Controllers;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.Models;
namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/itemsform")]
    public class ItemFormController : MpBaseController
    {
        private readonly AppDbContext _db;
        public ItemFormController(AppDbContext db) => _db = db;

        
    }
}