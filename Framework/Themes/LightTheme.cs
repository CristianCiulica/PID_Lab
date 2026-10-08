namespace Framework.Themes
{
    internal sealed class LightTheme : ITheme
    {
        private LightTheme() { }
        public static LightTheme Instance { get; } = new LightTheme();

        public string MenuBackgroundColor => "#FFFFFF";
        public string MenuItemBackgroundColor => "#E5EEFF";
        public string MenuItemForegroundColor => "#202B36";
        public string BackgroundColor => "#F1F4F7";
        public string CanvasBackgroundColor => "#F5F7F9";
        public string CanvasBorderBrush => "#DDE3E9";
        public string TextForeground => "#202B36";
        public string ButtonBackgroundColor => "#FFFFFF";
        public string ButtonForegroundColor => "#202B36";
        public string ButtonBorderBrush => "#DDE3E9";
    }
}
