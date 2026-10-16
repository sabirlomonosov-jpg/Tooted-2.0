using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace Tooted
{
    // Ühine ühendus andmebaasiga.
    internal static class Db
    {
        public static string DatabaseFile
        {
            get
            {
                // Eelistame kasutaja olemasolevat tooted.mdf faili.
                string original = @"C:\Users\opilane\source\repos\Tooted\Tooted\tooted.mdf";
                if (File.Exists(original)) return original;

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] names = { "tooted.mdf", "tooded_db.mdf" };
                foreach (string name in names)
                {
                    string[] candidates = {
                        Path.Combine(baseDir, name),
                        Path.GetFullPath(Path.Combine(baseDir, @"..\..\" + name)),
                        Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\" + name))
                    };
                    foreach (string candidate in candidates)
                        if (File.Exists(candidate)) return candidate;
                }
                throw new FileNotFoundException(
                    "Faili tooted.mdf ei leitud. Kontrolli asukohta failis Db.cs.");
            }
        }

        public static SqlConnection Open()
        {
            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = @"(LocalDB)\MSSQLLocalDB";
            builder.AttachDBFilename = DatabaseFile;
            builder.IntegratedSecurity = true;
            builder.ConnectTimeout = 15;
            SqlConnection c = new SqlConnection(builder.ConnectionString);
            c.Open();
            return c;
        }

        public static void Initialize()
        {
            Directory.CreateDirectory(ImageStore.Folder);
            Directory.CreateDirectory(Receipt.Folder);
            using (SqlConnection c = Open())
            {
                string[] queries = {
                    @"IF OBJECT_ID(N'dbo.Kategooriad', N'U') IS NULL
                      CREATE TABLE dbo.Kategooriad (
                        Id INT IDENTITY(1,1) PRIMARY KEY, Nimetus NVARCHAR(50) NOT NULL)",
                    @"IF OBJECT_ID(N'dbo.Tooted', N'U') IS NULL
                      CREATE TABLE dbo.Tooted (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Toodenimetus NVARCHAR(100) NOT NULL,
                        Kogus INT NOT NULL DEFAULT 0 CHECK(Kogus >= 0),
                        Hind DECIMAL(10,2) NOT NULL DEFAULT 0 CHECK(Hind >= 0),
                        Pilt NVARCHAR(255) NULL,
                        Kategooria_Id INT NULL REFERENCES dbo.Kategooriad(Id))",
                    @"IF OBJECT_ID(N'dbo.Kasutajad', N'U') IS NULL
                      CREATE TABLE dbo.Kasutajad (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Kasutajanimi NVARCHAR(50) NOT NULL UNIQUE,
                        ParooliRasi NVARCHAR(200) NOT NULL,
                        ParooliSool NVARCHAR(100) NOT NULL,
                        Roll NVARCHAR(10) NOT NULL CHECK(Roll IN (N'Omanik',N'Müüja')))",
                    @"IF OBJECT_ID(N'dbo.Kliendid', N'U') IS NULL
                      CREATE TABLE dbo.Kliendid (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Nimi NVARCHAR(100) NOT NULL,
                        Telefon NVARCHAR(30) NULL,
                        Epost NVARCHAR(100) NULL,
                        Kliendikaart NVARCHAR(50) NOT NULL UNIQUE,
                        Boonus INT NOT NULL DEFAULT 0 CHECK (Boonus >= 0))",
                    @"IF OBJECT_ID(N'dbo.Muugid', N'U') IS NULL
                      CREATE TABLE dbo.Muugid (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        Kuupaev DATETIME2 NOT NULL DEFAULT SYSDATETIME(),
                        KasutajaId INT NOT NULL REFERENCES dbo.Kasutajad(Id),
                        KlientId INT NULL REFERENCES dbo.Kliendid(Id),
                        Summa DECIMAL(10,2) NOT NULL CHECK(Summa >= 0))",
                    @"IF OBJECT_ID(N'dbo.Muugiread', N'U') IS NULL
                      CREATE TABLE dbo.Muugiread (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        MuukId INT NOT NULL REFERENCES dbo.Muugid(Id),
                        TooteId INT NOT NULL,
                        Tootenimetus NVARCHAR(100) NOT NULL,
                        Kogus INT NOT NULL CHECK(Kogus > 0),
                        Hind DECIMAL(10,2) NOT NULL CHECK(Hind >= 0))"
                };
                foreach (string q in queries)
                {
                    using (SqlCommand cmd = new SqlCommand(q, c))
                        cmd.ExecuteNonQuery();
                }
            }
        }

        public static DataTable Query(string sql)
        {
            using (SqlConnection c = Open())
            using (SqlDataAdapter da = new SqlDataAdapter(sql, c))
            {
                DataTable t = new DataTable();
                da.Fill(t);
                return t;
            }
        }
    }

    internal sealed class UserSession
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Role { get; set; }
        public bool IsOwner { get { return Role == "Omanik"; } }
    }
}
