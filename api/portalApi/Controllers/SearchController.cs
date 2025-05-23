using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.DTOs;
using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : MpBaseController
    {
        private readonly AppDbContext _db;
        public SearchController(AppDbContext db) => _db = db;

        // GET /api/search?q=term
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<ItemDto>>> Search([FromQuery] string q)
        //{
        //    if (string.IsNullOrWhiteSpace(q))
        //        return BadRequest("Query parameter 'q' is required.");

        //    var results = await _db.Items
        //        .Where(i =>
        //            EF.Functions.Like(i.Title, $"%{q}%") ||
        //            EF.Functions.Like(i.Description, $"%{q}%"))
        //        .Select(i => new ItemDto
        //        {
        //            Id = i.Id,
        //            Title = i.Title,
        //            Description = i.Description,
        //            Url = i.Url,
        //            AssetTypes = i.AssetTypes,
        //            Domain = i.Domain,
        //            Division = i.Division,
        //            ServiceLine = i.ServiceLine,
        //            DataSource = i.DataSource,
        //            PrivacyPhi = i.PrivacyPhi
        //        })
        //        .ToListAsync();

        //    return Ok(results);
        //}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemDto>>> SearchAI([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return BadRequest("Query parameter 'q' is required.");

            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetAsync($"http://127.0.0.1:8000/search?query={Uri.EscapeDataString(q)}");
                if (!response.IsSuccessStatusCode)
                    return StatusCode((int)response.StatusCode, "AI search service failed.");

                var aiResults = await response.Content.ReadFromJsonAsync<List<AiSearchResult>>();
                if (aiResults == null || !aiResults.Any())
                    return Ok(new List<ItemDto>());

                var ids = aiResults.Select(r => r.Id).ToList();

                // ?? Get current user
                var user = CurrentUser;
                var favoriteIds = await _db.UserFavorites
                    .Where(f => f.UserId == user.Id)
                    .Select(f => f.ItemId)
                    .ToListAsync();

                // ?? Query matching items from DB
                var matchedItems = await _db.Items
                    .Where(i => ids.Contains(i.Id))
                    .ToListAsync();

                // ?? Preserve AI sort order, and include IsFavorite
                var ordered = aiResults
                    .Select(r =>
                    {
                        var match = matchedItems.FirstOrDefault(i => i.Id == r.Id);
                        if (match == null) return null;

                        return new ItemDto
                        {
                            Id = match.Id,
                            Title = match.Title,
                            Description = match.Description,
                            Url = match.Url,
                            AssetTypes = match.AssetTypes,
                            Domain = match.Domain,
                            Division = match.Division,
                            ServiceLine = match.ServiceLine,
                            DataSource = match.DataSource,
                            PrivacyPhi = match.PrivacyPhi,
                            DateAdded = match.DateAdded,
                            Score = r.Score,
                            IsFavorite = favoriteIds.Contains(match.Id)
                        };
                    })
                    .Where(dto => dto != null)
                    .ToList();

                return Ok(ordered);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error contacting AI Search: {ex.Message}");
            }
        }


        public class AiSearchResult
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public double Score { get; set; }
            public double Original_Score { get; set; }
            public double Keyword_Boost { get; set; }
        }

    }
}
