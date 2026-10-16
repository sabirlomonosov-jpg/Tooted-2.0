using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tooted
{
    internal class RegisterForm : Form
    {
        private readonly bool first;
        private readonly UserSession admin;
        private TextBox username = new TextBox();
        private TextBox password = new TextBox();
        private TextBox confirm = new TextBox();
        private ComboBox role = new ComboBox();

        public RegisterForm(bool firstUser, UserSession adminSession)
        {
            first = firstUser;
            admin = adminSession;
            Text = "Kasutaja registreerimine";
            ClientSize = new Size(410, 285);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            string[] names = { "Kasutajanimi", "Parool (8+ märki)", "Korda parooli", "Roll" };
            for (int i = 0; i < names.Length; i++)
                Controls.Add(new Label { Text = names[i], Left = 15, Top = 30 + i * 45, Width = 150 });
            username.SetBounds(174, 27, 210, 26);
            password.SetBounds(174, 72, 210, 26);
            confirm.SetBounds(174, 117, 210, 26);
            role.SetBounds(174, 162, 210, 26);
            password.UseSystemPasswordChar = confirm.UseSystemPasswordChar = true;
            role.DropDownStyle = ComboBoxStyle.DropDownList;
            role.Items.Add("Müüja");
            if (first || (admin != null && admin.IsOwner)) role.Items.Add("Omanik");
            role.SelectedItem = first ? "Omanik" : "Müüja";
            role.Enabled = !first && admin != null && admin.IsOwner;
            Button save = new Button { Text = "Registreeri", Left = 174, Top = 217, Width = 210, Height = 36 };
            save.Click += (s, e) => Register();
            AcceptButton = save;
            Controls.AddRange(new Control[] { username, password, confirm, role, save });
        }

        private void Register()
        {
            if (username.Text.Trim().Length < 3 || username.Text.Trim().Length > 50)
            { MessageBox.Show("Kasutajanimi: 3-50 märki."); return; }
            if (password.Text.Length < 8)
            { MessageBox.Show("Parool peab olema vähemalt 8 märki."); return; }
            if (password.Text != confirm.Text)
            { MessageBox.Show("Paroolid ei ole samad."); return; }
            try
            {
                Users.Register(username.Text.Trim(), password.Text, role.SelectedItem.ToString(), admin);
                MessageBox.Show("Kasutaja registreeritud!");
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Registreerimine ebaõnnestus: " + ex.Message); }
        }
    }
}
