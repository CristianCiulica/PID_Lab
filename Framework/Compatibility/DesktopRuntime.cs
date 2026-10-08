using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Collections;

namespace Framework
{
    internal static class DesktopRuntime
    {
        private static IClassicDesktopStyleApplicationLifetime Lifetime => (IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
        public static Window MainWindow => Lifetime.MainWindow;
        public static IEnumerable<Window> Windows => Lifetime.Windows;
        public static void Shutdown() => Lifetime.Shutdown();
    }
    public static class PointCollectionExtensions
    {
        public static AvaloniaList<Point> Clone(this AvaloniaList<Point> points) => new AvaloniaList<Point>(points);
    }
}
namespace Framework
{
    internal static class DesktopRuntimeOwners
    {
        private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<Avalonia.Controls.Window,Avalonia.Controls.Window> Owners=new();
        public static void Set(Avalonia.Controls.Window child,Avalonia.Controls.Window owner) => Owners.Add(child,owner);
        public static void Show(Avalonia.Controls.Window child)
        {
            if(Owners.TryGetValue(child,out var owner)) child.Show(owner); else child.Show();
        }
    }
}
