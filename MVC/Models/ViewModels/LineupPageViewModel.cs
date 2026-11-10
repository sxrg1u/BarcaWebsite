namespace MVC.Models.ViewModels
{
    // Daten fuer die Lineup-Seite
    public class LineupPageViewModel
    {
        public List<Player> Players { get; set; } = new();

        // Alle gespeicherten Aufstellungen des Users (leer fuer Gaeste)
        public List<Lineup> SavedLineups { get; set; } = new();

        // Die gerade geladene Aufstellung (null = neue)
        public Lineup? Current { get; set; }
    }
}
