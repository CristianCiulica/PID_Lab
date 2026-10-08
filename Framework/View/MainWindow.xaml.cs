using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Framework.Messaging;
using Framework.Model;
using Framework.Themes;
using Framework.ViewModel;
using static Framework.Utilities.DrawingHelper;
using static Framework.View.UiHelper;
namespace Framework.View
{
    public partial class MainWindow : Window
    {
        private readonly MainVM _mainVM;
        private readonly IMessageBus _messageBus;
        private bool _syncingScroll;
        public MainWindow()
        {
            InitializeComponent();
            _messageBus=MessageBus.Instance;
            _mainVM=(MainVM)DataContext;
            _messageBus.Subscribe<RefreshOverlaysMessage>(this,_=>Update());
            Closed+=(_,_)=>_messageBus.Unsubscribe<RefreshOverlaysMessage>(this);
            foreach(var item in MenuItems(mainMenu))
            {
                var command=item.Command;
                if(command==_mainVM.MenuCommands.LoadGrayscaleImageCommand || command==_mainVM.MenuCommands.LoadColorImageCommand ||
                   command==_mainVM.MenuCommands.ClearCommand || command==_mainVM.MenuCommands.UseProcessedImageAsInitialImageCommand ||
                   item.Header?.ToString()=="Remove drawn shapes from both canvases")
                    item.CommandParameter=new object[]{initialImageCanvas,processedImageCanvas};
            }
            _mainVM.PropertyChanged+=(_,e)=>
            {
                if(e.PropertyName==nameof(MainVM.ScaleValue)) ApplyZoom();
                else if(e.PropertyName==nameof(MainVM.Theme)) UpdateThemeMenu();
            };
            AppleAppearance.Attach(this);
            UpdateThemeMenu();
        }
        private static System.Collections.Generic.IEnumerable<MenuItem> MenuItems(ItemsControl control)
        {
            foreach(var item in control.Items.OfType<MenuItem>())
            { yield return item; foreach(var child in MenuItems(item)) yield return child; }
        }
        public void Update()
        {
            RemoveUiElements(initialImageCanvas,processedImageCanvas);
            DrawUiElements(initialImageCanvas,processedImageCanvas,sliderZoom.Value,_mainVM);
        }
        private Point ImagePosition(object sender,PointerEventArgs e)
        {
            var p=e.GetPosition((Control)sender);
            return new Point(p.X/_mainVM.ScaleValue,p.Y/_mainVM.ScaleValue);
        }
        private void ImageMouseMove(object sender,PointerEventArgs e)
        {
            var type=sender==initialImage ? ImageCanvasType.Initial : ImageCanvasType.Processed;
            _messageBus.Publish(new MouseMoveMessage(ImagePosition(sender,e),type));
        }
        private void ImagePointerPressed(object sender,PointerPressedEventArgs e)
        {
            if(e.GetCurrentPoint((Control)sender).Properties.IsLeftButtonPressed) _mainVM.SelectPoint(ImagePosition(sender,e));
            else if(e.GetCurrentPoint((Control)sender).Properties.IsRightButtonPressed) _mainVM.ClearSelectedPoints();
        }
        private void ApplyZoom()
        {
            double scale=_mainVM.ScaleValue;
            UpdateShapesProperties(initialImageCanvas,scale); UpdateShapesProperties(processedImageCanvas,scale);
            _mainVM.InitialCanvasWidth=(_mainVM.InitialImage?.PixelSize.Width ?? 0)*scale;
            _mainVM.InitialCanvasHeight=(_mainVM.InitialImage?.PixelSize.Height ?? 0)*scale;
            _mainVM.ProcessedCanvasWidth=(_mainVM.ProcessedImage?.PixelSize.Width ?? 0)*scale;
            _mainVM.ProcessedCanvasHeight=(_mainVM.ProcessedImage?.PixelSize.Height ?? 0)*scale;
        }
        private void ScrollChanged(object sender,ScrollChangedEventArgs e)
        {
            if(_syncingScroll) return;
            _syncingScroll=true;
            var source=(ScrollViewer)sender;
            var target=source==initialImageCanvasScrollViewer ? processedImageCanvasScrollViewer : initialImageCanvasScrollViewer;
            target.Offset=source.Offset;
            _syncingScroll=false;
        }
        private void UpdateThemeMenu()
        {
            var name=ThemeRegistry.GetName(_mainVM.Theme);
            foreach(var item in themeMenu.Items.OfType<MenuItem>()) item.IsChecked=item.Header?.ToString()==name;
        }
        private void ThemeModeSelector(object sender,RoutedEventArgs e)
        {
            string name=((MenuItem)sender).Header.ToString();
            _mainVM.SetThemeMode(name); UpdateThemeMenu();
            Properties.Settings.Default.Theme=name; Properties.Settings.Default.Save();
        }
    }
}
