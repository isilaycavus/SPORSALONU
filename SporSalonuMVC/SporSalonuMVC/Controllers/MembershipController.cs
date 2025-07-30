using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models;

public class MembershipController : Controller
{
    private readonly SporSalonuDbContext _context;

    public MembershipController(SporSalonuDbContext context)
    {
        _context = context;
    }

    private bool UserIsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    public IActionResult Index()
    {
        if (!UserIsAdmin()) return RedirectToAction("Login", "User");
        var memberships = _context.Memberships.ToList();
        return View(memberships);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!UserIsAdmin()) return RedirectToAction("Login", "User");
        return View();
    }

    [HttpPost]
    public IActionResult Create(Membership membership)
    {
        if (!UserIsAdmin()) return RedirectToAction("Login", "User");

        if (!ModelState.IsValid) return View(membership);

        _context.Memberships.Add(membership);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        if (!UserIsAdmin()) return RedirectToAction("Login", "User");

        var membership = _context.Memberships.Find(id);
        if (membership == null) return NotFound();

        return View(membership);
    }

    [HttpPost]
    public IActionResult Edit(Membership membership)
    {
        if (!UserIsAdmin()) return RedirectToAction("Login", "User");

        if (!ModelState.IsValid) return View(membership);

        _context.Memberships.Update(membership);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (!UserIsAdmin()) return RedirectToAction("Login", "User");

        var membership = _context.Memberships.Find(id);
        if (membership != null)
        {
            _context.Memberships.Remove(membership);
            _context.SaveChanges();
        }

        return RedirectToAction("Index");
    }

    public IActionResult Available()
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (email == null) return RedirectToAction("Login", "User");

        var memberships = _context.Memberships.ToList();
        return View(memberships);
    }
    [HttpPost]
    public IActionResult Buy(int id)
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (email == null) return RedirectToAction("Login", "User");

        var user = _context.Users
            .Include(u => u.StudentDocuments)
            .FirstOrDefault(u => u.Email == email);

        if (user == null) return RedirectToAction("Login", "User");

        var membership = _context.Memberships.FirstOrDefault(m => m.Id == id);
        if (membership == null) return NotFound();

        // Eğer öğrenci paketi ise ve kullanıcı onaylı değilse → engelle
        if (membership.Name.ToLower().Contains("öğrenci") && !user.IsStudentVerified)
        {
            TempData["Error"] = "Bu öğrenci paketini yalnızca onaylı öğrenciler satın alabilir.";
            return RedirectToAction("Available");
        }

        var existing = _context.UserMemberships
            .Where(um => um.UserId == user.Id && um.EndDate > DateTime.UtcNow)
            .FirstOrDefault();

        if (existing != null)
        {
            TempData["Error"] = $"Zaten aktif bir üyeliğiniz var. Bitiş tarihi: {existing.EndDate:d}";
            return RedirectToAction("Available");
        }

        var userMembership = new UserMembership
        {
            UserId = user.Id,
            MembershipId = membership.Id,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(membership.DurationDays)
        };

        _context.UserMemberships.Add(userMembership);
        _context.SaveChanges();

        return RedirectToAction("MyMembership");
    }

    public IActionResult MyMembership()
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (email == null) return RedirectToAction("Login", "User");

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return RedirectToAction("Login", "User");

        var active = _context.UserMemberships
            .Include(um => um.Membership)
            .Where(um => um.UserId == user.Id && um.EndDate > DateTime.UtcNow)
            .OrderByDescending(um => um.EndDate)
            .FirstOrDefault();

        return View(active);
    }

    public IActionResult History()
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (email == null) return RedirectToAction("Login", "User");

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return RedirectToAction("Login", "User");

        var memberships = _context.UserMemberships
            .Include(um => um.Membership)
            .Where(um => um.UserId == user.Id)
            .OrderByDescending(um => um.StartDate)
            .ToList();

        return View(memberships);
    }

    public IActionResult AllUserMemberships()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToAction("Login", "User");

        var all = _context.UserMemberships
            .Include(um => um.User)
            .Include(um => um.Membership)
            .OrderByDescending(um => um.StartDate)
            .ToList();

        return View(all);
    }
    [HttpPost]
    public IActionResult DeleteUserMembership(int id)
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToAction("Login", "User");

        var userMembership = _context.UserMemberships.Find(id);
        if (userMembership != null)
        {
            userMembership.IsCancelled = true; // ❗ Artık silmek yerine iptal ediyoruz
            _context.SaveChanges();

            TempData["SuccessMembership"] = "Üyelik başarıyla iptal edildi.";
        }
        else
        {
            TempData["ErrorMembership"] = "Üyelik bulunamadı.";
        }

        return RedirectToAction("AllUserMemberships");
    }


}
