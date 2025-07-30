using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuMVC.Models
{
    [Table("studentdocuments")]
    public class StudentDocument
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("userid")]
        public int UserId { get; set; }

        [Required]
        [Column("filepath")]
        public string FilePath { get; set; } = string.Empty;

        [Column("uploadedat")]
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [Column("status")]
        public string Status { get; set; } = "Beklemede";

        [Column("adminnote")]
        public string? AdminNote { get; set; }

        public User? User { get; set; }
    }
}
