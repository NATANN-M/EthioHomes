using System.Diagnostics;
using EthioHomes.Models;
using Microsoft.AspNetCore.Mvc;

namespace EthioHomes.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var userType = HttpContext.Session.GetString("UserType");

            var userName = HttpContext.Session.GetString("UserName");
            if (userType == null)
            {
                // Pass userType to the view only if the user is logged in
                Console.WriteLine("empty fdgghh");
            }
            else
            {

                ViewBag.UserType = userType;
                ViewBag.UserName = userName; 


            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
