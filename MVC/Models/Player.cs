namespace MVC.Models
{
    public class Player
    {
        public int ShirtNumber { get; set; }
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";
        public string Position { get; set; } = "";
        public DateTime BirthDate { get; set; }
        public string ImagePath { get; set; } = "";

        // Alter wird aus dem Geburtsdatum berechnet
        public int Age
        {
            get
            {
                int age = DateTime.Today.Year - BirthDate.Year;
                if (BirthDate.Date > DateTime.Today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}
