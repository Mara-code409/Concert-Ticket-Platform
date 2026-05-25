namespace ConcertTicketPlatform.Core.Entities
{
    public class Ticket
    {
        public int Id { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public bool IsUsed { get; set; } = false;
        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

        public int ConcertId { get; set; }
        public Concert Concert { get; set; } = null!;

        public string UserId { get; set; } = string.Empty;
        public AppUser User { get; set; } = null!;
    }
}