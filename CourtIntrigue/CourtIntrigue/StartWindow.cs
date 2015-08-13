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
    public partial class StartWindow : Form
    {
        public bool StartGame { get; private set; }
        public bool SinglePlayerGame { get; private set; }
        public string PlayerName { get; private set; }
        public string Dynasty { get; private set; }
        public int AgeInYears { get; private set; }

        public StartWindow()
        {
            InitializeComponent();
        }

        private void startButton_Click(object sender, EventArgs e)
        {
            PlayerName = nameBox.Text;
            Dynasty = dynastyBox.Text;
            AgeInYears = (int)ageBox.Value;
            StartGame = true;
            Close();
        }

        private void quitButton_Click(object sender, EventArgs e)
        {
            StartGame = false;
            Close();
        }

        private void automatedButton_CheckedChanged(object sender, EventArgs e)
        {
            nameBox.Enabled = false;
            dynastyBox.Enabled = false;
            ageBox.Enabled = false;
            SinglePlayerGame = false;
        }

        private void singlePlayerButton_CheckedChanged(object sender, EventArgs e)
        {
            nameBox.Enabled = true;
            dynastyBox.Enabled = true;
            ageBox.Enabled = true;
            SinglePlayerGame = true;
        }
    }
}
