using System.ComponentModel.DataAnnotations;

namespace SutterAnalyticsApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public string UserPrincipalName { get; set; }

        public string DisplayName { get; set; }

        public string Email { get; set; }

        public string UserType { get; set; }

        public DateTime CreatedAt { get; set; }

        // ✅ Navigation properties
        public ICollection<UserFavorite> Favorites { get; set; } = new List<UserFavorite>();
        public ICollection<UserSearchHistory> SearchHistories { get; set; } = new List<UserSearchHistory>();
        public ICollection<UserLoginHistory> LoginHistories { get; set; } = new List<UserLoginHistory>();
        public ICollection<UserAssetOpenHistory> OpenHistories { get; set; } = new List<UserAssetOpenHistory>();
    }

}
