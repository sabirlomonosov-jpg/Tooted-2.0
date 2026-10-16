using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Tooted
{
    internal class KassaForm : Form
    {
        private sealed class Product
        {
            public int Id;
            public string Name;
            public string Image;
            public int Stock;
            public decimal Price;
        }
        private sealed class CartItem
        {
            public Product Product;
            public int Quantity;
        }
        private readonly UserSession user;
        private readonly Dictionary<int, Product> products = new Dictionary<int, Product>();
        private readonly Dictionary<int, CartItem> cart = new Dictionary<int, CartItem>();
        private readonly FlowLayoutPanel productGrid = new FlowLayoutPanel();
        private readonly DataGridView gridCart = new DataGridView();
        private readonly TextBox search = new TextBox();
        private readonly TextBox cardNumber = new TextBox();
        private readonly Label clientLabel = new Label();
        private readonly Label totalLabel = new Label();
        private int? clientId;
        private int? lastSaleId;

        public KassaForm(UserSession session)
        {
            if (session == null) throw new ArgumentNullException("session");
            user = session;
            Text = "Kassa - " + user.Login + " (" + user.Role + ")";
            ClientSize = new Size(1160, 720);
            MinimumSize = new Size(1180, 760);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 10);

            Controls.Add(new Label { Text = "TOOTED", Left = 20, Top = 12, Width = 400, Font = new Font("Segoe UI", 16, FontStyle.Bold) });
            search.SetBounds(20, 50, 505, 29);
            search.TextChanged += (s, e) => DrawProducts();
            Controls.Add(search);
            Button refresh = ButtonAt("Värskenda", 535, 48, 165, 31, (s, e) => ReloadProducts());
            Controls.Add(refresh);

            productGrid.SetBounds(20, 91, 680, 598);
            productGrid.AutoScroll = true;
            productGrid.BackColor = Color.FromArgb(245, 246, 249);
            productGrid.BorderStyle = BorderStyle.FixedSingle;
            productGrid.WrapContents = true;
            Controls.Add(productGrid);

            Controls.Add(new Label { Text = "OSTUKORV", Left = 730, Top = 12, Width = 370, Font = new Font("Segoe UI", 16, FontStyle.Bold) });
            Controls.Add(new Label { Text = "Kliendikaardi number:", Left = 730, Top = 59, Width = 220 });
            cardNumber.SetBounds(730, 81, 260, 29);
            cardNumber.TextChanged += (s, e) => { clientId = null; clientLabel.Text = "Klient: puudub"; };
            Controls.Add(cardNumber);
            Controls.Add(ButtonAt("Otsi", 1002, 80, 125, 32, (s, e) => FindCustomer()));
            clientLabel.SetBounds(730, 116, 397, 24);
            clientLabel.Text = "Klient: puudub";
            Controls.Add(clientLabel);

            gridCart.SetBounds(730, 149, 397, 367);
            gridCart.ReadOnly = true;
            gridCart.AllowUserToAddRows = false;
            gridCart.MultiSelect = false;
            gridCart.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridCart.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Controls.Add(gridCart);
            Controls.Add(ButtonAt("− 1 tk", 730, 523, 116, 35, (s, e) => ReduceItem()));
            Controls.Add(ButtonAt("Eemalda", 856, 523, 116, 35, (s, e) => RemoveItem()));
            Controls.Add(ButtonAt("Tühjenda", 982, 523, 145, 35, (s, e) => { cart.Clear(); DrawCart(); }));
            totalLabel.SetBounds(730, 576, 397, 35);
            totalLabel.Font = new Font("Segoe UI", 17, FontStyle.Bold);
            Controls.Add(totalLabel);
            Controls.Add(ButtonAt("SOORITA OST", 730, 625, 220, 50, (s, e) => Checkout()));
            Controls.Add(ButtonAt("Viimane tšekk", 960, 625, 167, 50, (s, e) => ReprintLast()));
            Load += (s, e) => ReloadProducts();
        }

        private Button ButtonAt(string text, int x, int y, int w, int h, EventHandler click)
        {
            Button b = new Button { Text = text, Left = x, Top = y, Width = w, Height = h };
            b.Click += click;
            return b;
        }

        private void ReloadProducts()
        {
            try
            {
                products.Clear();
                DataTable t = Db.Query("SELECT Id, Toodenimetus, Kogus, Hind, Pilt FROM Tooted ORDER BY Toodenimetus");
                foreach (DataRow r in t.Rows)
                {
                    Product p = new Product {
                        Id = Convert.ToInt32(r["Id"]), Name = Convert.ToString(r["Toodenimetus"]),
                        Stock = Convert.ToInt32(r["Kogus"]), Price = Convert.ToDecimal(r["Hind"]),
                        Image = Convert.ToString(r["Pilt"])
                    };
                    products[p.Id] = p;
                }
                // Kui toodete andmed muutusid, uuenda ka ostukorvi infot.
                foreach (int id in cart.Keys.ToList())
                {
                    if (!products.ContainsKey(id)) cart.Remove(id);
                    else if (cart[id].Quantity > products[id].Stock) cart.Remove(id);
                    else cart[id].Product = products[id];
                }
                DrawProducts(); DrawCart();
            }
            catch (Exception ex) { MessageBox.Show("Toodete laadimise viga: " + ex.Message); }
        }

        private void DrawProducts()
        {
            foreach (Control ctrl in productGrid.Controls)
            {
                PictureBox pic = ctrl.Controls.OfType<PictureBox>().FirstOrDefault();
                if (pic != null && pic.Image != null) pic.Image.Dispose();
            }
            productGrid.Controls.Clear();
            string needle = search.Text.Trim();
            foreach (Product p in products.Values.Where(x =>
                x.Name.IndexOf(needle, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                Panel tile = new Panel { Width = 198, Height = 205, BackColor = Color.White, Margin = new Padding(8), BorderStyle = BorderStyle.FixedSingle };
                PictureBox picture = new PictureBox { Left = 10, Top = 7, Width = 176, Height = 113, SizeMode = PictureBoxSizeMode.Zoom };
                picture.Image = ImageStore.Load(p.Image);
                tile.Controls.Add(picture);
                Label title = new Label { Left = 9, Top = 125, Width = 180, Height = 24,
                    Text = p.Name, AutoEllipsis = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
                tile.Controls.Add(title);
                Label price = new Label { Left = 9, Top = 149, Width = 180, Height = 20,
                    Text = p.Price.ToString("0.00") + " €  |  Laos: " + p.Stock,
                    ForeColor = p.Stock <= 5 ? Color.DarkRed : Color.DarkGreen };
                tile.Controls.Add(price);
                Button add = new Button { Text = p.Stock > 0 ? "Lisa ostukorvi" : "Otsas", Left = 9, Top = 173, Width = 178, Height = 27, Enabled = p.Stock > 0 };
                int id = p.Id;
                add.Click += (s, e) => AddToCart(id);
                tile.Controls.Add(add);
                productGrid.Controls.Add(tile);
            }
        }

        private void AddToCart(int id)
        {
            Product p = products[id];
            int count = cart.ContainsKey(id) ? cart[id].Quantity : 0;
            if (count >= p.Stock) { MessageBox.Show("Laos ei ole rohkem kaupa."); return; }
            if (!cart.ContainsKey(id)) cart[id] = new CartItem { Product = p, Quantity = 0 };
            cart[id].Quantity++;
            DrawCart();
        }

        private int? SelectedCartId()
        {
            if (gridCart.CurrentRow == null || gridCart.CurrentRow.Cells["Id"].Value == null) return null;
            return Convert.ToInt32(gridCart.CurrentRow.Cells["Id"].Value);
        }

        private void ReduceItem()
        {
            int? id = SelectedCartId();
            if (id == null || !cart.ContainsKey(id.Value)) return;
            if (--cart[id.Value].Quantity <= 0) cart.Remove(id.Value);
            DrawCart();
        }

        private void RemoveItem()
        {
            int? id = SelectedCartId();
            if (id == null) return;
            cart.Remove(id.Value);
            DrawCart();
        }

        private decimal Total() { return cart.Values.Sum(item => item.Product.Price * item.Quantity); }

        private void DrawCart()
        {
            gridCart.DataSource = cart.Values.Select(i => new {
                Id = i.Product.Id, Toode = i.Product.Name,
                Tk = i.Quantity, Hind = i.Product.Price,
                Summa = i.Quantity * i.Product.Price
            }).ToList();
            if (gridCart.Columns.Contains("Id")) gridCart.Columns["Id"].Visible = false;
            totalLabel.Text = "KOKKU: " + Total().ToString("0.00") + " €";
        }

        private void FindCustomer()
        {
            clientId = null;
            try
            {
                using (SqlConnection c = Db.Open())
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT Id,Nimi,Boonus FROM Kliendid WHERE Kliendikaart=@k", c))
                {
                    cmd.Parameters.AddWithValue("@k", cardNumber.Text.Trim());
                    using (SqlDataReader rd = cmd.ExecuteReader())
                    {
                        if (rd.Read())
                        {
                            clientId = rd.GetInt32(0);
                            clientLabel.Text = "Klient: " + rd.GetString(1) + ", boonus: " + rd.GetInt32(2);
                        }
                        else { clientLabel.Text = "Kaarti ei leitud"; MessageBox.Show("Kliendikaarti ei leitud."); }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Viga: " + ex.Message); }
        }

        private void Checkout()
        {
            if (cart.Count == 0) { MessageBox.Show("Ostukorv on tühi."); return; }
            decimal sum = Total();
            if (sum > 99999999.99M) { MessageBox.Show("Ostusumma on liiga suur."); return; }
            if (MessageBox.Show("Kinnita ost: " + sum.ToString("0.00") + " €?", "Müük", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            int saleId;
            try
            {
                using (SqlConnection c = Db.Open())
                using (SqlTransaction tr = c.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        using (SqlCommand cmd = new SqlCommand(
                            @"INSERT INTO Muugid (KasutajaId,KlientId,Summa)
                              VALUES(@u,@c,@s); SELECT CAST(SCOPE_IDENTITY() AS INT)", c, tr))
                        {
                            cmd.Parameters.AddWithValue("@u", user.Id);
                            cmd.Parameters.AddWithValue("@c", (object)clientId ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@s", sum);
                            saleId = (int)cmd.ExecuteScalar();
                        }
                        foreach (CartItem item in cart.Values.OrderBy(i => i.Product.Id))
                        {
                            // Pärast ostukorvi avamist võis hind või laoseis muutuda.
                            // UPDLOCK hoiab kontrolli ja müügi samas tehingus.
                            using (SqlCommand check = new SqlCommand(
                                "SELECT Hind, Kogus FROM Tooted WITH (UPDLOCK, HOLDLOCK) WHERE Id=@id", c, tr))
                            {
                                check.Parameters.AddWithValue("@id", item.Product.Id);
                                using (SqlDataReader reader = check.ExecuteReader())
                                {
                                    if (!reader.Read())
                                        throw new Exception("Toodet enam ei leitud: " + item.Product.Name);
                                    decimal currentPrice = reader.GetDecimal(0);
                                    int currentStock = reader.GetInt32(1);
                                    if (currentPrice != item.Product.Price)
                                        throw new Exception("Toote hind muutus. Värskenda ostukorvi: " + item.Product.Name);
                                    if (currentStock < item.Quantity)
                                        throw new Exception("Laos pole piisavalt toodet: " + item.Product.Name);
                                }
                            }
                            using (SqlCommand update = new SqlCommand(
                                "UPDATE Tooted SET Kogus=Kogus-@q WHERE Id=@id AND Kogus>=@q", c, tr))
                            {
                                update.Parameters.AddWithValue("@id", item.Product.Id);
                                update.Parameters.AddWithValue("@q", item.Quantity);
                                if (update.ExecuteNonQuery() != 1)
                                    throw new Exception("Laos pole piisavalt toodet: " + item.Product.Name);
                            }
                            using (SqlCommand row = new SqlCommand(
                                @"INSERT INTO Muugiread (MuukId,TooteId,Tootenimetus,Kogus,Hind)
                                  VALUES (@sale,@id,@name,@q,@price)", c, tr))
                            {
                                row.Parameters.AddWithValue("@sale", saleId);
                                row.Parameters.AddWithValue("@id", item.Product.Id);
                                row.Parameters.AddWithValue("@name", item.Product.Name);
                                row.Parameters.AddWithValue("@q", item.Quantity);
                                row.Parameters.AddWithValue("@price", item.Product.Price);
                                row.ExecuteNonQuery();
                            }
                        }
                        if (clientId.HasValue)
                        {
                            int points = (int)Math.Floor(sum / 10M);
                            using (SqlCommand bonus = new SqlCommand(
                                "UPDATE Kliendid SET Boonus=Boonus+@b WHERE Id=@id", c, tr))
                            {
                                bonus.Parameters.AddWithValue("@b", points);
                                bonus.Parameters.AddWithValue("@id", clientId.Value);
                                if (bonus.ExecuteNonQuery() != 1)
                                    throw new Exception("Klienti ei leitud.");
                            }
                        }
                        tr.Commit();
                    }
                    catch { tr.Rollback(); throw; }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ostu ei tehtud. " + ex.Message);
                ReloadProducts();
                return;
            }

            // Müük on nüüd salvestatud. PDF vea korral saab tšeki uuesti teha.
            lastSaleId = saleId;
            cart.Clear();
            cardNumber.Clear(); clientId = null;
            ReloadProducts();
            try
            {
                string path = Receipt.FromDatabase(saleId);
                MessageBox.Show("Müük tehtud!\nTšekk: " + path);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Müük salvestati, kuid PDF loomine ebaõnnestus: " + ex.Message +
                    "\nKasuta nuppu 'Viimane tšekk'.");
            }
        }

        private void ReprintLast()
        {
            if (!lastSaleId.HasValue) { MessageBox.Show("Selles seansis ei ole veel müüki."); return; }
            try { MessageBox.Show("Tšekk salvestatud:\n" + Receipt.FromDatabase(lastSaleId.Value)); }
            catch (Exception ex) { MessageBox.Show("PDF viga: " + ex.Message); }
        }
    }
}
