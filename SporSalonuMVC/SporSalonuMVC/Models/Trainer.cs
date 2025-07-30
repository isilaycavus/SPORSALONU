using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace SporSalonuMVC.Models
{
    public class Trainer
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Uzmanlık Alanı")]
        public string Specialty { get; set; } = string.Empty;

        [Display(Name = "Hakkında")]
        public string Description { get; set; } = string.Empty;

        // 🔧 DERSLER LİSTESİ (Include için şart)
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}
