using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class PropertyController : Controller
    {
        private readonly string connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

        // Show Add Property form
        public IActionResult AddProperty()
        {
            //int? userId = HttpContext.Session.GetInt32("UserId");
            //string userType = HttpContext.Session.GetString("UserType");

            //// Check if the user is logged in and is an owner
            //if (userId == null || userType != "Owner")
            //{
            //    return RedirectToAction("index", "User");
            //}

            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult AddProperty(Property property, List<IFormFile> images)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            // Redirect to login if the user is not logged in
            if (userId == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Assign the OwnerId from the session
            property.OwnerId = userId.Value;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Insert property details
                string insertPropertyQuery = @"INSERT INTO Properties 
            (Title, Location, Price, PropertyType, Status, Bedrooms, Bathrooms, Description, OwnerId)
            VALUES (@Title, @Location, @Price, @PropertyType, @Status, @Bedrooms, @Bathrooms, @Description, @OwnerId);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

                SqlCommand cmd = new SqlCommand(insertPropertyQuery, conn);
                cmd.Parameters.AddWithValue("@Title", property.Title);
                cmd.Parameters.AddWithValue("@Location", property.Location);
                cmd.Parameters.AddWithValue("@Price", property.Price);
                cmd.Parameters.AddWithValue("@PropertyType", property.PropertyType);
                cmd.Parameters.AddWithValue("@Status", property.Status);
                cmd.Parameters.AddWithValue("@Bedrooms", property.Bedrooms);
                cmd.Parameters.AddWithValue("@Bathrooms", property.Bathrooms);
                cmd.Parameters.AddWithValue("@Description", property.Description);
                cmd.Parameters.AddWithValue("@OwnerId", property.OwnerId);

                int propertyId = Convert.ToInt32(cmd.ExecuteScalar()); // Get the newly inserted property ID

                // Save uploaded images
                foreach (var image in images)
                {
                    if (image != null && image.Length > 0)
                    {
                        // Generate a unique file name
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                        string filePath = Path.Combine("wwwroot/uploads", fileName);

                        // Save the image to the server
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            image.CopyTo(stream);
                        }

                        // Insert image file path into PropertyImages table
                        string insertImageQuery = @"INSERT INTO PropertyImages (PropertyId, ImagePath)
                                            VALUES (@PropertyId, @ImagePath)";


                        SqlCommand imgCmd = new SqlCommand(insertImageQuery, conn);
                        imgCmd.Parameters.AddWithValue("@PropertyId", propertyId);
                        imgCmd.Parameters.AddWithValue("@ImagePath", fileName); // Save only the file name
                        imgCmd.ExecuteNonQuery();
                    }
                }
            }


            return RedirectToAction("ViewMyListings", "Property");
        }


    }

}