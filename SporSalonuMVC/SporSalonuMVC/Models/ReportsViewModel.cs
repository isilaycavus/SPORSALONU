using System.Collections.Generic;

namespace SporSalonuMVC.Models
{
    public class ReportsViewModel
    {
        public Dictionary<string, int> UsersPerMonth { get; set; } = new();
        public Dictionary<string, decimal> RevenuePerMonth { get; set; } = new();
        public Dictionary<string, int> PackageDistribution { get; set; } = new();
        public int ActiveMemberships { get; set; }
        public int ExpiredMemberships { get; set; }
    }
}
