using System;
using System.Windows.Forms;

namespace Tooted
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Db.Initialize();
                using (var login = new LoginForm())
                {
                    if (login.ShowDialog() != DialogResult.OK) return;
                    UserSession user = login.Session;
                    if (user.IsOwner) Application.Run(new Form1(user));
                    else Application.Run(new KassaForm(user));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Käivitamisel tekkis viga:\n" + ex.Message,
                    "Viga", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
