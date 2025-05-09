// Services/SettingsLoader.cs
using System;
using System.IO;
using Newtonsoft.Json;
using TimeManagementApp.Services;

namespace TimeManagementApp.Services
{
    /// <summary>
    /// Reads your settings.json and returns the saved Theme.
    /// </summary>
    public static class SettingsLoader
    {
        private class SettingsData
        {
            public bool   EnableNotifications { get; set; } = true;
            public string AppTheme           { get; set; } = "System Default";
        }

        private static readonly string settingsPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.json");

        public static ThemeService.Theme LoadTheme()
        {
            try
            {
                if (!File.Exists(settingsPath))
                    return ThemeService.Theme.System;

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
                return ThemeService.Theme.System;
            }
        }
    }
}
