using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models;

public class ReservationController : Controller
{
    private readonly SporSalonuDbContext _context;

    public ReservationController(SporSalonuDbContext context)
    {
        _context = context;
    }

    public IActionResult MyReservations()
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (email == null)
            return RedirectToAction("Login", "User");

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
            return RedirectToAction("Login", "User");

        var reservations = _context.Reservations
            .Include(r => r.Class)
            .ThenInclude(c => c!.Trainer)
            .Where(r => r.UserId == user.Id)
            .ToList();

        return View(reservations);
    }

    [HttpPost]
    public IActionResult Create(int classId)
    {
        var email = HttpContext.Session.GetString("UserEmail");
        if (email == null) return RedirectToAction("Login", "User");

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null) return RedirectToAction("Login", "User");

        var classItem = _context.Classes.FirstOrDefault(c => c.Id == classId);
        if (classItem == null)
        {
            TempData["ErrorReservation"] = "Ders bulunamadı.";
            return RedirectToAction("Index", "Class");
        }

        var activeMembership = _context.UserMemberships
            .FirstOrDefault(um => um.UserId == user.Id && um.EndDate > DateTime.UtcNow);

        if (activeMembership == null)
        {
            TempData["ErrorReservation"] = "Rezervasyon yapabilmek için aktif bir üyelik paketiniz olmalı.";
            return RedirectToAction("Index", "Class");
        }

        var alreadyReserved = _context.Reservations.Any(r => r.ClassId == classId && r.UserId == user.Id);
        if (alreadyReserved)
        {
            TempData["ErrorReservation"] = "Bu derse zaten rezervasyon yaptınız.";
            return RedirectToAction("Index", "Class");
        }

        var sameTimeReservation = _context.Reservations
            .Include(r => r.Class)
            .Any(r => r.UserId == user.Id && r.Class!.Schedule == classItem.Schedule);

        if (sameTimeReservation)
        {
            TempData["ErrorReservation"] = "Bu tarih ve saatte başka bir rezervasyonunuz zaten var.";
            return RedirectToAction("Index", "Class");
        }

        var reservation = new Reservation
        {
            UserId = user.Id,
            ClassId = classId
        };

        _context.Reservations.Add(reservation);
        _context.SaveChanges();

        TempData["SuccessReservation"] = "Rezervasyon başarıyla oluşturuldu.";

        return RedirectToAction("Index", "Class"); // ✅ Doğru satır
    }

    [HttpPost]
    public IActionResult Cancel(int id)
    {
        var email = HttpContext.Session.GetString("UserEmail");
        var role = HttpContext.Session.GetString("UserRole");

        if (email == null)
            return RedirectToAction("Login", "User");

        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        if (user == null)
            return RedirectToAction("Login", "User");

        var reservation = _context.Reservations.FirstOrDefault(r => r.Id == id);
        if (reservation == null)
            return NotFound();

        // Admin ise veya kendi rezervasyonunu iptal ediyorsa
        if (role == "Admin" || reservation.UserId == user.Id)
        {
            _context.Reservations.Remove(reservation);
            _context.SaveChanges();

            if (role == "Admin")
                return RedirectToAction("AllReservations");
            else
                return RedirectToAction("MyReservations");
        }

        return Unauthorized();
    }



    public IActionResult AllReservations()
    {
        if (HttpContext.Session.GetString("UserRole") != "Admin")
            return RedirectToAction("Login", "User");

        var reservations = _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Class)
                .ThenInclude(c => c!.Trainer)
            .ToList();

        return View(reservations);
    }
}
