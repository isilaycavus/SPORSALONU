public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalTrainers { get; set; }
    public int ActiveMemberships { get; set; }
    public int TodayClasses { get; set; }
    public string MostPopularPackage { get; set; } = string.Empty;
    public decimal MonthlyRevenue { get; set; }
    public Dictionary<string, decimal> PackageRevenue { get; set; } = new();
}
