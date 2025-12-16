namespace SutterAnalyticsApi.Services;

public static class ItemCsvFormat
{
    // Canonical CSV format used by:
    // - ItemsController.Export (download)
    // - ImportController.Preview/Commit (upload)
    public static readonly string[] Headers =
    [
        "Domain",
        "Product Group",
        "Title",
        "Description",
        "Status",
        "Product Status Notes",
        "Division",
        "Operating Entity",
        "Executive Sponsor Name",
        "Data Consumers",
        "Owner Name",
        "Owner Email",
        "Tech Delivery Mgr",
        "Regulatory/Compliance/Contractual",
        "Asset Type",
        "Featured",
        "BI Platform",
        "Url",
        "DB Server",
        "DB/Data Mart",
        "Database Table",
        "Source Rep",
        "dataSecurityClassification",
        "accessGroupName",
        "accessGroupDN",
        "Data Source",
        "Automation Classification",
        "user_visibility_string",
        "Default AD Group Names",
        "PHI",
        "PII",
        "Has RLS",
        "Epic Security Group tag",
        "Refresh Frequency",
        "Keep Long Term",
        "Potential to Consolidate",
        "Potential to Automate",
        "Last Modified Date",
        "Business Value by executive sponsor",
        // 2025 Must Do removed
        "Development Effort",
        "Estimated development hours",
        "Resources - Development",
        "Resources - Analysts",
        "Resources - Platform",
        "Resources - Data Engineering",
        "Product Impact Category",
        // Extra fields appended at the end (unchanged labels)
        "Id",
        // Service Line removed
        "Tags",
        "Dependencies",
        "Executive Sponsor Email"
    ];
}
