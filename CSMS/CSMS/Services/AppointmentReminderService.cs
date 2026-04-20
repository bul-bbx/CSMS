using CSMS.Data;
using Microsoft.EntityFrameworkCore;

namespace CSMS.Services
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AppointmentReminderService> _logger;

        public AppointmentReminderService(
            IServiceScopeFactory scopeFactory,
            ILogger<AppointmentReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentReminderService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendReminders(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending appointment reminders");
                }

                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task SendReminders(CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var _emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // Current time of day
            var now = DateTime.Now.TimeOfDay;

            // Reminder window: appointments in next 1 hour
            var reminderWindowStart = now;
            var reminderWindowEnd = now.Add(TimeSpan.FromHours(1));

            var appointments = await context.Appointments
                .Include(a => a.Car)
                    .ThenInclude(c => c.Customer)
                .Where(a =>
                    a.StartTime >= reminderWindowStart &&
                    a.StartTime <= reminderWindowEnd &&
                    !a.ReminderSent)
                .ToListAsync(stoppingToken);

            foreach (var appointment in appointments)
            {
                if (appointment.Car?.Customer?.Email == null)
                    continue;

                var timeLeft = appointment.StartTime - now;

                string timeText = timeLeft.TotalHours >= 1
                    ? $"{(int)timeLeft.TotalHours} hours {timeLeft.Minutes} minutes"
                    : $"{timeLeft.Minutes} minutes";

                await _emailService.SendAsync(
                    appointment.Car.Customer.Email,
                    "Appointment Reminder",
                    $@"
                        <h2>Appointment Reminder</h2>
                        <p>Your appointment is scheduled in <strong>{timeText}</strong>.</p>
                        <p><strong>Time:</strong> {appointment.StartTime:hh\\:mm}</p>
                    "
                );

                appointment.ReminderSent = true;
                _logger.LogInformation("Reminder sent for Appointment {AppointmentId}", appointment.Id);
            }

            await context.SaveChangesAsync(stoppingToken);
        }
    }
}
