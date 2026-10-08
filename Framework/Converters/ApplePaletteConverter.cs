using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using Framework.Themes;

namespace Framework.Converters
{
    // Shared presentation roles for the two themes.
    public sealed class ApplePaletteConverter : IValueConverter
    {
        public static bool IsDark(ITheme theme) => ThemeRegistry.GetName(theme) == "Dark";

        public static string Color(ITheme theme, string role)
        {
            theme ??= ThemeRegistry.GetTheme("Light");
            bool dark = IsDark(theme);
            return role switch
            {
                "Accent" => dark ? "#78A9FF" : "#007AFF",
                "AccentText" => dark ? theme.BackgroundColor : "#FFFFFF",
                "Backdrop" or "Background" => theme.BackgroundColor,
                "Surface" => theme.MenuBackgroundColor,
                "Canvas" => theme.CanvasBackgroundColor,
                "Text" => theme.TextForeground,
                "Secondary" => dark ? "#B1B1BA" : "#74808D",
                "Border" => theme.CanvasBorderBrush,
                "SoftAccent" => theme.MenuItemBackgroundColor,
                "Track" => dark ? "#505057" : "#D6DEE5",
                _ => theme.TextForeground
            };
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(parameter?.ToString() == "Variant")
                return IsDark(value as ITheme) ? ThemeVariant.Dark : ThemeVariant.Light;
            return Brush.Parse(Color(value as ITheme, parameter?.ToString() ?? "Text"));
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
