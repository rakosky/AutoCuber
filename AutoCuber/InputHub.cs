using System.Drawing;
using System.Runtime.InteropServices;
using static AutoCuber.Structs;

namespace AutoCuber
{

    public enum KeyPressType
    {
        PRESS, DOWN, UP
    }
    public static class InputHub
    {
        private static HashSet<ScanCodeShort> PressedKeys = new HashSet<ScanCodeShort>();
        public static void ReleaseAll()
        {
            // Determine currently pressed keys and release them
            Console.WriteLine("Releasing all keys");
            for (int i = 0; i < 256; i++)
            {
                if ((GetAsyncKeyState(i) & 0x8000) != 0)
                {
                    Console.WriteLine($"Releasing key {i}");
                    keybd_event((byte)i, 0, 0x0002, 0);
                }
            }
        }
        public static void SendKey(ScanCodeShort keycode, KeyPressType type = KeyPressType.PRESS)
        {
            INPUT[] Input = new INPUT[1];
            Input[0].type = 1; // 1 = Keyboard Input
            Input[0].U.ki.wScan = keycode;
            switch (type)
            {
                case KeyPressType.DOWN:
                    Input[0].U.ki.dwFlags = KEYEVENTF.SCANCODE;
                    SendInput(1, Input, INPUT.Size);
                    PressedKeys.Add(keycode);
                    break;
                case KeyPressType.UP:
                    Input[0].U.ki.dwFlags = KEYEVENTF.SCANCODE | KEYEVENTF.KEYUP;
                    SendInput(1, Input, INPUT.Size);
                    PressedKeys.Remove(keycode);
                    break;
                case KeyPressType.PRESS:
                    Input[0].U.ki.dwFlags = KEYEVENTF.SCANCODE;
                    SendInput(1, Input, INPUT.Size);
                    PressedKeys.Add(keycode);
                    Input[0].U.ki.dwFlags = KEYEVENTF.SCANCODE | KEYEVENTF.KEYUP;
                    SendInput(1, Input, INPUT.Size);
                    PressedKeys.Remove(keycode);
                    break;
            }
        }

        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref Point lpPoint);
        #region mouse
        public static void ClickOnPoint(Point clientPoint, IntPtr? wndHandle = null, bool rClick = false)
        {
            /// get screen coordinates

            if (wndHandle is not null)
            {
                ClientToScreen(wndHandle.Value, ref clientPoint);
            }
            /// set cursor on coords, and press mouse
            SetCursorPos(clientPoint.X, clientPoint.Y);

            var inputMouseDown = new INPUT();
            inputMouseDown.type = 0; /// input type mouse
            inputMouseDown.U.mi.dwFlags = rClick ? MOUSEEVENTF.RIGHTDOWN : MOUSEEVENTF.LEFTDOWN; /// button down

            var inputMouseUp = new INPUT();
            inputMouseUp.type = 0; /// input type mouse
            inputMouseUp.U.mi.dwFlags = rClick ? MOUSEEVENTF.RIGHTUP : MOUSEEVENTF.LEFTUP; /// button up

            var inputs = new INPUT[] { inputMouseDown, inputMouseUp };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }
        public static void DoubleClickOnPoint(Point clientPoint, IntPtr? wndHandle = null, bool rClick = false)
        {
            ClickOnPoint(clientPoint, wndHandle, rClick);
            Thread.Sleep(150);
            ClickOnPoint(clientPoint, wndHandle, rClick);
        }
        public static void ClickOnPointAndDrag(Point startPoint, Point endPoint, IntPtr? wndHandle = null, bool rClick = false)
        {
            /// get screen coordinates

            if (wndHandle is not null)
            {
                ClientToScreen(wndHandle.Value, ref startPoint);
                ClientToScreen(wndHandle.Value, ref endPoint);
            }
            /// set cursor on coords, and press mouse
            SetCursorPos(startPoint.X, startPoint.Y);

            var inputMouseDown = new INPUT();
            inputMouseDown.type = 0; /// input type mouse
            inputMouseDown.U.mi.dwFlags = rClick ? MOUSEEVENTF.RIGHTDOWN : MOUSEEVENTF.LEFTDOWN; /// left button down

            var downInput = new INPUT[] { inputMouseDown };
            SendInput((uint)downInput.Length, downInput, Marshal.SizeOf(typeof(INPUT)));
            Thread.Sleep(200);
            SetCursorPos(endPoint.X, endPoint.Y);

            var inputMouseUp = new INPUT();
            inputMouseUp.type = 0; /// input type mouse
            inputMouseUp.U.mi.dwFlags = rClick ? MOUSEEVENTF.RIGHTUP : MOUSEEVENTF.LEFTUP; /// left button up
            var upInput = new INPUT[] { inputMouseUp };
            SendInput((uint)upInput.Length, upInput, Marshal.SizeOf(typeof(INPUT)));

        }

        public static void SetCursorPosition(Point clientPoint, IntPtr? wndHandle = null)
        {
            if (wndHandle is not null)
            {
                ClientToScreen(wndHandle.Value, ref clientPoint);
            }
            SetCursorPos(clientPoint.X, clientPoint.Y);
        }

        //public static void MoveCursor(IntPtr wndHandle, Point clientPoint)
        //{
        //    /// get screen coordinates
        //    ClientToScreen(wndHandle, ref clientPoint);
        //    /// set cursor on coords, and press mouse
        //    Cursor.Position = new Point(clientPoint.X, clientPoint.Y);
        //}
        #endregion
    }
}
