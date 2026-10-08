using Emgu.CV;
using System.IO;
using Avalonia.Media.Imaging;
using SixLabors.ImageSharp;
using SkiaSharp;
namespace Framework.Utilities
{
    static class ImageConverter
    {
        public static Bitmap Convert<TColor>(Image<TColor,byte> image) where TColor : struct,IColor
        {
            using var bitmap=image.ToRgbImage();
            using var stream=new MemoryStream();
            bitmap.SaveAsPng(stream); stream.Position=0;
            return new Bitmap(stream);
        }
        public static Bitmap Convert(SKBitmap bitmap)
        {
            using var image=SKImage.FromBitmap(bitmap);
            using var data=image.Encode(SKEncodedImageFormat.Png,100);
            using var stream=data.AsStream();
            return new Bitmap(stream);
        }
    }
}
