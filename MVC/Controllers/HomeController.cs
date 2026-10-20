using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using MySqlConnector;
using System.Diagnostics;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connString;
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
                "SELECT Id, ShirtNumber, Name, Country, Position, BirthDate, ImagePath FROM Players ORDER BY ShirtNumber",
                conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                players.Add(new Player
                {
                    Id = reader.GetInt32(0),
                    ShirtNumber = reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Country = reader.GetString(3),
                    Position = reader.GetString(4),
                    BirthDate = reader.GetDateTime(5),
                    ImagePath = reader.GetString(6)
                });
            }
            reader.Close();

            // Favoriten des eingeloggten Users markieren
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                using var favCmd = new MySqlCommand("SELECT PlayerId FROM Favourites WHERE UserId = @u", conn);
                favCmd.Parameters.AddWithValue("@u", userId);
                using var favReader = favCmd.ExecuteReader();

                var favIds = new List<int>();
                while (favReader.Read())
                {
                    favIds.Add(favReader.GetInt32(0));
                }

                foreach (var p in players)
                {
                    p.IsFavourite = favIds.Contains(p.Id);
                }
            }

            return View(players);
        }

        [HttpPost]
        public IActionResult ToggleFavourite(int playerId, string returnTo = "Squad")
        {
            // Kommt der Klick von JavaScript (fetch), antworten wir mit JSON statt die Seite neu zu laden
            bool isFetch = Request.Headers["X-Requested-With"] == "fetch";

            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                if (isFetch) return Unauthorized();
                return RedirectToAction("Login");
            }

            using var conn = new MySqlConnection(_connString);
            conn.Open();

            // Erst loeschen, wenn nichts geloescht wurde, war er noch kein Favorit -> hinzufuegen
            using var del = new MySqlCommand("DELETE FROM Favourites WHERE UserId = @u AND PlayerId = @p", conn);
            del.Parameters.AddWithValue("@u", userId);
            del.Parameters.AddWithValue("@p", playerId);

            bool isFavourite = false;
            if (del.ExecuteNonQuery() == 0)
            {
                using var ins = new MySqlCommand("INSERT INTO Favourites (UserId, PlayerId) VALUES (@u, @p)", conn);
                ins.Parameters.AddWithValue("@u", userId);
                ins.Parameters.AddWithValue("@p", playerId);
                ins.ExecuteNonQuery();
                isFavourite = true;
            }

            if (isFetch)
            {
                return Json(new { isFavourite });
            }

            // Zurueck auf die Seite, von der geklickt wurde
            return RedirectToAction(returnTo == "Favourites" ? "Favourites" : "Squad");
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
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var players = new List<Player>();

            using var conn = new MySqlConnection(_connString);
            conn.Open();

            using var cmd = new MySqlCommand(
                @"SELECT p.Id, p.Name, p.Position, p.ImagePath
                  FROM Favourites f
                  JOIN Players p ON p.Id = f.PlayerId
                  WHERE f.UserId = @u
                  ORDER BY f.AddedAt",
                conn);
            cmd.Parameters.AddWithValue("@u", userId);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                players.Add(new Player
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Position = reader.GetString(2),
                    ImagePath = reader.GetString(3),
                    IsFavourite = true
                });
            }

            return View(players);
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
            using var conn = new MySqlConnection(_connString);
            conn.Open();

            using var cmd = new MySqlCommand("SELECT Id FROM Users WHERE Username = @u AND Password = @p", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", password);

            var result = cmd.ExecuteScalar();
            if (result != null)
            {
                // UserId merken, damit Favoriten wissen, wer eingeloggt ist
                HttpContext.Session.SetInt32("UserId", Convert.ToInt32(result));
                HttpContext.Session.SetString("Username", username);
                return RedirectToAction("Index");
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
            // Benutzername darf nicht leer und max. 20 Zeichen lang sein
            if (string.IsNullOrWhiteSpace(username) || username.Length > 20)
            {
                ViewBag.ErrorMessage = "Username must be 1 to 20 characters.";
                return View();
            }

            using var conn = new MySqlConnection(_connString);
            conn.Open();

            using var check = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @u", conn);
            check.Parameters.AddWithValue("@u", username);

            if (Convert.ToInt32(check.ExecuteScalar()) > 0)
            {
                ViewBag.ErrorMessage = "Username already exists.";
                return View();
            }

            using var insert = new MySqlCommand("INSERT INTO Users (Username, Password) VALUES (@u, @p)", conn);
            insert.Parameters.AddWithValue("@u", username);
            insert.Parameters.AddWithValue("@p", password);
            insert.ExecuteNonQuery();

            return RedirectToAction("Login");
        }

        public HomeController(IConfiguration config) { _connString = config.GetConnectionString("BarcaData")!; }
    }
}