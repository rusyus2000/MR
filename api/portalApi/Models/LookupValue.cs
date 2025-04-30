using System.ComponentModel.DataAnnotations;

namespace SutterAnalyticsApi.Models
{
    public class LookupValue
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Type { get; set; }    // e.g. "Domain", "Division", "ServiceLine", "DataSource", "AssetType"

        [Required]
        public string Value { get; set; }   // e.g. "Access to Care", "Power BI"
    }
}
