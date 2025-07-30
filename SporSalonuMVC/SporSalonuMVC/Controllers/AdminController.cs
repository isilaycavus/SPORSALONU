using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models;

public class AdminController : Controller
{
    private readonly SporSalonuDbContext _context;

    public AdminController(SporSalonuDbContext context)
    {
        _context = context;
    }

    public IActionResult Dashboard()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToAction("Login", "User");

        var memberships = _context.UserMemberships
            .Include(m => m.Membership)
            .ToList();

        var mostPopularPackage = memberships
            .Where(m => m.Membership != null)
            .GroupBy(m => m.Membership?.Name)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .FirstOrDefault();

        var now = DateTime.UtcNow;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var nextMonthStart = thisMonthStart.AddMonths(1);

        var monthlyRevenue = memberships
            .Where(um => um.StartDate >= thisMonthStart && um.StartDate < nextMonthStart)
            .Sum(um => (decimal?)um.Membership?.Price) ?? 0;

        var packageRevenue = memberships
            .Where(um => um.Membership != null)
            .GroupBy(um => um.Membership!.Name)
            .Select(g => new
            {
                PackageName = g.Key,
                TotalRevenue = g.Sum(x => x.Membership!.Price)
            })
            .ToDictionary(x => x.PackageName, x => x.TotalRevenue);

        var model = new AdminDashboardViewModel
        {
            TotalUsers = _context.Users.Count(),
            TotalTrainers = _context.Trainers.Count(),
            ActiveMemberships = memberships.Count(m => m.EndDate > DateTime.UtcNow),
            TodayClasses = _context.Classes.Count(c => c.Schedule.Date == DateTime.UtcNow.Date),
            MostPopularPackage = mostPopularPackage!,
            MonthlyRevenue = monthlyRevenue,
            PackageRevenue = packageRevenue
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Documents()
    {
        var documents = _context.StudentDocuments
            .Include(d => d.User)
            .OrderByDescending(d => d.UploadedAt)
            .ToList();

        return View(documents);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ApproveDocument(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return Unauthorized();

        var doc = _context.StudentDocuments.FirstOrDefault(d => d.Id == id);
        if (doc != null)
        {
            doc.Status = "Onaylandı";
            _context.SaveChanges();
        }

        return RedirectToAction("Documents");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectDocument(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return Unauthorized();

        var doc = _context.StudentDocuments.FirstOrDefault(d => d.Id == id);
        if (doc != null)
        {
            doc.Status = "Reddedildi";
            _context.SaveChanges();
        }

        return RedirectToAction("Documents");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteDocument(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return Unauthorized();

        var doc = _context.StudentDocuments.FirstOrDefault(d => d.Id == id);
        if (doc == null)
            return NotFound();

        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", doc.FilePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }

        _context.StudentDocuments.Remove(doc);
        _context.SaveChanges();

        TempData["Success"] = "Belge başarıyla silindi.";
        return RedirectToAction("Documents");
    }

    [HttpGet]
    public IActionResult Reports()
    {
        var userMemberships = _context.UserMemberships
            .Include(u => u.Membership)
            .ToList(); // 🔥 LINQ hatasını engellemek için

        // Kullanıcıların aylara göre sayısı
        var usersPerMonth = userMemberships
            .GroupBy(u => new { u.StartDate.Year, u.StartDate.Month })
            .Select(g => new
            {
                Month = $"{g.Key.Year}-{g.Key.Month.ToString("D2")}",
                Count = g.Select(x => x.UserId).Distinct().Count()
            })
            .OrderBy(x => x.Month)
            .ToDictionary(x => x.Month, x => x.Count);

        // Aylık gelir
        var revenuePerMonth = userMemberships
            .Where(u => u.Membership != null)
            .GroupBy(u => new { u.StartDate.Year, u.StartDate.Month })
            .Select(g => new
            {
                Month = $"{g.Key.Year}-{g.Key.Month.ToString("D2")}",
                Total = g.Sum(x => x.Membership!.Price)
            })
            .OrderBy(x => x.Month)
            .ToDictionary(x => x.Month, x => x.Total);

        // Paket dağılımı
        var packageDistribution = userMemberships
            .Where(u => u.Membership != null)
            .GroupBy(u => u.Membership!.Name)
            .Select(g => new { PackageName = g.Key, Count = g.Count() })
            .ToDictionary(x => x.PackageName, x => x.Count);

        var now = DateTime.UtcNow;
        var active = userMemberships.Count(u => u.EndDate > now && !u.IsCancelled);
        var expired = userMemberships.Count(u => u.EndDate <= now || u.IsCancelled);

        var model = new ReportsViewModel
        {
            UsersPerMonth = usersPerMonth,
            RevenuePerMonth = revenuePerMonth,
            PackageDistribution = packageDistribution,
            ActiveMemberships = active,
            ExpiredMemberships = expired
        };

        return View(model);
    }
    public IActionResult EntryLogs()
    {
        var logs = _context.EntryLogs
            .Include(e => e.UserCard)
            .ThenInclude(c => c!.User)
            .OrderByDescending(e => e.EntryTime)
            .ToList();

        return View(logs);
    }

}
