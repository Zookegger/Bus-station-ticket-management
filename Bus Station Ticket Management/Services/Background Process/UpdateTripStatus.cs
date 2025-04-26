
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bus_Station_Ticket_Management.Services
{
    public class UpdateTripStatus : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BackgroundJobSettings _settings;

        public UpdateTripStatus (IServiceScopeFactory scopeFactory, IOptions<BackgroundJobSettings> options) {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) {
                using (var scope = _scopeFactory.CreateScope()) {
                    var _context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                    var now = DateTime.Now;

                    var trips = await _context.Trips.ToListAsync(stoppingToken);

                    foreach (var trip in trips) {
                        if (now >= trip.ArrivalTime && trip.Status != "Completed") {
                            trip.Status = "Completed";
                        }
                        else if (now >= trip.DepartureTime && now < trip.ArrivalTime && trip.Status != "In Progress") {
                            
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromMinutes(_settings.TripStatusCheckIntervalSeconds), stoppingToken);
            }
        }
    }
}