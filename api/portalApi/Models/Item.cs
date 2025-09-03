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
        public bool Featured { get; set; }

        public int? DomainId { get; set; }
        public LookupValue DomainLookup { get; set; }

        public int? DivisionId { get; set; }
        public LookupValue DivisionLookup { get; set; }

        public int? ServiceLineId { get; set; }
        public LookupValue ServiceLineLookup { get; set; }

        public int? DataSourceId { get; set; }
        public LookupValue DataSourceLookup { get; set; }

        // Status (e.g., Published, Offline) via LookupValue of Type="Status"
        public int? StatusId { get; set; }
        public LookupValue StatusLookup { get; set; }

        // Product Owner (separate entity). One owner per item.
        public int? OwnerId { get; set; }
        public Owner Owner { get; set; }

        // Keep text properties available at the model level for compatibility
        // but do not map them to database columns. Consumers should prefer
        // the Lookup navigation properties (e.g. DomainLookup.Value).
        [NotMapped]
        public string Domain => DomainLookup?.Value;

        [NotMapped]
        public string Division => DivisionLookup?.Value;

        [NotMapped]
        public string ServiceLine => ServiceLineLookup?.Value;

        [NotMapped]
        public string DataSource => DataSourceLookup?.Value;

        [NotMapped]
        public string Status => StatusLookup?.Value;

        // Privacy: PHI or not
        public bool PrivacyPhi { get; set; }

        // When this asset was added
        public DateTime DateAdded { get; set; }

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
    }
}
