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
    public class ReviewsController : ControllerBase
    {
        private readonly AppDbContext               _context;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(AppDbContext context, ILogger<ReviewsController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? concertId, [FromQuery] int? artistId)
        {
            var query = _context.Reviews.AsQueryable();

            if (concertId.HasValue)
                query = query.Where(r => r.ConcertId == concertId.Value);

            if (artistId.HasValue)
                query = query.Where(r => r.Concert.ArtistId == artistId.Value
                                      && r.Concert.Date < DateTime.UtcNow);

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.ConcertId,
                    ConcertTitle = r.Concert.Title,
                    CreatedAt    = r.CreatedAt,
                    UserName     = r.User != null
                                    ? (r.User.FullName != "" ? r.User.FullName : r.User.Email)
                                    : "Anonim"
                })
                .ToListAsync();

            return Ok(reviews);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return BadRequest(new { message = "Rating-ul trebuie să fie între 1 și 5." });

            if (string.IsNullOrWhiteSpace(dto.Comment))
                return BadRequest(new { message = "Comentariul nu poate fi gol." });

            var concertExists = await _context.Concerts.AnyAsync(c => c.Id == dto.ConcertId);
            if (!concertExists) return NotFound(new { message = "Concertul nu există." });

            var userId = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);

            var review = new Review
            {
                ConcertId = dto.ConcertId,
                Rating    = dto.Rating,
                Comment   = dto.Comment.Trim(),
                UserId    = userId!,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Review added for concert {ConcertId}", dto.ConcertId);

            return CreatedAtAction(nameof(GetAll), new { concertId = review.ConcertId },
                new { review.Id, review.Rating, review.Comment });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return NotFound();
            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

    public record CreateReviewDto(int ConcertId, int Rating, string Comment);
}
