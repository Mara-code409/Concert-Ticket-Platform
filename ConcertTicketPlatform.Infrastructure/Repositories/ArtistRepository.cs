using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Core.Interfaces;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.Infrastructure.Repositories
{
    public class ArtistRepository : IArtistRepository
    {
        private readonly AppDbContext _context;

        public ArtistRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Artist>> GetAllAsync() =>
            await _context.Artists.ToListAsync();

        public async Task<Artist?> GetByIdAsync(int id) =>
            await _context.Artists.FindAsync(id);

        public async Task<Artist> CreateAsync(Artist artist)
        {
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
            return artist;
        }

        public async Task UpdateAsync(Artist artist)
        {
            _context.Entry(artist).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist != null)
            {
                _context.Artists.Remove(artist);
                await _context.SaveChangesAsync();
            }
        }
    }
}