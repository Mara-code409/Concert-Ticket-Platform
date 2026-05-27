namespace ConcertTicketPlatform.Web.Models
{
    public class ConcertDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime Date { get; set; }
        public decimal Price { get; set; }
        public int AvailableSeats { get; set; }
        public int TotalSeats { get; set; }
        public ArtistDto Artist { get; set; }
        public VenueDto Venue { get; set; }
        public CategoryDto Category { get; set; }
    }

    public class ArtistDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Genre { get; set; }
        public string ImageUrl { get; set; }
    }

    public class VenueDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string City { get; set; }
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}