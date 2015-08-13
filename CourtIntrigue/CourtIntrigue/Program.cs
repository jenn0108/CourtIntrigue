using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CourtIntrigue
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            StartWindow window = new StartWindow();
            Application.Run(window);

            if(window.StartGame)
            {
                MainWindow main = window.SinglePlayerGame ? new MainWindow(window.PlayerName, window.Dynasty, window.AgeInYears) : new MainWindow();
                Application.Run(main);
            }
        }
    }
}
