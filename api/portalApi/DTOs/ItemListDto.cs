namespace SutterAnalyticsApi.DTOs
{
    public class ItemListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }

        public int? AssetTypeId { get; set; }
        public string AssetTypeName { get; set; }

        public int? DomainId { get; set; }
        public int? DivisionId { get; set; }
        public int? ServiceLineId { get; set; }
        public int? DataSourceId { get; set; }

        public bool PrivacyPhi { get; set; }
        public bool Featured { get; set; }
        public DateTime DateAdded { get; set; }
        public double? Score { get; set; }
        public bool IsFavorite { get; set; }
    }
}

