using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace EthioHomes.Controllers
{
    public class PropertyController : Controller
    {

        private readonly string connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";
        public IActionResult Addproperty()
        {

            return View();

        }
        public IActionResult AddProperty(Property property)
        {


            
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string insertPropertyQuery = @"INSERT INTO Properties (UserId, Title, Location, Price, PropertyType, Status, Bedrooms, Bathrooms, Description)
                                       VALUES (@UserId, @Title, @Location, @Price, @PropertyType, @Status, @Bedrooms, @Bathrooms, @Description);
                                       SELECT SCOPE_IDENTITY();";

                SqlCommand cmd = new SqlCommand(insertPropertyQuery, conn);
               // cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@Title", property.Title);
                cmd.Parameters.AddWithValue("@Location", property.Location);
                cmd.Parameters.AddWithValue("@Price", property.Price);
                cmd.Parameters.AddWithValue("@PropertyType", property.PropertyType);
                cmd.Parameters.AddWithValue("@Status", property.Status);
                cmd.Parameters.AddWithValue("@Bedrooms", property.Bedrooms);
                cmd.Parameters.AddWithValue("@Bathrooms", property.Bathrooms);
                cmd.Parameters.AddWithValue("@Description", property.Description);

                return RedirectToAction("Index", "Home");
            }
        }
    }
}
