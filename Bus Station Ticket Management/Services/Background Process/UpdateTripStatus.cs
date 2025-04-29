
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bus_Station_Ticket_Management.Services
{
    public class UpdateTripStatusService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BackgroundJobSettings _settings;
        private readonly ILogger<UpdateTripStatusService> _logger;

        public UpdateTripStatusService(IServiceScopeFactory scopeFactory, IOptions<BackgroundJobSettings> options, ILogger<UpdateTripStatusService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        var _context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                        if (_context == null)
                        {
                            _logger.LogError("ApplicationDbContext is not available.");
                            throw new Exception("ApplicationDbContext is not available.");
                        }
                        var now = DateTime.Now;

                        var trips = await _context.Trips.ToListAsync(stoppingToken);

                        if (trips != null && trips.Any()) {
                            foreach (var trip in trips)
                            {
                                string newTripStatus = trip.Status ?? string.Empty; // Default to current status
                                string newVehicleStatus = string.Empty;
                                if (trip.Vehicle != null)
                                    newVehicleStatus = trip.Vehicle.Status ?? string.Empty;

                                if (now >= trip.ArrivalTime && trip.Status != "Completed")
                                {
                                    newTripStatus = "Completed";
                                    newVehicleStatus = "Stand By";
                                }
                                else if (now >= trip.DepartureTime && now < trip.ArrivalTime && trip.Status != "In Progress")
                                {
                                    newTripStatus = "In Progress";
                                    newVehicleStatus = "Busy";
                                }
                                else if (trip.Status != "Stand By")
                                {
                                    newTripStatus = "Stand By";
                                }

                                // Update the status only if it has changed
                                if (trip.Status != newTripStatus)
                                {
                                    if (trip.Vehicle != null)
                                        trip.Vehicle.Status = newVehicleStatus;
                                    trip.Status = newTripStatus;
                                }
                            }

                            await _context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating trip statuses.");
                }
                
                // await Task.Delay(TimeSpan.FromSeconds(_settings.TripStatusCheckIntervalSeconds), stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            _logger.LogInformation("UpdateTripStatusService stopped.");
        }
    }
}