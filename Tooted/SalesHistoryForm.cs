using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace Tooted
{
    internal class SalesHistoryForm : Form
    {
        private DataGridView grid = new DataGridView();

        public SalesHistoryForm(UserSession user)
        {
            if (user == null || !user.IsOwner) throw new UnauthorizedAccessException("Ainult omanikule.");
            Text = "Müükide ajalugu";
            ClientSize = new Size(850, 500);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 10);
            grid.SetBounds(15, 55, 820, 425);
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Controls.Add(grid);
            Button refresh = new Button { Text = "Värskenda", Left = 15, Top = 10, Width = 140, Height = 34 };
            Button pdf = new Button { Text = "Loo valitud tšekk PDF", Left = 170, Top = 10, Width = 225, Height = 34 };
            refresh.Click += (s, e) => Reload();
            pdf.Click += (s, e) =>
            {
                if (grid.CurrentRow == null) return;
                try
                {
                    int id = Convert.ToInt32(grid.CurrentRow.Cells["Id"].Value);
                    MessageBox.Show("Tšekk salvestatud:\n" + Receipt.FromDatabase(id));
                }
                catch (Exception ex) { MessageBox.Show("Viga: " + ex.Message); }
            };
            Controls.Add(refresh);
            Controls.Add(pdf);
            Load += (s, e) => Reload();
        }

        private void Reload()
        {
            try
            {
                grid.DataSource = Db.Query(
                    @"SELECT M.Id, M.Kuupaev, U.Kasutajanimi AS Muua,
                       C.Nimi AS Klient, M.Summa
                       FROM Muugid M JOIN Kasutajad U ON U.Id=M.KasutajaId
                       LEFT JOIN Kliendid C ON C.Id=M.KlientId
                       ORDER BY M.Kuupaev DESC, M.Id DESC");
                grid.ClearSelection();
            }
            catch (Exception ex) { MessageBox.Show("Viga: " + ex.Message); }
        }
    }
}
