using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Tooted
{
    internal class AdminForm : Form
    {
        private readonly UserSession user;
        private readonly DataGridView grid = new DataGridView();
        private readonly TextBox name = new TextBox();
        private readonly TextBox phone = new TextBox();
        private readonly TextBox email = new TextBox();
        private readonly TextBox card = new TextBox();
        private readonly NumericUpDown bonus = new NumericUpDown();
        private int? selected;

        public AdminForm(UserSession session)
        {
            if (session == null || !session.IsOwner) throw new UnauthorizedAccessException("Ainult omanikule.");
            user = session;
            Text = "Administraatori paneel - kliendid";
            ClientSize = new Size(1000, 630);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10);

            Label heading = new Label { Text = "KLIENTIDE HALDUS", Left = 20, Top = 15, Width = 600, Font = new Font("Segoe UI", 17, FontStyle.Bold) };
            Controls.Add(heading);
            string[] labels = { "Nimi", "Telefon", "E-post", "Kliendikaart", "Boonus" };
            Control[] inputs = { name, phone, email, card, bonus };
            for (int i = 0; i < labels.Length; i++)
            {
                Controls.Add(new Label { Text = labels[i], Left = 20, Top = 70 + i * 45, Width = 132 });
                inputs[i].SetBounds(160, 66 + i * 45, 243, 27);
                Controls.Add(inputs[i]);
            }
            bonus.Maximum = 100000000;
            Button generate = ButtonAt("Loo kaardinumber", 420, 202, 180, 32, (s, e) =>
                card.Text = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant());
            Controls.Add(generate);
            Controls.Add(ButtonAt("Lisa klient", 20, 309, 133, 39, (s, e) => Save(false)));
            Controls.Add(ButtonAt("Uuenda", 162, 309, 133, 39, (s, e) => Save(true)));
            Controls.Add(ButtonAt("Kustuta", 304, 309, 133, 39, (s, e) => Delete()));
            Controls.Add(ButtonAt("Tühjenda", 446, 309, 133, 39, (s, e) => ClearFields()));
            Controls.Add(ButtonAt("Registreeri kasutaja", 610, 309, 185, 39, (s, e) =>
            {
                using (var form = new RegisterForm(false, user)) form.ShowDialog(this);
            }));
            Controls.Add(ButtonAt("Müükide ajalugu", 807, 309, 165, 39, (s, e) =>
            {
                using (var form = new SalesHistoryForm(user)) form.ShowDialog(this);
            }));

            grid.SetBounds(20, 363, 952, 245);
            grid.ReadOnly = true;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.AllowUserToAddRows = false;
            grid.CellClick += GridClick;
            Controls.Add(grid);
            Load += (s, e) => Reload();
        }

        private Button ButtonAt(string title, int x, int y, int w, int h, EventHandler action)
        {
            Button b = new Button { Text = title, Left = x, Top = y, Width = w, Height = h };
            b.Click += action;
            return b;
        }

        private void Reload()
        {
            try
            {
                grid.DataSource = Db.Query("SELECT Id,Nimi,Telefon,Epost,Kliendikaart,Boonus FROM Kliendid ORDER BY Id DESC");
                grid.ClearSelection();
            }
            catch (Exception ex) { MessageBox.Show("Viga: " + ex.Message); }
        }

        private void GridClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = grid.Rows[e.RowIndex];
            selected = Convert.ToInt32(r.Cells["Id"].Value);
            name.Text = Convert.ToString(r.Cells["Nimi"].Value);
            phone.Text = Convert.ToString(r.Cells["Telefon"].Value);
            email.Text = Convert.ToString(r.Cells["Epost"].Value);
            card.Text = Convert.ToString(r.Cells["Kliendikaart"].Value);
            bonus.Value = Math.Max(bonus.Minimum, Math.Min(bonus.Maximum, Convert.ToDecimal(r.Cells["Boonus"].Value)));
        }

        private void Save(bool update)
        {
            if (update && !selected.HasValue) { MessageBox.Show("Vali klient tabelist."); return; }
            if (string.IsNullOrWhiteSpace(name.Text) || string.IsNullOrWhiteSpace(card.Text))
            { MessageBox.Show("Nimi ja kliendikaardi number on kohustuslikud."); return; }
            if (name.Text.Trim().Length > 100 || phone.Text.Trim().Length > 30 || email.Text.Trim().Length > 100 || card.Text.Trim().Length > 50)
            { MessageBox.Show("Üks sisestatud väärtus on liiga pikk."); return; }
            try
            {
                string sql = update
                    ? @"UPDATE Kliendid SET Nimi=@n,Telefon=@t,Epost=@e,Kliendikaart=@k,Boonus=@b WHERE Id=@id"
                    : @"INSERT INTO Kliendid (Nimi,Telefon,Epost,Kliendikaart,Boonus) VALUES (@n,@t,@e,@k,@b)";
                using (SqlConnection c = Db.Open())
                using (SqlCommand cmd = new SqlCommand(sql, c))
                {
                    cmd.Parameters.AddWithValue("@n", name.Text.Trim());
                    cmd.Parameters.AddWithValue("@t", phone.Text.Trim());
                    cmd.Parameters.AddWithValue("@e", email.Text.Trim());
                    cmd.Parameters.AddWithValue("@k", card.Text.Trim());
                    cmd.Parameters.AddWithValue("@b", Decimal.ToInt32(bonus.Value));
                    if (update) cmd.Parameters.AddWithValue("@id", selected.Value);
                    cmd.ExecuteNonQuery();
                }
                ClearFields(); Reload();
                MessageBox.Show("Andmed salvestatud.");
            }
            catch (SqlException ex) { MessageBox.Show("Andmebaasi viga (kontrolli kliendikaarti): " + ex.Message); }
            catch (Exception ex) { MessageBox.Show("Viga: " + ex.Message); }
        }

        private void Delete()
        {
            if (!selected.HasValue) { MessageBox.Show("Vali klient tabelist."); return; }
            if (MessageBox.Show("Kustutada klient?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                using (SqlConnection c = Db.Open())
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Kliendid WHERE Id=@id", c))
                {
                    cmd.Parameters.AddWithValue("@id", selected.Value);
                    cmd.ExecuteNonQuery();
                }
                ClearFields(); Reload();
            }
            catch (SqlException)
            { MessageBox.Show("Klienti ei saa kustutada, kui tal on salvestatud müüke."); }
        }

        private void ClearFields()
        {
            selected = null; name.Clear(); phone.Clear(); email.Clear(); card.Clear(); bonus.Value = 0;
            grid.ClearSelection();
        }
    }
}
