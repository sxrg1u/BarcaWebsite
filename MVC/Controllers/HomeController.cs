using Microsoft.AspNetCore.Mvc;
using MVC.Models;
using System.Collections.Generic;
using System.Diagnostics;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
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
            var players = new List<Player>
            {
                new() { Number = 1,  Name = "Marc-André ter Stegen", Country = "Germany",     Position = "GK",  Age = 33 },
                new() { Number = 25, Name = "Wojciech Szczęsny",     Country = "Poland",       Position = "GK",  Age = 35 },
                new() { Number = 2,  Name = "Pau Cubarsí",           Country = "Spain",        Position = "CB",  Age = 18 },
                new() { Number = 4,  Name = "Ronald Araújo",         Country = "Uruguay",      Position = "CB",  Age = 26 },
                new() { Number = 15, Name = "Andreas Christensen",   Country = "Denmark",      Position = "CB",  Age = 29 },
                new() { Number = 24, Name = "Eric García",           Country = "Spain",        Position = "CB",  Age = 24 },
                new() { Number = 23, Name = "Jules Koundé",          Country = "France",       Position = "RB",  Age = 27 },
                new() { Number = 3,  Name = "Alejandro Balde",       Country = "Spain",        Position = "LB",  Age = 22 },
                new() { Number = 28, Name = "Gerard Martín",         Country = "Spain",        Position = "LB",  Age = 22 },
                new() { Number = 8,  Name = "Pedri",                 Country = "Spain",        Position = "CM",  Age = 22 },
                new() { Number = 6,  Name = "Gavi",                  Country = "Spain",        Position = "CM",  Age = 21 },
                new() { Number = 21, Name = "Frenkie de Jong",       Country = "Netherlands",  Position = "CM",  Age = 28 },
                new() { Number = 17, Name = "Marc Casadó",           Country = "Spain",        Position = "CDM", Age = 21 },
                new() { Number = 16, Name = "Fermín López",          Country = "Spain",        Position = "CM",  Age = 22 },
                new() { Number = 20, Name = "Dani Olmo",             Country = "Spain",        Position = "AM",  Age = 27 },
                new() { Number = 22, Name = "Marc Bernal",           Country = "Spain",        Position = "CM",  Age = 18 },
                new() { Number = 10, Name = "Lamine Yamal",          Country = "Spain",        Position = "RW",  Age = 18 },
                new() { Number = 11, Name = "Raphinha",              Country = "Brazil",       Position = "LW",  Age = 28 },
                new() { Number = 7,  Name = "Ferran Torres",         Country = "Spain",        Position = "RW",  Age = 25 },
                new() { Number = 9,  Name = "Robert Lewandowski",    Country = "Poland",       Position = "ST",  Age = 37 },
                new() { Number = 19, Name = "Pau Víctor",            Country = "Spain",        Position = "ST",  Age = 24 },
            };

            return View(players);
        }

        public IActionResult Trophies()
        {
            return View();
        }

        public IActionResult Favourites()
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
