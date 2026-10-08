using System.Collections.Generic;
using System.Linq;

namespace Framework.Themes
{
    public static class ThemeRegistry
    {
        private static readonly Dictionary<string, ITheme> _themes =
            new Dictionary<string, ITheme>
            {
                { "Light", LightTheme.Instance },
                { "Dark", DarkTheme.Instance }
            };

        public static ITheme GetTheme(string name)
        {
            // Keep the light/dark preference when reading settings from previous versions.
            name = name switch
            {
                "Forest Green" or "Cobalt Blue" => "Dark",
                "Lime Green" or "Pale Pink" or "Lavender Violet" => "Light",
                _ => name
            };
            return name != null && _themes.TryGetValue(name, out var theme) ? theme : null;
        }

        public static string GetName(ITheme theme)
        {
            return _themes.FirstOrDefault(pair => pair.Value == theme).Key;
        }
    }
}
