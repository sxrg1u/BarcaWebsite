using System.Collections.Generic;

namespace MVC.Models
{
    public class TrophyStat
    {
        public int Count { get; set; }
        public string Name { get; set; } = "";
        public string LastWon { get; set; } = "";
    }

    public class TrophyRecord
    {
        public int Year { get; set; }
        public string Competition { get; set; } = "";
        public string Coach { get; set; } = "";
        public string Category { get; set; } = "";
    }

    public class TrophiesViewModel
    {
        public List<TrophyStat> Stats { get; set; } = new();
        public List<TrophyRecord> Records { get; set; } = new();
    }
}
