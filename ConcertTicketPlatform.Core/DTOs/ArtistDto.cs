namespace ConcertTicketPlatform.Core.DTOs
{
    public record ArtistDto(int Id, string Name, string Genre, string Bio, string ImageUrl);

    public record CreateArtistDto(string Name, string Genre, string Bio, string ImageUrl);
}
