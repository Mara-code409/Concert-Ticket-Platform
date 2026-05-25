namespace ConcertTicketPlatform.Core.DTOs
{
    public class ConcertDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string VenueName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
    }

    public class CreateConcertDto
    {
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int TotalSeats { get; set; }
        public int ArtistId { get; set; }
        public int VenueId { get; set; }
        public int CategoryId { get; set; }
    }
}