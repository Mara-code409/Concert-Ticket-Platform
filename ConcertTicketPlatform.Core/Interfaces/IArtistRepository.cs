using ConcertTicketPlatform.Core.Entities;

namespace ConcertTicketPlatform.Core.Interfaces
{
    public interface IArtistRepository
    {
        Task<IEnumerable<Artist>> GetAllAsync();
        Task<Artist?> GetByIdAsync(int id);
        Task<Artist> CreateAsync(Artist artist);
        Task UpdateAsync(Artist artist);
        Task DeleteAsync(int id);
    }
}