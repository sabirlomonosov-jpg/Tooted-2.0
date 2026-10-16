using System;
using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace Tooted
{
    internal static class PasswordHelper
    {
        private const int Iterations = 150000;

        public static void MakeHash(string password, out string salt, out string hash)
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create()) rng.GetBytes(saltBytes);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
                hash = Convert.ToBase64String(pbkdf2.GetBytes(32));
            salt = Convert.ToBase64String(saltBytes);
        }

        public static bool Verify(string password, string salt, string hash)
        {
            byte[] expected = Convert.FromBase64String(hash);
            byte[] actual;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), Iterations, HashAlgorithmName.SHA256))
                actual = pbkdf2.GetBytes(32);
            int result = expected.Length ^ actual.Length;
            for (int i = 0; i < Math.Min(expected.Length, actual.Length); i++)
                result |= expected[i] ^ actual[i];
            return result == 0;
        }
    }

    internal static class Users
    {
        public static bool AnyUsers()
        {
            using (SqlConnection c = Db.Open())
            using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Kasutajad", c))
                return (int)cmd.ExecuteScalar() > 0;
        }

        public static UserSession Login(string username, string password)
        {
            using (SqlConnection c = Db.Open())
            using (SqlCommand cmd = new SqlCommand(
                "SELECT Id, Kasutajanimi, Roll, ParooliSool, ParooliRasi FROM Kasutajad WHERE Kasutajanimi=@u", c))
            {
                cmd.Parameters.AddWithValue("@u", username);
                using (SqlDataReader rd = cmd.ExecuteReader())
                {
                    if (!rd.Read()) return null;
                    if (!PasswordHelper.Verify(password, rd.GetString(3), rd.GetString(4))) return null;
                    return new UserSession { Id = rd.GetInt32(0), Login = rd.GetString(1), Role = rd.GetString(2) };
                }
            }
        }

        // Omaniku saab registreerida ainult esimesel käivitamisel või olemasoleva omaniku kaudu.
        public static void Register(string username, string password, string role, UserSession admin)
        {
            if (role != "Omanik" && role != "Müüja") throw new Exception("Tundmatu roll.");
            string salt, hash;
            PasswordHelper.MakeHash(password, out salt, out hash);

            using (SqlConnection c = Db.Open())
            using (SqlTransaction tr = c.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    bool empty;
                    using (var countCmd = new SqlCommand("SELECT COUNT(*) FROM Kasutajad WITH (TABLOCKX, HOLDLOCK)", c, tr))
                        empty = (int)countCmd.ExecuteScalar() == 0;

                    if (empty && role != "Omanik") throw new Exception("Esimene kasutaja peab olema Omanik.");
                    if (!empty && (admin == null || !admin.IsOwner))
                        throw new Exception("Uusi kasutajaid saab lisada ainult Omanik.");

                    using (var cmd = new SqlCommand(
                        "INSERT INTO Kasutajad (Kasutajanimi, ParooliRasi, ParooliSool, Roll) VALUES (@u,@h,@s,@r)", c, tr))
                    {
                        cmd.Parameters.AddWithValue("@u", username);
                        cmd.Parameters.AddWithValue("@h", hash);
                        cmd.Parameters.AddWithValue("@s", salt);
                        cmd.Parameters.AddWithValue("@r", role);
                        cmd.ExecuteNonQuery();
                    }
                    tr.Commit();
                }
                catch
                {
                    tr.Rollback();
                    throw;
                }
            }
        }
    }
}
