using System.Security.Cryptography;
using System.Text;

namespace MVC.Services
{
    // Speichert Passwoerter nie im Klartext: PBKDF2 mit zufaelligem Salt
    // Format in der DB: v1.<Durchgaenge>.<Salt base64>.<Hash base64>
    public static class PasswordHasher
    {
        private const string Prefix = "v1";
        private const int Iterations = 100_000;
        private const int SaltSize = 16;
        private const int HashSize = 32;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
            return $"{Prefix}.{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool IsHashed(string stored) => stored.StartsWith(Prefix + ".");

        public static bool Verify(string password, string stored)
        {
            if (!IsHashed(stored))
            {
                // Altes Passwort aus der Zeit vor dem Hashing: wird nach dem Login automatisch ersetzt
                return CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(password), Encoding.UTF8.GetBytes(stored));
            }

            string[] parts = stored.Split('.');
            if (parts.Length != 4 || !int.TryParse(parts[1], out int iterations) || iterations <= 0) return false;

            // Kaputter Eintrag in der DB soll den Login nicht mit einem Fehler 500 abbrechen
            try
            {
                byte[] salt = Convert.FromBase64String(parts[2]);
                byte[] expected = Convert.FromBase64String(parts[3]);
                if (expected.Length == 0) return false;

                byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
                return CryptographicOperations.FixedTimeEquals(actual, expected);
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
