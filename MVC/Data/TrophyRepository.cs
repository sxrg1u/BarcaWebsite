using MVC.Models;
using MySqlConnector;

namespace MVC.Data
{
    // Alle SQL-Zugriffe auf die Tabelle Trophies (parametrisierte Abfragen)
    public class TrophyRepository
    {
        private readonly Database _db;

        public TrophyRepository(Database db) { _db = db; }

        public List<Trophy> GetAll()
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                "SELECT Id, Year, Competition, Coach, Category FROM Trophies ORDER BY Year DESC, Id DESC", conn);
            return ReadTrophies(cmd);
        }

        public Trophy? GetById(int id)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                "SELECT Id, Year, Competition, Coach, Category FROM Trophies WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            return ReadTrophies(cmd).FirstOrDefault();
        }

        public void Create(Trophy t)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                "INSERT INTO Trophies (Year, Competition, Coach, Category) VALUES (@year, @comp, @coach, @cat)", conn);
            AddParameters(cmd, t);
            cmd.ExecuteNonQuery();
        }

        public void Update(Trophy t)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                "UPDATE Trophies SET Year = @year, Competition = @comp, Coach = @coach, Category = @cat WHERE Id = @id", conn);
            AddParameters(cmd, t);
            cmd.Parameters.AddWithValue("@id", t.Id);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("DELETE FROM Trophies WHERE Id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // Bereits verwendete Wettbewerbsnamen (Vorschlaege im Formular, gleiche Schreibweise wie die Pokal-Deko)
        public List<string> GetCompetitions()
        {
            var names = new List<string>();
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("SELECT DISTINCT Competition FROM Trophies ORDER BY Competition", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) names.Add(reader.GetString(0));
            return names;
        }

        private static void AddParameters(MySqlCommand cmd, Trophy t)
        {
            cmd.Parameters.AddWithValue("@year", t.Year);
            cmd.Parameters.AddWithValue("@comp", t.Competition);
            cmd.Parameters.AddWithValue("@coach", t.Coach);
            cmd.Parameters.AddWithValue("@cat", t.Category.ToString());
        }

        private static List<Trophy> ReadTrophies(MySqlCommand cmd)
        {
            var trophies = new List<Trophy>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                trophies.Add(new Trophy
                {
                    Id = reader.GetInt32(0),
                    Year = reader.GetInt32(1),
                    Competition = reader.GetString(2),
                    Coach = reader.GetString(3),
                    Category = Enum.Parse<CategoryEnum>(reader.GetString(4))
                });
            }
            return trophies;
        }
    }
}
