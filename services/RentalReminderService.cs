using Microsoft.Data.SqlClient;
using System.Globalization;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using EthioHomes.services.EthioHomes.Services;

namespace EthioHomes.Services
{
    public class RentalReminderService : BackgroundService
    {
        private readonly IConfiguration _config;
        private readonly EmailService _emailService;
        private readonly string _connectionString;

        public RentalReminderService(IConfiguration config, EmailService emailService)
        {
            _config = config;
            _emailService = emailService;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckAndSendReminders();

                // Wait for 24 hours
                await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
            }
        }

        private async Task CheckAndSendReminders()
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                SELECT 
                    u.Name,
                    u.Email,
                    b.EndDate,
                    p.Title AS PropertyTitle
                FROM Bookings b
                INNER JOIN Users u ON u.Id = b.UserId
                INNER JOIN Properties p ON p.Id = b.PropertyId
                WHERE b.Status = 'Paid'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string name = reader["Name"].ToString();
                        string email = reader["Email"].ToString();
                        string property = reader["PropertyTitle"].ToString();
                        DateTime endDate = Convert.ToDateTime(reader["EndDate"]);

                        Console.WriteLine(email);
                        Console.WriteLine(endDate);
                       

                        DateTime today = DateTime.Today;

                        if ((endDate - today).TotalDays == 2)
                        {
                            // Reminder 2 days before
                            string subject = "Reminder: Your rental is ending soon!";
                            string body = $@"
Dear {name},

This is a friendly reminder that your rental for <b>{property}</b> will end on <b>{endDate.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture)}</b>.

If you wish to extend or book again, please take action before your stay ends.

Regards,<br>EthioHomes Team";

                            _emailService.SendEmail(email, subject, body);
                        }
                        else if (endDate.Date == today)
                        {
                            // Reminder on last day
                            string subject = "Today is your last day of rental";
                            string body = $@"
Dear {name},

Your stay at <b>{property}</b> ends today (<b>{endDate.ToString("MMMM dd, yyyy", CultureInfo.InvariantCulture)}</b>).

Thank you for using EthioHomes. We hope you had a pleasant experience. You may book again anytime.

Best regards,<br>EthioHomes Team";

                            _emailService.SendEmail(email, subject, body);
                        }
                    }
                }
            }
        }
    }
}
