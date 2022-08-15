using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Text;

namespace EVE_Bot.Controllers
{
    static public class Emulators
    {
        static IntPtr hWnd = Window.hWnd;

        volatile static public bool AllowControlEmulator = true;

        volatile static public bool AllowHighLVLControl = false;

        //static public WinApi.Rect windowRect = new WinApi.Rect();


        //volatile static public WinApi.Point LastCoords = new WinApi.Point();
        volatile static public int LastX = 0;
        volatile static public int LastY = 0;

        //static Emulators()
        //{
        //    WinApi.GetWindowRect(hWnd, ref windowRect);
        //    windowRect.left = windowRect.left + 8;
        //    windowRect.top = windowRect.top + 8;
        //    Console.WriteLine($"{windowRect.left} {windowRect.top}");
        //}

        static Random r = new Random();

        static public void LockTargets(List<int> EnemyCoordsArray)
        {
            while (!AllowControlEmulator && !AllowHighLVLControl)
                System.Threading.Thread.Sleep(20);

            AllowControlEmulator = false;

            WinApi.PostMessage(hWnd, (uint)WinApi.KeyboardMessages.WM_IME_KEYDOWN, (int)WinApi.VirtualKeyShort.VK_CONTROL, 0);
            System.Threading.Thread.Sleep(500);
            for (int i = 0; i < EnemyCoordsArray.Count; i += 2)
            {
                ClickLBForLockTargets(EnemyCoordsArray[i], EnemyCoordsArray[i + 1]);
            }
            WinApi.PostMessage(hWnd, (uint)WinApi.KeyboardMessages.WM_IME_KEYUP, (int)WinApi.VirtualKeyShort.VK_CONTROL, 0);
            System.Threading.Thread.Sleep(500);

            AllowControlEmulator = true;
        }

        static public void ClickLBForLockTargets(int x, int y)
        {
            x = x + r.Next(-2, 2);
            y = y + r.Next(-2, 2);
            //WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(x, y));
            Mouse.Move(x, y);
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONDOWN, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONUP, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
        }

        static public void ClickLB(int x, int y)
        {
            x = x + r.Next(-2, 2);
            y = y + r.Next(-2, 2);

            while (!AllowControlEmulator && !AllowHighLVLControl)
                System.Threading.Thread.Sleep(20);

            AllowControlEmulator = false;

            //WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(x, y));

            Mouse.Move(x, y);
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONDOWN, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONUP, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);

            AllowControlEmulator = true;
        }

        static public void ClickRB(int x, int y)
        {
            x = x + r.Next(-2, 2);
            y = y + r.Next(-2, 2);

            while (!AllowControlEmulator && !AllowHighLVLControl)
                System.Threading.Thread.Sleep(20);

            AllowControlEmulator = false;

            //WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(x, y));
            Mouse.Move(x, y);
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_RBUTTONDOWN, (int)WinApi.VirtualKeyShort.RBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_RBUTTONUP, (int)WinApi.VirtualKeyShort.RBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);

            AllowControlEmulator = true;
        }

        static public void Drag(int DownX, int DownY, int UpX, int UpY)
        {
            DownX = DownX + r.Next(-2, 2);
            DownY = DownY + r.Next(-2, 2);

            UpX = UpX + r.Next(-2, 2);
            UpY = UpY + r.Next(-2, 2);

            while (!AllowControlEmulator && !AllowHighLVLControl)
                System.Threading.Thread.Sleep(20);

            AllowControlEmulator = false;

            //WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(DownX, DownY));
            Mouse.Move(DownX, DownY);
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONDOWN, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(DownX, DownY));
            System.Threading.Thread.Sleep(100);

            //WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(UpX, UpY));
            Mouse.Move(UpX, UpY);
            System.Threading.Thread.Sleep(500);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONUP, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(UpX, UpY));
            System.Threading.Thread.Sleep(100);

            AllowControlEmulator = true;
        }

        static public void PressButton(int Button)
        {
            while (!AllowControlEmulator && !AllowHighLVLControl)
                System.Threading.Thread.Sleep(20);

            AllowControlEmulator = false;

            WinApi.PostMessage(hWnd, (uint)WinApi.KeyboardMessages.WM_IME_KEYDOWN, Button, 0);
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.KeyboardMessages.WM_IME_KEYUP, Button, 0);
            System.Threading.Thread.Sleep(100);

            AllowControlEmulator = true;
        }
    }

    public class Point
    {
        public int X;
        public int Y;
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    static public class Mouse
    {
        static int X1;
        static int Y1;

        //static int X2;
        //static int Y2;

        static Random r = new Random();
        static Mouse()
        {
            WinApi.Point LastCoords = new WinApi.Point();
            WinApi.GetCursorPos(out LastCoords);
            Emulators.LastX = LastCoords.x;
            Emulators.LastY = LastCoords.y;
        }
        static public void Move(int x, int y)
        {
            //x = x + Emulators.windowRect.left + r.Next(-2, 2);
            //y = y + Emulators.windowRect.top + r.Next(-2, 2);
            X1 = Emulators.LastX;
            Y1 = Emulators.LastY;

            if (X1 == x && Y1 == y)
                return;

            //// константа по времени
            //var t = 100 + r.Next(-50, 50);

            //double vx = sx / t;
            //double vy = sy / t;

            //константа по скорости
            double v = 130 + r.Next(-2, 2);

            double sx = x - X1;
            double sy = y - Y1;

            double s = Math.Sqrt(Math.Pow(sx, 2) + Math.Pow(sy, 2));

            double t = s / v;

            var vx = sx / t;
            var vy = sy / t;

            double XСutOnScreen = vx; // время кадра = 7
            double XDistanceComplete = 0;

            double YСutOnScreen = vy;
            double YDistanceComplete = 0;


            for (; Math.Abs(XDistanceComplete) < Math.Abs(sx) || Math.Abs(YDistanceComplete) < Math.Abs(sy); XDistanceComplete += XСutOnScreen, YDistanceComplete += YСutOnScreen)
            {
                WinApi.PostMessage(Window.hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(X1 + Convert.ToInt32(XDistanceComplete), Y1 + Convert.ToInt32(YDistanceComplete)));
                //WinApi.SetCursorPos(X1 + Convert.ToInt32(XDistanceComplete), Y1 + Convert.ToInt32(YDistanceComplete));
                //Console.WriteLine($"X {X1 + Convert.ToInt32(XDistanceComplete)} Y {Y1 + Convert.ToInt32(YDistanceComplete)}");
                System.Threading.Thread.Sleep(Convert.ToInt32(t) + r.Next(Convert.ToInt32(-t * 0.1), Convert.ToInt32(t * 0.1)));
            }
            WinApi.PostMessage(Window.hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(x, y));
            //WinApi.SetCursorPos(x, y+23);
            //Console.WriteLine($"X {x} Y {y}");
            System.Threading.Thread.Sleep(Convert.ToInt32(t) + r.Next(Convert.ToInt32(-t * 0.1), Convert.ToInt32(t * 0.1)));
            Emulators.LastX = x;
            Emulators.LastY = y;
        }
    }
}
