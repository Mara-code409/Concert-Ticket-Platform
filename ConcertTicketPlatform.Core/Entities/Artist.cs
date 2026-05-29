using System;

namespace ConcertTicketPlatform.Core.Entities
{
    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string Genre { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;

        public ICollection<Concert> Concerts { get; set; } = new List<Concert>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}