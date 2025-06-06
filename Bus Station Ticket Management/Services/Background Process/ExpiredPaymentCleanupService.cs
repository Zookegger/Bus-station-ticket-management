using Bus_Station_Ticket_Management.DataAccess;
using Bus_Station_Ticket_Management.Models;
using Microsoft.EntityFrameworkCore;

namespace Bus_Station_Ticket_Management.Services.BackgroundProcess
{
    // Background service to cleanup expired payments
    // and failed payments
    public class ExpiredPaymentCleanupService(IServiceScopeFactory scopeFactory, ILogger<ExpiredPaymentCleanupService> logger) : BackgroundService
    {
        // Cleanup interval in minutes
        private const int CleanupIntervalMinutes = 2;

        // Payment status constants
        private const int PaymentStatusPending = 0;
        private const int PaymentStatusFailed = 2;

        // Task state properties
        public DateTime LastRunTime { get; private set; }
        public bool LastRunSuccess { get; private set; }
        public string? LastRunError { get; private set; }
        public int LastRunProcessedCount { get; private set; }
        public TimeSpan LastRunDuration { get; private set; }

        // Task status
        public bool IsRunning { get; private set; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            logger.LogInformation("ExpiredPaymentCleanupService started.");
            IsRunning = true;

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    var runStartTime = DateTime.Now;
                    bool runSuccess = true;
                    string? runError = null;
                    int runProcessedCount = 0;

                    // Create a new scope for each iteration
                    // to avoid the issue of the DbContext being disposed
                    // of after the transaction is committed
                    try
                    {

                        using var scope = scopeFactory.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                        await ProcessBatchPaymentAsync(dbContext, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        // If the service is faulted, set the task status to faulted
                        logger.LogError(ex, "Error while cleaning up expired payments");
                        logger.LogInformation("ExpiredPaymentCleanupService faulted.");
                        runSuccess = false;
                        runError = ex.Message;
                    }
                    finally
                    {
                        LastRunTime = DateTime.Now;
                        LastRunSuccess = runSuccess;
                        LastRunError = runError;
                        LastRunProcessedCount = runProcessedCount;
                        LastRunDuration = DateTime.Now - runStartTime;
                        logger.LogInformation($"ExpiredPaymentCleanupService completed in {LastRunDuration.TotalSeconds} seconds.");
                        
                        if (!stoppingToken.IsCancellationRequested)
                        {
                            // Wait 2 minutes before next check
                            await Task.Delay(TimeSpan.FromMinutes(CleanupIntervalMinutes), stoppingToken);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // If the service is canceled, set the task status to canceled
                logger.LogInformation("ExpiredPaymentCleanupService canceled.");
                IsRunning = false;
            }
            finally
            {
                // Log the service stopped
                logger.LogInformation("ExpiredPaymentCleanupService stopped.");
                IsRunning = false;
            }
        }


        private async Task ProcessBatchPaymentAsync(ApplicationDbContext dbContext, CancellationToken stoppingToken)
        {
            var now = DateTime.Now;
            int processedCount = 0;

            var payments = await dbContext.Payments
                .Where(p =>
                    (p.PaymentStatus == PaymentStatusPending && p.ExpiredAt < now) ||
                    (p.PaymentStatus == PaymentStatusFailed && p.ExpiredAt < now))
                .Include(p => p.Tickets)
                .ThenInclude(t => t.Seat)
                .ToListAsync(stoppingToken);

            foreach (var payment in payments)
            {
                if (payment.Tickets.Any(t => t.IsCanceled))
                {
                    logger.LogInformation($"Payment {payment.Id} has already been canceled.");
                    continue;
                }

                using var transaction = await dbContext.Database.BeginTransactionAsync(stoppingToken);
                try
                {
                    var isExpired = payment.PaymentStatus == PaymentStatusPending;
                    var ticketsProcessed = ProcessSinglePaymentAsync(dbContext, payment, isExpired, stoppingToken);
                    processedCount += ticketsProcessed;

                    await dbContext.SaveChangesAsync(stoppingToken);
                    await transaction.CommitAsync(stoppingToken);
                    logger.LogInformation($"{(isExpired ? "Expired" : "Failed")} Payment {payment.Id}: Released {ticketsProcessed} seats and removed tickets.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(stoppingToken);
                    logger.LogError(ex, "Error while processing payment");
                    throw;
                }
            }

            // Update the LastRunProcessedCount with the total processed count
            LastRunProcessedCount = processedCount;
        }

        private int ProcessSinglePaymentAsync(ApplicationDbContext dbContext, Payment payment, bool isExpiredPayment, CancellationToken stoppingToken)
        {
            if (!payment.Tickets.Any())
            {
                logger.LogInformation($"Payment {payment.Id} has no tickets.");
                return 0;
            }
            // Get the tickets
            var tickets = payment.Tickets.Where(t => t.PaymentId == payment.Id && !t.IsCanceled).ToList();

            // Release the seats
            foreach (var ticket in tickets.Where(t => t.Seat != null))
            {
                ticket.Seat.IsAvailable = true;
                ticket.IsCanceled = true;
                ticket.CancelationTime = DateTime.Now;
            }

            if (isExpiredPayment)
            {
                // Set the payment status to expired
                payment.PaymentStatus = PaymentStatusFailed;
            }

            // Remove the tickets
            dbContext.Tickets.RemoveRange(tickets);

            // Log the payment
            logger.LogInformation($"Payment {payment.Id} {(isExpiredPayment ? "expired" : "failed")}. Released {tickets.Count} seat(s) and removed {tickets.Count} ticket(s).");

            return tickets.Count;
        }
    }
}