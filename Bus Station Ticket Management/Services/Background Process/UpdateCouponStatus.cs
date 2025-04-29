
using Bus_Station_Ticket_Management.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bus_Station_Ticket_Management.Services
{
    public class UpdateCouponStatusService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly BackgroundJobSettings _settings;
        private readonly ILogger<UpdateCouponStatusService> _logger;


        public UpdateCouponStatusService(IServiceScopeFactory scopeFactory, IOptions<BackgroundJobSettings> options, ILogger<UpdateCouponStatusService> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("UpdateCouponStatusService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var _context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                            var now = DateTime.Now;

                            var coupons = await _context.Coupons.ToListAsync(stoppingToken);
                            foreach (var coupon in coupons)
                            {
                                if (now > coupon.StartPeriod && now < coupon.EndPeriod)
                                {
                                    coupon.IsActive = true;
                                }
                                else if (now > coupon.EndPeriod)
                                {
                                    coupon.IsActive = false;
                                }
                            }

                            await _context.SaveChangesAsync(stoppingToken);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while updating coupon statuses.");
                }
                
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            
            _logger.LogInformation("UpdateCouponStatusService stopped.");
        }
    }
}