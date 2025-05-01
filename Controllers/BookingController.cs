using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class BookingController : Controller
    {
        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

        public IActionResult ManageBookings()
        {
            int? ownerId = HttpContext.Session.GetInt32("UserId");
            if (ownerId == null)
            {
                return RedirectToAction("Login", "User");
            }

            List<dynamic> bookings = new List<dynamic>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"  
                  SELECT b.Id, b.PropertyId, p.Title AS PropertyTitle, u.Name AS UserName,  
                         b.StartDate, b.EndDate, b.BookingDate, b.Status, b.MessageToOwner  
                  FROM Bookings b  
                  JOIN Properties p ON b.PropertyId = p.Id  
                  JOIN Users u ON b.UserId = u.Id  
                  WHERE p.OwnerId = @OwnerId  
                  ORDER BY b.BookingDate DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OwnerId", ownerId);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    bookings.Add(new
                    {
                        Id = (int)reader["Id"],
                        PropertyId = (int)reader["PropertyId"],
                        PropertyTitle = reader["PropertyTitle"].ToString(),
                        UserName = reader["UserName"].ToString(),
                        StartDate = Convert.ToDateTime(reader["StartDate"]),
                        EndDate = Convert.ToDateTime(reader["EndDate"]),
                        BookingDate = Convert.ToDateTime(reader["BookingDate"]),
                        Status = reader["Status"].ToString(),
                        MessageToOwner = reader["MessageToOwner"].ToString()
                    });
                }
            }

            return View(bookings);
        }


        [HttpPost]
        public IActionResult ApproveBooking(int bookingId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Bookings SET Status = 'Approved' WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", bookingId);
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Booking approved.";
            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        public IActionResult RejectBooking(int bookingId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Bookings SET Status = 'Rejected' WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", bookingId);
                cmd.ExecuteNonQuery();
            }

            TempData["Success"] = "Booking rejected.";
            return RedirectToAction("ManageBookings");
        }

    }
}
