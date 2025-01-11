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

                TempData["Success"] = "Property saved successfully.";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SaveProperty: {ex.Message}");
                TempData["Error"] = "Unable to save property. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // Book Property
        [HttpPost]
        public IActionResult BookProperty(int propertyId)
        {
            try
            {
                int userId = GetLoggedInUserId();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO Bookings (UserId, PropertyId) 
                                     VALUES (@UserId, @PropertyId)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);

                    cmd.ExecuteNonQuery();
                }

                TempData["Success"] = "Property booked successfully.";
                return RedirectToAction("Details", "Property", new { id = propertyId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in BookProperty: {ex.Message}");
                TempData["Error"] = "Unable to book property. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }
    }
}

