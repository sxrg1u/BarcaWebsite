namespace MVC.Models
{
    public class User
    {
        public const string RoleUser = "User";
        public const string RoleAdmin = "Admin";

        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public string Role { get; set; } = RoleUser;
        public DateTime CreatedAt { get; set; }
        public int FavouriteCount { get; set; }

        public bool IsAdmin => Role == RoleAdmin;
    }
}
