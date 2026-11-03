using MySqlConnector;

namespace MVC.Data
{
    // Gemeinsamer Einstiegspunkt fuer alle ADO.NET-Zugriffe: liefert eine offene Verbindung
    public class Database
    {
        private readonly string _connString;

        public Database(IConfiguration config)
        {
            _connString = config.GetConnectionString("BarcaData")!;
        }

        public MySqlConnection Open()
        {
            var conn = new MySqlConnection(_connString);
            conn.Open();
            return conn;
        }
    }
}
