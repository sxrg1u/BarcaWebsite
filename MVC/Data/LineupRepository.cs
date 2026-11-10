using MVC.Models;
using MySqlConnector;

namespace MVC.Data
{
    // Alle SQL-Zugriffe auf die Tabellen Lineups und LineupPlayers.
    // Jede Abfrage prueft die UserId, damit niemand fremde Aufstellungen sieht oder aendert.
    public class LineupRepository
    {
        private readonly Database _db;

        public LineupRepository(Database db) { _db = db; }

        // Alle Aufstellungen eines Benutzers, zuletzt geaenderte zuerst (ohne Slots)
        public List<Lineup> GetAll(int userId)
        {
            var list = new List<Lineup>();
            using var conn = _db.Open();
            using var cmd = new MySqlCommand(
                @"SELECT l.Id, l.Name, l.Formation, l.CaptainId, l.UpdatedAt,
                         (SELECT COUNT(*) FROM LineupPlayers lp WHERE lp.LineupId = l.Id AND lp.Slot < @starters)
                  FROM Lineups l
                  WHERE l.UserId = @u
                  ORDER BY l.UpdatedAt DESC, l.Id DESC", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@starters", Lineup.StarterCount);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Lineup
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Formation = reader.GetString(2),
                    CaptainId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    UpdatedAt = reader.GetDateTime(4),
                    StarterPlayers = reader.GetInt32(5)
                });
            }
            return list;
        }

        public int Count(int userId)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Lineups WHERE UserId = @u", conn);
            cmd.Parameters.AddWithValue("@u", userId);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Eine Aufstellung mit allen Slots, nur wenn sie dem Benutzer gehoert
        public Lineup? Get(int id, int userId)
        {
            using var conn = _db.Open();

            Lineup lineup;
            using (var cmd = new MySqlCommand(
                "SELECT Id, Name, Formation, CaptainId, UpdatedAt FROM Lineups WHERE Id = @id AND UserId = @u", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@u", userId);
                using var reader = cmd.ExecuteReader();
                if (!reader.Read()) return null;

                lineup = new Lineup
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Formation = reader.GetString(2),
                    CaptainId = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    UpdatedAt = reader.GetDateTime(4)
                };
            }

            using (var cmd = new MySqlCommand("SELECT Slot, PlayerId FROM LineupPlayers WHERE LineupId = @id", conn))
            {
                cmd.Parameters.AddWithValue("@id", id);
                using var reader = cmd.ExecuteReader();
                while (reader.Read()) lineup.Slots[reader.GetInt32(0)] = reader.GetInt32(1);
            }

            return lineup;
        }

        // Neu anlegen (Id = 0) oder bestehende ersetzen. Gibt die Id zurueck, oder null wenn sie nicht dem User gehoert.
        public int? Save(int userId, Lineup lineup)
        {
            using var conn = _db.Open();
            using var tx = conn.BeginTransaction();
            int id;

            if (lineup.Id == 0)
            {
                using var cmd = new MySqlCommand(
                    @"INSERT INTO Lineups (UserId, Name, Formation, CaptainId) VALUES (@u, @n, @f, @c);
                      SELECT LAST_INSERT_ID();", conn, tx);
                AddFields(cmd, userId, lineup);
                id = Convert.ToInt32(cmd.ExecuteScalar());
            }
            else
            {
                using var cmd = new MySqlCommand(
                    @"UPDATE Lineups SET Name = @n, Formation = @f, CaptainId = @c, UpdatedAt = CURRENT_TIMESTAMP
                      WHERE Id = @id AND UserId = @u", conn, tx);
                AddFields(cmd, userId, lineup);
                cmd.Parameters.AddWithValue("@id", lineup.Id);
                if (cmd.ExecuteNonQuery() == 0) return null; // gehoert jemand anderem oder wurde geloescht
                id = lineup.Id;

                using var del = new MySqlCommand("DELETE FROM LineupPlayers WHERE LineupId = @id", conn, tx);
                del.Parameters.AddWithValue("@id", id);
                del.ExecuteNonQuery();
            }

            foreach (var (slot, playerId) in lineup.Slots)
            {
                using var cmd = new MySqlCommand(
                    "INSERT INTO LineupPlayers (LineupId, Slot, PlayerId) VALUES (@id, @s, @p)", conn, tx);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@s", slot);
                cmd.Parameters.AddWithValue("@p", playerId);
                cmd.ExecuteNonQuery();
            }

            tx.Commit();
            return id;
        }

        public void Delete(int id, int userId)
        {
            using var conn = _db.Open();
            using var cmd = new MySqlCommand("DELETE FROM Lineups WHERE Id = @id AND UserId = @u", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.ExecuteNonQuery();
        }

        private static void AddFields(MySqlCommand cmd, int userId, Lineup lineup)
        {
            cmd.Parameters.AddWithValue("@u", userId);
            cmd.Parameters.AddWithValue("@n", lineup.Name);
            cmd.Parameters.AddWithValue("@f", lineup.Formation);
            cmd.Parameters.AddWithValue("@c", (object?)lineup.CaptainId ?? DBNull.Value);
        }
    }
}
