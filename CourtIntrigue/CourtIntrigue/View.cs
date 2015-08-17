using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace CourtIntrigue
{
    static class View
    {
        public static int NOTIFICATOR_SIZE = 200;
    }

    interface IView
    {
        void Display(Panel top, Panel bottom, Semaphore mutex);
    }

    class TextTopBottomButton : IView
    {
        private string upperText;
        private string[] lowerButtons;
        private bool[] lowerButtonEnable;
        private Panel top;
        private Panel bottom;
        private Semaphore mutex;
        private Notificator notificator;
        private Character target;
        private Character perspective;

        public int SelectedIndex { get; private set; }

        public TextTopBottomButton(Character target, Character perspective, string text, string[] buttons, bool[] buttonEnable, Notificator notificator)
        {
            this.target = target;
            this.perspective = perspective;
            upperText = text;
            lowerButtons = buttons;
            lowerButtonEnable = buttonEnable;
            SelectedIndex = -1;
            this.notificator = notificator;
        }

        public void Display(Panel top, Panel bottom, Semaphore mutex)
        {
            this.top = top;
            this.bottom = bottom;
            this.mutex = mutex;
            top.Controls.Clear();
            bottom.Controls.Clear();

            Label label = new Label() { Text = upperText, Size = new System.Drawing.Size(top.Width - View.NOTIFICATOR_SIZE, top.Height) };
            top.Controls.Add(label);
            if (target != null)
            {
                top.Controls.Add(new CharacterHeadshot(target, perspective) { Active = false });
                label.Left = 160;
            }

            notificator.Left = top.Width - View.NOTIFICATOR_SIZE;
            notificator.Top = 0;
            notificator.Size = new System.Drawing.Size(View.NOTIFICATOR_SIZE, top.Height);
            top.Controls.Add(notificator);

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
                    Enabled = lowerButtonEnable == null ? true : lowerButtonEnable[i]
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
        private Character[] upperButtons;
        private string[] lowerButtons;
        private bool[] upperButtonEnables;
        private bool[] lowerButtonEnables;
        private Panel top;
        private Panel bottom;
        private Semaphore mutex;
        private Notificator notificator;
        private Character perspectiveCharacter;

        public int SelectedIndex { get; private set; }
        public bool SelectedTop { get; private set; }

        public BothButton(Character perspective, Character[] topButtons, bool[] topButtonEnables, string[] bottomButtons, bool[] bottomButtonEnables, Notificator notificator)
        {
            perspectiveCharacter = perspective;
            upperButtons = topButtons;
            upperButtonEnables = topButtonEnables;
            lowerButtons = bottomButtons;
            lowerButtonEnables = bottomButtonEnables;
            this.notificator = notificator;
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
                Left = 0,
                Top = 0,
                Size = new System.Drawing.Size(top.Width- View.NOTIFICATOR_SIZE, top.Height),
                ColumnCount = (int)Math.Floor((top.Width - View.NOTIFICATOR_SIZE) /150.0),
                RowCount = (int)Math.Floor(top.Height/150.0)
            };
            int c = 0;
            int r = 0;
            for (int i = 0; i < upperButtons.Length; ++i)
            {
                CharacterHeadshot button = new CharacterHeadshot(upperButtons[i], perspectiveCharacter)
                {
                    Tag = i,
                    Active = upperButtonEnables == null ? true : upperButtonEnables[i]
                };
                button.SelectCharacter += TopButton_Click;
                upperTlp.Controls.Add(button, c, r);
                ++c;
                if (c > upperTlp.ColumnCount)
                {
                    ++r;
                    c = 0;
                }
            }
            top.Controls.Add(upperTlp);
            notificator.Left = top.Width - View.NOTIFICATOR_SIZE;
            notificator.Top = 0;
            notificator.Size = new System.Drawing.Size(View.NOTIFICATOR_SIZE, top.Height);
            top.Controls.Add(notificator);

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
                    AutoEllipsis = false,
                    Enabled = lowerButtonEnables == null ? true : lowerButtonEnables[i]
                };
                button.Click += BottomButton_Click;
                lowerTlp.Controls.Add(button, i, 0);
            }
            bottom.Controls.Add(lowerTlp);
        }

        private void TopButton_Click(object sender, EventArgs e)
        {
            SelectedTop = true;
            SelectedIndex = (int)(sender as Control).Tag;
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
