using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourtIntrigue
{
    internal partial class Notificator : UserControl
    {
        private List<string> notifications = new List<string>();

        public Notificator()
        {
            InitializeComponent();
            SizeChanged += Notificator_SizeChanged;

            Notificator_SizeChanged(this, EventArgs.Empty);
            GenerateTable();
        }

        private void Notificator_SizeChanged(object sender, EventArgs e)
        {
            table.Left = 0;
            table.Top = 31;
            table.Size = new Size(ClientSize.Width, ClientSize.Height - 31);
        }

        public void AddNotification(InformationInstance info)
        {
            notifications.Add("Gain Information: " + info.Description);

            Invoke((MethodInvoker)GenerateTable);
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            int closeRow = (int)(sender as Button).Tag;
            notifications.RemoveAt(closeRow);
            GenerateTable();
        }

        private void GenerateTable()
        {
            View.DisposeAndClearPanel(table);
            table.RowStyles.Clear();
            for (int i = 0; i < notifications.Count; ++i)
            {
                Label label = new Label() { Text = notifications[i], Dock = DockStyle.Fill };
                Button closeButton = new Button() { Text = "X", Tag = i };
                closeButton.Click += CloseButton_Click;
                table.Controls.Add(label, 0, i);
                table.Controls.Add(closeButton, 1, i);
                table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        }
    }
}
