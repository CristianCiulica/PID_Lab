using Avalonia;
using Avalonia.Controls;

using Framework.Model;
using Framework.ViewModel;

namespace Framework.View
{
    public partial class HistogramWindow : Window
    {
        private readonly HistogramVM _histogramVM;

        public HistogramWindow(MainVM mainVM, ImageCanvasType canvas)
            : this(mainVM)
        {
            ShowHistogram(canvas);
        }

        public HistogramWindow(MainVM mainVM, string title, dynamic values)
            : this(mainVM)
        {
            _histogramVM.Title = title;

            if (values != null)
                _histogramVM.CreateHistogram(values);
        }

        private HistogramWindow(MainVM mainVM)
        {
            InitializeComponent();
            UiHelper.SetMainWindowAsOwner(this);

            _histogramVM = new HistogramVM();
            _histogramVM.Theme = mainVM.Theme;

            DataContext = _histogramVM;
        }

        public void ShowHistogram(ImageCanvasType canvas)
        {
            _histogramVM.CreateHistogram(canvas);
        }

        private void WindowClosing(object sender, WindowClosingEventArgs e)
        {
            _histogramVM.Cleanup();
        }
    }
}
