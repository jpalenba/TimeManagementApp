using System;
using System.IO;
using Newtonsoft.Json;

namespace TimeManagementApp.Services
{
    /// <summary>
    /// Loads the saved app theme from settings.json.
    /// </summary>
    public static class SettingsLoader
    {
        private class SettingsData
        {
            public bool   EnableNotifications { get; set; } = true;
            public string AppTheme           { get; set; } = "System Default";
        }

        // location of the settings file
        private static readonly string settingsPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        /// <summary>
        /// Reads settings.json and returns the stored Theme.
        /// </summary>
        public static ThemeService.Theme LoadTheme()
        {
            try
            {
                if (!File.Exists(settingsPath))
                    return ThemeService.Theme.System;  // default if missing

                var json = File.ReadAllText(settingsPath);
                var data = JsonConvert.DeserializeObject<SettingsData>(json)
                           ?? new SettingsData();

                return data.AppTheme switch
                {
                    "Light" => ThemeService.Theme.Light,
                    "Dark"  => ThemeService.Theme.Dark,
                    _       => ThemeService.Theme.System
                };
            }
            catch
            {
                // fallback on error
                return ThemeService.Theme.System;
            }
        }
    }
}
