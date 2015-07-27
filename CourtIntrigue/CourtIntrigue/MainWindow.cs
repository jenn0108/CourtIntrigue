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
            game = new Game(NUM_PLAYERS,null);
            logger = new TextBoxLogger(this);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            game.BeginDay(logger);
            game.Tick(logger);
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
