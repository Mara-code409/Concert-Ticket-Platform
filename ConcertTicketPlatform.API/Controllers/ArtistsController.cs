using ConcertTicketPlatform.Core.DTOs;
using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConcertTicketPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtistsController : ControllerBase
    {
        private readonly IArtistService _artistService;
        private readonly ILogger<ArtistsController> _logger;

        public ArtistsController(IArtistService artistService, ILogger<ArtistsController> logger)
        {
            _artistService = artistService;
            _logger        = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Getting all artists");
            var artists = await _artistService.GetAllAsync();
            var dtos = artists.Select(a => new ArtistDto(a.Id, a.Name, a.Genre, a.Bio, a.ImageUrl));
            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var artist = await _artistService.GetByIdAsync(id);
            if (artist == null) return NotFound();
            return Ok(new ArtistDto(artist.Id, artist.Name, artist.Genre, artist.Bio, artist.ImageUrl));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateArtistDto dto)
        {
            var artist = new Artist
            {
                Name     = dto.Name,
                Genre    = dto.Genre,
                Bio      = dto.Bio,
                ImageUrl = dto.ImageUrl
            };
            var created = await _artistService.CreateAsync(artist);
            return CreatedAtAction(nameof(GetById), new { id = created.Id },
                new ArtistDto(created.Id, created.Name, created.Genre, created.Bio, created.ImageUrl));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateArtistDto dto)
        {
            var existing = await _artistService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            existing.Name     = dto.Name;
            existing.Genre    = dto.Genre;
            existing.Bio      = dto.Bio;
            existing.ImageUrl = dto.ImageUrl;
            await _artistService.UpdateAsync(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _artistService.GetByIdAsync(id);
            if (existing == null) return NotFound();
            await _artistService.DeleteAsync(id);
            return NoContent();
        }
    }
}
