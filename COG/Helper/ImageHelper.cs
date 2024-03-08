using Cognex.VisionPro;
using Emgu.CV;
using Emgu.CV.CvEnum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace COG.Helper
{
    public static class ImageHelper
    {
        public static Mat GetConvertMatImage(CogImage8Grey cogImage)
        {
            IntPtr cogIntptr = GetIntptr(cogImage, out int stride);
            byte[] byteArray = new byte[stride * cogImage.Height];
            Marshal.Copy(cogIntptr, byteArray, 0, byteArray.Length);
            Mat matImage = new Mat(new Size(cogImage.Width, cogImage.Height), DepthType.Cv8U, 1, cogIntptr, stride);

            return matImage;
        }

        public static IntPtr GetIntptr(CogImage8Grey image, out int stride)
        {
            unsafe
            {
                var cogPixelData = image.Get8GreyPixelMemory(CogImageDataModeConstants.Read, 0, 0, image.Width, image.Height);
                IntPtr ptrData = cogPixelData.Scan0;
                stride = cogPixelData.Stride;

                return ptrData;
            }
        }

    }
}
