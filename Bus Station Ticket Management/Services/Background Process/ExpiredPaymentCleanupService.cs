
using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.EntityFrameworkCore;
namespace Bus_Station_Ticket_Management.Services
{
    public class ExpiredPaymentCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiredPaymentCleanupService> _logger;

        public ExpiredPaymentCleanupService(IServiceScopeFactory scopeFactory, ILogger<ExpiredPaymentCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiredPaymentCleanupService started.");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var now = DateTime.Now;

                    var expiredPayments = await dbContext.Payments
                        .Where(p => p.PaymentStatus == 0 && p.ExpiredAt < now)
                        .ToListAsync(stoppingToken);

                    foreach (var payment in expiredPayments)
                    {
                        var tickets = await dbContext.Tickets
                        .Where(t => t.PaymentId == payment.Id)
                        .ToListAsync(stoppingToken);

                        var seatIds = tickets.Select(t => t.SeatId).ToList();

                        var seats = await dbContext.Seats
                            .Where(s => seatIds.Contains(s.Id))
                            .ToListAsync(stoppingToken);

                        foreach (var seat in seats)
                        {
                            seat.IsAvailable = true;
                        }

                        dbContext.Tickets.RemoveRange(tickets);
                        payment.PaymentStatus = 2;

                        _logger.LogInformation($"Payment {payment.Id} expired. Released {seats.Count} seat(s) and removed {tickets.Count} ticket(s).");
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while cleaning up expired payments");

                }

                await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // wait 2 minutes before next check
            }

            _logger.LogInformation("ExpiredPaymentCleanupService stopped.");
        }
    }
}