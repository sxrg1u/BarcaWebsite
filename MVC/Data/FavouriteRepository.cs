using MySqlConnector;

namespace MVC.Data
{
    // Alle SQL-Zugriffe auf die Zwischentabelle Favourites
    public class FavouriteRepository
    {
        private readonly Database _db;

        public FavouriteRepository(Database db) { _db = db; }

        public HashSet<int> GetPlayerIds(int userId)
        {
            var ids = new HashSet<int>();
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("SELECT PlayerId FROM Favourites WHERE UserId = @u", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) ids.Add(reader.GetInt32(0));
            return ids;
        }

        // Schaltet den Favoriten um und gibt zurueck, ob der Spieler jetzt ein Favorit ist
        public bool Toggle(int userId, int playerId)
        {
            using var conn = _db.Open();

            // Erst loeschen, wenn nichts geloescht wurde, war er noch kein Favorit -> hinzufuegen
            using var del = new MySqlCommand("DELETE FROM Favourites WHERE UserId = @u AND PlayerId = @p", conn);
            del.Parameters.AddWithValue("@u", userId);
            del.Parameters.AddWithValue("@p", playerId);
            if (del.ExecuteNonQuery() > 0) return false;

            using var ins = new MySqlCommand("INSERT INTO Favourites (UserId, PlayerId) VALUES (@u, @p)", conn);
            ins.Parameters.AddWithValue("@u", userId);
            ins.Parameters.AddWithValue("@p", playerId);
            ins.ExecuteNonQuery();
            return true;
        }
    }
}
