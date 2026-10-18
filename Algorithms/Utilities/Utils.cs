using System.Collections.Generic;
using Emgu.CV;
using Emgu.CV.Structure;
namespace Algorithms.Utilities
{
    public class Utils
    {
        #region Compute histogram
        public static int[] ComputeHistogram(Image<Gray, byte> inputImage)
        {
            int[] histogram = new int[256];

            for (int y = 0; y < inputImage.Height; ++y)
            {
                for (int x = 0; x < inputImage.Width; ++x)
                {
                    ++histogram[inputImage.Data[y, x, 0]];
                }
            }

            return histogram;
        }
        #endregion
        
        #region Compute normalized histogram
        public static double[] ComputeNormalizedHistogram(Image<Gray, byte> inputImage)
        {
            int[] histogram = ComputeHistogram(inputImage);
            double[] normalizedHistogram = new double[256];
            int totalPixels = inputImage.Width * inputImage.Height;
            
            for (int i = 0; i < 256; i++)
                normalizedHistogram[i] = (double)histogram[i] / totalPixels;
            
            return normalizedHistogram;
        }
        
        
        public static List<double[]> ComputeNormalizedHistogram(Image<Bgr, byte> inputImage)
        {
            List<double[]> normalizedHistograms = new List<double[]>();
            for (int c = 0; c < 3; c++)
            {
                using (Image<Gray, byte> channelImage = inputImage[c])
                {
                    normalizedHistograms.Add(ComputeNormalizedHistogram(channelImage));
                }
            }
            return normalizedHistograms;
        }
        
        #endregion
    }
    
}