using SutterAnalyticsApi.Models;

namespace SutterAnalyticsApi.Models
{
    public class UserFavorite
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int ItemId { get; set; }
        public Item Item { get; set; }
    }
}
