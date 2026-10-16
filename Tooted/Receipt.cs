using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace Tooted
{
    internal sealed class ReceiptLine
    {
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get { return Quantity * Price; } }
    }

    internal static class Receipt
    {
        public static string Folder { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Arved"); } }

        // Tšekkide korduvprintimine kasutab müügibaasis säilinud andmeid.
        public static string FromDatabase(int saleId)
        {
            DateTime date;
            string seller, customer;
            decimal total;
            List<ReceiptLine> lines = new List<ReceiptLine>();
            using (SqlConnection c = Db.Open())
            {
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT M.Kuupaev, U.Kasutajanimi, C.Nimi, M.Summa
                      FROM Muugid M JOIN Kasutajad U ON U.Id=M.KasutajaId
                      LEFT JOIN Kliendid C ON C.Id=M.KlientId WHERE M.Id=@id", c))
                {
                    cmd.Parameters.AddWithValue("@id", saleId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) throw new Exception("Müüki ei leitud.");
                        date = r.GetDateTime(0);
                        seller = r.GetString(1);
                        customer = r.IsDBNull(2) ? "-" : r.GetString(2);
                        total = r.GetDecimal(3);
                    }
                }
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Tootenimetus, Kogus, Hind FROM Muugiread WHERE MuukId=@id ORDER BY Id", c))
                {
                    cmd.Parameters.AddWithValue("@id", saleId);
                    using (SqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            lines.Add(new ReceiptLine { Name = r.GetString(0), Quantity = r.GetInt32(1), Price = r.GetDecimal(2) });
                }
            }
            return Generate(saleId, date, seller, customer, lines, total);
        }

        public static string Generate(int id, DateTime date, string seller, string customer,
            List<ReceiptLine> lines, decimal total)
        {
            Directory.CreateDirectory(Folder);
            string path = Path.Combine(Folder, "Tsekk_" + id.ToString("D6") + ".pdf");
            using (PdfDocument doc = new PdfDocument())
            {
                doc.Info.Title = "Tšekk " + id;
                XFont title = new XFont("Arial", 19, XFontStyle.Bold);
                XFont bold = new XFont("Arial", 10, XFontStyle.Bold);
                XFont normal = new XFont("Arial", 10, XFontStyle.Regular);
                PdfPage page = null;
                XGraphics g = null;
                double y = 0;
                Action newPage = () =>
                {
                    if (g != null) g.Dispose();
                    page = doc.AddPage();
                    page.Size = PdfSharp.PageSize.A5;
                    g = XGraphics.FromPdfPage(page);
                    y = 27;
                };
                newPage();
                g.DrawString("POOD", title, XBrushes.Black, 25, y + 16); y += 34;
                g.DrawString("TŠEKK  #" + id, bold, XBrushes.Black, 25, y + 11); y += 24;
                g.DrawString("Kuupäev: " + date.ToString("dd.MM.yyyy HH:mm"), normal, XBrushes.Black, 25, y + 10); y += 19;
                g.DrawString("Müüja: " + seller, normal, XBrushes.Black, 25, y + 10); y += 19;
                g.DrawString("Klient: " + customer, normal, XBrushes.Black, 25, y + 10); y += 27;
                g.DrawString("Toode", bold, XBrushes.Black, 25, y + 9);
                g.DrawString("Tk", bold, XBrushes.Black, 239, y + 9);
                g.DrawString("Hind", bold, XBrushes.Black, 278, y + 9);
                g.DrawString("Kokku", bold, XBrushes.Black, 348, y + 9);
                y += 10;
                g.DrawLine(XPens.Black, 25, y + 6, 395, y + 6); y += 22;
                foreach (ReceiptLine line in lines)
                {
                    if (y > 530)
                    {
                        newPage();
                        g.DrawString("Tšekk #" + id + " (järg)", bold, XBrushes.Black, 25, y + 9);
                        y += 29;
                    }
                    string name = line.Name.Length > 31 ? line.Name.Substring(0, 30) + "…" : line.Name;
                    g.DrawString(name, normal, XBrushes.Black, 25, y + 10);
                    g.DrawString(line.Quantity.ToString(), normal, XBrushes.Black, 239, y + 10);
                    g.DrawString(line.Price.ToString("0.00"), normal, XBrushes.Black, 278, y + 10);
                    g.DrawString(line.Total.ToString("0.00"), normal, XBrushes.Black, 348, y + 10);
                    y += 22;
                }
                if (y > 515) newPage();
                g.DrawLine(XPens.Black, 25, y + 4, 395, y + 4);
                y += 18;
                g.DrawString("SUMMA: " + total.ToString("0.00") + " EUR", title, XBrushes.Black, 25, y + 19);
                y += 44;
                g.DrawString("Täname ostu eest!", normal, XBrushes.Black, 25, y + 10);
                g.Dispose();
                doc.Save(path);
            }
            return path;
        }
    }
}
