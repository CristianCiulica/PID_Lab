using System;
using System.Drawing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Bmp;

// Only the byte image API used by the original framework is adapted here.
// The algorithm source files remain unchanged, with BGR channel ordering.
namespace Emgu.CV
{
    public interface IColor { }
    public interface IImage : IDisposable
    {
        int Width { get; }
        int Height { get; }
        System.Drawing.Size Size { get; }
    }
    public class Image<TColor, TDepth> : IImage where TColor : struct, IColor where TDepth : unmanaged
    {
        public int Width { get; }
        public int Height { get; }
        public System.Drawing.Size Size => new System.Drawing.Size(Width, Height);
        public TDepth[,,] Data { get; }
        private int Channels => typeof(TColor) == typeof(Structure.Gray) ? 1 : 3;
        public Image(int width, int height)
        {
            if (typeof(TDepth) != typeof(byte)) throw new NotSupportedException("Only byte images are used by this framework.");
            Width = width; Height = height; Data = new TDepth[height, width, Channels];
        }
        public Image(System.Drawing.Size size) : this(size.Width, size.Height) { }
        public Image<TColor, TDepth> Clone()
        {
            var result = new Image<TColor, TDepth>(Width, Height);
            Array.Copy(Data, result.Data, Data.Length);
            return result;
        }
        public Image<Structure.Gray, byte> this[int channel]
        {
            get
            {
                var result = new Image<Structure.Gray, byte>(Width, Height);
                var source = (byte[,,])(object)Data;
                for (int y=0; y<Height; y++) for (int x=0; x<Width; x++) result.Data[y,x,0] = source[y,x,channel];
                return result;
            }
        }
        // OpenCV's integer BGR -> Gray coefficients and rounding.
        public static byte Gray(byte b, byte g, byte r) => (byte)((b * 1868 + g * 9617 + r * 4899 + 8192) >> 14);
        public Image<TOut, byte> Convert<TOut, TByte>() where TOut : struct, IColor
        {
            var result = new Image<TOut, byte>(Width, Height);
            var source = (byte[,,])(object)Data;
            bool gray = typeof(TOut) == typeof(Structure.Gray);
            for (int y=0; y<Height; y++) for (int x=0; x<Width; x++)
            {
                if (gray) result.Data[y,x,0] = Channels==1 ? source[y,x,0] : Gray(source[y,x,0],source[y,x,1],source[y,x,2]);
                else for (int c=0; c<3; c++) result.Data[y,x,c] = source[y,x,Channels==1 ? 0 : c];
            }
            return result;
        }
        public static Image<TColor, byte> Load(string path)
        {
            using var decoded = SixLabors.ImageSharp.Image.Load<Rgba64>(path);
            decoded.Mutate(c => c.AutoOrient());
            var result = new Image<TColor, byte>(decoded.Width, decoded.Height);
            bool is16 = decoded.Metadata.DecodedImageFormat?.Name == "PNG" &&
                decoded.Metadata.GetPngMetadata().BitDepth == PngBitDepth.Bit16;
            double scale=1.0, shift=0.0;
            if(is16)
            {
                int min=65535,max=0;
                bool alpha=decoded.Metadata.GetPngMetadata().ColorType == PngColorType.RgbWithAlpha || decoded.Metadata.GetPngMetadata().ColorType == PngColorType.GrayscaleWithAlpha;
                for(int y=0;y<decoded.Height;y++) for(int x=0;x<decoded.Width;x++)
                {
                    var v=decoded[x,y];
                    min=Math.Min(min,Math.Min(v.R,Math.Min(v.G,v.B)));
                    max=Math.Max(max,Math.Max(v.R,Math.Max(v.G,v.B)));
                    if(alpha) { min=Math.Min(min,v.A); max=Math.Max(max,v.A); }
                }
                if(max>255)
                {
                    scale=max==min ? 0 : 255.0/(max-min);
                    shift=scale==0 ? min : -min*scale;
                }
            }
            byte Depth(ushort value) => is16 ? (byte)Math.Clamp((int)Math.Round(Math.Abs(value*scale+shift)),0,255) : (byte)(value>>8);
            for (int y=0; y<decoded.Height; y++) for (int x=0; x<decoded.Width; x++)
            {
                var pixel=decoded[x,y];
                byte r=Depth(pixel.R), g=Depth(pixel.G), b=Depth(pixel.B);
                if (result.Channels==1) result.Data[y,x,0]=Gray(b,g,r);
                else { result.Data[y,x,0]=b; result.Data[y,x,1]=g; result.Data[y,x,2]=r; }
            }
            return result;
        }
        public SixLabors.ImageSharp.Image<Rgb24> ToRgbImage()
        {
            var result = new SixLabors.ImageSharp.Image<Rgb24>(Width,Height);
            var source=(byte[,,])(object)Data;
            for(int y=0;y<Height;y++) for(int x=0;x<Width;x++) result[x,y]=Channels==1
                ? new Rgb24(source[y,x,0],source[y,x,0],source[y,x,0])
                : new Rgb24(source[y,x,2],source[y,x,1],source[y,x,0]);
            return result;
        }
        public void Save(string path)
        {
            using var bitmap=ToRgbImage();
            switch(System.IO.Path.GetExtension(path).ToLowerInvariant())
            {
                case ".png": bitmap.Save(path,new PngEncoder { ColorType=PngColorType.Rgb }); break;
                case ".bmp": bitmap.Save(path,new BmpEncoder { BitsPerPixel=BmpBitsPerPixel.Pixel24 }); break;
                default: bitmap.Save(path,new JpegEncoder { Quality=100 }); break;
            }
        }
        public void Dispose() { }
    }
}
namespace Emgu.CV.Structure
{
    public struct Gray : Emgu.CV.IColor { }
    public struct Bgr : Emgu.CV.IColor { }
}
