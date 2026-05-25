using Microsoft.AspNetCore.Mvc;
using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ArtistsController> _logger;

        public ArtistsController(AppDbContext context, ILogger<ArtistsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Getting all artists");
            var artists = await _context.Artists.ToListAsync();
            return Ok(artists);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null) return NotFound();
            return Ok(artist);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Artist artist)
        {
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = artist.Id }, artist);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Artist artist)
        {
            if (id != artist.Id) return BadRequest();
            _context.Entry(artist).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null) return NotFound();
            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}