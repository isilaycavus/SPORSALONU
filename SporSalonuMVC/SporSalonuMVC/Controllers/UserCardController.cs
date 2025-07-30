using Microsoft.AspNetCore.Mvc;
using SporSalonuMVC.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace SporSalonuMVC.Controllers
{
    public class UserCardController : Controller
    {
        private readonly SporSalonuDbContext _context;

        public UserCardController(SporSalonuDbContext context)
        {
            _context = context;
        }

        public IActionResult MyCards()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            var cards = _context.UserCards
                .Where(c => c.UserId == userId)
                .ToList();

            return View(cards);
        }
    }
}
