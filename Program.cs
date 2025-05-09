using System;
using System.Windows.Forms;
using TimeManagementApp.Services;   

namespace TimeManagementApp
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // load persisted theme from settings.json
            var theme = SettingsLoader.LoadTheme();
            ThemeService.ApplyTheme(theme);

            Application.Run(new MainMenuForm());
        }
    }
}
