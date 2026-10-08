using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System.Threading.Tasks;
namespace Framework.Compatibility
{
    internal static class MessageBox
    {
        public static void Show(string text) => Show(DesktopRuntime.MainWindow,text);
        public static async void Show(Window owner,string text,string title="")
        {
            var dialog=new Window {Title=title,Width=440,SizeToContent=SizeToContent.Height,CanResize=false,WindowStartupLocation=WindowStartupLocation.CenterOwner};
            var panel=new StackPanel {Margin=new Thickness(20),Spacing=20};
            panel.Children.Add(new TextBlock {Text=text,TextWrapping=TextWrapping.Wrap});
            var button=new Button {Content="OK",IsDefault=true,Width=75,HorizontalAlignment=HorizontalAlignment.Center};
            button.Click+=(_,_)=>dialog.Close(); panel.Children.Add(button); dialog.Content=panel;
            await dialog.ShowDialog(owner);
        }
    }
}
