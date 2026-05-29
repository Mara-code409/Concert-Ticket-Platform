using System;

namespace ConcertTicketPlatform.Core.Entities
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<Concert> Concerts { get; set; } = new List<Concert>();
        public ICollection<Artist> Artists { get; set; } = new List<Artist>();
    }
}