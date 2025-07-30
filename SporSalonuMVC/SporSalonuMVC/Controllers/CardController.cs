using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SporSalonuMVC.Models;

namespace SporSalonuMVC.Controllers
{
    public class CardController : Controller
    {
        private readonly SporSalonuDbContext _context;

        public CardController(SporSalonuDbContext context)
        {
            _context = context;
        }

        // Tüm kullanıcıları ve kartlarını listele
        public IActionResult Manage()
        {
            var users = _context.Users
                .Include(u => u.UserCards)
                .ToList();

            return View(users);
        }

        // Kart ekleme (AJAX)
        [HttpPost]
        public IActionResult Create(int userId, string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber) || cardNumber.Length != 16)
                return BadRequest("Kart numarası 16 haneli olmalıdır.");

            if (_context.UserCards.Any(c => c.CardNumber == cardNumber))
                return Conflict("Bu kart numarası zaten kayıtlı.");

            var card = new UserCard
            {
                UserId = userId,
                CardNumber = cardNumber,
                IsActive = true
            };

            _context.UserCards.Add(card);
            _context.SaveChanges();

            return Ok();
        }

        // Kart silme (AJAX)
        [HttpPost]
        public IActionResult Delete(int cardId)
        {
            var card = _context.UserCards.Find(cardId);
            if (card == null)
                return NotFound();

            _context.UserCards.Remove(card);
            _context.SaveChanges();

            return Ok();
        }
    }
}
