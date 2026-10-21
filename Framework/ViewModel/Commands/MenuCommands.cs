using Emgu.CV;
using Emgu.CV.Structure;

using System;
using Avalonia;
using Avalonia.Controls;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia.Media;
using System.Collections.Generic;
using Algorithms.Sections;
using Framework.View;
using Framework.Model;
using Framework.Services;
using Framework.ViewModel.ColorLevels;
using static Framework.Utilities.DrawingHelper;
using static Framework.Utilities.FileHelper;

using Algorithms.Tools;
using Algorithms.Utilities;

namespace Framework.ViewModel.Commands
{
    public class MenuCommands : BaseVM
    {
        private readonly MainVM _mainVM;
        private readonly IImageStore _imageStore;
        private readonly IWindowService _windowService;

        public MenuCommands(MainVM mainVM, IImageStore imageStore, IWindowService windowService)
        {
            _mainVM = mainVM;
            _imageStore = imageStore;
            _windowService = windowService;
        }

        #region Properties
        private Image<Gray, byte> GrayInitialImage
        {
            get => _imageStore.GrayInitialImage;
            set => _imageStore.GrayInitialImage = value;
        }

        private Image<Gray, byte> GrayProcessedImage
        {
            get => _imageStore.GrayProcessedImage;
            set => _imageStore.GrayProcessedImage = value;
        }

        private Image<Bgr, byte> ColorInitialImage
        {
            get => _imageStore.ColorInitialImage;
            set => _imageStore.ColorInitialImage = value;
        }

        private Image<Bgr, byte> ColorProcessedImage
        {
            get => _imageStore.ColorProcessedImage;
            set => _imageStore.ColorProcessedImage = value;
        }

        private PointCollection MouseClickCollection
        {
            get => _imageStore.MouseClickCollection;
        }

        private Point LastMouseClick
        {
            get => _imageStore.LastMouseClick;
        }

        private double ScaleValue
        {
            get => _mainVM.ScaleValue;
            set => _mainVM.ScaleValue = value;
        }
        #endregion

        #region File

        #region Load grayscale image
        private RelayCommand _loadGrayscaleImageCommand;
        public RelayCommand LoadGrayscaleImageCommand
        {
            get
            {
                if (_loadGrayscaleImageCommand == null)
                    _loadGrayscaleImageCommand = new RelayCommand(LoadGrayscaleImage);
                return _loadGrayscaleImageCommand;
            }
        }

        private async void LoadGrayscaleImage(object parameter)
        {
            var image = await AskForImage<Gray>("Select a grayscale picture");
            if (image != null)
            {
                Clear(parameter);
                GrayInitialImage = image;
            }
        }
        #endregion

        #region Load color image
        private ICommand _loadColorImageCommand;
        public ICommand LoadColorImageCommand
        {
            get
            {
                if (_loadColorImageCommand == null)
                    _loadColorImageCommand = new RelayCommand(LoadColorImage);
                return _loadColorImageCommand;
            }
        }

        private async void LoadColorImage(object parameter)
        {
            var image = await AskForImage<Bgr>("Select a color picture");
            if (image != null)
            {
                Clear(parameter);
                ColorInitialImage = image;
            }
        }
        #endregion

        #region Save processed image
        private ICommand _saveProcessedImageCommand;
        public ICommand SaveProcessedImageCommand
        {
            get
            {
                if (_saveProcessedImageCommand == null)
                    _saveProcessedImageCommand = new RelayCommand(SaveProcessedImage);
                return _saveProcessedImageCommand;
            }
        }

        private async void SaveProcessedImage(object parameter)
        {
            if (GrayProcessedImage == null && ColorProcessedImage == null)
            {
                MessageBox.Show("If you want to save your processed image, " +
                    "please load and process an image first!");
                return;
            }

            string imagePath = await SaveFileDialog("image.jpg");
            if (imagePath != null)
            {
                if (GrayProcessedImage != null)
                    SaveImage(GrayProcessedImage, imagePath);
                else
                    SaveImage(ColorProcessedImage, imagePath);

                Process.Start(new ProcessStartInfo("/usr/bin/open")
                {
                    ArgumentList = { imagePath },
                    UseShellExecute = false
                });
            }
        }
        #endregion

        #region Exit
        private ICommand _exitCommand;
        public ICommand ExitCommand
        {
            get
            {
                if (_exitCommand == null)
                    _exitCommand = new RelayCommand(Exit);
                return _exitCommand;
            }
        }

        private void Exit(object parameter)
        {
            DesktopRuntime.Shutdown();
        }
        #endregion

        #endregion

        #region Edit

        #region Remove drawn shapes
        private ICommand _removeDrawnShapesCommand;
        public ICommand RemoveDrawnShapesCommand
        {
            get
            {
                if (_removeDrawnShapesCommand == null)
                    _removeDrawnShapesCommand = new RelayCommand(RemoveDrawnShapes);
                return _removeDrawnShapesCommand;
            }
        }

        private void RemoveDrawnShapes(object parameter)
        {
            foreach (var canvas in parameter as object[] ?? new[] { parameter })
                RemoveUiElements(canvas as Canvas);
        }
        #endregion

        #region Clear initial canvas
        private ICommand _clearInitialCanvasCommand;
        public ICommand ClearInitialCanvasCommand
        {
            get
            {
                if (_clearInitialCanvasCommand == null)
                    _clearInitialCanvasCommand = new RelayCommand(ClearInitialCanvas);
                return _clearInitialCanvasCommand;
            }
        }

        private void ClearInitialCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);
            _imageStore.ClearInitial();
        }
        #endregion

        #region Clear processed canvas
        private ICommand _clearProcessedCanvasCommand;
        public ICommand ClearProcessedCanvasCommand
        {
            get
            {
                if (_clearProcessedCanvasCommand == null)
                    _clearProcessedCanvasCommand = new RelayCommand(ClearProcessedCanvas);
                return _clearProcessedCanvasCommand;
            }
        }

        private void ClearProcessedCanvas(object parameter)
        {
            RemoveUiElements(parameter as Canvas);
            _imageStore.ClearProcessed();
        }
        #endregion

        #region Closing all open windows and clear both canvases
        private ICommand _clearCommand;
        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                    _clearCommand = new RelayCommand(Clear);
                return _clearCommand;
            }
        }

        private void Clear(object parameter)
        {
            _windowService.CloseAllChildWindows();

            ScaleValue = 1;

            var canvases = (object[])parameter;
            ClearInitialCanvas(canvases[0]);
            ClearProcessedCanvas(canvases[1]);
        }
        #endregion

        #endregion

        #region Tools

        #region Magnifier
        private ICommand _magnifierCommand;
        public ICommand MagnifierCommand
        {
            get
            {
                if (_magnifierCommand == null)
                    _magnifierCommand = new RelayCommand(Magnifier);
                return _magnifierCommand;
            }
        }

        private void Magnifier(object parameter)
        {
            if (_windowService.IsMagnifierOpen || IsAreaSelected())
                _windowService.ShowMagnifier();
        }
        #endregion

        #region Visualize color levels

        #region Row color levels
        private ICommand _rowColorLevelsCommand;
        public ICommand RowColorLevelsCommand
        {
            get
            {
                if (_rowColorLevelsCommand == null)
                    _rowColorLevelsCommand = new RelayCommand(RowColorLevels);
                return _rowColorLevelsCommand;
            }
        }

        private void RowColorLevels(object parameter)
        {
            if (_windowService.IsRowColorLevelsOpen || IsAreaSelected())
                _windowService.ShowColorLevels(ColorLevelsType.Row);
        }
        #endregion

        #region Column color levels
        private ICommand _columnColorLevelsCommand;
        public ICommand ColumnColorLevelsCommand
        {
            get
            {
                if (_columnColorLevelsCommand == null)
                    _columnColorLevelsCommand = new RelayCommand(ColumnColorLevels);
                return _columnColorLevelsCommand;
            }
        }

        private void ColumnColorLevels(object parameter)
        {
            if (_windowService.IsColumnColorLevelsOpen || IsAreaSelected())
                _windowService.ShowColorLevels(ColorLevelsType.Column);
        }
        #endregion

        #endregion

        #region Visualize image histogram

        #region Initial image histogram
        private ICommand _histogramInitialImageCommand;
        public ICommand HistogramInitialImageCommand
        {
            get
            {
                if (_histogramInitialImageCommand == null)
                    _histogramInitialImageCommand = new RelayCommand(HistogramInitialImage);
                return _histogramInitialImageCommand;
            }
        }

        private void HistogramInitialImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            _windowService.ShowHistogram(ImageCanvasType.Initial);
        }
        #endregion

        #region Processed image histogram
        private ICommand _histogramProcessedImageCommand;
        public ICommand HistogramProcessedImageCommand
        {
            get
            {
                if (_histogramProcessedImageCommand == null)
                    _histogramProcessedImageCommand =
                        new RelayCommand(HistogramProcessedImage);
                return _histogramProcessedImageCommand;
            }
        }

        private void HistogramProcessedImage(object parameter)
        {
            if (!IsImageProcessed())
                return;

            _windowService.ShowHistogram(ImageCanvasType.Processed);
        }
        #endregion

        #endregion

        #region Copy image
        private ICommand _copyImageCommand;
        public ICommand CopyImageCommand
        {
            get
            {
                if (_copyImageCommand == null)
                    _copyImageCommand = new RelayCommand(CopyImage);
                return _copyImageCommand;
            }
        }

        private void CopyImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Copy(ColorInitialImage);
            }
            else if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Copy(GrayInitialImage);
            }
        }
        #endregion

        #region Invert image
        private ICommand _invertImageCommand;
        public ICommand InvertImageCommand
        {
            get
            {
                if (_invertImageCommand == null)
                    _invertImageCommand = new RelayCommand(InvertImage);
                return _invertImageCommand;
            }
        }

        private void InvertImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Invert(GrayInitialImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Invert(ColorInitialImage);
            }
        }
        #endregion

        #region Binarization
        private ICommand _binarizeImageCommand;
        public ICommand BinarizeImageCommand
        {
            get
            {
                if (_binarizeImageCommand == null)
                    _binarizeImageCommand = new RelayCommand(BinarizeImage);

                return _binarizeImageCommand;
            }
        }

        private void BinarizeImage(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Binarization(GrayInitialImage, 100);
            }
            else if (ColorInitialImage != null)
            {
                GrayProcessedImage = Tools.Binarization(ColorInitialImage, 100);
            }
        }
        #endregion
        

        #region Convert color image to grayscale image
        private ICommand _convertImageToGrayscaleCommand;
        public ICommand ConvertImageToGrayscaleCommand
        {
            get
            {
                if (_convertImageToGrayscaleCommand == null)
                    _convertImageToGrayscaleCommand =
                        new RelayCommand(ConvertImageToGrayscale);
                return _convertImageToGrayscaleCommand;
            }
        }

        private void ConvertImageToGrayscale(object parameter)
        {
            if (!IsImageLoaded())
                return;

            if (GrayInitialImage != null)
            {
                MessageBox.Show("It is possible to convert only color images!");
                return;
            }

            ClearProcessedCanvas(parameter);
            GrayProcessedImage = Tools.Convert(ColorInitialImage);
        }
        #endregion

        #endregion

        #region Pointwise operations
        private ICommand _contrastAndBrightnessCommand;
        public ICommand ContrastAndBrightnessCommand
        {
            get
            {
                if (_contrastAndBrightnessCommand == null)
                    _contrastAndBrightnessCommand = new RelayCommand(ContrastAndBrightness);
                return _contrastAndBrightnessCommand;
            }
        }

        private async void ContrastAndBrightness(object parameter)
        {
            var dialog = new DialogWindow(_mainVM,
                new List<string> { "α — Contrast", "β — Brightness" },
                values => values[0] > 0 ? null : "Alpha must be greater than 0.")
            {
                Title = "Contrast and Brightness"
            };
            await dialog.ShowDialog(DesktopRuntime.MainWindow);
            var values = dialog.GetValues();
            if (values == null)
                return;

            double alpha = values[0];
            double beta = values[1];
            
            ClearProcessedCanvas(parameter);
            
            if(GrayInitialImage != null)
                GrayProcessedImage = PointwiseOperations.ContrastAndBrightness(GrayInitialImage, alpha, beta);
            else if (ColorInitialImage != null)
                ColorProcessedImage = PointwiseOperations.ContrastAndBrightness(ColorInitialImage, alpha, beta);
        }

        private ICommand _gammaOperatorCommand;
        public ICommand GammaOperatorCommand
        {
            get
            {
                if (_gammaOperatorCommand == null)
                    _gammaOperatorCommand = new RelayCommand(GammaOperator);
                return _gammaOperatorCommand;
            }
        }

        private async void GammaOperator(object parameter)
        {
            if (!IsImageLoaded())
                return;

            var dialog = new DialogWindow(_mainVM,
                new List<string> { "γ — Gamma" },
                values => values[0] > 0 ? null : "Gamma must be greater than 0.")
            {
                Title = "Gamma operator"
            };

            await dialog.ShowDialog(DesktopRuntime.MainWindow);
            var values = dialog.GetValues();
            if (values == null)
                return;

            double gamma = values[0];
            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
                GrayProcessedImage = PointwiseOperations.GammaOperator(GrayInitialImage, gamma);
            else if (ColorInitialImage != null)
                ColorProcessedImage = PointwiseOperations.GammaOperator(ColorInitialImage, gamma);
        }
        #endregion

        #region Filters
        #endregion

        #region Morphological operations
        #endregion

        #region Geometric transformations
        #endregion

        #region Segmentation
        #endregion

        #region Use processed image as initial image
        private ICommand _useProcessedImageAsInitialImageCommand;
        public ICommand UseProcessedImageAsInitialImageCommand
        {
            get
            {
                if (_useProcessedImageAsInitialImageCommand == null)
                    _useProcessedImageAsInitialImageCommand =
                        new RelayCommand(UseProcessedImageAsInitialImage);
                return _useProcessedImageAsInitialImageCommand;
            }
        }

        private void UseProcessedImageAsInitialImage(object parameter)
        {
            if (!IsImageProcessed())
                return;

            var canvases = (object[])parameter;

            ClearInitialCanvas(canvases[0]);

            if (GrayProcessedImage != null)
            {
                GrayInitialImage = GrayProcessedImage;
            }
            else if (ColorProcessedImage != null)
            {
                ColorInitialImage = ColorProcessedImage;
            }

            ClearProcessedCanvas(canvases[1]);
        }
        #endregion

        #region Helpers
        private async System.Threading.Tasks.Task<Image<TColor, byte>> AskForImage<TColor>(
            string dialogTitle)
            where TColor : struct, IColor
        {
            _windowService.CloseAllChildWindows();

            string fileName = await LoadFileDialog(dialogTitle);
            return fileName != null ? ReadImage<TColor>(fileName) : null;
        }

        private static Image<TColor, byte> ReadImage<TColor>(string fileName)
            where TColor : struct, IColor
        {
            try
            {
                return LoadImage<TColor>(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"The image could not be opened:\n{fileName}\n\n{ex.Message}");
                return null;
            }
        }

        private bool IsImageLoaded()
        {
            if (GrayInitialImage != null || ColorInitialImage != null)
                return true;

            MessageBox.Show("Please add an image!");
            return false;
        }

        private bool IsImageProcessed()
        {
            if (GrayProcessedImage != null || ColorProcessedImage != null)
                return true;

            MessageBox.Show("Please process an image first!");
            return false;
        }

        private bool IsAreaSelected()
        {
            if (!IsImageLoaded())
                return false;

            if (MouseClickCollection.Count > 0)
                return true;

            MessageBox.Show("Please select an area first!");
            return false;
        }
        #endregion
        
        #region Mirror
        private ICommand _mirrorCommand;
        public ICommand MirrorImageCommand
        {
            get
            {
                if (_mirrorCommand == null)
                    _mirrorCommand = new RelayCommand(Mirror);

                return _mirrorCommand;
            }
        }

        private void Mirror(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.Mirroring(GrayInitialImage);
            }
            else if (ColorInitialImage != null)
            {
                ColorProcessedImage = Tools.Mirroring(ColorInitialImage);
            }
        }
        #endregion

        #region Rotate
        
        private ICommand _rotateClockwiseCommand;

        public ICommand RotateClockwiseCommand
        {
            get
            {
                if (_rotateClockwiseCommand == null)
                    _rotateClockwiseCommand = new RelayCommand(RotateClockwise);
                return _rotateClockwiseCommand;
            }
        }

        private void RotateClockwise(object parameter)
        {
            if (!IsImageLoaded())
                return;
            
            ClearProcessedCanvas(parameter);
            if (GrayInitialImage != null)
            {
                GrayProcessedImage = Tools.RotateClockwise(GrayInitialImage);
            } else if (ColorInitialImage != null)
                ColorProcessedImage = Tools.RotateClockwise(ColorInitialImage);
        }
    
        private ICommand _rotateCounterclockwiseCommand;

        public ICommand RotateCounterclockwiseCommand
        {
            get
            {
                if (_rotateCounterclockwiseCommand == null)
                    _rotateCounterclockwiseCommand =
                        new RelayCommand(RotateCounterclockwise);

                return _rotateCounterclockwiseCommand;
            }
        }

        private void RotateCounterclockwise(object parameter)
        {
            if (!IsImageLoaded())
                return;

            ClearProcessedCanvas(parameter);

            if (GrayInitialImage != null)
                GrayProcessedImage = Tools.RotateAntiClockwise(GrayInitialImage);
            else if (ColorInitialImage != null)
                ColorProcessedImage = Tools.RotateAntiClockwise(ColorInitialImage);
        }


        #endregion
        
        #region Crop
        
        private ICommand _cropImageCommand;
        public ICommand CropImageCommand
        {
            get
            {
                if (_cropImageCommand == null)
                    _cropImageCommand = new RelayCommand(Crop);

                return _cropImageCommand;
            }
        }

        private void Crop(object parameter)
        {
            if (!IsImageLoaded())
                return;
            if (MouseClickCollection.Count < 2)
            {
                MessageBox.Show(
                    "Selecteaza doua colturi");
                return;
            }
            
            int count = MouseClickCollection.Count;

            Point p1 = MouseClickCollection[count - 2];
            Point p2 = MouseClickCollection[count - 1];
            
            int left = (int)Math.Floor(Math.Min(p1.X, p2.X));
            int top = (int)Math.Floor(Math.Min(p1.Y, p2.Y));
            int right = (int)Math.Floor(Math.Max(p1.X, p2.X));
            int bottom = (int)Math.Floor(Math.Max(p1.Y, p2.Y));
            
            var image = _imageStore.GetImage(ImageCanvasType.Initial);

            if (left < 0 || top < 0 || right >= image.Width || bottom >= image.Height)
            {
                MessageBox.Show("Selecteaza colturile in interior");
                return;
            }
            
            int width = right - left + 1;
            int height = bottom - top + 1;
            ClearProcessedCanvas(parameter);
            
            if (GrayInitialImage != null)
                GrayProcessedImage =
                    Tools.Crop(GrayInitialImage, left, top, width, height);
            else
                ColorProcessedImage =
                    Tools.Crop(ColorInitialImage, left, top, width, height);
        }

        #endregion
        
        #region MinError Thresholding
        private ICommand _minErrThresholdCommand;
        public ICommand MinErrThresholdCommand
        {
            get
            {
                if (_minErrThresholdCommand == null)
                    _minErrThresholdCommand = new RelayCommand(BinarizeImage_MinErrorThreshold);

                return _minErrThresholdCommand;
            }
        }

        private void BinarizeImage_MinErrorThreshold(object parameter)
        {
            if (!IsImageLoaded())
                return;

            if (GrayInitialImage != null)
            {
                int T = Thresholding.MinErr(GrayInitialImage);
                var result = Tools.Binarization(GrayInitialImage, T);

                ClearProcessedCanvas(parameter);
                GrayProcessedImage = result;
            }
            else if (ColorInitialImage != null)
            {
                using var grayImage = Tools.Convert(ColorInitialImage);

                int T = Thresholding.MinErr(grayImage);
                var result = Tools.Binarization(grayImage, T);

                ClearProcessedCanvas(parameter);
                GrayProcessedImage = result;
            }
        }
        #endregion
        
    }
}
