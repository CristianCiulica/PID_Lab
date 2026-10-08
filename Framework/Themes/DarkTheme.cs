namespace Framework.Themes
{
    internal sealed class DarkTheme : ITheme
    {
        private DarkTheme() { }
        public static DarkTheme Instance { get; } = new DarkTheme();

        public string MenuBackgroundColor => "#28282B";
        public string MenuItemBackgroundColor => "#303E58";
        public string MenuItemForegroundColor => "#F4F4F6";
        public string BackgroundColor => "#1C1C1E";
        public string CanvasBackgroundColor => "#222225";
        public string CanvasBorderBrush => "#414147";
        public string TextForeground => "#F4F4F6";
        public string ButtonBackgroundColor => "#28282B";
        public string ButtonForegroundColor => "#F4F4F6";
        public string ButtonBorderBrush => "#414147";
    }
}
