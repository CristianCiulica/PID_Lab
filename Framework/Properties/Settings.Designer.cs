using System;
using System.IO;
using Framework.Themes;
namespace Framework.Properties
{
    internal sealed class Settings
    {
        public static Settings Default { get; } = new Settings();
        private static string PathName => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PID_Framework", "theme.txt");
        public string Theme { get; set; } = "Light";
        private Settings()
        {
            try { if(File.Exists(PathName)) Theme=File.ReadAllText(PathName).Trim(); } catch(IOException) { }
            Theme = ThemeRegistry.GetName(ThemeRegistry.GetTheme(Theme)) ?? "Light";
        }
        public void Save() { Directory.CreateDirectory(Path.GetDirectoryName(PathName)); File.WriteAllText(PathName,Theme); }
    }
}
