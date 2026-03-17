using EthioHomes.services.EthioHomes.Services;
using Microsoft.AspNetCore.Mvc;
using EthioHomes.services;  // Match actual namespace
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.Data.SqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;
using System.Configuration;

namespace EthioHomes.Controllers
{
    public class BookingController : Controller
    {
        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

        private readonly EmailService _emailService;
        private IConfiguration _configuration;

        string ngrokBaseUrl = "https://6668-196-190-62-228.ngrok-free.app";

        public BookingController(EmailService emailService, IConfiguration config)
        {
            _emailService = emailService;

            _configuration = config;
        }

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
            string renterEmail = "";
            string renterName = "";
            string propertyName = "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Update booking status
                string updateQuery = "UPDATE Bookings SET Status = 'Approved' WHERE Id = @Id";
                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@Id", bookingId);
                updateCmd.ExecuteNonQuery();

                // Get renter email, name and property name
                string fetchQuery = @"
    SELECT U.Email, U.Name, P.Title
    FROM Bookings B
    JOIN Users U ON B.UserId = U.Id
    JOIN Properties P ON B.PropertyId = P.Id
    WHERE B.Id = @Id";


                SqlCommand fetchCmd = new SqlCommand(fetchQuery, conn);
                fetchCmd.Parameters.AddWithValue("@Id", bookingId);

                using (SqlDataReader reader = fetchCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        renterEmail = reader.GetString(0);
                        renterName = reader.GetString(1);
                        propertyName = reader.GetString(2);
                    }
                }
            }

            // Send Email
            string subject = "Booking Approved – EthioHomes";
            string body = $@"
Hi {renterName},

Congratulations! Your booking request for <b>{propertyName}</b> has been <b>approved</b>.

What happens next?
- Please log in to your EthioHomes account.
- Review and accept the rental agreement for your booking.
- Once you accept the agreement, you can proceed to make your payment securely through our platform.

If you have any questions or need assistance, feel free to reply to this email or contact our support team.

Thank you for choosing EthioHomes. We wish you a pleasant stay!

Best regards,
The EthioHomes Team
";
            _emailService.SendEmail(renterEmail, subject, body);


            TempData["Success"] = "Booking approved.";
            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        [HttpPost]
        public IActionResult RejectBooking(int bookingId)
        {
            string renterEmail = "";
            string renterName = "";
            string propertyName = "";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Update booking status
                string updateQuery = "UPDATE Bookings SET Status = 'Rejected' WHERE Id = @Id";
                SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                updateCmd.Parameters.AddWithValue("@Id", bookingId);
                updateCmd.ExecuteNonQuery();

                // Get renter email, name and property name
                string fetchQuery = @"
    SELECT U.Email, U.Name, P.Title
    FROM Bookings B
    JOIN Users U ON B.UserId = U.Id
    JOIN Properties P ON B.PropertyId = P.Id
    WHERE B.Id = @Id";


                SqlCommand fetchCmd = new SqlCommand(fetchQuery, conn);
                fetchCmd.Parameters.AddWithValue("@Id", bookingId);

                using (SqlDataReader reader = fetchCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        renterEmail = reader.GetString(0);
                        renterName = reader.GetString(1);
                        propertyName = reader.GetString(2);
                    }
                }
            }

            // Send Email
            string subject = "Booking Rejected – EthioHomes";
            string body = $@"
Hi {renterName},

We regret to inform you that your booking request for <b>{propertyName}</b> has not been approved at this time.

We understand this may be disappointing. Please know that there are many other wonderful properties available on EthioHomes that may suit your needs.

What can you do next?
- Visit the EthioHomes website to explore and book other available properties.
- If you have any questions or need assistance, feel free to reply to this email or contact our support team.

Thank you for considering EthioHomes. We hope to assist you in finding the perfect place soon!

Best regards,
The EthioHomes Team
";
            _emailService.SendEmail(renterEmail, subject, body);


            TempData["Success"] = "Booking rejected.";
            return RedirectToAction("ManageBookings");
        }

        [HttpPost]
        public IActionResult SubmitAgreement(int bookingId, bool acceptTerms, bool responsibility)
        {

            if (!acceptTerms || !responsibility)
            {
                TempData["Error"] = "You must accept all terms before proceeding.";
                return RedirectToAction("Agreement", new { bookingId });
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                    INSERT INTO Agreements (BookingId, UserId, OwnerId, AgreementDate, IsAccepted)
                    SELECT b.Id, b.UserId, p.OwnerId, GETDATE(), 1
                    FROM Bookings b
                    JOIN Properties p ON b.PropertyId = p.Id
                    WHERE b.Id = @BookingId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.ExecuteNonQuery();
            }
            using (SqlConnection con2 = new SqlConnection(connectionString))
            {
                con2.Open();
                string query2 = @"UPDATE Bookings SET AgreementAccepted = 1  WHERE Id = @BookingId";
                using (SqlCommand cmd2 = new SqlCommand(query2, con2))
                {
                    cmd2.Parameters.AddWithValue("@BookingId", bookingId);
                    cmd2.ExecuteNonQuery();
                }
            }


            TempData["Success"] = "Agreement accepted.";
            return RedirectToAction("ProceedToPayment", new {  bookingId });

        }

        public IActionResult Agreement(int bookingId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            dynamic agreementData = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
                    SELECT b.Id AS BookingId, b.StartDate, b.EndDate, b.BookingDate,
                           p.Title AS PropertyTitle, p.Location, p.Price,
                           o.Name AS OwnerName, o.Email AS OwnerEmail,
                           u.Name AS UserName
                    FROM Bookings b
                    JOIN Properties p ON b.PropertyId = p.Id
                    JOIN Users o ON p.OwnerId = o.Id
                    JOIN Users u ON b.UserId = u.Id
                    WHERE b.Id = @BookingId AND b.UserId = @UserId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                cmd.Parameters.AddWithValue("@UserId", userId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    agreementData = new
                    {
                        BookingId = (int)reader["BookingId"],
                        StartDate = (DateTime)reader["StartDate"],
                        EndDate = (DateTime)reader["EndDate"],
                        BookingDate = (DateTime)reader["BookingDate"],
                        PropertyTitle = reader["PropertyTitle"].ToString(),
                        Address = reader["Location"].ToString(),
                        Price = Convert.ToDecimal(reader["Price"]),
                        OwnerName = reader["OwnerName"].ToString(),
                        OwnerEmail = reader["OwnerEmail"].ToString(),
                        UserName = reader["UserName"].ToString()
                    };
                }
            }

            if (agreementData == null)
            {
                TempData["Error"] = "Booking not found or access denied.";
                return RedirectToAction("Index", "Home");
            }

            return View(agreementData);
        }

        [HttpPost]
        public IActionResult CompletePayment(IFormCollection form)
        {
            int bookingId = Convert.ToInt32(form["bookingId"]);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string checkSql = "SELECT AgreementAccepted FROM Bookings WHERE Id = @Id";
                using (SqlCommand checkCmd = new SqlCommand(checkSql, conn))
                {
                    checkCmd.Parameters.AddWithValue("@Id", bookingId);
                    var accepted = checkCmd.ExecuteScalar();
                    if (accepted == null || Convert.ToBoolean(accepted) == false)
                    {
                        TempData["error"] = "Booking not found or agreement not accepted.";
                        return RedirectToAction("Bookings", "Booking");
                    }
                    string updateSql = "UPDATE Bookings SET IsPaid = 1, Status = 'Paid' WHERE Id = @Id";
                    using (SqlCommand cmd = new SqlCommand(updateSql, conn))
                    {

                        cmd.Parameters.AddWithValue("@Id", bookingId);
                        cmd.ExecuteNonQuery();
                    }

                }

              
            }

            TempData["success"] = "Payment successful. Your booking is now confirmed.";
            return RedirectToAction("Bookings", "Booking");
        }

        [HttpGet]
        public IActionResult NextStep(int bookingId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT AgreementAccepted, IsPaid FROM Bookings WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", bookingId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bool agreementAccepted = reader["AgreementAccepted"] != DBNull.Value && Convert.ToBoolean(reader["AgreementAccepted"]);
                            bool isPaid = reader["IsPaid"] != DBNull.Value && Convert.ToBoolean(reader["IsPaid"]);

                            if (!agreementAccepted)
                                return RedirectToAction("Agreement", new { bookingId });

                            if (!isPaid)
                                return RedirectToAction("ProceedToPayment", new { bookingId });

                            TempData["success"] = "Booking is fully complete.";
                            return RedirectToAction("Bookings");
                        }
                    }
                }
            }

            TempData["error"] = "Booking not found.";
            return RedirectToAction("Bookings");
        }


        [HttpGet]
        public IActionResult ProceedToPayment(int bookingId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            dynamic bookingData = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
           SELECT b.Id AS BookingId, b.StartDate, b.EndDate, b.BookingDate,
       b.PropertyId, p.Title AS PropertyTitle, p.Price,
       u.Name AS UserName, u.Email AS UserEmail,
       o.Name AS OwnerName, o.Phone AS OwnerPhone
FROM Bookings b
JOIN Properties p ON b.PropertyId = p.Id
JOIN Users u ON b.UserId = u.Id
JOIN Users o ON p.OwnerId = o.Id
WHERE b.Id = @BookingId
";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    DateTime startDate = Convert.ToDateTime(reader["StartDate"]);
                    DateTime endDate = Convert.ToDateTime(reader["EndDate"]);
                    decimal monthlyPrice = Convert.ToDecimal(reader["Price"]);

                    int totalDays = (endDate - startDate).Days;
                    int totalMonths = (int)Math.Ceiling(totalDays / 30.0m);

                    decimal totalPrice;

                    if (totalMonths > 3)
                    {
                        // Charge only for 3 months
                        totalPrice = 3 * monthlyPrice;
                    }
                    else
                    {
                        // Calculate daily price based on monthly price (assuming 30 days per month)
                        decimal dailyPrice = monthlyPrice / 30m;
                        totalPrice = dailyPrice * totalDays;
                    }




                    bookingData = new
                    {
                        BookingId = (int)reader["BookingId"],
                        PropertyTitle = reader["PropertyTitle"].ToString(),
                        UserName = reader["UserName"].ToString(),
                        OwnerName = reader["OwnerName"].ToString(),
                        StartDate = startDate,
                        EndDate = endDate,
                        Price = totalPrice
                    };
                }
            }

            if (bookingData == null)
            {
                TempData["Error"] = "Booking not found.";
                return RedirectToAction("Home", "Index");
            }

            return View(bookingData);
        }


        [HttpPost]
        [Route("Booking/CompletePayment")]
        [HttpPost]
        public IActionResult CompletePayment(int bookingId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Get property id to remove from saved
                int propertyId = 0;
                string getPropQuery = "SELECT PropertyId FROM Bookings WHERE Id = @BookingId";
                using (SqlCommand cmd = new SqlCommand(getPropQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingId", bookingId);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                        propertyId = Convert.ToInt32(result);
                }

                // Delete from SavedProperties if exists
                if (propertyId != 0)
                {
                    string deleteSavedQuery = "DELETE FROM SavedProperties WHERE PropertyId = @PropertyId AND UserId = @UserId";
                    using (SqlCommand cmd = new SqlCommand(deleteSavedQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Update Booking to paid 
                string updateBookingQuery = "UPDATE Bookings SET Status = 'Paid',IsPaid=1 WHERE Id = @BookingId";
                using (SqlCommand cmd = new SqlCommand(updateBookingQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@BookingId", bookingId);
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Success"] = "Payment completed successfully!";
            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        public async Task<IActionResult> StartChapaPayment(int bookingId, string payerPhone)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            dynamic bookingData = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"
        SELECT b.Id AS BookingId, b.StartDate, b.EndDate, b.BookingDate,
               b.PropertyId, p.Title AS PropertyTitle, p.Price,
               u.Name AS UserName, u.Email AS UserEmail,
               o.Name AS OwnerName, o.Phone AS OwnerPhone
        FROM Bookings b
        JOIN Properties p ON b.PropertyId = p.Id
        JOIN Users u ON b.UserId = u.Id
        JOIN Users o ON p.OwnerId = o.Id
        WHERE b.Id = @BookingId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@BookingId", bookingId);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    DateTime start = (DateTime)reader["StartDate"];
                    DateTime end = (DateTime)reader["EndDate"];
                    decimal price = Convert.ToDecimal(reader["Price"]);
                    int days = (end - start).Days;
                    int months = (int)Math.Ceiling(days / 30.0m);
                    decimal total = months > 3 ? 3 * price : (price / 30m) * days;

                    bookingData = new
                    {
                        BookingId = bookingId,
                        Amount = total,
                        Email = reader["UserEmail"].ToString(),
                        UserName = reader["UserName"].ToString(),
                        OwnerPhone = reader["OwnerPhone"].ToString()
                    };
                }
            }

            if (bookingData == null)
                return RedirectToAction("Dashboard", "Home");

            var payerNameParts = bookingData.UserName.Split(' ');
            string firstName = payerNameParts[0];
            string lastName = payerNameParts.Length > 1 ? payerNameParts[1] : "User";

            var txRef = "TX-" + Guid.NewGuid().ToString();




/////////////////////////////////////////////////////
///

            // string ngrokBaseUrl = "https://76bf-196-189-191-87.ngrok-free.app"; // Replace with live Ngrok URL
            string returnUrl = $"{ngrokBaseUrl}/Booking/PaymentCallback?txRef={txRef}";

            /////////////////////////////////////////////////
            var paymentService = new PaymentService(_configuration);
            var redirectUrl = await paymentService.CreateChapaPayment(
                amount: bookingData.Amount,
                currency: "ETB",
                email: bookingData.Email,
                firstName: firstName,
                lastName: lastName,
                txRef: txRef,
                returnUrl: returnUrl,
                phoneNumber: payerPhone //  This is payer's phone 
            );

            if (redirectUrl != null)
            {
                // Save payment log
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string insert = @"INSERT INTO Payments (BookingId, Amount, OwnerPhone, PayerId, PaymentDate, PaymentMethod, ChapaTransactionId, Status)
                              VALUES (@BookingId, @Amount, @OwnerPhone, @PayerId, GETDATE(), 'Telebirr', @TxRef, 'Pending')";

                    using (SqlCommand cmd = new SqlCommand(insert, conn))
                    {
                        cmd.Parameters.AddWithValue("@BookingId", bookingData.BookingId);
                        cmd.Parameters.AddWithValue("@Amount", bookingData.Amount);
                        cmd.Parameters.AddWithValue("@OwnerPhone", bookingData.OwnerPhone);
                        cmd.Parameters.AddWithValue("@PayerId", userId);
                        cmd.Parameters.AddWithValue("@TxRef", txRef);
                        cmd.ExecuteNonQuery();
                    }
                }

                return Redirect(redirectUrl);
            }

            TempData["Error"] = "Failed to initiate payment.";
            return RedirectToAction("ProceedToPayment", new { bookingId });
        }


        [HttpGet]
        public IActionResult PaymentCallback(string txRef)
        {
            if (string.IsNullOrWhiteSpace(txRef))
            {
                return BadRequest("Missing or invalid transaction reference.");
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    //  Update booking as Paid
                    string updateBooking = @"
                UPDATE Bookings 
                SET Status = 'Paid', IsPaid = 1
                WHERE Id = (SELECT BookingId FROM Payments WHERE ChapaTransactionId = @TxRef)";
                    using (SqlCommand cmd = new SqlCommand(updateBooking, conn))
                    {
                        cmd.Parameters.AddWithValue("@TxRef", txRef);
                        cmd.ExecuteNonQuery();
                    }

                    //  Update payment status
                    string updatePayment = @"
                UPDATE Payments 
                SET Status = 'Paid', PaymentDate = GETDATE()
                WHERE ChapaTransactionId = @TxRef";
                    using (SqlCommand cmd = new SqlCommand(updatePayment, conn))
                    {
                        cmd.Parameters.AddWithValue("@TxRef", txRef);
                        cmd.ExecuteNonQuery();
                    }

                    //  Fetch data to email owner
                    string fetchDetailsQuery = @"
                SELECT 
                    u.Name AS RenterName,
                    u.Email AS RenterEmail,
                    o.Email AS OwnerEmail,
                    p.Title AS PropertyTitle,
                    pay.Amount
                FROM Payments pay
                INNER JOIN Bookings b ON b.Id = pay.BookingId
                INNER JOIN Users u ON u.Id = b.UserId
                INNER JOIN Properties p ON p.Id = b.PropertyId
                INNER JOIN Users o ON o.Id = p.OwnerId
                WHERE pay.ChapaTransactionId = @TxRef";

                    string renterName = "", renterEmail = "", propertyTitle = "", ownerEmail = "";
                    decimal amount = 0;

                    using (SqlCommand cmd = new SqlCommand(fetchDetailsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@TxRef", txRef);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                renterName = reader["RenterName"].ToString();
                                renterEmail = reader["RenterEmail"].ToString();
                                propertyTitle = reader["PropertyTitle"].ToString();
                                ownerEmail = reader["OwnerEmail"].ToString();
                                amount = Convert.ToDecimal(reader["Amount"]);
                            }
                        }
                    }

                    Console.WriteLine(ownerEmail);
                    //  Send email to owner
                    if (!string.IsNullOrEmpty(ownerEmail))
                    {
                        string subject = "New Payment Received for Your Property";
                        string body = $@"
Dear Owner,

You have received a new payment for your property: {propertyTitle}.

Payment Details:
- Renter Name: {renterName}
- Renter Email: {renterEmail}
- Amount Paid: {amount:C}

Please check your EthioHomes dashboard for more information.

Regards,
EthioHomes Team";

                        Console.WriteLine(ownerEmail);
                        Console.WriteLine(subject);
                        Console.WriteLine(body);
                        _emailService.SendEmail(ownerEmail, subject, body); 
                    }
                }

                TempData["Success"] = "Payment completed successfully!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Payment processing failed. Please contact support.";
                return RedirectToAction("Index", "Home");
            }
        }






    }
}
