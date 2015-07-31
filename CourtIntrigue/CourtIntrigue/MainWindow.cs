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
        private static int NUM_PLAYERS = 5; //For testing purposes 5 is much easier to read and understand

        Game game;
        TextBoxLogger logger;
        int dayState = 0;

        public MainWindow()
        {
            InitializeComponent();
            logger = new TextBoxLogger(this);
            game = new Game(logger, NUM_PLAYERS, null);

            if(Debugger.IsAttached)
            {
                restartButton.Visible = true;
                debugButton.Visible = true;
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            //There is one day start followed by 5 action ticks.
            //We'll loop through them as the user clicks the button.
            if((dayState % 6) == 0)
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
                main.debugBox.AppendText(text + "\r\n");
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
            debugBox.Clear();
            dayState = 0;
            game = new Game(logger, NUM_PLAYERS, null);
        }
    }
}
