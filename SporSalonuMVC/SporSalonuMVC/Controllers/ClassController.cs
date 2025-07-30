using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models;
using SporSalonuMVC.ViewModels;

public class ClassController : Controller
{
    private readonly SporSalonuDbContext _context;

    public ClassController(SporSalonuDbContext context)
    {
        _context = context;
    }

    private bool UserIsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    public IActionResult Index()
    {
        var now = DateTime.UtcNow;

        var classes = _context.Classes
            .Include(c => c.Trainer)
            .Include(c => c.Reservations)
            .Where(c => c.Schedule > now)
            .OrderBy(c => c.Schedule)
            .ToList();

        foreach (var cls in classes)
        {
            cls.Reservations ??= new List<Reservation>();
        }

        var email = HttpContext.Session.GetString("UserEmail");
        var user = _context.Users.FirstOrDefault(u => u.Email == email);

        // Kullanıcıysa ve aktif üyeliği yoksa uyarı ver
        if (!UserIsAdmin() && user != null)
        {
            var hasActiveMembership = _context.UserMemberships
                .Any(um => um.UserId == user.Id && um.EndDate >= DateTime.UtcNow);

            if (!hasActiveMembership)
            {
                TempData["ErrorClass"] = "Rezervasyon yapabilmek için aktif bir üyelik paketiniz olmalı.";
            }
        }

        var reservedClassIds = user != null
            ? _context.Reservations
                .Where(r => r.UserId == user.Id)
                .Select(r => r.ClassId)
                .ToList()
            : new List<int>();

        ViewBag.UserReservedClassIds = reservedClassIds;
        ViewBag.IsAdmin = UserIsAdmin();

        return View(classes);
    }
    [HttpGet]
    public IActionResult Create()
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var model = new ClassCreateViewModel
        {
            AvailableTrainers = _context.Trainers.ToList(),
            HourOptions = Enumerable.Range(9, 15).Select(h => h.ToString("D2") + ":00").ToList()
        };

        // 🔍 Saatleri konsola yaz
        Console.WriteLine("🚀 HourOptions: " + string.Join(", ", model.HourOptions));

        return View(model);
    }

    [HttpPost]
    public IActionResult Create(ClassCreateViewModel model)
    {
        if (model.Date < DateTime.Today || model.Date > DateTime.Today.AddDays(7))
        {
            ModelState.AddModelError("Date", "Sadece bugünden itibaren 7 gün içinde ders oluşturabilirsiniz.");
            model.AvailableTrainers = _context.Trainers.ToList();
            model.HourOptions = Enumerable.Range(9, 15).Select(h => h.ToString("D2") + ":00").ToList();
            return View(model);
        }

        if (!UserIsAdmin()) return Unauthorized();

        if (!ModelState.IsValid)
        {
            model.AvailableTrainers = _context.Trainers.ToList();
            model.HourOptions = Enumerable.Range(9, 15).Select(h => h.ToString("D2") + ":00").ToList();
            return View(model);
        }

        // Local datetime oluştur (saat seçimi + tarih birleştir)
        var selectedDateTime = model.Date.Add(TimeSpan.Parse(model.SelectedHour));

        // ❗ PostgreSQL için UTC olarak ayarla
        selectedDateTime = DateTime.SpecifyKind(selectedDateTime, DateTimeKind.Utc);

        // ❗ Aynı tarihte aynı saat için çakışma kontrolü
        bool sameClassExists = _context.Classes
     .Any(c => c.Schedule == selectedDateTime && c.Name == model.Name);

        bool sameTrainerConflict = _context.Classes
    .Any(c => c.Schedule == selectedDateTime && c.TrainerId == model.TrainerId);

        if (sameTrainerConflict)
        {
            ModelState.AddModelError("", "Bu eğitmen bu tarih ve saatte başka bir derste görevlidir.");
            model.AvailableTrainers = _context.Trainers.ToList();
            model.HourOptions = Enumerable.Range(9, 15).Select(h => h.ToString("D2") + ":00").ToList();
            return View(model);
        }


        if (sameClassExists)
        {
            ModelState.AddModelError("", "Bu tarih ve saatte aynı isimde bir ders zaten mevcut.");
            model.AvailableTrainers = _context.Trainers.ToList();
            model.HourOptions = Enumerable.Range(9, 15).Select(h => h.ToString("D2") + ":00").ToList();
            return View(model);
        }


        // Yeni ders oluştur
        var newClass = new Class
        {
            Name = model.Name,
            Schedule = selectedDateTime,
            Description = model.Description,
            Capacity = model.Capacity,
            TrainerId = model.TrainerId
        };

        _context.Classes.Add(newClass);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }



    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var cls = _context.Classes
            .Include(c => c.Reservations)
            .FirstOrDefault(c => c.Id == id);

        if (cls == null)
        {
            TempData["ErrorClass"] = "Ders bulunamadı.";
            return RedirectToAction("Index");
        }

        if (cls.Reservations != null && cls.Reservations.Any())
        {
            _context.Reservations.RemoveRange(cls.Reservations);
        }

        _context.Classes.Remove(cls);
        _context.SaveChanges();

        TempData["SuccessClass"] = "Ders başarıyla silindi.";
        return RedirectToAction("Index");
    }
    [HttpGet]
    public IActionResult Edit(int id)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var cls = _context.Classes.FirstOrDefault(c => c.Id == id);
        if (cls == null) return NotFound();

        var model = new ClassCreateViewModel
        {
            Id = cls.Id,
            Name = cls.Name,
            TrainerId = cls.TrainerId,
            // ❌ ToLocalTime() KALDIRILDI
            Date = cls.Schedule.Date,
            SelectedHour = cls.Schedule.ToString("HH:mm"),
            Description = cls.Description,
            Capacity = cls.Capacity,
            AvailableTrainers = _context.Trainers.ToList(),
            HourOptions = Enumerable.Range(9, 15).Select(h => $"{h:00}:00").ToList()
        };

        return View(model);
    }


    [HttpPost]
    public IActionResult Edit(ClassCreateViewModel model)
    {
        if (model.Date < DateTime.Today || model.Date > DateTime.Today.AddDays(7))
        {
            ModelState.AddModelError("Date", "Sadece bugünden itibaren 7 gün içinde ders düzenleyebilirsiniz.");
            model.AvailableTrainers = _context.Trainers.ToList();
            model.HourOptions = Enumerable.Range(9, 15).Select(h => h.ToString("D2") + ":00").ToList();
            return View(model);
        }

        if (!UserIsAdmin()) return Unauthorized();

        if (!ModelState.IsValid)
        {
            model.AvailableTrainers = _context.Trainers.ToList();
            model.HourOptions = Enumerable.Range(9, 15).Select(h => h.ToString("D2") + ":00").ToList();
            return View(model);
        }

        // Local saat bilgisi alınıyor
        var selectedDateTime = model.Date.Add(TimeSpan.Parse(model.SelectedHour));
        selectedDateTime = DateTime.SpecifyKind(selectedDateTime, DateTimeKind.Utc);

        // Aynı gün ve saatte başka ders var mı (bu ders harici)
        bool sameClassExists = _context.Classes
     .Any(c => c.Id != model.Id && c.Schedule == selectedDateTime && c.Name == model.Name);

        bool sameTrainerConflict = _context.Classes
    .Any(c => c.Id != model.Id && c.Schedule == selectedDateTime && c.TrainerId == model.TrainerId);

        if (sameTrainerConflict)
        {
            ModelState.AddModelError("", "Bu eğitmen bu tarih ve saatte başka bir derste görevlidir.");
            model.AvailableTrainers = _context.Trainers.ToList();
            model.HourOptions = Enumerable.Range(9, 15).Select(h => h.ToString("D2") + ":00").ToList();
            return View(model);
        }

        if (sameClassExists)
        {
            ModelState.AddModelError("", "Bu tarih ve saatte aynı isimde bir ders zaten mevcut.");
            model.AvailableTrainers = _context.Trainers.ToList();
            model.HourOptions = Enumerable.Range(9, 15).Select(h => h.ToString("D2") + ":00").ToList();
            return View(model);
        }

        var cls = _context.Classes.FirstOrDefault(c => c.Id == model.Id);
        if (cls == null) return NotFound();

        cls.Name = model.Name;
        cls.Schedule = selectedDateTime;
        cls.Description = model.Description;
        cls.Capacity = model.Capacity;
        cls.TrainerId = model.TrainerId;

        _context.SaveChanges();

        return RedirectToAction("Index");
    }


}
