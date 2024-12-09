using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using EthioHomes.Models;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;

namespace EthioHomes.Controllers
{
    public class UserController : Controller
    {
        private readonly string connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

        // Show the Sign-Up form
        public IActionResult SignUp()
        {
            return View();
        }

        // Handle Sign-Up 
      //  [HttpPost]
        public IActionResult SignUp(User user)
        {
            using (SqlConnection conn = new(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Users (Name, Email, Password, UserType) VALUES (@Name, @Email, @Password, @UserType)";
                SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", user.Password);
                cmd.Parameters.AddWithValue("@UserType", user.UserType);
                cmd.ExecuteNonQuery();
            }
            ViewBag.Message = "Sign Up Successful";
            return RedirectToAction("Login");
        }

        // Show the Login form
        public IActionResult Login()
        {
            return View();
        }

        // Handle Login submission
        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            using (SqlConnection conn = new(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Users WHERE Email = @Email AND Password = @Password";
                SqlCommand cmd = new(query, conn);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Password", password);

                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ViewBag.UserType = reader["UserType"].ToString();
                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Message = "Invalid email or password.";
            return View();
        }
    }
}
