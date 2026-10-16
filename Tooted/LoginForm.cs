using System;
using System.Drawing;
using System.Windows.Forms;

namespace Tooted
{
    internal class LoginForm : Form
    {
        private TextBox username = new TextBox();
        private TextBox password = new TextBox();
        public UserSession Session { get; private set; }

        public LoginForm()
        {
            Text = "Pood - sisselogimine";
            ClientSize = new Size(380, 245);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            Label heading = new Label { Text = "POOD", Left = 25, Top = 20, Width = 320, Font = new Font("Segoe UI", 18, FontStyle.Bold) };
            Label lblLogin = new Label { Text = "Kasutajanimi", Left = 25, Top = 75, Width = 110 };
            Label lblPwd = new Label { Text = "Parool", Left = 25, Top = 113, Width = 110 };
            username.SetBounds(145, 72, 210, 26);
            password.SetBounds(145, 110, 210, 26);
            password.UseSystemPasswordChar = true;
            Button login = new Button { Text = "Logi sisse", Left = 25, Top = 158, Width = 155, Height = 39 };
            Button register = new Button { Text = "Registreeri", Left = 195, Top = 158, Width = 160, Height = 39 };
            login.Click += (s, e) => SignIn();
            register.Click += (s, e) =>
            {
                try
                {
                    if (Users.AnyUsers())
                    {
                        MessageBox.Show("Uusi kasutajaid saab registreerida Omanik administraatori paneelil.");
                        return;
                    }
                    using (var f = new RegisterForm(true, null)) f.ShowDialog(this);
                }
                catch (Exception ex) { MessageBox.Show("Viga: " + ex.Message); }
            };
            AcceptButton = login;
            Controls.AddRange(new Control[] { heading, lblLogin, lblPwd, username, password, login, register });
        }

        private void SignIn()
        {
            try
            {
                Session = Users.Login(username.Text.Trim(), password.Text);
                if (Session == null) { MessageBox.Show("Vale kasutajanimi või parool."); return; }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Viga: " + ex.Message); }
        }
    }
}
