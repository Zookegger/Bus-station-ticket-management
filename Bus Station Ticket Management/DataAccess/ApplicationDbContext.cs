using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Routes>()
                .HasOne(r => r.StartLocation)
                .WithMany()
                .HasForeignKey(r => r.StartId)
                .OnDelete(DeleteBehavior.NoAction); // Chặn xóa cascade

            modelBuilder.Entity<Routes>()
                .HasOne(r => r.DestinationLocation)
                .WithMany()
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.NoAction); // Chặn xóa cascade

            // Định nghĩa quan hệ và tắt xóa cascade
            modelBuilder.Entity<TripDriverAssignment>()
                .HasOne(tda => tda.Trip)
                .WithMany(t => t.TripDriverAssignments)
                .HasForeignKey(tda => tda.TripId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<TripDriverAssignment>()
                .HasOne(tda => tda.Driver)
                .WithMany(d => d.TripDriverAssignments)
                .HasForeignKey(tda => tda.DriverId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        public DbSet<Driver> Drivers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleType> VehicleTypes { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public DbSet<Seat> Seats { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Routes> Routes { get; set; }
        public DbSet<Rating> Ratings { get; set; }
        public DbSet<TripDriverAssignment> TripDriverAssignments { get; set; }
    }
}
