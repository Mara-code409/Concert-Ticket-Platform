using ConcertTicketPlatform.Core.Entities;

namespace ConcertTicketPlatform.Core.Interfaces
{
    public interface IConcertRepository
    {
        Task<IEnumerable<Concert>> GetAllAsync();
        Task<Concert?> GetByIdAsync(int id);
        Task<Concert> CreateAsync(Concert concert);
        Task UpdateAsync(Concert concert);
        Task DeleteAsync(int id);
    }
}