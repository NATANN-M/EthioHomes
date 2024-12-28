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
        public IActionResult AddProperty(Property property, List<IFormFile> images)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            // user is not logged in
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

                int propertyId = Convert.ToInt32(cmd.ExecuteScalar()); // Get  inserted property ID

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
                        imgCmd.Parameters.AddWithValue("@ImagePath", fileName); //  file name
                        imgCmd.ExecuteNonQuery();
                    }
                }
            }


            return RedirectToAction("ViewMyListings", "Property");
        }

        ///////////********* FOR Detail page "clicking the Details or View more  button" *************///////  ///////
        public IActionResult Details(int id)
        {
            Property property = null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

               
                string query = "SELECT * FROM Properties WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    property = new Property
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
                }
                reader.Close();

                // associated images
                if (property != null)
                {
                    string imageQuery = "SELECT * FROM PropertyImages WHERE PropertyId = @PropertyId";
                    SqlCommand imgCmd = new SqlCommand(imageQuery, conn);
                    imgCmd.Parameters.AddWithValue("@PropertyId", property.Id);

                    SqlDataReader imgReader = imgCmd.ExecuteReader();
                    while (imgReader.Read())
                    {
                        property.Images.Add(new PropertyImage
                        {
                            Id = Convert.ToInt32(imgReader["Id"]),
                            PropertyId = property.Id,
                            ImagePath = imgReader["ImagePath"].ToString()
                        });
                    }
                }
            }

            return property != null ? View(property) : NotFound();
        }



    }

}