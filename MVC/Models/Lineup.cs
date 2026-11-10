namespace MVC.Models
{
    // Eine gespeicherte Aufstellung (ein Benutzer kann mehrere haben)
    public class Lineup
    {
        // Slot 0-10 = Startelf, 11-19 = Ersatzbank
        public const int StarterCount = 11;
        public const int BenchCount = 9;
        public const int SlotCount = StarterCount + BenchCount;

        public const int NameMaxLength = 40;
        public const int MaxPerUser = 30;

        // Systeme, die man auswaehlen kann (Koordinaten stehen in lineup.js)
        public static readonly string[] Formations =
        {
            "4-3-3", "4-2-3-1", "4-4-2", "4-1-2-1-2", "3-5-2", "3-4-3", "5-3-2"
        };

        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Formation { get; set; } = "4-3-3";
        public int? CaptainId { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Fuer die Liste: wie viele Spieler in der Startelf stehen
        public int StarterPlayers { get; set; }

        // Slot -> PlayerId
        public Dictionary<int, int> Slots { get; set; } = new();
    }
}
