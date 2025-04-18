using Bus_Station_Ticket_Management.Models;
using Microsoft.EntityFrameworkCore;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options)
        : base(options)
    {
    }

    public DbSet<Trip> Trips { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Routes> Routes { get; set; }
    public DbSet<VehicleType> VehicleTypes { get; set; }
}