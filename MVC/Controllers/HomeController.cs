using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Diagnostics;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        static readonly Dictionary<string, string> registeredUsers = new Dictionary<string, string> { { "admin", "1234" } };
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Squad()
        {
            var players = new List<Player>();

            return View(players);
        }

        public IActionResult Trophies()
        {
            var model = new TrophiesViewModel();

            return View(model);
        }

        public IActionResult Favourites()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [Route("Home/Error/{statusCode?}")]
        public IActionResult Error(int? statusCode = null)
        {
            if (statusCode == 404)
            {
                return View("NotFound");
            }

            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            foreach (var user in registeredUsers)
            {
                if (user.Key == username && user.Value == password)
                {
                    return RedirectToAction("Index");
                }
            }
            ViewBag.ErrorMessage = "Invalid username or password.";
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            if (registeredUsers.ContainsKey(username))
            {
                ViewBag.ErrorMessage = "Username already exists.";
                return View();
            }
            else
            {
                registeredUsers.Add(username, password);
                return RedirectToAction("Login");
            }
        }
    }
}