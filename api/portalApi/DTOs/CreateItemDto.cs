public class CreateItemDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    // Single AssetTypeId (references LookupValues)
    public int? AssetTypeId { get; set; }
    // New lookup id fields (reference LookupValues.Id)
    public int? DomainId { get; set; }
    public int? DivisionId { get; set; }
    public int? ServiceLineId { get; set; }
    public int? DataSourceId { get; set; }
    public string Domain { get; set; }
    public string Division { get; set; }
    public string ServiceLine { get; set; }
    public string DataSource { get; set; }
    public bool PrivacyPhi { get; set; }
    // Free-form tags that can be attached to the item. Sent from the UI as repeated form entries.
    public List<string> Tags { get; set; } = new();
    // Promotion flag
    public bool Featured { get; set; }
}
