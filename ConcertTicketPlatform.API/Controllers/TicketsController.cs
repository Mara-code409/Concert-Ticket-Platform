using System.Security.Claims;
using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TicketsController> _logger;

        public TicketsController(AppDbContext context, ILogger<TicketsController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyTickets()
        {
            var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tickets = await _context.Tickets
                .Where(t => t.UserId == userId)
                .Select(t => new
                {
                    t.Id,
                    t.SeatNumber,
                    t.IsUsed,
                    t.PurchasedAt,
                    ConcertTitle = t.Concert.Title,
                    ConcertDate  = t.Concert.Date,
                    ConcertPrice = t.Concert.Price
                })
                .OrderByDescending(t => t.PurchasedAt)
                .ToListAsync();

            return Ok(tickets);
        }

        [HttpPost("buy")]
        public async Task<IActionResult> BuyTicket([FromBody] BuyTicketDto dto)
        {
            var concert = await _context.Concerts.FindAsync(dto.ConcertId);
            if (concert == null) return NotFound(new { message = "Concertul nu există." });

            if (concert.AvailableSeats <= 0)
                return BadRequest(new { message = "Nu mai sunt locuri disponibile." });

            var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            var ticket = new Ticket
            {
                ConcertId   = dto.ConcertId,
                UserId      = userId!,
                SeatNumber  = $"A{concert.TotalSeats - concert.AvailableSeats + 1}",
                PurchasedAt = DateTime.UtcNow,
                IsUsed      = false
            };

            concert.AvailableSeats--;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Ticket bought: Concert {ConcertId}, User {UserId}", dto.ConcertId, userId);
            return Ok(new { ticket.Id, ticket.SeatNumber, message = "Bilet cumpărat cu succes!" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record BuyTicketDto(int ConcertId);
}
