using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

public class PropertyController : Controller
{
    private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

    public IActionResult ViewMyListings()
    {
        int? userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "User");
        }

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // Get properties owned by the logged-in user
            string propertyQuery = "SELECT * FROM Properties WHERE OwnerId = @OwnerId";
            SqlCommand propertyCmd = new SqlCommand(propertyQuery, conn);
            propertyCmd.Parameters.AddWithValue("@OwnerId", userId);

            SqlDataReader propertyReader = propertyCmd.ExecuteReader();
            List<Property> properties = new List<Property>();

            while (propertyReader.Read())
            {
                properties.Add(new Property
                {
                    Id = Convert.ToInt32(propertyReader["Id"]),
                    Title = propertyReader["Title"].ToString(),
                    Location = propertyReader["Location"].ToString(),
                    Price = Convert.ToDecimal(propertyReader["Price"]),
                    PropertyType = propertyReader["PropertyType"].ToString(),
                    Status = propertyReader["Status"].ToString(),
                    Bedrooms = Convert.ToInt32(propertyReader["Bedrooms"]),
                    Bathrooms = Convert.ToInt32(propertyReader["Bathrooms"]),
                    Description = propertyReader["Description"].ToString(),
                    ImagePaths = new List<string>() // Initialize image paths
                });
            }
            propertyReader.Close();

            // Get images for each property
            foreach (var property in properties)
            {
                string imageQuery = "SELECT ImagePath FROM PropertyImages WHERE PropertyId = @PropertyId";
                SqlCommand imageCmd = new SqlCommand(imageQuery, conn);
                imageCmd.Parameters.AddWithValue("@PropertyId", property.Id);

                SqlDataReader imageReader = imageCmd.ExecuteReader();
                while (imageReader.Read())
                {
                    property.ImagePaths.Add(imageReader["ImagePath"].ToString());
                }
                imageReader.Close();
            }

            return View(properties);
        }
    }
}