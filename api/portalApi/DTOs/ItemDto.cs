namespace SutterAnalyticsApi.DTOs
{
    public class ItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        // Single asset type (lookup)
        public int? AssetTypeId { get; set; }
        public string AssetTypeName { get; set; }
        
        // Lookup IDs for filters (referencing LookupValues)
        public int? DomainId { get; set; }
        public int? DivisionId { get; set; }
        public int? ServiceLineId { get; set; }
        public int? DataSourceId { get; set; }
        public string Domain { get; set; }
        public string Division { get; set; }
        public string ServiceLine { get; set; }
        public string DataSource { get; set; }
        public bool PrivacyPhi { get; set; }

        // New field
        public DateTime DateAdded { get; set; }
        public bool Featured { get; set; }
        public double? Score { get; set; }
        public bool IsFavorite { get; set; }
        // Tags associated with the item (free-form)
        public List<string> Tags { get; set; } = new();

        // Status (lookup)
        public int? StatusId { get; set; }
        public string Status { get; set; }

        // Owner
        public int? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerEmail { get; set; }
    }
}
