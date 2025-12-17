using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SutterAnalyticsApi.Data;
using SutterAnalyticsApi.DTOs;
using SutterAnalyticsApi.Models;
using Microsoft.Extensions.Configuration;

namespace SutterAnalyticsApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : MpBaseController
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;
        public SearchController(AppDbContext db, IConfiguration config, IHttpClientFactory httpFactory)
        {
            _db = db;
            _config = config;
            _httpFactory = httpFactory;
        }

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

            try
            {
                var handler = new HttpClientHandler
                {
                    UseDefaultCredentials = false,
                    Credentials = null
                };
                var client = new HttpClient(handler)
                {
                    BaseAddress = new Uri("http://smf-appweb-qa/AI_search/")
                };
                //_httpFactory.CreateClient("SearchApi");
                var response = await client.GetAsync($"search?query={Uri.EscapeDataString(q)}");
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                   // _logger.LogError($"AI search failed: {response.StatusCode} - {content}");
                    return StatusCode((int)response.StatusCode, $"AI search service failed: {response.StatusCode}");
                }

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

                // ?? Visibility restriction & lean projection
                var scoreMap = aiResults.ToDictionary(r => r.Id, r => (double?)r.Score);

                var isAdmin = user?.UserType == "Admin";
                int? publishedId = null;
                if (!isAdmin)
                {
                    var pub = await _db.LookupValues.FirstOrDefaultAsync(l => l.Type == "Status" && l.Value == "Published");
                    publishedId = pub?.Id;
                }

                var query = _db.Items.AsNoTracking().Where(i => ids.Contains(i.Id));
                if (!isAdmin && publishedId.HasValue)
                {
                    query = query.Where(i => !i.StatusId.HasValue || i.StatusId == publishedId.Value);
                }

                var matchedItems = await query
                    .Select(i => new ItemListDto
                    {
                        Id = i.Id,
                        Title = i.Title,
                        Description = i.Description,
                        Url = i.Url,
                        AssetTypeId = i.AssetTypeId,
                        AssetTypeName = i.AssetType != null ? i.AssetType.Value : null,
                        DomainId = i.DomainId,
                        DivisionId = i.DivisionId,
                        DataSourceId = i.DataSourceId,
                        PrivacyPhi = i.PrivacyPhi,
                        DateAdded = i.DateAdded,
                        Score = 0
                    })
                    .ToListAsync();

                // ?? Preserve AI sort order, and include IsFavorite
                // Merge and preserve AI order
                var map = matchedItems.ToDictionary(m => m.Id);
                var ordered = new List<ItemListDto>(matchedItems.Count);
                foreach (var r in aiResults)
                {
                    if (map.TryGetValue(r.Id, out var m))
                    {
                        m.Score = scoreMap[r.Id];
                        m.IsFavorite = favoriteIds.Contains(m.Id);
                        ordered.Add(m);
                    }
                }

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
