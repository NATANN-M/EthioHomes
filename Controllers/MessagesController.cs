using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class MessagesController : Controller
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
        public IActionResult ViewMessages(int propertyId, int receiverId )
        {
            try
            {
                int userId = GetLoggedInUserId();

                List<Message> messages = new List<Message>();

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // if it is null
                    if (receiverId == 0)
                    {
                        string receiverQuery = @"
                    SELECT DISTINCT 
                        CASE 
                            WHEN m.SenderId = @UserId THEN m.ReceiverId
                            ELSE m.SenderId
                        END AS ReceiverId
                    FROM Messages m
                    WHERE m.PropertyId = @PropertyId AND (@UserId IN (m.SenderId, m.ReceiverId))";

                        SqlCommand receiverCmd = new SqlCommand(receiverQuery, conn);
                        receiverCmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        receiverCmd.Parameters.AddWithValue("@UserId", userId);

                        object receiverResult = receiverCmd.ExecuteScalar();
                        if (receiverResult != null)
                        {
                            receiverId = Convert.ToInt32(receiverResult);
                        }
                    }

                    // Retrieve messages
                    string query = @"
                SELECT m.*, u1.Name AS SenderName, u2.Name AS ReceiverName
                FROM Messages m
                JOIN Users u1 ON m.SenderId = u1.Id
                JOIN Users u2 ON m.ReceiverId = u2.Id
                WHERE m.PropertyId = @PropertyId
                AND (@UserId IN (m.SenderId, m.ReceiverId))
                ORDER BY m.SentDate";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    cmd.Parameters.AddWithValue("@UserId", userId);

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
                ViewBag.ReceiverId = receiverId;
                ViewBag.UserId = userId;

                return View(messages);
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ViewMessages: {ex.Message}");
                TempData["Error"] = "Unable to load messages. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }



        [HttpPost]
        public IActionResult SendMessage(Message message, int propertyId, int receiverId, string content)
        {

            System.Diagnostics.Debug.WriteLine($"Session UserId: {HttpContext.Session.GetInt32("UserId")}");

            try
            {
                int senderId = GetLoggedInUserId();

                // Debugging: Log received values
                System.Diagnostics.Debug.WriteLine($"SenderId: {senderId}");
                System.Diagnostics.Debug.WriteLine($"PropertyId: {propertyId}");
                System.Diagnostics.Debug.WriteLine($"ReceiverId: {receiverId}");
                System.Diagnostics.Debug.WriteLine($"Content: {content}");

                if (propertyId == 0 || receiverId == 0 || string.IsNullOrWhiteSpace(content))
                {
                    TempData["Error"] = "Invalid input values.";
                    return RedirectToAction("ViewMessages", new { propertyId });
                }

                using (SqlConnection conn = new(connectionString))
                {
                    conn.Open();

                    string query = @"INSERT INTO Messages (PropertyId, SenderId, ReceiverId, Content, SentDate)
                             VALUES (@PropertyId, @SenderId, @ReceiverId, @Content, GETDATE())";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    cmd.Parameters.AddWithValue("@SenderId", senderId);
                    cmd.Parameters.AddWithValue("@ReceiverId", receiverId);
                    cmd.Parameters.AddWithValue("@Content", content);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    // Debugging: Log query execution
                    System.Diagnostics.Debug.WriteLine($"{rowsAffected} row(s) inserted into Messages table.");
                }

                return RedirectToAction("ViewMessages", new { propertyId });
            }
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SendMessage: {ex.Message}");
                TempData["Error"] = "Unable to send message. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }


        public IActionResult Conversations()
        {
            try
            {
                int userId = GetLoggedInUserId();

                List<Property> conversations = new List<Property>();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                SELECT DISTINCT p.Id, p.Title, 
                                CASE 
                                    WHEN m.SenderId = @UserId THEN m.ReceiverId
                                    ELSE m.SenderId
                                END AS ChatUserId
                FROM Messages m
                JOIN Properties p ON m.PropertyId = p.Id
                WHERE p.OwnerId = @UserId OR m.ReceiverId = @UserId OR m.SenderId = @UserId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@UserId", userId);

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
            catch (UnauthorizedAccessException)
            {
                return RedirectToAction("Login", "User");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Conversations: {ex.Message}");
                TempData["Error"] = "Unable to load conversations. Please try again.";
                return RedirectToAction("Error", "Home");
            }
        }

        [HttpPost]
        public IActionResult RemoveConversation(int propertyId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login", "User");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Remove all messages for this property and user
                string deleteMessages = "DELETE FROM Messages WHERE PropertyId = @PropertyId AND (SenderId = @UserId OR ReceiverId = @UserId)";
                using (SqlCommand cmd = new SqlCommand(deleteMessages, conn))
                {
                    cmd.Parameters.AddWithValue("@PropertyId", propertyId);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.ExecuteNonQuery();
                }
            }
            TempData["Success"] = "Conversation removed.";
            return RedirectToAction("Conversations");
        }


    }
}
