using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace Tooted
{
    public partial class Form1 : Form
    {
        private readonly UserSession user;
        private int? selectedId;
        private string imageFile = "";
        private string newImagePath;

        // Tühja konstruktorit on vaja Visual Studio Designer jaoks.
        public Form1() : this(null) { }

        internal Form1(UserSession session)
        {
            user = session;
            InitializeComponent();
            Load += Form1_Load;
            btnLisa.Click += btnLisa_Click;
            btnUuenda.Click += btnUuenda_Click;
            btnKustuta.Click += btnKustuta_Click;
            btnKustutaKategooria.Click += btnKustutaKategooria_Click;
            dataGridTooted.CellClick += dataGridTooted_CellClick;
            FormClosed += (s, e) => { if (picPilt.Image != null) picPilt.Image.Dispose(); };
            cmbKategooria.DropDownStyle = ComboBoxStyle.DropDownList;
            dataGridTooted.ReadOnly = true;
            dataGridTooted.MultiSelect = false;
            dataGridTooted.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridTooted.AllowUserToAddRows = false;
            dataGridTooted.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Height = 550;
            Text = "Toodete haldus - " + (user == null ? "Designer" : user.Login);
            // Uuesti ei seo Otsi faili ja Lisa kategooria sündmusi: need on Designeris.


            Button cashier = new Button { Text = "Ava kassa", Left = 12, Top = 448, Width = 150, Height = 35 };
            cashier.Click += (s, e) => { using (var f = new KassaForm(user)) f.ShowDialog(this); LaadiTooted(); };
            Controls.Add(cashier);
            Button admin = new Button { Text = "Kliendid / administraator", Left = 170, Top = 448, Width = 210, Height = 35 };
            admin.Click += (s, e) => { using (var f = new AdminForm(user)) f.ShowDialog(this); };
            Controls.Add(admin);
            Button history = new Button { Text = "Müükide ajalugu", Left = 388, Top = 448, Width = 160, Height = 35 };
            history.Click += (s, e) => { using (var f = new SalesHistoryForm(user)) f.ShowDialog(this); };
            Controls.Add(history);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (user == null || !user.IsOwner)
            {
                // Ka Form1 otsene avamine ei anna müüjale omaniku õiguseid.
                if (!DesignMode) { MessageBox.Show("Ligipääs ainult omanikule."); Close(); }
                return;
            }
            try { LaadiKategooriad(); LaadiTooted(); }
            catch (Exception ex) { Error(ex); }
        }

        private void LaadiKategooriad()
        {
            DataTable dt = Db.Query("SELECT Id, Nimetus FROM Kategooriad ORDER BY Nimetus");
            cmbKategooria.DataSource = null;
            cmbKategooria.DisplayMember = "Nimetus";
            cmbKategooria.ValueMember = "Id";
            cmbKategooria.DataSource = dt;
        }

        private void LaadiTooted()
        {
            dataGridTooted.DataSource = Db.Query(
                @"SELECT T.Id, T.Toodenimetus, T.Kogus, T.Hind, T.Pilt,
                         T.Kategooria_Id, K.Nimetus AS Kategooria
                  FROM Tooted T LEFT JOIN Kategooriad K ON K.Id=T.Kategooria_Id
                  ORDER BY T.Id DESC");
            if (dataGridTooted.Columns.Contains("Pilt")) dataGridTooted.Columns["Pilt"].Visible = false;
            if (dataGridTooted.Columns.Contains("Kategooria_Id")) dataGridTooted.Columns["Kategooria_Id"].Visible = false;
            dataGridTooted.ClearSelection();
        }

        private bool ValidData()
        {
            if (string.IsNullOrWhiteSpace(txtNimetus.Text))
            { MessageBox.Show("Sisesta toote nimetus."); return false; }
            if (txtNimetus.Text.Trim().Length > 100)
            { MessageBox.Show("Toote nimi võib olla kuni 100 märki."); return false; }
            if (numKogus.Value < 0 || numHind.Value < 0)
            { MessageBox.Show("Kogus ja hind ei tohi olla negatiivsed."); return false; }
            if (cmbKategooria.SelectedValue == null || cmbKategooria.SelectedValue is DataRowView)
            { MessageBox.Show("Lisa kõigepealt kategooria."); return false; }
            return true;
        }

        private void btnLisa_Click(object sender, EventArgs e)
        {
            if (!ValidData()) return;
            try
            {
                string filename = newImagePath != null ? ImageStore.Save(newImagePath) : "";
                using (SqlConnection c = Db.Open())
                using (SqlCommand cmd = new SqlCommand(
                    @"INSERT INTO Tooted (Toodenimetus,Kogus,Hind,Pilt,Kategooria_Id)
                      VALUES (@n,@q,@p,@img,@cat)", c))
                {
                    AddProductParams(cmd, filename);
                    cmd.ExecuteNonQuery();
                }
                ClearInput(); LaadiTooted();
                MessageBox.Show("Toode lisatud.");
            }
            catch (Exception ex) { Error(ex); }
        }

        private void btnUuenda_Click(object sender, EventArgs e)
        {
            if (selectedId == null) { MessageBox.Show("Vali tabelist toode."); return; }
            if (!ValidData()) return;
            try
            {
                string filename = newImagePath != null ? ImageStore.Save(newImagePath) : imageFile;
                using (SqlConnection c = Db.Open())
                using (SqlCommand cmd = new SqlCommand(
                    @"UPDATE Tooted SET Toodenimetus=@n,Kogus=@q,Hind=@p,
                      Pilt=@img,Kategooria_Id=@cat WHERE Id=@id", c))
                {
                    AddProductParams(cmd, filename);
                    cmd.Parameters.AddWithValue("@id", selectedId.Value);
                    cmd.ExecuteNonQuery();
                }
                ClearInput(); LaadiTooted();
                MessageBox.Show("Toode uuendatud.");
            }
            catch (Exception ex) { Error(ex); }
        }

        private void AddProductParams(SqlCommand cmd, string filename)
        {
            cmd.Parameters.AddWithValue("@n", txtNimetus.Text.Trim());
            cmd.Parameters.AddWithValue("@q", Decimal.ToInt32(numKogus.Value));
            SqlParameter price = cmd.Parameters.Add("@p", SqlDbType.Decimal);
            price.Precision = 10; price.Scale = 2; price.Value = numHind.Value;
            cmd.Parameters.AddWithValue("@img", filename ?? "");
            cmd.Parameters.AddWithValue("@cat", Convert.ToInt32(cmbKategooria.SelectedValue));
        }

        private void btnKustuta_Click(object sender, EventArgs e)
        {
            if (selectedId == null) { MessageBox.Show("Vali tabelist toode."); return; }
            if (MessageBox.Show("Kustutada valitud toode?", "Kinnitus", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            try
            {
                using (SqlConnection c = Db.Open())
                using (SqlCommand cmd = new SqlCommand("DELETE FROM Tooted WHERE Id=@id", c))
                { cmd.Parameters.AddWithValue("@id", selectedId.Value); cmd.ExecuteNonQuery(); }
                ClearInput(); LaadiTooted();
            }
            catch (Exception ex) { Error(ex); }
        }

        // Olemasolev Designer kasutab seda sündmust faili valimise nupule.
        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Filter = "Pildifailid|*.png;*.jpg;*.jpeg";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    newImagePath = dlg.FileName;
                    ShowPictureFromPath(newImagePath);
                }
            }
        }

        private void ShowPictureFromPath(string path)
        {
            Image old = picPilt.Image;
            try
            {
                using (Image img = Image.FromFile(path))
                    picPilt.Image = new Bitmap(img);
            }
            catch (Exception ex) { MessageBox.Show("Pilti ei saa avada: " + ex.Message); return; }
            if (old != null) old.Dispose();
        }

        private void ShowStoredImage(string file)
        {
            Image old = picPilt.Image;
            picPilt.Image = ImageStore.Load(file);
            if (old != null) old.Dispose();
        }

        private void dataGridTooted_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            DataGridViewRow r = dataGridTooted.Rows[e.RowIndex];
            if (r.IsNewRow || r.Cells["Id"].Value == null || r.Cells["Id"].Value == DBNull.Value) return;
            selectedId = Convert.ToInt32(r.Cells["Id"].Value);
            txtNimetus.Text = Convert.ToString(r.Cells["Toodenimetus"].Value);
            numKogus.Value = Math.Max(numKogus.Minimum,
                Math.Min(numKogus.Maximum, Convert.ToDecimal(r.Cells["Kogus"].Value)));
            numHind.Value = Math.Max(numHind.Minimum,
                Math.Min(numHind.Maximum, Convert.ToDecimal(r.Cells["Hind"].Value)));
            if (r.Cells["Kategooria_Id"].Value != null && r.Cells["Kategooria_Id"].Value != DBNull.Value)
                cmbKategooria.SelectedValue = Convert.ToInt32(r.Cells["Kategooria_Id"].Value);
            imageFile = Convert.ToString(r.Cells["Pilt"].Value);
            newImagePath = null;
            ShowStoredImage(imageFile);
        }

        private void ClearInput()
        {
            selectedId = null; imageFile = ""; newImagePath = null;
            txtNimetus.Clear(); numKogus.Value = 0; numHind.Value = 0;
            Image old = picPilt.Image; picPilt.Image = null;
            if (old != null) old.Dispose();
            dataGridTooted.ClearSelection();
        }

        private void lisaKategooria_btn_Click(object sender, EventArgs e)
        {
            string name = SimplePrompt.Ask("Uue kategooria nimi", "Lisa kategooria");
            if (string.IsNullOrWhiteSpace(name)) return;
            if (name.Length > 50) { MessageBox.Show("Kuni 50 märki."); return; }
            try
            {
                using (SqlConnection c = Db.Open())
                using (SqlCommand cmd = new SqlCommand("INSERT INTO Kategooriad(Nimetus) VALUES (@n)", c))
                { cmd.Parameters.AddWithValue("@n", name.Trim()); cmd.ExecuteNonQuery(); }
                LaadiKategooriad();
            }
            catch (Exception ex) { Error(ex); }
        }

        private void btnKustutaKategooria_Click(object sender, EventArgs e)
        {
            if (cmbKategooria.SelectedValue == null || cmbKategooria.SelectedValue is DataRowView) return;
            int id = Convert.ToInt32(cmbKategooria.SelectedValue);
            try
            {
                using (SqlConnection c = Db.Open())
                using (SqlCommand cmd = new SqlCommand(
                    "DELETE FROM Kategooriad WHERE Id=@id AND NOT EXISTS(SELECT 1 FROM Tooted WHERE Kategooria_Id=@id)", c))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    if (cmd.ExecuteNonQuery() == 0)
                        MessageBox.Show("Kategooriat ei saa kustutada: selles on veel tooteid.");
                }
                LaadiKategooriad();
            }
            catch (Exception ex) { Error(ex); }
        }

        private void label1_Click(object sender, EventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void Error(Exception ex) { MessageBox.Show("Viga: " + ex.Message); }
    }

    internal static class SimplePrompt
    {
        public static string Ask(string label, string title)
        {
            using (Form f = new Form { Text = title, ClientSize = new Size(370, 125), StartPosition = FormStartPosition.CenterParent, FormBorderStyle = FormBorderStyle.FixedDialog, MaximizeBox = false, MinimizeBox = false })
            {
                Label l = new Label { Left = 12, Top = 10, Text = label, Width = 345 };
                TextBox input = new TextBox { Left = 12, Top = 35, Width = 345 };
                Button ok = new Button { Text = "OK", Left = 275, Top = 73, Width = 82, DialogResult = DialogResult.OK };
                f.Controls.Add(l); f.Controls.Add(input); f.Controls.Add(ok);
                f.AcceptButton = ok;
                return f.ShowDialog() == DialogResult.OK ? input.Text : null;
            }
        }
    }
}
