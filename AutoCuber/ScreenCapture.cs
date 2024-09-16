using System;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using Rectangle = System.Drawing.Rectangle;
using System.Dynamic;
using System.Windows.Forms;

namespace GMSMacro
{

    public static class ScreenCapture
    {
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);
        /// <summary>
        /// Helper class containing Gdi32 API functions
        /// </summary>
        private class GDI32
        {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        /// <summary>
        /// Helper class containing User32 API functions
        /// </summary>
        private class User32
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT
            {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [StructLayout(LayoutKind.Sequential)]
            public struct POINT
            {
                public int x;
                public int y;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowRect(IntPtr hWnd, ref RECT rect);
            // Import the GetClientRect function
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool GetClientRect(IntPtr hWnd, ref RECT lpRect);
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);
            [DllImport("user32.dll")]
            public static extern int GetSystemMetrics(int nIndex);
        }

        public static Bitmap CaptureScreen()
        {
            // System metric constants for screen width and height
            const int SM_CXSCREEN = 0;
            const int SM_CYSCREEN = 1;

            // Get screen dimensions using system metrics
            int screenWidth = User32.GetSystemMetrics(SM_CXSCREEN);
            int screenHeight = User32.GetSystemMetrics(SM_CYSCREEN);


            // Create a bitmap with the screen dimensions
            Bitmap bitmap = new Bitmap(screenHeight, screenWidth);

            // Create a graphics object from the bitmap
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                // Capture the screen
                g.CopyFromScreen(0, 0, 0, 0, new Size(screenWidth, screenHeight));
            }
            return bitmap;
        }

        public static Bitmap CaptureWindow(IntPtr handle, Rectangle? rect = null)
        {
            // Get the hDC of the target window
            IntPtr hdcSrc = User32.GetWindowDC(handle);
            // Get the size
            User32.RECT windowRect = new User32.RECT();
            User32.GetWindowRect(handle, ref windowRect);

            // Get the size of the client area (the window content)
            var clientRect = GetClientAreaRectangle(handle);
            // Create a device context we can copy to
            IntPtr hdcDest = GDI32.CreateCompatibleDC(hdcSrc);
            // Create a bitmap we can copy to, using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32.CreateCompatibleBitmap(hdcSrc, clientRect.Width, clientRect.Height);
            // Select the bitmap object
            IntPtr hOld = GDI32.SelectObject(hdcDest, hBitmap);
            // BitBlt over
            GDI32.BitBlt(hdcDest, 0, 0, clientRect.Width, clientRect.Height, hdcSrc, clientRect.X-windowRect.left, clientRect.Y-windowRect.top, GDI32.SRCCOPY);
            // Restore selection
            GDI32.SelectObject(hdcDest, hOld);

            // Get a .NET image object for it
            var img = Image.FromHbitmap(hBitmap);
            // Clean up
            GDI32.DeleteObject(hBitmap); // Release the bitmap resource
            GDI32.DeleteDC(hdcDest); // Release the device context
            User32.ReleaseDC(handle, hdcSrc); // Release the device context

            if (rect is not null)
            {
                img = img.Clone(rect.Value, PixelFormat.Format32bppRgb);
            }
            return img;
        }
        private static Rectangle GetClientAreaRectangle(IntPtr handle)
        {
            User32.RECT clientRect = new User32.RECT();
            User32.GetClientRect(handle, ref clientRect);

            User32.POINT upperLeft = new User32.POINT { x = clientRect.left, y = clientRect.top };
            User32.ClientToScreen(handle, ref upperLeft);

            User32.POINT lowerRight = new User32.POINT { x = clientRect.right, y = clientRect.bottom };
            User32.ClientToScreen(handle, ref lowerRight);

            return new Rectangle(upperLeft.x, upperLeft.y, lowerRight.x - upperLeft.x, lowerRight.y - upperLeft.y);
        }
    }
}