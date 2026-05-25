using Microsoft.AspNetCore.Mvc;
using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Infrastructure.Data;
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
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var venues = await _context.Venues.ToListAsync();
            return Ok(venues);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();
            return Ok(venue);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Venue venue)
        {
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = venue.Id }, venue);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Venue venue)
        {
            if (id != venue.Id) return BadRequest();
            _context.Entry(venue).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
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