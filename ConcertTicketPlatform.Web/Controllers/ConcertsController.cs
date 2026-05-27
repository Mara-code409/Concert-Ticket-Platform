using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.Web.Controllers
{
    public class ConcertsController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public ConcertsController(AppDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index() => View();

        public async Task<IActionResult> Details(int id)
        {
            var concert = await _db.Concerts.FindAsync(id);
            if (concert == null) return NotFound();

            var isPast = concert.Date < DateTime.UtcNow;
            ViewData["ConcertId"]  = id;
            ViewData["ArtistId"]   = concert.ArtistId;
            ViewData["IsPast"]     = isPast;
            ViewData["HasTicket"]  = false;
            ViewData["HasReview"]  = false;

            if (User.Identity!.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    ViewData["HasTicket"] = await _db.Tickets
                        .AnyAsync(t => t.UserId == user.Id && t.ConcertId == id);
                    ViewData["HasReview"] = await _db.Reviews
                        .AnyAsync(r => r.UserId == user.Id && r.ConcertId == id);
                }
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BuyTicket(int id)
        {
            var concert = await _db.Concerts.FindAsync(id);
            if (concert == null) return NotFound();

            if (concert.Date < DateTime.UtcNow)
            {
                TempData["Error"] = "Nu poți cumpăra bilet la un concert care a avut deja loc.";
                return RedirectToAction("Details", new { id });
            }

            if (concert.AvailableSeats <= 0)
            {
                TempData["Error"] = "Nu mai sunt locuri disponibile pentru acest concert.";
                return RedirectToAction("Details", new { id });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var ticket = new Ticket
            {
                ConcertId   = id,
                UserId      = user.Id,
                SeatNumber  = $"A{concert.TotalSeats - concert.AvailableSeats + 1}",
                PurchasedAt = DateTime.UtcNow,
                IsUsed      = false
            };

            concert.AvailableSeats--;
            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Bilet cumpărat cu succes! Locul tău: {ticket.SeatNumber}";
            return RedirectToAction("MyTickets", "Account");
        }
    }
}
