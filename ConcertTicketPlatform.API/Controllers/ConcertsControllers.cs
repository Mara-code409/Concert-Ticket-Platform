using Microsoft.AspNetCore.Mvc;
using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConcertsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ConcertsController> _logger;

        public ConcertsController(AppDbContext context, ILogger<ConcertsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Getting all concerts");
            var concerts = await _context.Concerts
                .Include(c => c.Artist)
                .Include(c => c.Venue)
                .Include(c => c.Category)
                .ToListAsync();
            return Ok(concerts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var concert = await _context.Concerts
                .Include(c => c.Artist)
                .Include(c => c.Venue)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (concert == null) return NotFound();
            return Ok(concert);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Concert concert)
        {
            _context.Concerts.Add(concert);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = concert.Id }, concert);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Concert concert)
        {
            if (id != concert.Id) return BadRequest();
            _context.Entry(concert).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var concert = await _context.Concerts.FindAsync(id);
            if (concert == null) return NotFound();
            _context.Concerts.Remove(concert);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}