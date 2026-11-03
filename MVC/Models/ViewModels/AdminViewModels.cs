using System.ComponentModel.DataAnnotations;

namespace MVC.Models.ViewModels
{
    // Formular zum Erstellen und Bearbeiten eines Spielers (Admin)
    public class PlayerFormViewModel
    {
        // Positionen wie im ENUM der Datenbank, mit lesbarem Namen fuers Dropdown
        public static readonly (string Code, string Label)[] Positions =
        {
            ("GK", "Goalkeeper"), ("LB", "Left back"), ("CB", "Centre back"), ("RB", "Right back"),
            ("DM", "Defensive midfielder"), ("CM", "Central midfielder"), ("AM", "Attacking midfielder"),
            ("LW", "Left winger"), ("RW", "Right winger"), ("ST", "Striker")
        };

        public int? Id { get; set; }

        [Range(1, 99, ErrorMessage = "Shirt number must be between 1 and 99.")]
        public int? ShirtNumber { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name can have at most 100 characters.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(50, ErrorMessage = "Country can have at most 50 characters.")]
        public string Country { get; set; } = "";

        [Required(ErrorMessage = "Please choose a position.")]
        public string Position { get; set; } = "";

        [Required(ErrorMessage = "Birth date is required.")]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; }

        // Bisheriges Foto (beim Bearbeiten), wird nur ersetzt wenn ein neues hochgeladen wird
        public string? ImagePath { get; set; }

        public IFormFile? ImageFile { get; set; }
    }

    // Formular zum Erstellen und Bearbeiten eines Titels (Admin)
    public class TrophyFormViewModel
    {
        public int? Id { get; set; }

        [Required(ErrorMessage = "Year is required.")]
        [Range(1899, 2100, ErrorMessage = "Year must be between 1899 and 2100.")]
        public int? Year { get; set; }

        [Required(ErrorMessage = "Competition is required.")]
        [StringLength(100, ErrorMessage = "Competition can have at most 100 characters.")]
        public string Competition { get; set; } = "";

        [Required(ErrorMessage = "Coach is required.")]
        [StringLength(100, ErrorMessage = "Coach can have at most 100 characters.")]
        public string Coach { get; set; } = "";

        [Required(ErrorMessage = "Please choose a category.")]
        public CategoryEnum? Category { get; set; }
    }

    // Neues Passwort fuer einen Benutzer setzen (Admin)
    public class ResetPasswordViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 4, ErrorMessage = "Password must be 4 to 100 characters.")]
        public string NewPassword { get; set; } = "";

        [Compare(nameof(NewPassword), ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}
