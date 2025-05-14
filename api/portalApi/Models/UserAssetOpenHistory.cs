using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Models
{
    public class UserAssetOpenHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }

        public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    }
}
