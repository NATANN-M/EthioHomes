using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class PropertyActionsController : Controller
    {
        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

        private int GetLoggedInUserId()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                throw new UnauthorizedAccessException("User not logged in.");
            }
         
            return userId.Value;
        }

        // Save Property
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

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"INSERT INTO Bookings (PropertyId, UserId, StartDate, EndDate, BookingDate, Status, MessageToOwner)
                         VALUES (@PropertyId, @UserId, @StartDate, @EndDate, GETDATE(), Default, @MessageToOwner)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                cmd.Parameters.AddWithValue("@UserId", userId.Value);
                cmd.Parameters.AddWithValue("@StartDate", startDate);
                cmd.Parameters.AddWithValue("@EndDate", endDate);
                cmd.Parameters.AddWithValue("@MessageToOwner", messageToOwner ?? "");

                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Booking request submitted successfully!";
            return RedirectToAction("ViewMessages", "Messages", new { propertyId = propertyId });
        }

    }
}

