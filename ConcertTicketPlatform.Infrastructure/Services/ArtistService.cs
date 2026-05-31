using ConcertTicketPlatform.Core.Entities;
using ConcertTicketPlatform.Core.Interfaces;

namespace ConcertTicketPlatform.Infrastructure.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _repository;

        public ArtistService(IArtistRepository repository)
        {
            _repository = repository;
        }

        public Task<IEnumerable<Artist>> GetAllAsync() => _repository.GetAllAsync();

        public Task<Artist?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

        public async Task<Artist> CreateAsync(Artist artist)
        {
            if (string.IsNullOrWhiteSpace(artist.Name))
                throw new ArgumentException("Numele artistului este obligatoriu.");
            return await _repository.CreateAsync(artist);
        }

        public Task UpdateAsync(Artist artist) => _repository.UpdateAsync(artist);

        public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
    }
}
