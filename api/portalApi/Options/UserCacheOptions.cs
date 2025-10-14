namespace SutterAnalyticsApi.Options
{
    public class UserCacheOptions
    {
        public int SlidingMinutes { get; set; } = 20; // default 20 minutes
        public int LoginHistoryThrottleMinutes { get; set; } = 60; // default 12 hours
    }
}

