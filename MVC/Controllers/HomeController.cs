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
            var model = new TrophiesViewModel
            {
                Stats = new List<TrophyStat>
                {
                    new() { Count = 28, Name = "LaLiga",           LastWon = "last won 2024/25" },
                    new() { Count = 32, Name = "Copa del Rey",     LastWon = "last won 2025" },
                    new() { Count = 5,  Name = "Champions League", LastWon = "last won 2015" },
                    new() { Count = 13, Name = "Other Trophies",   LastWon = "super cups, UEFA" },
                },
                Records = new List<TrophyRecord>
                {
                    new() { Year = 2025, Competition = "LaLiga",               Coach = "Hansi Flick",   Category = "Domestic" },
                    new() { Year = 2025, Competition = "Copa del Rey",         Coach = "Hansi Flick",   Category = "Domestic" },
                    new() { Year = 2025, Competition = "Supercopa de España",  Coach = "Hansi Flick",   Category = "Domestic" },
                    new() { Year = 2023, Competition = "LaLiga",               Coach = "Xavi",          Category = "Domestic" },
                    new() { Year = 2023, Competition = "Supercopa de España",  Coach = "Xavi",          Category = "Domestic" },
                    new() { Year = 2019, Competition = "LaLiga",               Coach = "Ernesto Valverde", Category = "Domestic" },
                    new() { Year = 2018, Competition = "LaLiga",               Coach = "Ernesto Valverde", Category = "Domestic" },
                    new() { Year = 2018, Competition = "Copa del Rey",         Coach = "Ernesto Valverde", Category = "Domestic" },
                    new() { Year = 2017, Competition = "Copa del Rey",         Coach = "Luis Enrique",  Category = "Domestic" },
                    new() { Year = 2016, Competition = "Copa del Rey",         Coach = "Luis Enrique",  Category = "Domestic" },
                    new() { Year = 2016, Competition = "Supercopa de España",  Coach = "Luis Enrique",  Category = "Domestic" },
                    new() { Year = 2015, Competition = "LaLiga",               Coach = "Luis Enrique",  Category = "Domestic" },
                    new() { Year = 2015, Competition = "Copa del Rey",         Coach = "Luis Enrique",  Category = "Domestic" },
                    new() { Year = 2015, Competition = "UEFA Champions League", Coach = "Luis Enrique", Category = "Europe" },
                    new() { Year = 2015, Competition = "UEFA Super Cup",       Coach = "Luis Enrique",  Category = "Europe" },
                    new() { Year = 2015, Competition = "FIFA Club World Cup",  Coach = "Luis Enrique",  Category = "World" },
                    new() { Year = 2013, Competition = "LaLiga",               Coach = "Tito Vilanova", Category = "Domestic" },
                    new() { Year = 2012, Competition = "Copa del Rey",         Coach = "Pep Guardiola", Category = "Domestic" },
                    new() { Year = 2011, Competition = "LaLiga",               Coach = "Pep Guardiola", Category = "Domestic" },
                    new() { Year = 2011, Competition = "UEFA Champions League", Coach = "Pep Guardiola", Category = "Europe" },
                    new() { Year = 2011, Competition = "Supercopa de España",  Coach = "Pep Guardiola", Category = "Domestic" },
                    new() { Year = 2010, Competition = "LaLiga",               Coach = "Pep Guardiola", Category = "Domestic" },
                    new() { Year = 2010, Competition = "Supercopa de España",  Coach = "Pep Guardiola", Category = "Domestic" },
                    new() { Year = 2010, Competition = "UEFA Super Cup",       Coach = "Pep Guardiola", Category = "Europe" },
                    new() { Year = 2010, Competition = "FIFA Club World Cup",  Coach = "Pep Guardiola", Category = "World" },
                },
            };

            return View(model);
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
