public class CreateItemDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Url { get; set; }
    public List<string> AssetTypes { get; set; } = new();
    public string Domain { get; set; }
    public string Division { get; set; }
    public string ServiceLine { get; set; }
    public string DataSource { get; set; }
    public bool PrivacyPhi { get; set; }
}
