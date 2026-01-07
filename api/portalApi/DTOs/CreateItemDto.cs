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
    // ServiceLine removed
    public int? BiPlatformId { get; set; }
    public string Domain { get; set; }
    public string Division { get; set; }
    // ServiceLine removed
    public string? BiPlatform { get; set; }
    public bool? PrivacyPhi { get; set; }
    public bool? PrivacyPii { get; set; }
    public bool? HasRls { get; set; }
    // Free-form tags that can be attached to the item. Sent from the UI as repeated form entries.
    public List<string> Tags { get; set; } = new();

    // Status (lookup) - respected only for admins; defaults to Published
    public int? StatusId { get; set; }

    // Owner - prefer OwnerId; if not provided, backend may create using Name/Email
    public int? OwnerId { get; set; }
    public string OwnerName { get; set; }
    public string OwnerEmail { get; set; }

    // Operating Entity (lookup)
    public int? OperatingEntityId { get; set; }
    public string OperatingEntity { get; set; }

    // Executive Sponsor (Owner)
    public int? ExecutiveSponsorId { get; set; }
    public string ExecutiveSponsorName { get; set; }
    public string ExecutiveSponsorEmail { get; set; }

    // Data Consumers: simple free-text field
    public string? DataConsumers { get; set; }

    // Free-form metadata
    public string Dependencies { get; set; }
    public string DefaultAdGroupNames { get; set; }

    // Refresh Frequency (lookup)
    public int? RefreshFrequencyId { get; set; }
    public string RefreshFrequency { get; set; }

    // User-specified last modified date
    public DateTime? LastModifiedDate { get; set; }

    // Additional catalog metadata (free-text fields)
    public string? ProductGroup { get; set; }
    public string? ProductStatusNotes { get; set; }
    public string? DataConsumersText { get; set; }
    public string? TechDeliveryManager { get; set; }
    public string? RegulatoryComplianceContractual { get; set; }
    public string? DataSource { get; set; }
    public string? DbServer { get; set; }
    public string? DbDataMart { get; set; }
    public string? DatabaseTable { get; set; }
    public string? SourceRep { get; set; }
    public string? DataSecurityClassification { get; set; }
    public string? AccessGroupName { get; set; }
    public string? AccessGroupDn { get; set; }
    public string? AutomationClassification { get; set; }
    public string? UserVisibilityString { get; set; }
    public string? EpicSecurityGroupTag { get; set; }
    public string? KeepLongTerm { get; set; }

    // New optional lookup fields (ids and/or names)
    public int? PotentialToConsolidateId { get; set; }
    public string? PotentialToConsolidate { get; set; }

    public int? PotentialToAutomateId { get; set; }
    public string? PotentialToAutomate { get; set; }

    public int? SponsorBusinessValueId { get; set; }
    public string? SponsorBusinessValue { get; set; }

    // MustDo2025 removed

    public int? DevelopmentEffortId { get; set; }
    public string? DevelopmentEffort { get; set; }

    public int? EstimatedDevHoursId { get; set; }
    public string? EstimatedDevHours { get; set; }

    public int? ResourcesDevelopmentId { get; set; }
    public string? ResourcesDevelopment { get; set; }

    public int? ResourcesAnalystsId { get; set; }
    public string? ResourcesAnalysts { get; set; }

    public int? ResourcesPlatformId { get; set; }
    public string? ResourcesPlatform { get; set; }

    public int? ResourcesDataEngineeringId { get; set; }
    public string? ResourcesDataEngineering { get; set; }

    // Product Impact Category
    public int? ProductImpactCategoryId { get; set; }
    public string? ProductImpactCategory { get; set; }
}
