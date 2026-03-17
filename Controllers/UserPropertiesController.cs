using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class UserPropertiesController : Controller
    {
        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

        private int GetLoggedInUserId()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
          
            return userId.Value;
        }

        // View saved properties
        public IActionResult MySavedProperties()
        {
            try
            {
                int userId = GetLoggedInUserId();
                List<Property> savedProperties = new List<Property>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT p.*
                                     FROM SavedProperties sp
                                     JOIN Properties p ON sp.PropertyId = p.Id
                                     WHERE sp.UserId = @UserId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        savedProperties.Add(new Property
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Title = reader["Title"].ToString(),
                            Location = reader["Location"].ToString(),
                            Price = Convert.ToDecimal(reader["Price"]),
                            PropertyType = reader["PropertyType"].ToString(),
                            Status = reader["Status"].ToString(),
                            Bedrooms = Convert.ToInt32(reader["Bedrooms"]),
                            Bathrooms = Convert.ToInt32(reader["Bathrooms"]),
                            Description = reader["Description"].ToString()
                        });
                    }
                }

                return View(savedProperties);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in MySavedProperties: {ex.Message}");
                TempData["Error"] = "Unable to load saved properties. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        // View booked properties
        public IActionResult MyBookings()
        {
            int userId = GetLoggedInUserId();
                List<Booking> bookings = new List<Booking>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT b.*, p.Title, p.Location, p.Price, p.PropertyType
                 FROM Bookings b
                 JOIN Properties p ON b.PropertyId = p.Id
                 WHERE b.UserId = @UserId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        bookings.Add(new Booking
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            PropertyId = Convert.ToInt32(reader["PropertyId"]),
                            BookingDate = Convert.ToDateTime(reader["BookingDate"]),
                            Status = reader["Status"].ToString(),
                            AgreementAccepted = reader["AgreementAccepted"] != DBNull.Value && Convert.ToBoolean(reader["AgreementAccepted"]),
                            IsPaid = reader["IsPaid"] != DBNull.Value && Convert.ToBoolean(reader["IsPaid"])
                            // Map other properties as needed
                        });
                    }

                }
            

                return View(bookings);
           
        }

        [HttpPost]
        public IActionResult CancelBooking(int bookingId)
        {
            try
            {
                int userId = GetLoggedInUserId();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. Delete related agreements first
                    string deleteAgreements = "DELETE FROM Agreements WHERE BookingId = @BookingId";
                    using (SqlCommand cmd = new SqlCommand(deleteAgreements, conn))
                    {
                        cmd.Parameters.AddWithValue("@BookingId", bookingId);
                        cmd.ExecuteNonQuery();
                    }

                    // 2. Then delete the booking (only if not paid)
                    string deleteBooking = "DELETE FROM Bookings WHERE Id = @BookingId AND UserId = @UserId AND (IsPaid = 0 OR IsPaid IS NULL)";
                    using (SqlCommand cmd = new SqlCommand(deleteBooking, conn))
                    {
                        cmd.Parameters.AddWithValue("@BookingId", bookingId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                            TempData["Success"] = "Booking cancelled successfully.";
                        else
                            TempData["Error"] = "Unable to cancel booking. It may already be paid or does not exist.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error cancelling booking: " + ex.Message;
            }
            return RedirectToAction("MyBookings");
        }


        [HttpPost]
        public IActionResult RemoveSavedProperty(int id)
        {
            try
            {
                int userId = GetLoggedInUserId();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM SavedProperties WHERE PropertyId = @PropertyId AND UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", id);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }
                TempData["Success"] = "Property removed from saved list.";
            }
            catch
            {
                TempData["Error"] = "Failed to remove property.";
            }
            return RedirectToAction("MySavedProperties");
        }


    }
}
