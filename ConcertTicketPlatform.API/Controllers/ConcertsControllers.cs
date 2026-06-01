using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Core.Interfaces;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConcertsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConcertService _concertService;
        private readonly ILogger<ConcertsController> _logger;

        public ConcertsController(AppDbContext context, IConcertService concertService, ILogger<ConcertsController> logger)
        {
            _context        = context;
            _concertService = concertService;
            _logger         = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? category,
            [FromQuery] string? city,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] bool? availableOnly)
        {
            _logger.LogInformation("Getting all concerts");

            var query = _context.Concerts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Title.Contains(search) || c.Artist.Name.Contains(search));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(c => c.Category.Name == category);

            if (!string.IsNullOrWhiteSpace(city))
                query = query.Where(c => c.Venue.City == city);

            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);

            if (dateFrom.HasValue)
                query = query.Where(c => c.Date >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(c => c.Date <= dateTo.Value);

            if (availableOnly == true)
                query = query.Where(c => c.AvailableSeats > 0);

            var concerts = await query
                .OrderBy(c => c.Date)
                .Select(c => new {
                    c.Id,
                    c.Title,
                    c.Date,
                    c.Price,
                    c.TotalSeats,
                    c.AvailableSeats,
                    ArtistName = c.Artist.Name,
                    ArtistGenre = c.Artist.Genre,
                    VenueName = c.Venue.Name,
                    VenueCity = c.Venue.City,
                    CategoryName = c.Category.Name
                })
                .ToListAsync();

            return Ok(concerts);
        }

        [HttpGet("filters")]
        public async Task<IActionResult> GetFilterOptions()
        {
            var categories = await _context.Categories.Select(c => c.Name).OrderBy(n => n).ToListAsync();
            var cities = await _context.Venues.Select(v => v.City).Distinct().OrderBy(c => c).ToListAsync();
            var maxPrice = await _context.Concerts.MaxAsync(c => (decimal?)c.Price) ?? 0;
            return Ok(new { categories, cities, maxPrice });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var concert = await _context.Concerts
                .Where(c => c.Id == id)
                .Select(c => new {
                    c.Id,
                    c.Title,
                    c.Date,
                    c.Price,
                    c.TotalSeats,
                    c.AvailableSeats,
                    c.ArtistId,
                    ArtistName = c.Artist.Name,
                    ArtistGenre = c.Artist.Genre,
                    ArtistBio = c.Artist.Bio,
                    VenueName = c.Venue.Name,
                    VenueCity = c.Venue.City,
                    VenueAddress = c.Venue.Address,
                    CategoryName = c.Category.Name
                })
                .FirstOrDefaultAsync();
            if (concert == null) return NotFound();
            return Ok(concert);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Concert concert)
        {
            var created = await _concertService.CreateAsync(concert);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { created.Id, created.Title });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Concert concert)
        {
            if (id != concert.Id) return BadRequest();
            await _concertService.UpdateAsync(concert);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _concertService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _concertService.DeleteAsync(id);
            return NoContent();
        }
    }
}