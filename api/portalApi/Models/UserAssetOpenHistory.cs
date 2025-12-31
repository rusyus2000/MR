using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Models
{
    public class UserAssetOpenHistory
    {
        public const string TypeDetails = "Details";
        public const string TypeUrl = "URL";

        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        // Stores the click source (e.g., Details, Url).
        public string ClickType { get; set; } = TypeDetails;

        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    }
}
