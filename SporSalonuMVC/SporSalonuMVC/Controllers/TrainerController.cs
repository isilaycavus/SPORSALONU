using Microsoft.AspNetCore.Mvc;
using SporSalonuMVC.Models;
using Microsoft.EntityFrameworkCore; // 👈 Include için şart


public class TrainerController : Controller
{
    private readonly SporSalonuDbContext _context;

    public TrainerController(SporSalonuDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var trainers = _context.Trainers.ToList();
        return View(trainers);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        return View();
    }

    [HttpPost]
    public IActionResult Create(Trainer trainer)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        if (!ModelState.IsValid)
            return View(trainer);

        _context.Trainers.Add(trainer);
        _context.SaveChanges();
        return RedirectToAction("Index");
    }

    private bool UserIsAdmin()
    {
        return HttpContext.Session.GetString("UserRole") == "Admin";
    }

    public IActionResult Calendar(int id)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var trainer = _context.Trainers
            .Include(t => t.Classes)
            .FirstOrDefault(t => t.Id == id);

        if (trainer == null)
            return NotFound();

        var classes = trainer.Classes
            .Where(c => c.Schedule >= DateTime.UtcNow) // 👈 sadece gelecektekiler
            .ToList();

        ViewBag.TrainerName = trainer.FullName;
        return View(classes);
    }




    // ✅ Silme işlemi
    [HttpPost]
    public IActionResult Delete(int id)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var trainer = _context.Trainers.FirstOrDefault(t => t.Id == id);
        if (trainer == null)
        {
            TempData["ErrorTrainer"] = "Eğitmen bulunamadı.";
            return RedirectToAction("Index");
        }

        var hasClasses = _context.Classes.Any(c => c.TrainerId == id);
        if (hasClasses)
        {
            TempData["ErrorTrainer"] = "Bu eğitmenin dersleri bulunduğu için silinemez.";
            return RedirectToAction("Index");
        }

        _context.Trainers.Remove(trainer);
        _context.SaveChanges();

        TempData["SuccessTrainer"] = "Eğitmen başarıyla silindi.";
        return RedirectToAction("Index");

    }
    [HttpGet]
    public IActionResult Edit(int id)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        var trainer = _context.Trainers.FirstOrDefault(t => t.Id == id);
        if (trainer == null)
        {
            TempData["ErrorTrainer"] = "Eğitmen bulunamadı.";
            return RedirectToAction("Index");
        }

        return View(trainer);
    }

    [HttpPost]
    public IActionResult Edit(Trainer trainer)
    {
        if (!UserIsAdmin())
            return RedirectToAction("Login", "User");

        if (!ModelState.IsValid)
            return View(trainer);

        var existingTrainer = _context.Trainers.FirstOrDefault(t => t.Id == trainer.Id);
        if (existingTrainer == null)
        {
            TempData["ErrorTrainer"] = "Eğitmen bulunamadı.";
            return RedirectToAction("Index");
        }

        existingTrainer.FullName = trainer.FullName;
        existingTrainer.Specialty = trainer.Specialty;
        existingTrainer.Description = trainer.Description;

        _context.SaveChanges();

        TempData["SuccessTrainer"] = "Eğitmen başarıyla güncellendi.";
        return RedirectToAction("Index");
    }

}
