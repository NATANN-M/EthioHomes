using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class ManagePropertyController : Controller
    {
        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

        // GET: Edit Property
        [HttpGet]
        public IActionResult EditProperty(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }
            Console.WriteLine($"getting property with ID: {id} for Owner: {userId}");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Properties WHERE Id = @Id AND OwnerId = @OwnerId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@OwnerId", userId);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Property property = new Property
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
                    };
                    return View(property);
                }
            }
            return NotFound();
        }

        // POST: Edit Property
        [HttpPost]
        public IActionResult EditProperty(Property property)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = @"UPDATE Properties
                                 SET Title = @Title, Location = @Location, Price = @Price, 
                                     PropertyType = @PropertyType, Status = @Status, 
                                     Bedrooms = @Bedrooms, Bathrooms = @Bathrooms, 
                                     Description = @Description
                                 WHERE Id = @Id AND OwnerId = @OwnerId";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Title", property.Title);
                cmd.Parameters.AddWithValue("@Location", property.Location);
                cmd.Parameters.AddWithValue("@Price", property.Price);
                cmd.Parameters.AddWithValue("@PropertyType", property.PropertyType);
                cmd.Parameters.AddWithValue("@Status", property.Status);
                cmd.Parameters.AddWithValue("@Bedrooms", property.Bedrooms);
                cmd.Parameters.AddWithValue("@Bathrooms", property.Bathrooms);
                cmd.Parameters.AddWithValue("@Description", property.Description);
                cmd.Parameters.AddWithValue("@Id", property.Id);
                cmd.Parameters.AddWithValue("@OwnerId", userId);

                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("ViewMyListings", "Property");
        }

        // POST: Delete Property
        [HttpPost]
        public IActionResult DeleteProperty(int id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            Console.WriteLine($"DeleteProperty called for Property ID: {id} and Owner: {userId}");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Properties WHERE Id = @Id AND OwnerId = @OwnerId";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@OwnerId", userId);

                int rowsAffected = cmd.ExecuteNonQuery();
               
                
                Console.WriteLine($"{rowsAffected} rows affected.");  //how many row affected after delete
            }

            return RedirectToAction("ViewMyListings", "Property");
        }

    }
}
