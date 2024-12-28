using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class MessagesController : Controller
    {
        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

        // View messages for a specific property and user
        public IActionResult ViewMessages(int propertyId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            List<Message> messages = new List<Message>();
            int receiverId = 0;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Fetch property owner ID
                string ownerQuery = "SELECT OwnerId FROM Properties WHERE Id = @PropertyId";
                SqlCommand ownerCmd = new SqlCommand(ownerQuery, conn);
                ownerCmd.Parameters.AddWithValue("@PropertyId", propertyId);

                object ownerResult = ownerCmd.ExecuteScalar();
                if (ownerResult != null)
                {
                    receiverId = Convert.ToInt32(ownerResult);
                }

                // Fetch messages
                string query = @"SELECT m.*, u1.Name AS SenderName, u2.Name AS ReceiverName
                         FROM Messages m
                         JOIN Users u1 ON m.SenderId = u1.Id
                         JOIN Users u2 ON m.ReceiverId = u2.Id
                         WHERE m.PropertyId = @PropertyId
                         ORDER BY m.SentDate";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PropertyId", propertyId);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    messages.Add(new Message
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        PropertyId = Convert.ToInt32(reader["PropertyId"]),
                        SenderId = Convert.ToInt32(reader["SenderId"]),
                        ReceiverId = Convert.ToInt32(reader["ReceiverId"]),
                        Content = reader["Content"].ToString(),
                        SentDate = Convert.ToDateTime(reader["SentDate"]),
                    });
                }
            }

            ViewBag.PropertyId = propertyId;
            ViewBag.ReceiverId = receiverId; // Pass the owner's ID to the view
            ViewBag.UserId = userId;
            return View(messages);
        }


        // Send a new message
        [HttpPost]
        [HttpPost]
        public IActionResult SendMessage(int propertyId, int receiverId, string content)
        {
            int? senderId = HttpContext.Session.GetInt32("UserId");
            if (senderId == null)
            {
                Console.WriteLine("User not logged in. Redirecting to login.");
                return RedirectToAction("Login", "User");
            }

            Console.WriteLine($"Sending Message:");
            Console.WriteLine($"PropertyId: {propertyId}, SenderId: {senderId.Value}, ReceiverId: {receiverId}, Content: {content}");

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO Messages (PropertyId, SenderId, ReceiverId, Content, SentDate)
                             VALUES (@PropertyId, @SenderId, @ReceiverId, @Content, GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    cmd.Parameters.AddWithValue("@SenderId", senderId.Value);
                    cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                    cmd.Parameters.AddWithValue("@Content", content);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    Console.WriteLine($"{rowsAffected} row(s) inserted into Messages table.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting message: {ex.Message}");
                return RedirectToAction("Error", "Home");
            }

            return RedirectToAction("ViewMessages", new { propertyId });
        }




        // View all conversations grouped by property (for Owners)
        public IActionResult Conversations()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            List<Property> conversations = new List<Property>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"SELECT DISTINCT p.Id, p.Title
                         FROM Messages m
                         JOIN Properties p ON m.PropertyId = p.Id
                         WHERE m.ReceiverId = @OwnerId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@OwnerId", userId.Value);

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    conversations.Add(new Property
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        Title = reader["Title"].ToString(),
                    });
                }
            }

            return View(conversations);
        }


    }
}
