using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SporSalonuMVC.Models
{
    public class Class
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ders Adı")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Tarih ve Saat")]
        public DateTime Schedule { get; set; }

        [Required]
        [Display(Name = "Eğitmen")]
        public int TrainerId { get; set; }

        [Display(Name = "Kontenjan")]
        [Range(1, 100, ErrorMessage = "Kontenjan 1 ile 100 arasında olmalıdır.")]
        public int Capacity { get; set; }

        [ValidateNever]
        [ForeignKey("TrainerId")]
        public Trainer? Trainer { get; set; } // ✅ Null olabilir olarak tanımla


        public ICollection<Reservation> Reservations { get; set; } = [];
    }
}
