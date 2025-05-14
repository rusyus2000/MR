namespace SutterAnalyticsApi.Models
{
    public class UserSearchHistory
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public string Query { get; set; }

        public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    }
}
