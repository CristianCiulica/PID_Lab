using Emgu.CV;
using Emgu.CV.Structure;

namespace Algorithms.Tools
{
    public class Tools
    {
        #region Copy
        public static Image<Gray, byte> Copy(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = inputImage.Clone();
            return result;
        }

        public static Image<Bgr, byte> Copy(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = inputImage.Clone();
            return result;
        }
        #endregion

        #region Invert
        public static Image<Gray, byte> Invert(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[y, x, 0] = (byte)(255 - inputImage.Data[y, x, 0]);
                }
            }

            return result;
        }

        public static Image<Bgr, byte> Invert(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[y, x, 0] =
                        (byte)(255 - inputImage.Data[y, x, 0]);
                    result.Data[y, x, 1] =
                        (byte)(255 - inputImage.Data[y, x, 1]);
                    result.Data[y, x, 2] =
                        (byte)(255 - inputImage.Data[y, x, 2]);
                }
            }

            return result;
        }
        #endregion

        #region Binarization
        public static Image<Gray, byte> Binarization(
            Image<Gray, byte> inputImage)
        {
            int T = 100;
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    if (inputImage.Data[y, x, 0] > T)
                        result.Data[y, x, 0] = 255;
                    else
                        result.Data[y, x, 0] = 0;
                }
            }

            return result;
        }

        public static Image<Gray, byte> Binarization(
            Image<Bgr, byte> inputImage)
        {
            using var grayImage = Convert(inputImage);
            return Binarization(grayImage);
        }
        #endregion
        
        #region Mirror
        public static Image<Gray, byte> Mirroring(Image<Gray, byte> inputImage)
        {
            Image<Gray, byte> result = new Image<Gray, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[y, x, 0] = inputImage.Data[y, inputImage.Width - 1 - x, 0];
                }
            }

            return result;
        }

        public static Image<Bgr, byte> Mirroring(Image<Bgr, byte> inputImage)
        {
            Image<Bgr, byte> result = new Image<Bgr, byte>(inputImage.Size);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    result.Data[y, x, 0] = inputImage.Data[y, inputImage.Width - 1 - x, 0];
                    result.Data[y, x, 1] = inputImage.Data[y, inputImage.Width - 1 - x, 1];
                    result.Data[y, x, 2] = inputImage.Data[y, inputImage.Width - 1 - x, 2];
                }
            }

            return result;
        }
        #endregion

        #region Convert color image to grayscale image
        public static Image<Gray, byte> Convert(
            Image<Bgr, byte> inputImage)
        {
            Image<Gray, byte> result =
                inputImage.Convert<Gray, byte>();

            return result;
        }
        #endregion
    }
}