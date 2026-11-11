using System;
using Emgu.CV;
using Emgu.CV.Structure;
using SixLabors.ImageSharp.Formats.Png;

namespace Algorithms.Sections
{
    public class Filters
    {
        public static long[,] IntegralImage(Image<Gray, byte> f)
        {
            if (f == null)
                throw new ArgumentNullException("f");

            long[,] I = new long[f.Height, f.Width];
            for (int y = 0; y < f.Height; y++)
            {
                for (int x = 0; x < f.Width; x++)
                {
                    if (x == 0 && y == 0)
                        I[y, x] = f.Data[y, x, 0];
                    else if (x == 0)
                        I[y, x] = I[y - 1, x] + f.Data[y, x, 0];
                    else if (y == 0)
                        I[y, x] = I[y, x - 1] + f.Data[y, x, 0];
                    else
                        I[y, x] = I[y, x - 1] + I[y - 1, x] - I[y - 1, x - 1] + f.Data[y, x, 0];
                }
            }
            return I;
        }

        public static Image<Gray, byte> AverageFilter(Image<Gray, byte> inputImage, int h, int w)
        {
            if (inputImage == null)
                throw new ArgumentNullException("inputImage");
            if (h <= 0 || w <= 0 || h % 2 == 0 || w % 2 == 0)
                throw new ArgumentException("Dimensiunile mastii trebuie sa fie pozitive si impare");

            int ry = h / 2;
            int rx = w / 2;
            
            Image<Gray, byte> f = new Image<Gray, byte>(inputImage.Width + 2 * rx, inputImage.Height + 2 * ry);
            CvInvoke.CopyMakeBorder(inputImage, f, ry, ry, rx, rx, Emgu.CV.CvEnum.BorderType.Reflect101);

            long[,] I = IntegralImage(f);
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);
            
            for (int y = ry; y < f.Height - ry; y++)
            {
                for (int x = rx; x < f.Width - rx; x++)
                {
                    int x0 = x - rx;
                    int x1 = x + rx;
                    int y0 = y - ry;
                    int y1 = y + ry;
                    
                    long A = I[y1, x1];
                    long sum;

                    if (x0 == 0 && y0 == 0)
                        sum = A;
                    else if (y0 == 0)
                    {
                        long B = I[y1, x0 - 1];
                        sum = A - B;
                    }
                    else if (x0 == 0)
                    {
                        long D = I[y0 - 1, x1];
                        sum = A - D;
                    }
                    else
                    {
                        long B = I[y1, x0 - 1];
                        long C = I[y0 - 1, x0 - 1];
                        long D = I[y0 - 1, x1];
                        
                        sum = A + C - B - D;
                    }
                    double average = sum / ((double)h * w);
                    result.Data[y - ry, x - rx, 0] = (byte)(average + 0.5);
                }
            }

            f.Dispose();
            return result;
        }
        
        public static Image<Bgr, byte> AverageFilter(Image<Bgr, byte> inputImage, int h, int w)
        {
            if (inputImage == null)
                throw new ArgumentNullException("inputImage");

            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);
            
            for (int c = 0; c < 3; c++)
            {
                Image<Gray, byte> channel = inputImage[c];
                Image<Gray, byte> filteredChannel = AverageFilter(channel, h, w);

                for (int y = 0; y < inputImage.Height; y++)
                {
                    for (int x = 0; x < inputImage.Width; x++)
                    {
                        result.Data[y, x, c] = filteredChannel.Data[y, x, 0];
                    }
                }

                channel.Dispose();
                filteredChannel.Dispose();
            }

            return result;
        }

        public static Image<TColor, TDepth> ApplyFilter<TColor, TDepth>(Image<TColor, TDepth> inputImage, double[,] mask)
            where TColor : struct, IColor
            where TDepth : new()
        {
            Image<TColor, TDepth> resultImage = new Image<TColor, TDepth>(inputImage.Size);
            int h = mask.GetLength(0);
            int w = mask.GetLength(1);
            int h_offset = h / 2, w_offset = w / 2;
            var borderImage = new Image<TColor, TDepth>(inputImage.Width + 2 * w_offset, inputImage.Height + 2 * h_offset);
            CvInvoke.CopyMakeBorder(inputImage, borderImage, h_offset, h_offset, w_offset, w_offset, Emgu.CV.CvEnum.BorderType.Reflect101);
           
            int channels;
            if (typeof(TColor) == typeof(Gray))
                channels = 1;
            else
                channels = 3;

            
            for (int y = h_offset; y < borderImage.Height - h_offset; ++y)
            for (int x = w_offset; x < borderImage.Width - w_offset; ++x)
            for (int channel = 0; channel < channels; ++channel)
            {
                double sum = 0.0;
                for (int i = -h_offset; i <= h_offset; i++)
                for (int j = -w_offset; j <= w_offset; j++)
                {
                    double pixel = Convert.ToDouble(borderImage.Data[y + i, x + j, channel]);
                    sum += mask[i + h_offset, j + w_offset] * pixel;
                }
                if (typeof(TDepth) == typeof(byte))
                {
                    byte clampedSum = (byte)Math.Max(0, Math.Min(255, sum + 0.5));
                    resultImage.Data[y - h_offset, x - w_offset, channel] = (TDepth)Convert.ChangeType(clampedSum, typeof(TDepth));
                }
                else
                {
                    resultImage.Data[y - h_offset, x - w_offset, channel] = (TDepth)Convert.ChangeType(sum, typeof(TDepth));
                }
            }
            return resultImage;
        }
        
        public static double[,] GaussMaskX(double sigma)
        {
            int l = (int)Math.Ceiling(4 * sigma);
            if (l % 2 == 0)
                l++;

            int r = l / 2;
            double[,] mask = new double[1, l];
            double sum = 0.0;

            for (int z = -r; z <= r; z++)
            {
                double value = Math.Exp(-(z * z) / (2 * sigma * sigma));
                mask[0, z + r] = value;
                sum += value;
            }

            for (int x = 0; x < l; x++)
                mask[0, x] /= sum;

            return mask;
        }

        public static double[,] GaussMaskY(double sigma)
        {
            int l = (int)Math.Ceiling(4 * sigma);
            if (l % 2 == 0)
                l++;

            int r = l / 2;
            double[,] mask = new double[l, 1];
            double sum = 0.0;

            for (int z = -r; z <= r; z++)
            {
                double value = Math.Exp(-(z * z) / (2 * sigma * sigma));
                mask[z + r, 0] = value;
                sum += value;
            }

            for (int y = 0; y < l; y++)
                mask[y, 0] /= sum;

            return mask;
        }

        public static Image<TColor, TDepth> GaussianFilter<TColor, TDepth>(
            Image<TColor, TDepth> inputImage,
            double sigma)
            where TColor : struct, IColor
            where TDepth : new()
        {
            return GaussianFilter(inputImage, sigma, sigma);
        }

        public static Image<TColor, TDepth> GaussianFilter<TColor, TDepth>(
            Image<TColor, TDepth> inputImage,
            double sigmaX,
            double sigmaY)
            where TColor : struct, IColor
            where TDepth : new()
        {
            double[,] maskX = GaussMaskX(sigmaX);
            double[,] maskY = GaussMaskY(sigmaY);

            using var resultX = ApplyFilter(inputImage, maskX);
            return ApplyFilter(resultX, maskY);
        }
        
    }
}
