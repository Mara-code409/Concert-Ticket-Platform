using ConcertTicketPlatform.Core.Entities;

namespace ConcertTicketPlatform.Core.Interfaces
{
    public interface IArtistService
    {
        Task<IEnumerable<Artist>> GetAllAsync();
        Task<Artist?> GetByIdAsync(int id);
        Task<Artist> CreateAsync(Artist artist);
        Task UpdateAsync(Artist artist);
        Task DeleteAsync(int id);
    }
}
