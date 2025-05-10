using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ROUTES: Prevent cascade delete on Start and Destination
            modelBuilder.Entity<Routes>()
                .HasOne(r => r.StartLocation)
                .WithMany()
                .HasForeignKey(r => r.StartId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Routes>()
                .HasOne(r => r.DestinationLocation)
                .WithMany()
                .HasForeignKey(r => r.DestinationId)
                .OnDelete(DeleteBehavior.NoAction);

            // SEATS and TRIPS
            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Trip)
                .WithMany()
                .HasForeignKey(s => s.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // TICKETS: Restrict deletion of Trip or Seat if related Tickets exist
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Trip)
                .WithMany()
                .HasForeignKey(t => t.TripId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany()
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict);

            // TRIP-DRIVER ASSIGNMENTS
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.TripDriverAssignments)
                .WithOne(tda => tda.Trip)
                .HasForeignKey(tda => tda.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Driver>()
                .HasMany(d => d.TripDriverAssignments)
                .WithOne(tda => tda.Driver)
                .HasForeignKey(tda => tda.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            // DRIVER LICENSES
            modelBuilder.Entity<Driver>()
                .HasMany(d => d.DriverLicenses)
                .WithOne(dl => dl.Driver)
                .HasForeignKey(dl => dl.DriverId);

            modelBuilder.Entity<DriverLicense>()
                .HasKey(dl => new { dl.DriverId, dl.LicenseId });

            // COUPONS: Specify decimal precision
            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountAmount)
                .HasColumnType("decimal(18,2)");

            // SCHEDULE: Keyless entity
            modelBuilder.Entity<ScheduleItem>().HasNoKey();
        }

        // DbSets
        public DbSet<Driver> Drivers { get; set; }
        public DbSet<DriverLicense> DriverLicenses { get; set; }
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
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<VnPayment> VnPayments { get; set; }
        public DbSet<ScheduleItem> Schedules { get; set; }
    }
}
