using System.ComponentModel.DataAnnotations;

namespace SporSalonuMVC.Models
{
    public class Membership
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Paket Adı")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Süre (gün)")]
        public int DurationDays { get; set; }

        [Display(Name = "Fiyat")]
        public decimal Price { get; set; }
    }
}
