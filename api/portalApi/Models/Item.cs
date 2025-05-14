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

        // Stored as comma-separated values in the DB
        public string AssetTypesCsv { get; set; }

        [Required]
        public string Domain { get; set; }

        public string Division { get; set; }
        public string ServiceLine { get; set; }
        public string DataSource { get; set; }

        // Privacy: PHI or not
        public bool PrivacyPhi { get; set; }

        // When this asset was added
        public DateTime DateAdded { get; set; }

        // Not mapped helper
        [NotMapped]
        public List<string> AssetTypes
        {
            get => string.IsNullOrWhiteSpace(AssetTypesCsv)
                ? new List<string>()
                : new List<string>(AssetTypesCsv.Split(','));
            set => AssetTypesCsv = string.Join(',', value);
        }
    }
}
