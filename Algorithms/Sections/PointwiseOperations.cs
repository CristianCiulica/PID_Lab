using Emgu.CV;
using Emgu.CV.Structure;
using System;

namespace Algorithms.Sections
{
    public class PointwiseOperations
    {
        public static Image<Gray, byte> ContrastAndBrightness(Image<Gray, byte> inputImage, double alpha, double beta)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Width, inputImage.Height);
            byte[] lut = new byte[256];

            for (int r = 0; r <= 255; r++)
            {
                double value = alpha * r + beta;
                value = Math.Clamp(value, 0, 255);
                lut[r] = (byte)value;
            }
            
            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    byte value = inputImage.Data[y, x, 0];
                    result.Data[y, x, 0] = lut[value];
                }
            }
            return result;
        }
        
        public static Image<Bgr, byte> ContrastAndBrightness(Image<Bgr, byte> inputImage, double alpha, double beta)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Width, inputImage.Height);
            byte[] lut = new byte[256];

            for (int r = 0; r <= 255; r++)
            {
                double value = alpha * r + beta;
                value = Math.Clamp(value, 0, 255);
                lut[r] = (byte)value;
            }

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        byte value = inputImage.Data[y, x, c];
                        result.Data[y, x, c] = lut[value];
                    }
                }
            }
            return result;
        }
        
        public static Image<Gray, byte> GammaOperator(Image<Gray, byte> inputImage, double gamma)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Width, inputImage.Height);
            byte[] lut = new byte[256];

            for (int r = 0; r <= 255; r++)
            {
                double value = Math.Pow(r / 255.0, gamma) * 255; 
                lut[r] = (byte)value;
            }

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    byte value = inputImage.Data[y, x, 0];
                    result.Data[y, x, 0] = lut[value];
                }
            }
            
            return result;
        }

        public static Image<Bgr, byte> GammaOperator(Image<Bgr, byte> inputImage, double gamma)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Width, inputImage.Height);
            byte[] lut = new byte[256];

            for (int r = 0; r <= 255; r++)
            {
                double value = Math.Pow(r / 255.0, gamma) * 255; 
                lut[r] = (byte)value;
            }
           
            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        byte value = inputImage.Data[y, x, c];
                        result.Data[y, x, c] = lut[value];
                    }
                }
            }
            return result;
        }
    }
}
