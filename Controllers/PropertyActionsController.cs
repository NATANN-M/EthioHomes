using EthioHomes.Models;
using EthioHomes.services.EthioHomes.Services;
using EthioHomes.services; // Include your EmailService namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;

namespace EthioHomes.Controllers
{
    public class PropertyActionsController : Controller
    {
        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";
        private readonly EmailService _emailService;

        public PropertyActionsController(EmailService emailService)
        {
            _emailService = emailService;
        }

        private int GetLoggedInUserId()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User not logged in.");
            }

            return userId.Value;
        }

        //  Save Property
        [HttpPost]
        public IActionResult SaveProperty(int propertyId)
        {
            try
            {
                int userId = GetLoggedInUserId();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO SavedProperties (UserId, PropertyId) 
                                     VALUES (@UserId, @PropertyId)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);

                    cmd.ExecuteNonQuery();
                }

                return RedirectToAction("Details", "Property", new { id = propertyId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveProperty: {ex.Message}");
                TempData["Error"] = "Unable to save property. Please try again.";
                return RedirectToAction("Login", "User");
            }
        }

        //  Booking with Email Notification
        [HttpPost]
        public IActionResult BookPropertyRequest(int propertyId, DateTime startDate, DateTime endDate, string messageToOwner)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            if (endDate <= startDate)
            {
                TempData["Error"] = "End Date must be after Start Date.";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }

            string propertyName = "";
            string ownerEmail = "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                //  Step 1: Fetch property name & owner's email
                string infoQuery = @"SELECT P.Title, U.Email 
                                     FROM Properties P
                                     JOIN Users U ON P.OwnerId = U.Id
                                     WHERE P.Id = @PropertyId";

                using (SqlCommand infoCmd = new SqlCommand(infoQuery, conn))
                {
                    infoCmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    using (SqlDataReader reader = infoCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            propertyName = reader["Title"].ToString();
                            ownerEmail = reader["Email"].ToString();
                        }
                    }
                }

                //  Step 2: Insert booking request
                string insertQuery = @"INSERT INTO Bookings (PropertyId, UserId, StartDate, EndDate, BookingDate, Status, MessageToOwner)
                                       VALUES (@PropertyId, @UserId, @StartDate, @EndDate, GETDATE(), Default, @MessageToOwner)";

                using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    cmd.Parameters.AddWithValue("@UserId", userId.Value);
                    cmd.Parameters.AddWithValue("@StartDate", startDate);
                    cmd.Parameters.AddWithValue("@EndDate", endDate);
                    cmd.Parameters.AddWithValue("@MessageToOwner", messageToOwner ?? "");

                    cmd.ExecuteNonQuery();
                }
            }

            //  Step 3: Send Email to Property Owner
            if (!string.IsNullOrEmpty(ownerEmail))
            {
                string subject = $"New Booking Request for {propertyName}";
                string body = $"Dear Property Owner,\n\n" +
                              $"You have received a new booking request for your property: {propertyName}.\n\n" +
                              $"Start Date: {startDate:yyyy-MM-dd}\n" +
                              $"End Date: {endDate:yyyy-MM-dd}\n" +
                              $"Message from renter: {messageToOwner ?? "No message."}\n\n" +
                              $"Please log in to your EthioHomes dashboard to take action.\n\n" +
                              $"Regards,\nEthioHomes Team";

                _emailService.SendEmail(ownerEmail, subject, body);
            }

            TempData["Success"] = "Booking request submitted successfully!";
            return RedirectToAction("Details", "Property", new { Id = propertyId });
        }
    }
}
