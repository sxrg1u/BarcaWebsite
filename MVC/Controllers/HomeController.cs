using Microsoft.AspNetCore.Mvc;
using MVC.Data;
using MVC.Models;
using MVC.Models.ViewModels;
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
        private readonly LineupRepository _lineups;

        public HomeController(UserRepository users, PlayerRepository players,
                              TrophyRepository trophies, FavouriteRepository favourites,
                              LineupRepository lineups)
        {
            _users = users;
            _players = players;
            _trophies = trophies;
            _favourites = favourites;
            _lineups = lineups;
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

        // Aufstellung bauen: jeder darf, gespeichert wird nur fuer eingeloggte User
        // id = gespeicherte Aufstellung laden, ohne id = neue Aufstellung bauen
        public IActionResult Lineup(int? id)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            var model = new LineupPageViewModel { Players = _players.GetAll() };

            if (userId != null)
            {
                model.SavedLineups = _lineups.GetAll(userId.Value);
                if (id != null)
                {
                    model.Current = _lineups.Get(id.Value, userId.Value);
                    // Fremde oder geloeschte Aufstellung -> einfach eine neue anfangen
                    if (model.Current == null) return RedirectToAction("Lineup", new { id = (int?)null });
                }
            }

            return View(model);
        }

        // id = 0 -> neu anlegen, sonst ueberschreiben. slots: 20 Eintraege (0-10 Startelf, 11-19 Bank), 0 = leer.
        [HttpPost]
        public IActionResult SaveLineup(int id, string? name, string formation, int[] slots, int? captainId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            name = name?.Trim() ?? "";
            if (name.Length == 0 || name.Length > Models.Lineup.NameMaxLength)
            {
                return BadRequest(new { error = $"Name must be 1 to {Models.Lineup.NameMaxLength} characters." });
            }

            if (!Models.Lineup.Formations.Contains(formation) || slots == null || slots.Length > Models.Lineup.SlotCount)
            {
                return BadRequest(new { error = "Invalid lineup." });
            }

            if (id == 0 && _lineups.Count(userId.Value) >= Models.Lineup.MaxPerUser)
            {
                return BadRequest(new { error = $"You can save up to {Models.Lineup.MaxPerUser} lineups. Delete one first." });
            }

            var validIds = _players.GetAll().Select(p => p.Id).ToHashSet();
            var lineup = new Lineup { Id = id, Name = name, Formation = formation };

            for (int slot = 0; slot < slots.Length; slot++)
            {
                int playerId = slots[slot];
                if (playerId == 0) continue;

                // Unbekannter Spieler oder doppelt aufgestellt -> ungueltig
                if (!validIds.Contains(playerId) || lineup.Slots.ContainsValue(playerId))
                {
                    return BadRequest(new { error = "Invalid lineup." });
                }
                lineup.Slots[slot] = playerId;
            }

            // Kapitaen muss in der Startelf stehen
            if (captainId != null && lineup.Slots.Any(s => s.Key < Models.Lineup.StarterCount && s.Value == captainId))
            {
                lineup.CaptainId = captainId;
            }

            int? savedId = _lineups.Save(userId.Value, lineup);
            if (savedId == null) return NotFound(new { error = "This lineup no longer exists." });

            return Json(new { id = savedId });
        }

        [HttpPost]
        public IActionResult DeleteLineup(int id, int? currentId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            _lineups.Delete(id, userId.Value);

            // Wurde eine andere Aufstellung geloescht, bleibt die aktuelle offen
            return RedirectToAction("Lineup", new { id = currentId == id ? null : currentId });
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
                return RedirectToAction("Login", new { returnUrl = Request.Path + Request.QueryString });
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
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
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
            return RedirectBack(returnUrl);
        }

        // Zurueck zur Seite, von der aus man auf Login geklickt hat (nur eigene Seiten, nie Login/Register)
        private IActionResult RedirectBack(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                && !returnUrl.StartsWith("/Home/Login", StringComparison.OrdinalIgnoreCase)
                && !returnUrl.StartsWith("/Home/Register", StringComparison.OrdinalIgnoreCase))
            {
                return LocalRedirect(returnUrl);
            }

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
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string password, string? confirmPassword, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
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

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View();
            }

            if (_users.UsernameExists(username))
            {
                ViewBag.ErrorMessage = "Username already exists.";
                return View();
            }

            _users.Create(username, PasswordHasher.Hash(password));

            // Direkt einloggen, damit man sich nach dem Registrieren nicht nochmal anmelden muss
            var created = _users.GetByUsername(username);
            if (created == null) return RedirectToAction("Login", new { returnUrl });

            HttpContext.Session.SetInt32("UserId", created.Id);
            HttpContext.Session.SetString("Username", created.Username);
            return RedirectBack(returnUrl);
        }
    }
}
