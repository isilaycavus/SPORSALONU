using Microsoft.AspNetCore.Mvc;
using SporSalonuMVC.Models;
using System.Linq;

namespace SporSalonuMVC.Controllers
{
    public class EntryController : Controller
    {
        private readonly SporSalonuDbContext _context;

        public EntryController(SporSalonuDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
            {
                ViewBag.Message = "Kart numarası boş olamaz.";
                return View();
            }

            var card = _context.UserCards.FirstOrDefault(c => c.CardNumber == cardNumber && c.IsActive);
            if (card == null)
            {
                ViewBag.Message = "Kart numarası geçersiz veya aktif değil.";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == card.UserId);
            if (user == null)
            {
                ViewBag.Message = "Kart numarası bir kullanıcıya ait değil.";
                return View();
            }

            // 🧠 1. Çıkış yapılmamış eski kayıt var mı?
            var existingLog = _context.EntryLogs
                .Where(e => e.UserCardId == card.Id && e.ExitTime == null)
                .OrderByDescending(e => e.EntryTime)
                .FirstOrDefault();

            if (existingLog != null)
            {
                // ⏱ 2. Eğer varsa çıkış saatini doldur
                existingLog.ExitTime = DateTime.UtcNow;
                _context.EntryLogs.Update(existingLog);
                _context.SaveChanges();

                ViewBag.Message = $"Güle güle {user.FullName}, çıkış saatiniz: {existingLog.ExitTime?.ToLocalTime():HH:mm}";
            }
            else
            {
                // 🚪 3. Eğer yoksa yeni giriş kaydı oluştur
                var newEntry = new EntryLog
                {
                    UserCardId = card.Id,
                    EntryTime = DateTime.UtcNow
                };

                _context.EntryLogs.Add(newEntry);
                _context.SaveChanges();

                ViewBag.Message = $"Hoşgeldiniz, {user.FullName}. Giriş saatiniz: {newEntry.EntryTime.ToLocalTime():HH:mm}";
            }

            return View();
        }

    }
}
