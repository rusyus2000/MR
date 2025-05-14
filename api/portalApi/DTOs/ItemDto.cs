namespace SutterAnalyticsApi.DTOs
{
    public class ItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public List<string> AssetTypes { get; set; }
        public string Domain { get; set; }
        public string Division { get; set; }
        public string ServiceLine { get; set; }
        public string DataSource { get; set; }
        public bool PrivacyPhi { get; set; }

        // New field
        public DateTime DateAdded { get; set; }
        public double? Score { get; set; }
        public bool IsFavorite { get; set; }
    }
}
