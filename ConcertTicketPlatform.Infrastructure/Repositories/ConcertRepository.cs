using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Core.Interfaces;
using ConcertTicketPlatform.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.Infrastructure.Repositories
{
    public class ConcertRepository : IConcertRepository
    {
        private readonly AppDbContext _context;

        public ConcertRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Concert>> GetAllAsync() =>
            await _context.Concerts
                .Include(c => c.Artist)
                .Include(c => c.Venue)
                .Include(c => c.Category)
                .ToListAsync();

        public async Task<Concert?> GetByIdAsync(int id) =>
            await _context.Concerts
                .Include(c => c.Artist)
                .Include(c => c.Venue)
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == id);

        public async Task<Concert> CreateAsync(Concert concert)
        {
            _context.Concerts.Add(concert);
            await _context.SaveChangesAsync();
            return concert;
        }

        public async Task UpdateAsync(Concert concert)
        {
            _context.Entry(concert).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var concert = await _context.Concerts.FindAsync(id);
            if (concert != null)
            {
                _context.Concerts.Remove(concert);
                await _context.SaveChangesAsync();
            }
        }
    }
}