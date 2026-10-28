using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Algorithms.Sections
{
    public class Filters
    {
        public static Image<TColor, TDepth> ApplyFilter<TColor, TDepth>(
            Image<TColor, TDepth> image, double[,] filter)
            where TColor : struct, IColor
            where TDepth : new()
        {
            Image<TColor, TDepth> result = new Image<TColor, TDepth>(image.Size);
            int height = filter.GetLength(0) / 2;
            int width = filter.GetLength(1) / 2;
            int channels = image.Data.GetLength(2);
            
            Image<TColor, TDepth> paddedImage = new Image<TColor, TDepth>(image.Width + 2 * width, image.Height + 2 * height);
            
            CvInvoke.CopyMakeBorder(image, paddedImage, height, height, width, width, Emgu.CV.CvEnum.BorderType.Reflect101);
            for(int y=height; y<paddedImage.Height-height; y++)
            {
                for(int x=width; x<paddedImage.Width-width; x++)
                {
                    for (int c = 0; c < channels; c++)
                    {
                        double sum = 0.0;

                        for(int i=-height; i<=height; i++)
                        {
                            for(int j=-width; j<=width; j++)
                            {
                                double pixel = Convert.ToDouble(paddedImage.Data[y + i, x + j, c]);
                                sum += pixel * filter[i+height, j+width];
                            }
                        }

                        double rounded = Math.Round(sum);
                        if (typeof(TDepth) == typeof(byte))
                            rounded = Math.Clamp(rounded, 0, 255);

                        result.Data[y - height, x - width, c] =
                            (TDepth)Convert.ChangeType(rounded, typeof(TDepth));
                    }
                }
            }
            return result;
        }
    }
}
