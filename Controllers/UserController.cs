using Azure.Identity;
using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

public class UserController : Controller
{
    private readonly string connectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=EthioHomesDB;Trusted_Connection=True;";

    public IActionResult SignUp()
    {
        return View();
    }

    [HttpPost]
    public IActionResult SignUp(User user)
    {
        try
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
        }catch(Exception e)
        {
            throw (e);


        }
        ViewBag.Message = "Sign Up Successful";
        return RedirectToAction("Login");
    }

    public IActionResult Login()
    {
        return View(); // Show login form
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = "SELECT * FROM Users WHERE Email = @Email AND Password = @Password";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Email", email);
            cmd.Parameters.AddWithValue("@Password", password);

            SqlDataReader reader = cmd.ExecuteReader();
        
            if (reader.Read())
            {
                // Store user data in session
                HttpContext.Session.SetInt32("UserId", Convert.ToInt32(reader["Id"]));
                HttpContext.Session.SetString("UserType", reader["UserType"].ToString());
                HttpContext.Session.SetString("userName", reader["Name"].ToString());
                string userName = reader["Name"].ToString();
              

                // Redirect based on user type
                if (reader["UserType"].ToString() == "Owner")
                {
                    ViewBag.useName = userName;
                    return RedirectToAction("AddProperty", "Property", new {userName});
                }

                return RedirectToAction("Index", "Home", new {  userName });
            }
        }

        ViewBag.Message = "Invalid email or password.";
        return View();
    }

    public IActionResult Logout()
    {

        TempData["ShowLogoutConfirm"] = true;
        // Clear the session
        HttpContext.Session.Remove("UserId");
        HttpContext.Session.Remove("UserType");

        // Redirect to the home page or login page
        return RedirectToAction("index", "Home");
    }
}
