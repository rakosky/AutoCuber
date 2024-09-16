using GMSMacro;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Tesseract;

namespace AutoCuber
{
    public static class ImageHelpers
    {
        public static Point? FindImageCoords(Bitmap srcImg, Bitmap targetImg, double tolerance = .1)
        {

            var coords = SearchBitmap(srcImg, targetImg, tolerance);
            if (coords != Rectangle.Empty)
                return new Point(coords.X, coords.Y);

            return null;
        }

        public static async Task<Point?> FindOneImageCoordsInProcAsync(IntPtr handle, Bitmap[] targetImgs, double tolerance = .1, int timeout = 10000) 
        {
            var tasks = targetImgs.Select(img => ImageHelpers.FindImageCoordsInProcAsync(handle, img, tolerance, timeout));
            var completedTask = await Task.WhenAny(tasks);
            return await completedTask;
        }

        public static async Task<Point?> FindImageCoordsInProcAsync(IntPtr handle, Bitmap origTargetImg, double tolerance = .1, int timeout = 10000)
        {
            var targetImg = (Bitmap)origTargetImg.Clone();

            var sw = new Stopwatch();
            sw.Start();
            var retryDelay = 100;

            while (sw.ElapsedMilliseconds < timeout)
            {
                var srcImg = ScreenCapture.CaptureWindow(handle);
                var found = FindImageCoords(srcImg, targetImg, tolerance);

                if (found is not null)
                    return await Task.FromResult(found);

                await Task.Delay(retryDelay);
            }
            return null;
        }

        private static Rectangle SearchBitmap(Bitmap bigBmp, Bitmap smallBmp, double tolerance)
        {
            BitmapData smallData =
                smallBmp.LockBits(new Rectangle(0, 0, smallBmp.Width, smallBmp.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format24bppRgb);
            BitmapData bigData =
                bigBmp.LockBits(new Rectangle(0, 0, bigBmp.Width, bigBmp.Height),
                        ImageLockMode.ReadOnly,
                        PixelFormat.Format24bppRgb);
            try
            {
                int smallStride = smallData.Stride;
                int bigStride = bigData.Stride;

                int bigWidth = bigBmp.Width;
                int bigHeight = bigBmp.Height - smallBmp.Height + 1;
                int smallWidth = smallBmp.Width * 3;
                int smallHeight = smallBmp.Height;

                Rectangle location = Rectangle.Empty;
                int margin = Convert.ToInt32(255.0 * tolerance);

                unsafe
                {
                    byte* pSmall = (byte*)(void*)smallData.Scan0;
                    byte* pBig = (byte*)(void*)bigData.Scan0;

                    int smallOffset = smallStride - smallBmp.Width * 3;
                    int bigOffset = bigStride - bigBmp.Width * 3;

                    bool matchFound = true;

                    for (int y = 0; y < bigHeight; y++)
                    {
                        for (int x = 0; x < bigWidth; x++)
                        {
                            byte* pBigBackup = pBig;
                            byte* pSmallBackup = pSmall;

                            //Look for the small picture.
                            for (int i = 0; i < smallHeight; i++)
                            {
                                int j = 0;
                                matchFound = true;
                                for (j = 0; j < smallWidth; j++)
                                {
                                    //With tolerance: pSmall value should be between margins.
                                    int inf = pBig[0] - margin;
                                    int sup = pBig[0] + margin;
                                    if (sup < pSmall[0] || inf > pSmall[0])
                                    {
                                        matchFound = false;
                                        break;
                                    }

                                    pBig++;
                                    pSmall++;
                                }

                                if (!matchFound) break;

                                //We restore the pointers.
                                pSmall = pSmallBackup;
                                pBig = pBigBackup;

                                //Next rows of the small and big pictures.
                                pSmall += smallStride * (1 + i);
                                pBig += bigStride * (1 + i);
                            }

                            //If match found, we return.
                            if (matchFound)
                            {
                                location.X = x;
                                location.Y = y;
                                location.Width = smallBmp.Width;
                                location.Height = smallBmp.Height;
                                break;
                            }
                            //If no match found, we restore the pointers and continue.
                            else
                            {
                                pBig = pBigBackup;
                                pSmall = pSmallBackup;
                                pBig += 3;
                            }
                        }

                        if (matchFound) break;

                        pBig += bigOffset;
                    }
                }
                return location;
            }
            finally
            {
                bigBmp.UnlockBits(bigData);
                smallBmp.UnlockBits(smallData);

            }
        }
    }
}
