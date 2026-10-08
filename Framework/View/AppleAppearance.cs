using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Framework.Converters;
using Framework.Themes;
using Framework.ViewModel;
using OxyPlot;
using PlotView = OxyPlot.Avalonia.PlotView;
using OxyPlot.Series;
using System.Runtime.CompilerServices;

namespace Framework.View
{
    // Shared plot presentation only; the models, series and tool commands are unchanged.
    internal static class AppleAppearance
    {
        private sealed class SeriesColor { public OxyColor Value; }
        private static readonly ConditionalWeakTable<Series,SeriesColor> OriginalColors = new();
        private static OxyColor ReadableColor(OxyColor color, bool dark)
        {
            if(!dark) return color;
            if(color==OxyColors.Blue) return OxyColor.Parse("#67A8FF");
            if(color==OxyColors.Green) return OxyColor.Parse("#65D38E");
            if(color==OxyColors.Red) return OxyColor.Parse("#FF7774");
            if(color==OxyColors.Black) return OxyColor.Parse("#ECF3F8");
            return color;
        }
        public static void Attach(Window window)
        {
            INotifyPropertyChanged observed = null;
            ITheme appliedTheme = null;
            bool resourcesApplied = false;
            void Refresh()
            {
                var theme = window.DataContext switch { MainVM vm => vm.Theme, ThemedVM vm => vm.Theme, _ => null };
                if(!resourcesApplied || !ReferenceEquals(appliedTheme,theme))
                {
                    void SetColorResource(string name, string role)
                    {
                        var color = Avalonia.Media.Color.Parse(ApplePaletteConverter.Color(theme,role));
                        window.Resources[name+"Color"] = color;
                        window.Resources[name+"Brush"] = new SolidColorBrush(color);
                    }
                    SetColorResource("ThemeForeground","Text");
                    SetColorResource("ThemeBackground","Canvas");
                    SetColorResource("ThemeBorderLow","Border");
                    SetColorResource("ThemeBorderMid","Track");
                    SetColorResource("ThemeBorderHigh","Accent");
                    SetColorResource("ThemeAccent","Accent");
                    SetColorResource("Highlight","Accent");
                    SetColorResource("HighlightForeground","AccentText");
                    appliedTheme = theme;
                    resourcesApplied = true;
                }
                var text = OxyColor.Parse(ApplePaletteConverter.Color(theme,"Text"));
                var muted = OxyColor.Parse(ApplePaletteConverter.Color(theme,"Secondary"));
                var line = OxyColor.Parse(ApplePaletteConverter.Color(theme,"Track"));
                foreach(var view in window.GetVisualDescendants().OfType<PlotView>())
                {
                    var model = view.Model;
                    if(model == null || (model.TextColor == text && model.DefaultFont == "Helvetica Neue" &&
                        model.PlotAreaBorderColor == line && model.Axes.All(axis => axis.TextColor == muted && axis.TicklineColor == line))) continue;
                    model.DefaultFont = "Helvetica Neue";
                    model.DefaultFontSize = 11;
                    model.TextColor = text;
                    model.PlotAreaBorderColor = line;
                    foreach(var axis in model.Axes)
                    {
                        axis.TextColor = muted;
                        axis.AxislineColor = line;
                        axis.TicklineColor = line;
                    }
                    bool dark=ApplePaletteConverter.IsDark(theme);
                    foreach(var series in model.Series)
                    {
                        if(series is LineSeries lineSeries)
                        {
                            var original=OriginalColors.GetValue(series,_=>new SeriesColor {Value=lineSeries.Color});
                            lineSeries.Color=ReadableColor(original.Value,dark);
                        }
                        else if(series is RectangleBarSeries bars)
                        {
                            var original=OriginalColors.GetValue(series,_=>new SeriesColor {Value=bars.StrokeColor});
                            bars.StrokeColor=ReadableColor(original.Value,dark);
                        }
                    }
                    model.InvalidatePlot(false);
                }
            }
            void Changed(object sender, PropertyChangedEventArgs e) => Dispatcher.UIThread.Post(Refresh,DispatcherPriority.Loaded);
            void Observe()
            {
                if(observed != null) observed.PropertyChanged -= Changed;
                observed = window.DataContext as INotifyPropertyChanged;
                if(observed != null) observed.PropertyChanged += Changed;
                Dispatcher.UIThread.Post(Refresh,DispatcherPriority.Loaded);
            }
            window.DataContextChanged += (_,_) => Observe();
            window.Loaded += (_,_) => Refresh();
            window.LayoutUpdated += (_,_) => Refresh();
            window.Closed += (_,_) => { if(observed != null) observed.PropertyChanged -= Changed; };
            Observe();
        }
    }
}
