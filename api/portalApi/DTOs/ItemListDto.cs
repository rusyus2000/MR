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
        // ServiceLine removed
        public int? DataSourceId { get; set; }

        public bool? PrivacyPhi { get; set; }
        public bool? PrivacyPii { get; set; }
        public bool? HasRls { get; set; }
        public bool? Featured { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public double? Score { get; set; }
        public bool IsFavorite { get; set; }

        public int? OperatingEntityId { get; set; }
        public string OperatingEntity { get; set; }
        public int? RefreshFrequencyId { get; set; }
        public string RefreshFrequency { get; set; }

        // Additional catalog metadata (free-text)
        public string? ProductGroup { get; set; }
        public string? ProductStatusNotes { get; set; }
        public string? DataConsumersText { get; set; }
        public string? TechDeliveryManager { get; set; }
        public string? RegulatoryComplianceContractual { get; set; }
        public string? BiPlatform { get; set; }
        public string? DbServer { get; set; }
        public string? DbDataMart { get; set; }
        public string? DatabaseTable { get; set; }
        public string? SourceRep { get; set; }
        public string? DataSecurityClassification { get; set; }
        public string? AccessGroupName { get; set; }
        public string? AccessGroupDn { get; set; }
        public string? AutomationClassification { get; set; }
        public string? UserVisibilityString { get; set; }
        public string? UserVisibilityNumber { get; set; }
        public string? EpicSecurityGroupTag { get; set; }
        public string? KeepLongTerm { get; set; }

        // New optional lookups (ID + display)
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
}
