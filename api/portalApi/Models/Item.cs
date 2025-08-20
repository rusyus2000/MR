using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        // Stored as comma-separated values in the DB for asset types
        public string AssetTypesCsv { get; set; }

        // Tags are normalized into a separate table (ItemTag -> Tag)
        // The old TagsCsv column has been removed in migrations.
        public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();

        // New lookup FK columns (code-first). We keep the original string fields
        // for backward compatibility and to allow a smooth migration.
        public int? AssetTypeId { get; set; }
        public LookupValue AssetType { get; set; }

        public int? DomainId { get; set; }
        public LookupValue DomainLookup { get; set; }

        public int? DivisionId { get; set; }
        public LookupValue DivisionLookup { get; set; }

        public int? ServiceLineId { get; set; }
        public LookupValue ServiceLineLookup { get; set; }

        public int? DataSourceId { get; set; }
        public LookupValue DataSourceLookup { get; set; }

        [Required]
        public string Domain { get; set; }

        public string Division { get; set; }
        public string ServiceLine { get; set; }
        public string DataSource { get; set; }

        // Privacy: PHI or not
        public bool PrivacyPhi { get; set; }

        // When this asset was added
        public DateTime DateAdded { get; set; }

        // Not mapped helper for asset types
        [NotMapped]
        public List<string> AssetTypes
        {
            get => string.IsNullOrWhiteSpace(AssetTypesCsv)
                ? new List<string>()
                : new List<string>(AssetTypesCsv.Split(','));
            set => AssetTypesCsv = string.Join(',', value);
        }

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
