using ConcertTicketPlatform.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ConcertTicketPlatform.Infrastructure.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Artist> Artists { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Concert> Concerts { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Concert>()
                .HasOne(c => c.Artist)
                .WithMany(a => a.Concerts)
                .HasForeignKey(c => c.ArtistId);

            builder.Entity<Concert>()
                .HasOne(c => c.Venue)
                .WithMany(v => v.Concerts)
                .HasForeignKey(c => c.VenueId);

            builder.Entity<Concert>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Concerts)
                .HasForeignKey(c => c.CategoryId);

            builder.Entity<Ticket>()
                .HasOne(t => t.Concert)
                .WithMany(c => c.Tickets)
                .HasForeignKey(t => t.ConcertId);

            builder.Entity<Review>()
                .HasOne(r => r.Concert)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.ConcertId);
        }
    }
}