using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace CourtIntrigue
{
    interface IView
    {
        void Display(Panel top, Panel bottom, Semaphore mutex);
    }

    class TextTopBottomButton : IView
    {
        private string upperText;
        private string[] lowerButtons;
        private Panel top;
        private Panel bottom;
        private Semaphore mutex;

        public int SelectedIndex { get; private set; }

        public TextTopBottomButton(string text, string[] buttons)
        {
            upperText = text;
            lowerButtons = buttons;
            SelectedIndex = -1;
        }

        public void Display(Panel top, Panel bottom, Semaphore mutex)
        {
            this.top = top;
            this.bottom = bottom;
            this.mutex = mutex;
            top.Controls.Clear();
            bottom.Controls.Clear();
            top.Controls.Add(new Label() { Text = upperText, Size = top.Size });

            TableLayoutPanel tlp = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = lowerButtons.Length,
                RowCount = 1
            };
            for (int i = 0; i < lowerButtons.Length; ++i)
            {
                Button button = new Button()
                {
                    Text = lowerButtons[i],
                    Tag = i,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    AutoEllipsis = false,
                };
                button.Click += Button_Click;
                tlp.Controls.Add(button, i, 0);
            }
            bottom.Controls.Add(tlp);
        }

        private void Button_Click(object sender, EventArgs e)
        {
            SelectedIndex = (int)(sender as Button).Tag;
            top.Controls.Clear();
            bottom.Controls.Clear();
            mutex.Release();
        }
    }

    class BothButton : IView
    {
        private string[] upperButtons;
        private string[] lowerButtons;
        private Panel top;
        private Panel bottom;
        private Semaphore mutex;

        public int SelectedIndex { get; private set; }
        public bool SelectedTop { get; private set; }

        public BothButton(string[] topButtons, string[] bottomButtons)
        {
            upperButtons = topButtons;
            lowerButtons = bottomButtons;
            SelectedIndex = -1;
        }

        public void Display(Panel top, Panel bottom, Semaphore mutex)
        {
            this.top = top;
            this.bottom = bottom;
            this.mutex = mutex;
            top.Controls.Clear();
            bottom.Controls.Clear();

            TableLayoutPanel upperTlp = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = upperButtons.Length
            };
            for (int i = 0; i < upperButtons.Length; ++i)
            {
                Button button = new Button()
                {
                    Text = upperButtons[i],
                    Tag = i,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    AutoEllipsis = false
                };
                button.Click += TopButton_Click;
                upperTlp.Controls.Add(button, 0, i);
            }
            top.Controls.Add(upperTlp);

            TableLayoutPanel lowerTlp = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                ColumnCount = lowerButtons.Length,
                RowCount = 1
            };
            for (int i = 0; i < lowerButtons.Length; ++i)
            {
                Button button = new Button()
                {
                    Text = lowerButtons[i],
                    Tag = i,
                    AutoSize = true,
                    AutoSizeMode = AutoSizeMode.GrowAndShrink,
                    AutoEllipsis = false
                };
                button.Click += BottomButton_Click;
                lowerTlp.Controls.Add(button, i, 0);
            }
            bottom.Controls.Add(lowerTlp);
        }

        private void TopButton_Click(object sender, EventArgs e)
        {
            SelectedTop = true;
            SelectedIndex = (int)(sender as Button).Tag;
            top.Controls.Clear();
            bottom.Controls.Clear();
            mutex.Release();
        }

        private void BottomButton_Click(object sender, EventArgs e)
        {
            SelectedTop = false;
            SelectedIndex = (int)(sender as Button).Tag;
            top.Controls.Clear();
            bottom.Controls.Clear();
            mutex.Release();
        }
    }
}
