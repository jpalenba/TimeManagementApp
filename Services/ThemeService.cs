// Services/ThemeService.cs
using System;
using System.Drawing;

namespace TimeManagementApp.Services
{
    public static class ThemeService
    {
        public enum Theme { Light, Dark, System }

        private static Theme _current = Theme.System;
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

        public static void ApplyTheme(Theme theme) => Current = theme;

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
