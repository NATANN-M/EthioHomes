using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using EthioHomes.Models;

public class OwnerController : Controller
{
    private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";



    // SHOW all approved and paid bookings for current owner
    public IActionResult ManageRenters()
    {
        var ownerId = HttpContext.Session.GetInt32("UserId");
        var bookings = new List<Booking>();

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            const string query = @"
            SELECT 
    b.Id, b.StartDate, b.EndDate, b.BookingDate,
    b.Status, b.AgreementAccepted, b.IsPaid,
    b.CheckInStatus, b.CheckedInAt, b.CheckedOutAt,
    u.Name AS RenterName,
    p.Title AS PropertyName
FROM Bookings b
INNER JOIN Properties p ON b.PropertyId = p.Id
INNER JOIN Users u ON b.UserId = u.Id
WHERE p.OwnerId = @OwnerId 
  AND LTRIM(RTRIM(LOWER(b.Status))) = 'Paid'
  AND b.IsPaid = 1
";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@OwnerId", ownerId);
            conn.Open();

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var booking = new Booking
                    {
                        Id = reader.GetInt32(0),
                        StartDate = reader.GetDateTime(1),
                        EndDate = reader.GetDateTime(2),
                        BookingDate = reader.GetDateTime(3),
                        Status = reader.GetString(4),
                        AgreementAccepted = reader.GetBoolean(5),
                        IsPaid = reader.GetBoolean(6),
                        CheckInStatus = reader.IsDBNull(7) ? "NotCheckedIn" : reader.GetString(7),
                        CheckedInAt = reader.IsDBNull(8) ? null : reader.GetDateTime(8),
                        CheckedOutAt = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                        RenterName = reader.IsDBNull(10) ? "" : reader.GetString(10),
                        PropertyName = reader.IsDBNull(11) ? "" : reader.GetString(11),



                        StayStatus = DateTime.Now > reader.GetDateTime(2) ? "Ended" : "Active"
                    };
                    bookings.Add(booking);
                }
            }
        }

        return View(bookings);
    }
    // POST: Owner/CheckIn
    // GET: Owner/CheckIn
    public IActionResult CheckIn(int id)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"
            UPDATE Bookings 
            SET CheckInStatus = 'CheckedIn', CheckedInAt = GETDATE(), CheckedOutAt = NULL
            WHERE Id = @Id AND (CheckInStatus IS NULL OR CheckInStatus = 'CheckedOut')
        ";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("ManageRenters");
    }

    // POST: Owner/CheckOut
    [HttpPost]
    public IActionResult CheckOut(int id)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"
            UPDATE Bookings 
            SET CheckInStatus = 'CheckedOut', CheckedOutAt = GETDATE()
            WHERE Id = @Id AND CheckInStatus = 'CheckedIn'
        ";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("ManageRenters");
    }

    // POST: Owner/ResetCheckIn
    [HttpPost]
    public IActionResult ResetCheckIn(int id)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            string query = @"
            UPDATE Bookings 
            SET CheckInStatus = NULL, CheckedInAt = NULL, CheckedOutAt = NULL
            WHERE Id = @Id
        ";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        return RedirectToAction("ManageRenters");
    }

}
