using Microsoft.AspNetCore.Mvc;
using MVC.Data;
using MVC.Filters;
using MVC.Models;
using MVC.Models.ViewModels;
using MVC.Services;

namespace MVC.Controllers
{
    // Admin-Bereich: Spieler, Titel und Benutzer verwalten. Nur fuer Benutzer mit der Rolle Admin.
    [AdminOnly]
    public class AdminController : Controller
    {
        private static readonly string[] AllowedImageTypes = { ".png", ".jpg", ".jpeg", ".webp" };
        private const long MaxImageBytes = 2 * 1024 * 1024;

        private readonly UserRepository _users;
        private readonly PlayerRepository _players;
        private readonly TrophyRepository _trophies;
        private readonly IWebHostEnvironment _env;

        public AdminController(UserRepository users, PlayerRepository players,
                               TrophyRepository trophies, IWebHostEnvironment env)
        {
            _users = users;
            _players = players;
            _trophies = trophies;
            _env = env;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Players");
        }

        // ---------- Spieler ----------

        public IActionResult Players()
        {
            return View(_players.GetAll());
        }

        [HttpGet]
        public IActionResult PlayerCreate()
        {
            return View("PlayerForm", new PlayerFormViewModel());
        }

        [HttpGet]
        public IActionResult PlayerEdit(int id)
        {
            var p = _players.GetById(id);
            if (p == null) return NotFound();

            return View("PlayerForm", new PlayerFormViewModel
            {
                Id = p.Id,
                ShirtNumber = p.ShirtNumber,
                Name = p.Name,
                Country = p.Country,
                Position = p.Position,
                BirthDate = p.BirthDate,
                ImagePath = p.ImagePath
            });
        }

        [HttpPost]
        public IActionResult PlayerSave(PlayerFormViewModel model)
        {
            // Beim Bearbeiten muss der Spieler existieren
            if (model.Id != null && _players.GetById(model.Id.Value) == null) return NotFound();

            if (model.BirthDate != null && model.BirthDate.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.BirthDate), "Birth date cannot be in the future.");
            }

            if (model.Position != "" && !PlayerFormViewModel.Positions.Any(x => x.Code == model.Position))
            {
                ModelState.AddModelError(nameof(model.Position), "Please choose a position.");
            }

            if (model.ShirtNumber != null)
            {
                string? owner = _players.GetNameByShirtNumber(model.ShirtNumber.Value, model.Id);
                if (owner != null)
                {
                    ModelState.AddModelError(nameof(model.ShirtNumber), $"Number {model.ShirtNumber} is already taken by {owner}.");
                }
            }

            string? imageError = ValidateImage(model.ImageFile);
            if (imageError != null)
            {
                ModelState.AddModelError(nameof(model.ImageFile), imageError);
            }

            if (!ModelState.IsValid)
            {
                return View("PlayerForm", model);
            }

            var player = new Player
            {
                Id = model.Id ?? 0,
                ShirtNumber = model.ShirtNumber,
                Name = model.Name.Trim(),
                Country = model.Country.Trim(),
                Position = model.Position,
                BirthDate = model.BirthDate!.Value,
                ImagePath = model.ImagePath
            };

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                player.ImagePath = SaveImage(model.ImageFile, player.Name);
            }

            if (model.Id == null)
            {
                _players.Create(player);
                TempData["Message"] = $"{player.Name} was added to the squad.";
            }
            else
            {
                _players.Update(player);
                TempData["Message"] = $"{player.Name} was updated.";
            }

            return RedirectToAction("Players");
        }

        [HttpPost]
        public IActionResult PlayerDelete(int id)
        {
            var p = _players.GetById(id);
            if (p == null) return NotFound();

            _players.Delete(id);
            TempData["Message"] = $"{p.Name} was removed from the squad.";
            return RedirectToAction("Players");
        }

        // ---------- Titel ----------

        public IActionResult Trophies()
        {
            return View(_trophies.GetAll());
        }

        [HttpGet]
        public IActionResult TrophyCreate()
        {
            ViewBag.Competitions = _trophies.GetCompetitions();
            return View("TrophyForm", new TrophyFormViewModel { Year = DateTime.Today.Year });
        }

        [HttpGet]
        public IActionResult TrophyEdit(int id)
        {
            var t = _trophies.GetById(id);
            if (t == null) return NotFound();

            ViewBag.Competitions = _trophies.GetCompetitions();
            return View("TrophyForm", new TrophyFormViewModel
            {
                Id = t.Id,
                Year = t.Year,
                Competition = t.Competition,
                Coach = t.Coach,
                Category = t.Category
            });
        }

        [HttpPost]
        public IActionResult TrophySave(TrophyFormViewModel model)
        {
            if (model.Id != null && _trophies.GetById(model.Id.Value) == null) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Competitions = _trophies.GetCompetitions();
                return View("TrophyForm", model);
            }

            var trophy = new Trophy
            {
                Id = model.Id ?? 0,
                Year = model.Year!.Value,
                Competition = model.Competition.Trim(),
                Coach = model.Coach.Trim(),
                Category = model.Category!.Value
            };

            if (model.Id == null)
            {
                _trophies.Create(trophy);
                TempData["Message"] = $"{trophy.Competition} {trophy.Year} was added.";
            }
            else
            {
                _trophies.Update(trophy);
                TempData["Message"] = $"{trophy.Competition} {trophy.Year} was updated.";
            }

            return RedirectToAction("Trophies");
        }

        [HttpPost]
        public IActionResult TrophyDelete(int id)
        {
            var t = _trophies.GetById(id);
            if (t == null) return NotFound();

            _trophies.Delete(id);
            TempData["Message"] = $"{t.Competition} {t.Year} was deleted.";
            return RedirectToAction("Trophies");
        }

        // ---------- Benutzer ----------

        public IActionResult Users()
        {
            return View(_users.GetAll());
        }

        [HttpPost]
        public IActionResult UserToggleRole(int id)
        {
            var user = _users.GetById(id);
            if (user == null) return NotFound();

            // Sich selbst darf man nicht herabstufen, sonst gibt es evtl. keinen Admin mehr
            if (id == HttpContext.Session.GetInt32("UserId"))
            {
                TempData["Error"] = "You cannot change your own role.";
                return RedirectToAction("Users");
            }

            string newRole = user.IsAdmin ? MVC.Models.User.RoleUser : MVC.Models.User.RoleAdmin;
            _users.UpdateRole(id, newRole);
            TempData["Message"] = $"{user.Username} is now {newRole}.";
            return RedirectToAction("Users");
        }

        [HttpGet]
        public IActionResult UserResetPassword(int id)
        {
            var user = _users.GetById(id);
            if (user == null) return NotFound();

            return View("ResetPassword", new ResetPasswordViewModel { Id = user.Id, Username = user.Username });
        }

        [HttpPost]
        public IActionResult UserResetPassword(ResetPasswordViewModel model)
        {
            var user = _users.GetById(model.Id);
            if (user == null) return NotFound();

            model.Username = user.Username;
            if (!ModelState.IsValid)
            {
                return View("ResetPassword", model);
            }

            _users.UpdatePassword(user.Id, PasswordHasher.Hash(model.NewPassword));
            TempData["Message"] = $"Password of {user.Username} was changed.";
            return RedirectToAction("Users");
        }

        [HttpPost]
        public IActionResult UserDelete(int id)
        {
            var user = _users.GetById(id);
            if (user == null) return NotFound();

            if (id == HttpContext.Session.GetInt32("UserId"))
            {
                TempData["Error"] = "You cannot delete your own account.";
                return RedirectToAction("Users");
            }

            if (user.IsAdmin && _users.CountAdmins() <= 1)
            {
                TempData["Error"] = "The last admin cannot be deleted.";
                return RedirectToAction("Users");
            }

            _users.Delete(id);
            TempData["Message"] = $"{user.Username} was deleted.";
            return RedirectToAction("Users");
        }

        // ---------- Hilfsmethoden ----------

        // Prueft Dateityp und Groesse des hochgeladenen Fotos (null = alles in Ordnung)
        private static string? ValidateImage(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageTypes.Contains(ext)) return "Only PNG, JPG or WEBP images are allowed.";
            if (file.Length > MaxImageBytes) return "The image must be smaller than 2 MB.";
            return null;
        }

        // Speichert das Foto unter wwwroot und gibt den Web-Pfad fuer die Datenbank zurueck
        private string SaveImage(IFormFile file, string playerName)
        {
            string ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            string slug = new string(playerName.Where(char.IsLetterOrDigit).ToArray());
            string fileName = $"{(slug.Length > 0 ? slug : "player")}-{Guid.NewGuid():N}{ext}";

            string folder = Path.Combine(_env.WebRootPath, "images", "Players", "ProfilPhotos");
            Directory.CreateDirectory(folder);

            using var stream = System.IO.File.Create(Path.Combine(folder, fileName));
            file.CopyTo(stream);

            return "/images/Players/ProfilPhotos/" + fileName;
        }
    }
}
