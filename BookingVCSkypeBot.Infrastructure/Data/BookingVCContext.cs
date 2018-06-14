using System.Data.Entity;
using BookingVCSkypeBot.Core.Entities;

namespace BookingVCSkypeBot.Infrastructure.Data
{
    public class BookingVCContext : DbContext
    {
        public BookingVCContext() : base("DbConnection")
        { }

        public DbSet<Location> Locations { get; set; }
        public DbSet<VC> VCs { get; set; }
    }
}