using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Core.Interfaces;

namespace ConcertTicketPlatform.Infrastructure.Services
{
    public class ConcertService : IConcertService
    {
        private readonly IConcertRepository _repository;

        public ConcertService(IConcertRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Concert>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Concert?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public async Task<Concert> CreateAsync(Concert concert)
        {
            if (concert.AvailableSeats > concert.TotalSeats)
                throw new ArgumentException("Locurile disponibile nu pot depăși totalul.");
            return await _repository.CreateAsync(concert);
        }

        public async Task UpdateAsync(Concert concert)
        {
            if (concert.AvailableSeats > concert.TotalSeats)
                throw new ArgumentException("Locurile disponibile nu pot depăși totalul.");
            await _repository.UpdateAsync(concert);
        }

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
