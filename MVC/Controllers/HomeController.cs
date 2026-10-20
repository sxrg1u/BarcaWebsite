using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MySqlConnector;
using System.Diagnostics;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connString;
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

            using var conn = new MySqlConnection(_connString);
            conn.Open();

            using var cmd = new MySqlCommand(
                "SELECT ShirtNumber, Name, Country, Position, BirthDate, ImagePath FROM Players ORDER BY ShirtNumber",
                conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                players.Add(new Player
                {
                    ShirtNumber = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Country = reader.GetString(2),
                    Position = reader.GetString(3),
                    BirthDate = reader.GetDateTime(4),
                    ImagePath = reader.GetString(5)
                });
            }
            return View(players);
        }

        public IActionResult Trophies()
        {
            var trophies = new List<Trophy>();

            using var conn = new MySqlConnection(_connString);
            conn.Open();

            using var cmd = new MySqlCommand(
                "SELECT Year, Competition, Coach, Category FROM Trophies ORDER BY Year desc",
                conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                trophies.Add(new Trophy
                {
                    Year = reader.GetInt32(0),
                    Competition = reader.GetString(1),
                    Coach = reader.GetString(2),
                    Category = Enum.Parse<CategoryEnum>(reader.GetString(3))

                });
            }

            return View(trophies);
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

        public HomeController(IConfiguration config) { _connString = config.GetConnectionString("BarcaData")!; }
    }
}