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

            // record search history for the current user (best-effort)
            try
            {
                var user = CurrentUser;
                if (user != null)
                {
                    _db.UserSearchHistories.Add(new UserSearchHistory
                    {
                        UserId = user.Id,
                        Query = q,
                        SearchedAt = DateTime.UtcNow
                    });
                    await _db.SaveChangesAsync();
                }
            }
            catch
            {
                // ignore logging failures
            }

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
                    .Include(i => i.ItemTags)
                        .ThenInclude(it => it.Tag)
                    .Include(i => i.AssetType)
                    .Include(i => i.DomainLookup)
                    .Include(i => i.DivisionLookup)
                    .Include(i => i.ServiceLineLookup)
                    .Include(i => i.DataSourceLookup)
                    .Include(i => i.Owner)
                    .Include(i => i.StatusLookup)
                    .Where(i => ids.Contains(i.Id))
                    .ToListAsync();

                // Restrict visibility: non-admins see only Published
                var isAdmin = user?.UserType == "Admin";
                if (!isAdmin)
                {
                    var published = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Status" && l.Value == "Published");
                    if (published != null)
                    {
                        matchedItems = matchedItems.Where(i => !i.StatusId.HasValue || i.StatusId == published.Id).ToList();
                    }
                }

                // ?? Preserve AI sort order, and include IsFavorite
                var ordered = aiResults
                    .Select(r =>
                    {
                        var match = matchedItems.FirstOrDefault(i => i.Id == r.Id);
                        if (match == null) return null;

                        return new ItemListDto
                        {
                            Id = match.Id,
                            Title = match.Title,
                            Description = match.Description,
                            Url = match.Url,
                            AssetTypeId = match.AssetTypeId,
                            AssetTypeName = match.AssetType != null ? match.AssetType.Value : null,
                            Featured = match.Featured,
                            DomainId = match.DomainId,
                            DivisionId = match.DivisionId,
                            ServiceLineId = match.ServiceLineId,
                            DataSourceId = match.DataSourceId,
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
