using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace CourtIntrigue
{
    public partial class MainWindow : Form
    {
        private static int NUM_PLAYERS = 10; // Now that we have jobs we need > 6 people.

        Game game;
        StringBuilder logData = new StringBuilder();
        TextBoxLogger logger;
        int dayState = 0;

        public MainWindow()
        {
            InitializeComponent();
            logger = new TextBoxLogger(this);
            game = new Game(logger, NUM_PLAYERS, null);
            UpdateDate();

            if(Debugger.IsAttached)
            {
                restartButton.Visible = true;
                debugButton.Visible = true;
            }
        }


        public void UpdateDate()
        {
            int tick = game.CurrentTime % 5;
            int day = (game.CurrentTime / 5) % 10;
            int season = (game.CurrentTime / 50) % 4;
            int year = game.CurrentTime / 200;
            string[] seasons = new string[] { "Winter", "Spring", "Summer", "Fall" };
            string[] ticks = new string[] { "Early Morning", "Morning", "Afternoon", "Late Afternoon", "Evening" };
            dateLabel.Text = string.Format("{0}, Day {1} of {2}, Year {3}", ticks[tick], day+1, seasons[season], year+1);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            UpdateDate();
            //There is one day start followed by 5 action ticks.
            //We'll loop through them as the user clicks the button.
            if ((dayState % 6) == 0)
            {
                game.BeginDay();
            }
            else
            {
                game.Tick();
            }
            ++dayState;
        }

        class TextBoxLogger : Logger
        {
            private MainWindow main;

            public TextBoxLogger(MainWindow window)
            {
                main = window;
            }

            public void PrintText(string text)
            {
                main.logData.Append(text + "\r\n");
            }
        }

        private void debugButton_Click(object sender, EventArgs e)
        {
            //Inspect game here for information
            game.Log("DebugBreak()");
            Debugger.Break();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dayState = 0;
            game = new Game(logger, NUM_PLAYERS, null);
            UpdateDate();
        }

        private void speedStep_Click(object sender, EventArgs e)
        {
            for (int i=0; i<240; ++i)
            {
                nextButton_Click(sender, e);
            }

        }

        private void journalButton_Click(object sender, EventArgs e)
        {
            if(splitContainer2.Panel1.Controls.Count > 0 && splitContainer2.Panel1.Controls[0] is JournalForm)
            {
                splitContainer2.Panel1.Controls.Clear();
            }
            else
            {
                splitContainer2.Panel1.Controls.Clear();
                JournalForm journalForm = new JournalForm(game.AllCharacters, game);
                journalForm.TopLevel = false;
                splitContainer2.Panel1.Controls.Add(journalForm);
                journalForm.Show();
            }
        }

        private void logButton_Click(object sender, EventArgs e)
        {
            if (splitContainer2.Panel1.Controls.Count > 0 && splitContainer2.Panel1.Controls[0] is TextBox)
            {
                splitContainer2.Panel1.Controls.Clear();
            }
            else
            {
                splitContainer2.Panel1.Controls.Clear();
                TextBox box = new TextBox()
                {
                    Multiline = true,
                    Left = 0,
                    Top = 0,
                    Size = splitContainer2.Panel1.Size,
                    ScrollBars = ScrollBars.Vertical
                };
                splitContainer2.Panel1.Controls.Add(box);
                box.Show();
                box.AppendText(logData.ToString());
            }
        }
    }
}
