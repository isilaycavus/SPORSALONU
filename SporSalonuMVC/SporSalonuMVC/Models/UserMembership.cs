using System.ComponentModel.DataAnnotations.Schema;

namespace SporSalonuMVC.Models
{
    public class UserMembership
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public int MembershipId { get; set; }

        [ForeignKey("MembershipId")]
        public Membership? Membership { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime EndDate { get; set; }
        public bool IsCancelled { get; set; } = false;

    }
}
