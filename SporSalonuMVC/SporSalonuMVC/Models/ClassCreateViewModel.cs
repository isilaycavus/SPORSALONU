using System.ComponentModel.DataAnnotations;
using SporSalonuMVC.Attributes;


namespace SporSalonuMVC.ViewModels
{
    public class ClassCreateViewModel
    {
        [Required(ErrorMessage = "Ders adı gereklidir.")]
        [Display(Name = "Ders Adı")]
        public string Name { get; set; } = string.Empty;
        [Required(ErrorMessage = "Açıklama gereklidir.")]
        [Display(Name = "Açıklama")]
        public string Description { get; set; } = string.Empty;


        [Required(ErrorMessage = "Tarih gereklidir.")]
        [Display(Name = "Tarih")]
        [DataType(DataType.Date)]
        [DateInNext7Days] // ✅ Burayı ekledik
        public DateTime Date { get; set; } = DateTime.Today;



        [Required(ErrorMessage = "Saat seçilmelidir.")]
        [Display(Name = "Saat")]
        public string SelectedHour { get; set; } = string.Empty;// Saat string olarak tutuluyor

        [Required(ErrorMessage = "Eğitmen seçilmelidir.")]
        [Display(Name = "Eğitmen")]
        public int TrainerId { get; set; }

        [Display(Name = "Kontenjan")]
        [Range(1, 100, ErrorMessage = "Kontenjan 1 ile 100 arasında olmalıdır.")]
        public int Capacity { get; set; }

        public List<string> HourOptions { get; set; } = Enumerable.Range(9, 15)
            .Select(h => h.ToString("D2") + ":00").ToList();  // 09:00–23:00 saat arası

        public List<SporSalonuMVC.Models.Trainer> AvailableTrainers { get; set; } = new();
        public int Id { get; set; }

    }
}
