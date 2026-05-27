using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.Web.Controllers
{
    public class ReviewsController : Controller
    {
        private readonly AppDbContext         _db;
        private readonly UserManager<AppUser> _userManager;

        public ReviewsController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db          = db;
            _userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int concertId, int rating, string comment)
        {
            if (rating < 1 || rating > 5)
                ModelState.AddModelError("", "Rating-ul trebuie să fie între 1 și 5.");

            if (string.IsNullOrWhiteSpace(comment))
                ModelState.AddModelError("", "Comentariul nu poate fi gol.");

            if (comment?.Length > 500)
                ModelState.AddModelError("", "Comentariul nu poate depăși 500 de caractere.");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = string.Join(" | ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return RedirectToAction("MyTickets", "Account");
            }

            var concert = await _db.Concerts.FindAsync(concertId);
            if (concert == null) return NotFound();

            if (concert.Date > DateTime.UtcNow)
            {
                TempData["Error"] = "Poți lăsa o recenzie doar pentru un concert la care ai participat deja.";
                return RedirectToAction("MyTickets", "Account");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var hasTicket = await _db.Tickets
                .AnyAsync(t => t.UserId == user.Id && t.ConcertId == concertId);
            if (!hasTicket)
            {
                TempData["Error"] = "Trebuie să ai bilet la acest concert pentru a lăsa o recenzie.";
                return RedirectToAction("MyTickets", "Account");
            }

            var alreadyReviewed = await _db.Reviews
                .AnyAsync(r => r.ConcertId == concertId && r.UserId == user.Id);
            if (alreadyReviewed)
            {
                TempData["Error"] = "Ai lăsat deja o recenzie pentru acest concert.";
                return RedirectToAction("MyTickets", "Account");
            }

            var review = new Review
            {
                ConcertId = concertId,
                Rating    = rating,
                Comment   = comment!.Trim(),
                UserId    = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _db.Reviews.Add(review);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Recenzia ta pentru \"{concert.Title}\" a fost adaugata!";
            return RedirectToAction("MyTickets", "Account");
        }
    }
}
