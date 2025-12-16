using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SutterAnalyticsApi.Models
{
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Navigation
        public ICollection<Item> Items { get; set; } = new List<Item>();
    }
}

