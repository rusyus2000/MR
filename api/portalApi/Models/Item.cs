using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace SutterAnalyticsApi.Models
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required, Url]
        public string Url { get; set; }

        // (Legacy) previously asset types were stored as CSV; replaced by AssetTypeId

        // Tags are normalized into a separate table (ItemTag -> Tag)
        // The old TagsCsv column has been removed in migrations.
        public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();

        // New lookup FK columns (code-first). We keep the original string fields
        // for backward compatibility and to allow a smooth migration.
        public int? AssetTypeId { get; set; }
        public LookupValue AssetType { get; set; }

        // Promotion flag: admin can mark an item as featured
        public bool? Featured { get; set; }

        public int? DomainId { get; set; }
        public LookupValue DomainLookup { get; set; }

        public int? DivisionId { get; set; }
        public LookupValue DivisionLookup { get; set; }

        // ServiceLine removed; use OperatingEntity instead

        public int? DataSourceId { get; set; }
        public LookupValue DataSourceLookup { get; set; }

        // Status (e.g., Published, Offline) via LookupValue of Type="Status"
        public int? StatusId { get; set; }
        public LookupValue StatusLookup { get; set; }

        // Product Owner (separate entity). One owner per item.
        public int? OwnerId { get; set; }
        public Employee Owner { get; set; }

        // Keep text properties available at the model level for compatibility
        // but do not map them to database columns. Consumers should prefer
        // the Lookup navigation properties (e.g. DomainLookup.Value).
        [NotMapped]
        public string Domain => DomainLookup?.Value;

        [NotMapped]
        public string Division => DivisionLookup?.Value;

        // Removed ServiceLine computed property

        [NotMapped]
        public string DataSource => DataSourceLookup?.Value;

        [NotMapped]
        public string Status => StatusLookup?.Value;

        // Privacy: PHI or not
        public bool? PrivacyPhi { get; set; }
        // Additional privacy/governance flags
        public bool? PrivacyPii { get; set; }
        public bool? HasRls { get; set; }

        // When this asset was added
        public DateTime DateAdded { get; set; }

        // Operating entity (lookup)
        public int? OperatingEntityId { get; set; }
        public LookupValue OperatingEntityLookup { get; set; }

        // Executive sponsor (use Owner entity for person records)
        public int? ExecutiveSponsorId { get; set; }
        public Employee ExecutiveSponsor { get; set; }

        // Refresh frequency (lookup)
        public int? RefreshFrequencyId { get; set; }
        public LookupValue RefreshFrequencyLookup { get; set; }

        // User-specified last modified date (distinct from UpdatedAt)
        public DateTime? LastModifiedDate { get; set; }

        // Free-form metadata
        public string? Dependencies { get; set; }
        public string? DefaultAdGroupNames { get; set; }

        // Import metadata for idempotency and audit
        public byte[]? ContentHash { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Additional catalog metadata (free-text fields)
        [MaxLength(500)]
        public string? ProductGroup { get; set; }

        [MaxLength(500)]
        public string? ProductStatusNotes { get; set; }

        // Data Consumers: free-text (semicolon-separated) replacing prior m:m normalization
        [MaxLength(100)]
        public string? DataConsumers { get; set; }

        [MaxLength(100)]
        public string? TechDeliveryManager { get; set; }

        [MaxLength(100)]
        public string? RegulatoryComplianceContractual { get; set; }

        [MaxLength(100)]
        public string? BiPlatform { get; set; }

        [MaxLength(100)]
        public string? DbServer { get; set; }

        [MaxLength(100)]
        public string? DbDataMart { get; set; }

        [MaxLength(100)]
        public string? DatabaseTable { get; set; }

        // Interpreted as "Source Rep" (typo in request)
        [MaxLength(100)]
        public string? SourceRep { get; set; }

        [MaxLength(100)]
        public string? DataSecurityClassification { get; set; }

        [MaxLength(100)]
        public string? AccessGroupName { get; set; }

        [MaxLength(100)]
        public string? AccessGroupDn { get; set; }

        [MaxLength(100)]
        public string? AutomationClassification { get; set; }

        [MaxLength(100)]
        public string? UserVisibilityString { get; set; }

        [MaxLength(100)]
        public string? UserVisibilityNumber { get; set; }

        [MaxLength(100)]
        public string? EpicSecurityGroupTag { get; set; }

        [MaxLength(100)]
        public string? KeepLongTerm { get; set; }

        // No CSV-backed multi-value asset types anymore. Use the single
        // AssetType lookup (AssetTypeId) and the boolean `Featured`.

        // Not mapped helper for tags (reads from normalized ItemTags relationship)
        [NotMapped]
        public List<string> Tags
        {
            get => ItemTags == null ? new List<string>() : ItemTags.Select(it => it.Tag?.Value).Where(v => v != null).ToList();
            set
            {
                // This helper is not used to persist tags; use controller logic to manage Tag entities.
                // Keep setter as no-op to avoid accidental usage.
            }
        }

        // Note: former many-to-many relationship (ItemDataConsumer) was removed.
        // Helper to expose list form derived from the free-text field when needed.
        [NotMapped]
        public List<string> DataConsumersList => string.IsNullOrWhiteSpace(DataConsumers)
            ? new List<string>()
            : DataConsumers.Split(';').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();

        // Additional optional lookup fields (nullable FKs to LookupValues)
        public int? PotentialToConsolidateId { get; set; }
        public LookupValue PotentialToConsolidate { get; set; }

        public int? PotentialToAutomateId { get; set; }
        public LookupValue PotentialToAutomate { get; set; }

        public int? SponsorBusinessValueId { get; set; }
        public LookupValue SponsorBusinessValue { get; set; }

        // MustDo2025 removed

        public int? DevelopmentEffortId { get; set; }
        public LookupValue DevelopmentEffortLookup { get; set; }

        public int? EstimatedDevHoursId { get; set; }
        public LookupValue EstimatedDevHoursLookup { get; set; }

        public int? ResourcesDevelopmentId { get; set; }
        public LookupValue ResourcesDevelopmentLookup { get; set; }

        public int? ResourcesAnalystsId { get; set; }
        public LookupValue ResourcesAnalystsLookup { get; set; }

        public int? ResourcesPlatformId { get; set; }
        public LookupValue ResourcesPlatformLookup { get; set; }

        public int? ResourcesDataEngineeringId { get; set; }
        public LookupValue ResourcesDataEngineeringLookup { get; set; }

        // Product Impact Category (lookup)
        public int? ProductImpactCategoryId { get; set; }
        public LookupValue ProductImpactCategory { get; set; }
    }
}
