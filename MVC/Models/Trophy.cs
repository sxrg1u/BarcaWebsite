namespace MVC.Models
{
    public class Trophy
    {
        public int Year { get; set; }
        public string Competition { get; set; } = "";
        public string Coach { get; set; } = "";
        public CategoryEnum Category { get; set; }
    }

    public enum CategoryEnum
    {
        National, International
    }
}