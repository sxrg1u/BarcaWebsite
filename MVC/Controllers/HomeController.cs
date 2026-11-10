using Microsoft.AspNetCore.Mvc;
using MVC.Data;
using MVC.Models;
using MVC.Services;
using System.Diagnostics;

namespace MVC.Controllers
{
    // Oeffentliche Seiten: Controller nimmt die Anfrage entgegen, holt die Daten aus den Repositories
    // und gibt sie an die View weiter. SQL steht ausschliesslich in den Repositories.
    public class HomeController : Controller
    {
        private readonly UserRepository _users;
        private readonly PlayerRepository _players;
        private readonly TrophyRepository _trophies;
        private readonly FavouriteRepository _favourites;

        public HomeController(UserRepository users, PlayerRepository players,
                              TrophyRepository trophies, FavouriteRepository favourites)
        {
            _users = users;
            _players = players;
            _trophies = trophies;
            _favourites = favourites;
        }

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
            var players = _players.GetAll();

            // Favoriten des eingeloggten Users markieren
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId != null)
            {
                var favIds = _favourites.GetPlayerIds(userId.Value);
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

            if (_players.GetById(playerId) == null)
            {
                return NotFound();
            }

            bool isFavourite = _favourites.Toggle(userId.Value, playerId);

            if (isFetch)
            {
                return Json(new { isFavourite });
            }

            // Zurueck auf die Seite, von der geklickt wurde
            return RedirectToAction(returnTo == "Favourites" ? "Favourites" : "Squad");
        }

        public IActionResult Trophies()
        {
            return View(_trophies.GetAll());
        }

        public IActionResult Favourites()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            return View(_players.GetFavourites(userId.Value));
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
            var user = _users.GetByUsername(username?.Trim() ?? "");

            if (user == null || string.IsNullOrEmpty(password) || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }

            // Altes Klartext-Passwort aus der Zeit vor dem Hashing: jetzt sicher abspeichern
            if (!PasswordHasher.IsHashed(user.PasswordHash))
            {
                _users.UpdatePassword(user.Id, PasswordHasher.Hash(password));
            }

            // UserId merken, damit Favoriten wissen, wer eingeloggt ist
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Username", user.Username);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            // Session leeren, damit niemand mehr eingeloggt ist
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password)
        {
            username = username?.Trim() ?? "";

            // Benutzername darf nicht leer und max. 20 Zeichen lang sein
            if (username.Length == 0 || username.Length > 20)
            {
                ViewBag.ErrorMessage = "Username must be 1 to 20 characters.";
                return View();
            }

            if (string.IsNullOrEmpty(password) || password.Length < 4 || password.Length > 100)
            {
                ViewBag.ErrorMessage = "Password must be 4 to 100 characters.";
                return View();
            }

            if (_users.UsernameExists(username))
            {
                ViewBag.ErrorMessage = "Username already exists.";
                return View();
            }

            _users.Create(username, PasswordHasher.Hash(password));

            return RedirectToAction("Login");
        }
    }
}
