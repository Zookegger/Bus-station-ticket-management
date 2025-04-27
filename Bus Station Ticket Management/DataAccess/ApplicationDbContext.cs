using Bus_Station_Ticket_Management.Models;
using Microsoft.AspNetCore.Identity;
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

            modelBuilder.Entity<Seat>()
                .HasOne(s => s.Trip)
                .WithMany()
                .HasForeignKey(s => s.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // When a Trip is deleted, restrict deletion if it has related Tickets
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Trip)
                .WithMany()
                .HasForeignKey(t => t.TripId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent trip deletion if tickets exist

            // When a Seat is deleted, restrict deletion if it has related Tickets
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Seat)
                .WithMany()
                .HasForeignKey(t => t.SeatId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent seat deletion if tickets exist

            // When a Trip is deleted, delete the related TripDriverAssignments
            modelBuilder.Entity<Trip>()
                .HasMany(t => t.TripDriverAssignments)
                .WithOne(tda => tda.Trip)
                .HasForeignKey(tda => tda.TripId)
                .OnDelete(DeleteBehavior.Cascade);

            // When a Driver is deleted, keep the TripDriverAssignment (Driver will become null if nullable)
            modelBuilder.Entity<TripDriverAssignment>()
                .HasOne(tda => tda.Driver)
                .WithMany(d => d.TripDriverAssignments)
                .HasForeignKey(tda => tda.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountAmount)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<ScheduleItem>().HasNoKey();
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
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<VnPayment> VnPayments { get; set; }
        public DbSet<ScheduleItem> Schedules { get; set; }
    }
}
