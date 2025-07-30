using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuMVC.Models
{
    public class EntryLog
    {
        public int Id { get; set; }

        [Required]
        public int UserCardId { get; set; }

        [ForeignKey("UserCardId")]
        public UserCard? UserCard { get; set; }

        public DateTime EntryTime { get; set; } = DateTime.UtcNow;

        public DateTime? ExitTime { get; set; }
    }
}
