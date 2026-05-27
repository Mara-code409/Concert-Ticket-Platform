namespace ConcertTicketPlatform.Core.Entities
{
    public class Concert
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int TotalSeats { get; set; }
        public int AvailableSeats { get; set; }

        public int ArtistId { get; set; }
        public Artist? Artist { get; set; }

        public int VenueId { get; set; }
        public Venue? Venue { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}