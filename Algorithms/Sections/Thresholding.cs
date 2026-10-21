using System;
using Emgu.CV.Structure;
using Emgu.CV;
using Algorithms.Utilities;

namespace Algorithms.Sections
{
    public class Thresholding
    {
        public static int MinErr(Image<Gray, byte> image)
        {
            double[] p = Utils.ComputeNormalizedHistogram(image);

            int T = 0;
            double minErr = 1000;

            for (int t = 0; t <= 255; t++)
            {
                double P1 = 0;
                double P2 = 0;
                double mu1 = 0;
                double mu2 = 0;
                double s1 = 0;
                double s2 = 0;

                for (int k = 0; k <= t; k++)
                {
                    P1 += p[k];
                    mu1 += k * p[k];
                }

                P2 = 1 - P1;
                if (P1 <= 0 || P2 <= 0)
                    continue;

                for (int k = t + 1; k <= 255; k++) 
                    mu2 += k * p[k];
                mu1 /= P1;
                mu2 /= P2;
                
                for (int k = 0; k <= t; k++)
                    s1 += (k - mu1) * (k - mu1) * p[k];

                for (int k = t + 1; k <= 255; k++)
                    s2 += (k - mu2) * (k - mu2) * p[k];
                
                s1 /= P1;
                s2 /= P2;

                if (s1 <= 0 || s2 <= 0)
                    continue;

                double Err = 1 + P1 * Math.Log(s1) + P2 * Math.Log(s2) 
                               - 2 * P1 * Math.Log(P1) - 2 * P2 * Math.Log(P2);

                if (Err < minErr)
                {
                    minErr = Err;
                    T = t;
                }
            }
            return T;
        }
    }
}
