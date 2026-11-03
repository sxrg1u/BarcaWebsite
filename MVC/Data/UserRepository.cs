using MVC.Models;
using MySqlConnector;

namespace MVC.Data
{
    // Alle SQL-Zugriffe auf die Tabelle Users (parametrisierte Abfragen)
    public class UserRepository
    {
        private readonly Database _db;

        public UserRepository(Database db) { _db = db; }

        public User? GetByUsername(string username)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                "SELECT Id, Username, Password, Role, CreatedAt FROM Users WHERE Username = @u", conn);
            cmd.Parameters.AddWithValue("@u", username);
            return ReadUsers(cmd).FirstOrDefault();
        }

        public User? GetById(int id)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                "SELECT Id, Username, Password, Role, CreatedAt FROM Users WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return ReadUsers(cmd).FirstOrDefault();
        }

        // Alle Benutzer inkl. Anzahl Favoriten (fuer die Benutzerverwaltung)
        public List<User> GetAll()
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                @"SELECT u.Id, u.Username, u.Password, u.Role, u.CreatedAt, COUNT(f.PlayerId)
                  FROM Users u
                  LEFT JOIN Favourites f ON f.UserId = u.Id
                  GROUP BY u.Id, u.Username, u.Password, u.Role, u.CreatedAt
                  ORDER BY u.Id", conn);
            return ReadUsers(cmd, withFavouriteCount: true);
        }

        public string? GetRole(int id)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("SELECT Role FROM Users WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteScalar() as string;
        }

        public bool UsernameExists(string username)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Username = @u", conn);
            cmd.Parameters.AddWithValue("@u", username);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public int CountAdmins()
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Users WHERE Role = 'Admin'", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Create(string username, string passwordHash)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("INSERT INTO Users (Username, Password) VALUES (@u, @p)", conn);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", passwordHash);
            cmd.ExecuteNonQuery();
        }

        public void UpdatePassword(int id, string passwordHash)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("UPDATE Users SET Password = @p WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@p", passwordHash);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public void UpdateRole(int id, string role)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("UPDATE Users SET Role = @r WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@r", role);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // Favoriten des Benutzers verschwinden automatisch (ON DELETE CASCADE)
        public void Delete(int id)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("DELETE FROM Users WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        private static List<User> ReadUsers(MySqlCommand cmd, bool withFavouriteCount = false)
        {
            var users = new List<User>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                users.Add(new User
                {
                    Id = reader.GetInt32(0),
                    Username = reader.GetString(1),
                    PasswordHash = reader.GetString(2),
                    Role = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    FavouriteCount = withFavouriteCount ? reader.GetInt32(5) : 0
                });
            }
            return users;
        }
    }
}
