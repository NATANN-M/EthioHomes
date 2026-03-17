using System.Diagnostics;
using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userType = HttpContext.Session.GetString("UserType");
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("UserName");

            ViewBag.Success = TempData["Success"];
            ViewBag.HasPendingRequests = false; // Default value
            ViewBag.HasApprovalNotification = false; // Default value

            if (userType != null && userId.HasValue)
            {
                ViewBag.UserType = userType;
                ViewBag.UserName = userName;

                using (var conn = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;"))
                {
                    conn.Open();

                    if (userType == "Owner")
                    {
                        // Get pending bookings for OWNER'S PROPERTIES
                        var query = @"
                    SELECT COUNT(*) 
                    FROM Bookings b
                    INNER JOIN Properties p ON b.PropertyId = p.Id
                    WHERE p.OwnerId = @OwnerId 
                    AND b.Status = 'Pending'";

                        var cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@OwnerId", userId.Value);
                        ViewBag.HasPendingRequests = (int)cmd.ExecuteScalar() > 0;
                    }
                    else 
                    {
                        // Get approved bookings for THIS RENTER
                        var query = "SELECT COUNT(*) FROM Bookings WHERE UserId = @UserId AND Status = 'Approved'";
                        var cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@UserId", userId.Value);
                        ViewBag.HasApprovalNotification = (int)cmd.ExecuteScalar() > 0;
                    }
                }
            }

            if (userId != null)
            {
                using (SqlConnection conn = new SqlConnection("Server = (localdb)\\MSSQLLocalDB; Database = EthioHomesDB; Trusted_Connection = True; "))
                {
                    conn.Open();
                    string query = @"
                SELECT TOP 1 Id, EndDate, ReminderStatus 
                FROM Bookings 
                WHERE UserId = @UserId 
                AND IsPaid = 1 
                AND (DATEDIFF(DAY, GETDATE(), EndDate) <= 2 AND DATEDIFF(DAY, GETDATE(), EndDate) >= 0)
                AND (ReminderStatus IS NULL OR ReminderStatus = 'Shown')";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        var reader = cmd.ExecuteReader();
                        if (reader.Read())
                        {
                            ViewBag.ShowReminder = true;
                            ViewBag.BookingId = reader["Id"];
                        }
                    }
                }
            }




            return View();


        }


        [HttpPost]
        public IActionResult HandleReminderDecision(int bookingId, string decision)
        {
            using (SqlConnection conn = new SqlConnection("Server = (localdb)\\MSSQLLocalDB; Database = EthioHomesDB; Trusted_Connection = True; "))
            {
                conn.Open();

                string update = "UPDATE Bookings SET ReminderStatus = @Status WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(update, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", decision);
                    cmd.Parameters.AddWithValue("@Id", bookingId);
                    cmd.ExecuteNonQuery();
                }

                if (decision == "Cancel")
                {
                    string cancel = "UPDATE Bookings SET Status = 'Cancelled' WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(cancel, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", bookingId);
                        cmd.ExecuteNonQuery();
                    }

                    return RedirectToAction("Index", "Home");
                }

                if (decision == "BookAgain")
                {
                    string getProperty = "SELECT PropertyId FROM Bookings WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(getProperty, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", bookingId);
                        int propertyId = (int)cmd.ExecuteScalar();

                        return RedirectToAction("Details", "Property", new { id = propertyId, showBooking = true });
                    }
                }
            }

            return RedirectToAction("Index", "Home");
        }






        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
