using System;
using System.Drawing;

namespace TimeManagementApp.Services
{
    // Manages app theme state and notifications
    public static class ThemeService
    {
        public enum Theme { Light, Dark, System }

        private static Theme _current = Theme.System;
        // Current theme; raises ThemeChanged when set
        public static Theme Current
        {
            get => _current;
            private set
            {
                if (_current == value) return;
                _current = value;
                ThemeChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static event EventHandler ThemeChanged;

        // Change the theme
        public static void ApplyTheme(Theme theme) => Current = theme;

        // Get background and foreground colors for the current theme
        public static (Color Back, Color Fore) GetColors()
        {
            return Current switch
            {
                Theme.Light  => (Color.White, Color.Black),
                Theme.Dark   => (Color.FromArgb(30, 30, 47), Color.White),
                Theme.System => (SystemColors.Window, SystemColors.ControlText),
                _            => (Color.White, Color.Black)
            };
        }
    }
}
