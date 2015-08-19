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
using System.Threading;

namespace CourtIntrigue
{
    public partial class MainWindow : Form
    {
        private static int NUM_PLAYERS = 30; // Now that we have jobs we need > 6 people.

        PlayerCharacter player;
        Game game;
        StringBuilder logData = new StringBuilder();
        TextBoxLogger logger;
        Semaphore playerOptionWaiter = new Semaphore(0, 1);
        IView currentView = null;
        Thread gameThread;
        int automatedCurrentStep = 0;

        public MainWindow()
        {
            this.FormClosed += MainWindow_FormClosed;
            InitializeComponent();
            logger = new TextBoxLogger(this);
            game = new Game(logger, NUM_PLAYERS);
            UpdateDate();

            if(Debugger.IsAttached)
            {
                restartButton.Visible = true;
                debugButton.Visible = true;
            }

            speedStep.Visible = true;
            restartButton.Visible = true;
            nextButton.Visible = true;
            playerStatusStrip.Visible = false;
        }

        public MainWindow(string playerName, string playerDynasty, int age)
        {
            this.FormClosed += MainWindow_FormClosed;
            InitializeComponent();
            logger = new TextBoxLogger(this);
            game = new Game(logger, NUM_PLAYERS);

            player = new PlayerCharacter(this, playerName, -Game.GetYearInTicks(age), new Dynasty(playerDynasty), 500, game, Character.GenderEnum.Male);
            game.AddPlayer(player);
            UpdateDate();
            UpdateStatus();

            if (Debugger.IsAttached)
            {
                restartButton.Visible = true;
                debugButton.Visible = true;
            }
            speedStep.Visible = false;
            restartButton.Visible = false;
            nextButton.Visible = false;
            playerStatusStrip.Visible = true;

            gameThread = new Thread(GameThread);
            gameThread.Start();
        }

        private void MainWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            //We need to stop the game thread or we'll never actually quit.
            if(gameThread != null)
                gameThread.Abort();
        }

        public void UpdateDate()
        {
            int tick = game.CurrentTime % 5;
            int day = (game.CurrentTime / 5) % 10;
            int season = (game.CurrentTime / 50) % 4;
            int year = game.CurrentTime / 200;
            string[] seasons = new string[] { "Winter", "Spring", "Summer", "Fall" };
            string[] ticks = new string[] { "Early Morning", "Morning", "Afternoon", "Late Afternoon", "Evening" };
            dateLabel.Text = string.Format("{0}, Day {1} of {2}, Year {3}\n{4}", ticks[tick], day+1, seasons[season], year+1, game.CurrentTime);
        }

        public void UpdateStatus()
        {
            playerNameLabel.Text = player.Fullname;
            playerNameLabel.ToolTipText = string.Format("You are {0} of house {1}", player.Name, player.Dynasty.Name);

            prestigeLabel.Text = player.Prestige.ToString();
            StringBuilder tooltipBuilder = new StringBuilder();
            tooltipBuilder.AppendLine("Prestige");
            foreach (var mod in player.CurrentPrestigeModifiers)
            {
                tooltipBuilder.Append('(');
                if (mod.DailyChange > 0)
                    tooltipBuilder.Append('+');
                tooltipBuilder.Append(mod.DailyChange);
                tooltipBuilder.Append(") ");
                tooltipBuilder.AppendLine(mod.Description);
            }
            prestigeLabel.ToolTipText = tooltipBuilder.ToString();

            goldLabel.Text = player.Money.ToString();
            goldLabel.ToolTipText = string.Format("You have {0} gold", player.Money);
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

        private void restartButton_Click(object sender, EventArgs e)
        {
            game = new Game(logger, NUM_PLAYERS);
            UpdateDate();
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if((automatedCurrentStep % 6) == 0)
            {
                game.BeginDay();
            }
            else
            {
                game.Tick();
            }
            ++automatedCurrentStep;
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
                ClearScreen();
            }
            else
            {
                splitContainer2.Panel1.Controls.Clear();
                JournalForm journalForm = new JournalForm(game.AllCharacters, game, player);
                journalForm.TopLevel = false;
                splitContainer2.Panel1.Controls.Add(journalForm);
                journalForm.Show();
            }
        }

        private void logButton_Click(object sender, EventArgs e)
        {
            if (splitContainer2.Panel1.Controls.Count > 0 && splitContainer2.Panel1.Controls[0] is TextBox)
            {
                ClearScreen();
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

        private void GameThread()
        {
            //We just cycle through the days forever.
            int dayState = 0;
            while(true)
            {
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
        }

        private void ClearScreen()
        {
            if (currentView != null)
            {
                ShowView();
            }
            else
            {
                splitContainer2.Panel1.Controls.Clear();
            }
        }

        private void ShowView()
        {
            UpdateDate();
            UpdateStatus();
            currentView.Display(splitContainer2.Panel1, splitContainer2.Panel2, playerOptionWaiter);
        }

        internal void LaunchView(IView view)
        {
            currentView = view;
            Invoke( (MethodInvoker)ShowView);
            playerOptionWaiter.WaitOne();
        }
    }
}
