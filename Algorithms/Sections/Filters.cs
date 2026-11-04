using System;
using Emgu.CV;
using Emgu.CV.Structure;
using SixLabors.ImageSharp.Formats.Png;

namespace Algorithms.Sections
{
    public class Filters
    {
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
