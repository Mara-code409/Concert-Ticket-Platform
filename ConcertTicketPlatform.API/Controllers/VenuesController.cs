using ConcertTicketPlatform.Core.DTOs;
using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VenuesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VenuesController> _logger;

        public VenuesController(AppDbContext context, ILogger<VenuesController> logger)
        {
            _context = context;
            _logger  = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var venues = await _context.Venues
                .Select(v => new VenueDto(v.Id, v.Name, v.City, v.Address, v.Capacity))
                .ToListAsync();
            return Ok(venues);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();
            return Ok(new VenueDto(venue.Id, venue.Name, venue.City, venue.Address, venue.Capacity));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] VenueDto dto)
        {
            var venue = new Venue { Name = dto.Name, City = dto.City, Address = dto.Address, Capacity = dto.Capacity };
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = venue.Id },
                new VenueDto(venue.Id, venue.Name, venue.City, venue.Address, venue.Capacity));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] VenueDto dto)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();
            venue.Name     = dto.Name;
            venue.City     = dto.City;
            venue.Address  = dto.Address;
            venue.Capacity = dto.Capacity;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();
            _context.Venues.Remove(venue);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
