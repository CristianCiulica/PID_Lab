using System;
using Emgu.CV;
using Emgu.CV.Structure;

namespace Algorithms.Sections
{
    public class Filters
    {
        public static Image<Gray, byte> ApplyFilter(Image<Gray, byte> image, double[,] filter)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(image.Size);
            int height = filter.GetLength(0) / 2;
            int width = filter.GetLength(1) / 2;
            
            Image<Gray, byte> paddedImage = new Image<Gray, byte>(image.Width + 2 * width, image.Height + 2 * height);
            CvInvoke.CopyMakeBorder(image, paddedImage, height, height, width, width, Emgu.CV.CvEnum.BorderType.Reflect101);
            for(int y=height; y<paddedImage.Height-height; y++)
            {
                for(int x=width; x<paddedImage.Width-width; x++)
                {
                    double sum = 0;

                    for(int i=-height; i<=height; i++)
                    {
                        for(int j=-width; j<=width; j++)
                        {
                            sum += paddedImage.Data[y+i, x+j, 0] * filter[i+height, j+width];
                        }
                    }

                    int value = (int)Math.Round(sum);
                    result.Data[y - height, x - width, 0] = (byte)Math.Clamp(value, 0, 255);
                }
            }
            return result;
        }
    }
}
