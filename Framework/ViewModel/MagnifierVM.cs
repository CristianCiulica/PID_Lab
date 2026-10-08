using Emgu.CV;
using Emgu.CV.Structure;
using Framework.Messaging;
using Framework.Model;
using System;
using SkiaSharp;
using Avalonia.Media;
using ImageConverter = Framework.Utilities.ImageConverter;
using Point = Avalonia.Point;

namespace Framework.ViewModel
{
    public class MagnifierVM : ThemedVM
    {
        private readonly IImageStore _imageStore;
        private Point _lastPoint;

        public MagnifierVM()
            : this(MessageBus.Instance, ImageStore.Instance)
        {
        }

        public MagnifierVM(IMessageBus messageBus, IImageStore imageStore)
            : base(messageBus)
        {
            _imageStore = imageStore;
        }

        #region Rendering
        public void RefreshMagnifierImages()
        {
            _lastPoint = _imageStore.LastMouseClick;

            if (_imageStore.GrayInitialImage != null)
                InitialMagnifierImage = RenderGrid(_imageStore.GrayInitialImage);
            else if (_imageStore.ColorInitialImage != null)
                InitialMagnifierImage = RenderGrid(_imageStore.ColorInitialImage);
            else
                InitialMagnifierImage = null;

            if (_imageStore.GrayProcessedImage != null)
                ProcessedMagnifierImage = RenderGrid(_imageStore.GrayProcessedImage);
            else if (_imageStore.ColorProcessedImage != null)
                ProcessedMagnifierImage = RenderGrid(_imageStore.ColorProcessedImage);
            else
                ProcessedMagnifierImage = null;
        }

        private Avalonia.Media.Imaging.Bitmap RenderGrid(Image<Bgr, byte> image)
        {
            return RenderGrid(image.Width, image.Height, (x, y) =>
            {
                int b = image.Data[y, x, 0];
                int g = image.Data[y, x, 1];
                int r = image.Data[y, x, 2];
                return (System.Drawing.Color.FromArgb(r, g, b), r + "\n" + g + "\n" + b, b <= 128 && g <= 128 && r <= 128);
            });
        }

        private Avalonia.Media.Imaging.Bitmap RenderGrid(Image<Gray, byte> image)
        {
            return RenderGrid(image.Width, image.Height, (x, y) =>
            {
                int gray = image.Data[y, x, 0];
                return (System.Drawing.Color.FromArgb(gray, gray, gray), "\n" + gray + "\n", gray <= 128);
            });
        }

        private Avalonia.Media.Imaging.Bitmap RenderGrid(int imageWidth, int imageHeight,
            Func<int, int, (System.Drawing.Color Fill, string Text, bool IsDark)> describePixel)
        {
            int step = GridSize / 9;

            using var grid = new SKBitmap(GridSize, GridSize);
            using var canvas = new SKCanvas(grid);
            canvas.Clear(SKColors.White);
            using var typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold);
            using var paint = new SKPaint { Typeface=typeface, TextSize=7*96f/72f, IsAntialias=true };
            for(int dx=-4; dx<=4; dx++) for(int dy=-4; dy<=4; dy++)
            {
                int left=(dx+4)*step, top=(dy+4)*step;
                if(_lastPoint.Y+dy>=0 && _lastPoint.X+dx>=0 && _lastPoint.Y+dy<imageHeight && _lastPoint.X+dx<imageWidth)
                {
                    var pixel=describePixel((int)_lastPoint.X+dx,(int)_lastPoint.Y+dy);
                    paint.Color=new SKColor(pixel.Fill.R,pixel.Fill.G,pixel.Fill.B);
                    canvas.DrawRect(left,top,step,step,paint);
                    paint.Color=pixel.IsDark ? SKColors.White : SKColors.Black;
                    var lines=pixel.Text.Split('\n');
                    for(int i=0;i<lines.Length;i++) canvas.DrawText(lines[i],left+2,top+11+i*11,paint);
                }
            }
            return ImageConverter.Convert(grid);
        }

        #endregion

        #region Properties
        public int GridSize { get; set; } = 450;

        private Avalonia.Media.Imaging.Bitmap _initialMagnifierImage;
        public Avalonia.Media.Imaging.Bitmap InitialMagnifierImage
        {
            get => _initialMagnifierImage;
            set { _initialMagnifierImage = value; NotifyPropertyChanged(nameof(InitialMagnifierImage)); }
        }

        private Avalonia.Media.Imaging.Bitmap _processedMagnifierImage;
        public Avalonia.Media.Imaging.Bitmap ProcessedMagnifierImage
        {
            get => _processedMagnifierImage;
            set { _processedMagnifierImage = value; NotifyPropertyChanged(nameof(ProcessedMagnifierImage)); }
        }
        #endregion
    }
}
