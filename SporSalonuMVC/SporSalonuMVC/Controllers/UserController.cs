using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using SporSalonuMVC.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SporSalonuMVC.Controllers
{
    public class UserController : Controller
    {
        private readonly SporSalonuDbContext _context;

        public UserController(SporSalonuDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                ViewBag.ErrorMessage = string.Join(" | ", allErrors);
                return View(user);
            }

            try
            {
                user.Role = "User";
                _context.Users.Add(user);
                _context.SaveChanges();

                var cardNumber = GenerateCardNumber();

                var card = new UserCard
                {
                    UserId = user.Id,
                    CardNumber = cardNumber,
                    IsActive = true
                };
                _context.UserCards.Add(card);
                _context.SaveChanges();

                return RedirectToAction("Success", new { cardNumber = cardNumber });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                ViewBag.ErrorMessage = "HATA: " + msg;
                return View(user);
            }
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(LoginViewModel loginUser)
        {
            if (!ModelState.IsValid)
                return View(loginUser);

            var user = _context.Users
                .Include(u => u.UserCards)
                .FirstOrDefault(u => u.Email == loginUser.Email && u.Password == loginUser.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Geçersiz giriş bilgileri.");
                return View(loginUser);
            }

            // 🔥 BURAYA EKLE!
            HttpContext.Session.SetInt32("UserId", user.Id);

            HttpContext.Session.SetString("UserEmail", user.Email ?? string.Empty);
            HttpContext.Session.SetString("UserRole", user.Role ?? "User");
            HttpContext.Session.SetString("UserCard", user.UserCards?.FirstOrDefault()?.CardNumber ?? "");

            return RedirectToAction("Index", "Home");
        }


        public IActionResult Profile()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");

            var user = _context.Users
                .Include(u => u.UserCards)
                .FirstOrDefault(u => u.Email == email);

            return user == null ? RedirectToAction("Login") : View(user);
        }



        [HttpGet]
        public IActionResult Edit()
        {
            var cardNumber = HttpContext.Session.GetString("UserCard");
            if (string.IsNullOrEmpty(cardNumber))
                return RedirectToAction("Login");

            var user = _context.Users
                .Include(u => u.UserCards)
                .FirstOrDefault(u => u.UserCards!.Any(c => c.CardNumber == cardNumber));

            return user == null ? NotFound() : View(user);
        }

        [HttpPost]
        public IActionResult Edit(User updatedUser)
        {
            if (!ModelState.IsValid)
                return View(updatedUser);

            var user = _context.Users
                .Include(u => u.UserCards)
                .FirstOrDefault(u => u.Id == updatedUser.Id);

            if (user == null)
                return NotFound();

            user.FullName = updatedUser.FullName;
            user.Email = updatedUser.Email;

            _context.SaveChanges();

            TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
            return RedirectToAction("Profile");
        }

        public IActionResult Success()
        {
            var cardNumber = HttpContext.Request.Query["cardNumber"];
            ViewBag.CardNumber = cardNumber;
            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private string GenerateCardNumber()
        {
            var random = new Random();
            string cardNumber;
            do
            {
                cardNumber = string.Join("", Enumerable.Range(0, 16).Select(_ => random.Next(0, 10).ToString()));
            } while (_context.UserCards.Any(u => u.CardNumber == cardNumber));
            return cardNumber;
        }

        [HttpPost]
        public IActionResult UploadStudentDocument(IFormFile studentDocument)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "User")
                return Forbid();

            if (studentDocument == null || studentDocument.Length == 0)
            {
                TempData["UploadMessage"] = "Geçerli bir dosya seçiniz.";
                return RedirectToAction("Profile");
            }

            // 📌 Burada artık Email kullanıyoruz.
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login");

            var user = _context.Users
                .Include(u => u.UserCards)
                .FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
                return RedirectToAction("Login");

            var cardNumber = user.UserCards?.FirstOrDefault()?.CardNumber ?? "belirsiz_kart";

            var fileName = $"{cardNumber}_ogrenci_belgesi_{Path.GetFileName(studentDocument.FileName)}";
            var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "documents", fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                studentDocument.CopyTo(stream);
            }

            var doc = new StudentDocument
            {
                UserId = user.Id,
                FilePath = "/documents/" + fileName,
                Status = "Beklemede"
            };

            _context.StudentDocuments.Add(doc);
            _context.SaveChanges();

            TempData["UploadMessage"] = "Öğrenci belgesi başarıyla yüklendi.";
            return RedirectToAction("Profile");
        }


        public IActionResult MyDocuments()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login");

            var user = _context.Users
                .Include(u => u.StudentDocuments)
                .FirstOrDefault(u => u.Email == userEmail);

            if (user == null)
                return RedirectToAction("Login");

            var documents = user.StudentDocuments ?? new List<StudentDocument>();
            return View(documents);
        }
        [HttpPost]
        public async Task<IActionResult> UploadPhoto(IFormFile ProfilePhotoFile)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Email == userEmail);
            if (user == null)
                return RedirectToAction("Login");

            if (ProfilePhotoFile != null && ProfilePhotoFile.Length > 0)
            {
                // Eski fotoğraf varsa sil
                if (!string.IsNullOrEmpty(user.ProfilePhoto))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", user.ProfilePhoto);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ProfilePhotoFile.FileName);
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await ProfilePhotoFile.CopyToAsync(stream);
                }

                user.ProfilePhoto = fileName;
                _context.Update(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Profile");
        }

    }

}
