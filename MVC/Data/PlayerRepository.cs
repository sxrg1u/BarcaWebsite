using MVC.Models;
using MySqlConnector;

namespace MVC.Data
{
    // Alle SQL-Zugriffe auf die Tabelle Players (parametrisierte Abfragen)
    public class PlayerRepository
    {
        private const string Columns = "Id, ShirtNumber, Name, Country, Position, BirthDate, ImagePath";

        private readonly Database _db;

        public PlayerRepository(Database db) { _db = db; }

        public List<Player> GetAll()
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                $"SELECT {Columns} FROM Players ORDER BY ShirtNumber IS NULL, ShirtNumber", conn);
            return ReadPlayers(cmd);
        }

        // Alle Spieler, die der Benutzer als Favorit markiert hat (in der Reihenfolge des Hinzufuegens)
        public List<Player> GetFavourites(int userId)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                @"SELECT p.Id, p.ShirtNumber, p.Name, p.Country, p.Position, p.BirthDate, p.ImagePath
                  FROM Favourites f
                  JOIN Players p ON p.Id = f.PlayerId
                  WHERE f.UserId = @u
                  ORDER BY f.AddedAt", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            var players = ReadPlayers(cmd);
            players.ForEach(p => p.IsFavourite = true);
            return players;
        }

        public Player? GetById(int id)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand($"SELECT {Columns} FROM Players WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return ReadPlayers(cmd).FirstOrDefault();
        }

        public void Create(Player p)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                @"INSERT INTO Players (ShirtNumber, Name, Country, Position, BirthDate, ImagePath)
                  VALUES (@no, @name, @country, @pos, @birth, @img)", conn);
            AddParameters(cmd, p);
            cmd.ExecuteNonQuery();
        }

        public void Update(Player p)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                @"UPDATE Players
                  SET ShirtNumber = @no, Name = @name, Country = @country, Position = @pos,
                      BirthDate = @birth, ImagePath = @img
                  WHERE Id = @id", conn);
            AddParameters(cmd, p);
            cmd.Parameters.AddWithValue("@id", p.Id);
            cmd.ExecuteNonQuery();
        }

        // Favoriten dieses Spielers verschwinden automatisch (ON DELETE CASCADE)
        public void Delete(int id)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("DELETE FROM Players WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // Name des Spielers, der die Nummer schon traegt (oder null, wenn sie frei ist)
        public string? GetNameByShirtNumber(int shirtNumber, int? exceptId)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                "SELECT Name FROM Players WHERE ShirtNumber = @no AND Id <> @id LIMIT 1", conn);
            cmd.Parameters.AddWithValue("@no", shirtNumber);
            cmd.Parameters.AddWithValue("@id", exceptId ?? 0);
            return cmd.ExecuteScalar() as string;
        }

        private static void AddParameters(MySqlCommand cmd, Player p)
        {
            cmd.Parameters.AddWithValue("@no", (object?)p.ShirtNumber ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@name", p.Name);
            cmd.Parameters.AddWithValue("@country", p.Country);
            cmd.Parameters.AddWithValue("@pos", p.Position);
            cmd.Parameters.AddWithValue("@birth", p.BirthDate);
            cmd.Parameters.AddWithValue("@img", (object?)p.ImagePath ?? DBNull.Value);
        }

        private static List<Player> ReadPlayers(MySqlCommand cmd)
        {
            var players = new List<Player>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                players.Add(new Player
                {
                    Id = reader.GetInt32(0),
                    ShirtNumber = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                    Name = reader.GetString(2),
                    Country = reader.GetString(3),
                    Position = reader.GetString(4),
                    BirthDate = reader.GetDateTime(5),
                    ImagePath = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
            return players;
        }
    }
}
