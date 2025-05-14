namespace SutterAnalyticsApi.Models
{
    public class UserLoginHistory
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public DateTime LoggedInAt { get; set; } = DateTime.UtcNow;
    }
}
