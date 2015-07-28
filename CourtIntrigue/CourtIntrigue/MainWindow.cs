using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourtIntrigue
{
    public partial class MainWindow : Form
    {
        private static int NUM_PLAYERS = 20;

        Game game;
        TextBoxLogger logger;

        public MainWindow()
        {
            InitializeComponent();
            logger = new TextBoxLogger(this);
            game = new Game(logger, NUM_PLAYERS, null);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            game.BeginDay();
            game.Tick();
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
    }
}
