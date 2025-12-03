namespace SutterAnalyticsApi.Models
{
    public class ItemDataConsumer
    {
        public int ItemId { get; set; }
        public Item Item { get; set; }

        public int DataConsumerId { get; set; }
        public LookupValue DataConsumer { get; set; }
    }
}

