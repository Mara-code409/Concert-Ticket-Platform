using Microsoft.AspNetCore.Identity;

namespace ConcertTicketPlatform.Core.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}