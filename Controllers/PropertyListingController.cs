using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class PropertyListingController : Controller
    {
        private readonly string connectionString = "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

        //
        /// search filters
        public IActionResult Index(string location, string propertyType, decimal? maxPrice)
        {
            List<Property> properties = new List<Property>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();


                //  base query
                string query = "SELECT * FROM Properties WHERE Status = 'Available'";

                //  add filter user input
                if (!string.IsNullOrEmpty(location))
                {
                    query += " AND Location LIKE '%" + location + "%'";
                }

                if (!string.IsNullOrEmpty(propertyType))
                {
                    query += " AND PropertyType = '" + propertyType + "'";
                }

                if (maxPrice.HasValue)
                {
                    query += " AND Price <= " + maxPrice.Value;
                }

                // Create the command
                SqlCommand cmd = new SqlCommand(query, conn);


                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    properties.Add(new Property
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
                    });
                }
                reader.Close();

                // images for each property
                foreach (var property in properties)
                {
                    string imageQuery = "SELECT * FROM PropertyImages WHERE PropertyId = @PropertyId";
                    SqlCommand imageCmd = new SqlCommand(imageQuery, conn);
                    imageCmd.Parameters.AddWithValue("@PropertyId", property.Id);

                    SqlDataReader imageReader = imageCmd.ExecuteReader();
                    while (imageReader.Read())
                    {
                        property.Images.Add(new PropertyImage
                        {
                            Id = Convert.ToInt32(imageReader["Id"]),
                            PropertyId = property.Id,
                            ImagePath = imageReader["ImagePath"].ToString()
                        });
                    }
                    imageReader.Close();
                }
            }

            return View(properties);
        }

    }
}