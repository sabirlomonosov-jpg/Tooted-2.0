using System;
using System.Drawing;
using System.IO;

namespace Tooted
{
    internal static class ImageStore
    {
        public static string Folder { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pictures"); } }

        // Salvestame andmebaasi ainult pildifaili nime, mitte arvuti absoluutset teed.
        public static string Save(string source)
        {
            Directory.CreateDirectory(Folder);
            string ext = Path.GetExtension(source).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                throw new Exception("Vali JPG või PNG pilt.");
            string filename = Guid.NewGuid().ToString("N") + ext;
            File.Copy(source, Path.Combine(Folder, filename));
            return filename;
        }

        public static string Find(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            string[] candidates = {
                Path.Combine(Folder, Path.GetFileName(value)),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pildid", Path.GetFileName(value)),
                Path.Combine(Path.GetDirectoryName(Db.DatabaseFile), "Pictures", Path.GetFileName(value)),
                Path.Combine(Path.GetDirectoryName(Db.DatabaseFile), "Pildid", Path.GetFileName(value)),
                Path.Combine(Path.GetDirectoryName(Db.DatabaseFile), Path.GetFileName(value)),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Path.GetFileName(value)),
                value
            };
            foreach (string p in candidates)
                if (File.Exists(p)) return p;
            return null;
        }

        public static Image Load(string stored)
        {
            string path = Find(stored);
            if (path == null) return null;
            // Bitmap koopia vabastab algse faili luku.
            try
            {
                using (var input = Image.FromFile(path))
                    return new Bitmap(input);
            }
            catch { return null; }
        }
    }
}
