using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SutterAnalyticsApi.Models
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Value { get; set; }

        public ICollection<ItemTag> ItemTags { get; set; } = new List<ItemTag>();
    }
}

