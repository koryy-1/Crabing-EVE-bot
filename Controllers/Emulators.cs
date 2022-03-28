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

        static public void LockTargets(List<int> EnemyCoordsArray)
        {
            while (!AllowControlEmulator)
            {
                System.Threading.Thread.Sleep(20);
            }
            AllowControlEmulator = false;

            WinApi.PostMessage(hWnd, (uint)WinApi.KeyboardMessages.WM_IME_KEYDOWN, (int)WinApi.VirtualKeyShort.VK_CONTROL, 0);
            System.Threading.Thread.Sleep(100);
            for (int i = 0; i < EnemyCoordsArray.Count; i += 2)
            {
                ClickLBForLockTargets(EnemyCoordsArray[i], EnemyCoordsArray[i + 1]);
            }
            WinApi.PostMessage(hWnd, (uint)WinApi.KeyboardMessages.WM_IME_KEYUP, (int)WinApi.VirtualKeyShort.VK_CONTROL, 0);
            System.Threading.Thread.Sleep(100);

            AllowControlEmulator = true;
        }

        static public void ClickLBForLockTargets(int x, int y)
        {
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONDOWN, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONUP, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
        }

        static public void ClickLB(int x, int y)
        {
            while (!AllowControlEmulator)
            {
                System.Threading.Thread.Sleep(20);
            }
            AllowControlEmulator = false;

            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONDOWN, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONUP, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);

            AllowControlEmulator = true;
        }

        static public void ClickRB(int x, int y)
        {
            while (!AllowControlEmulator)
            {
                System.Threading.Thread.Sleep(20);
            }
            AllowControlEmulator = false;

            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_RBUTTONDOWN, (int)WinApi.VirtualKeyShort.RBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_RBUTTONUP, (int)WinApi.VirtualKeyShort.RBUTTON, WinApi.MakeLParam(x, y));
            System.Threading.Thread.Sleep(100);

            AllowControlEmulator = true;
        }

        static public void Drag(int DownX, int DownY, int UpX, int UpY)
        {
            while (!AllowControlEmulator)
            {
                System.Threading.Thread.Sleep(20);
            }
            AllowControlEmulator = false;

            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(DownX, DownY));
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONDOWN, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(DownX, DownY));
            System.Threading.Thread.Sleep(100);

            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_MOUSEMOVE, 0, WinApi.MakeLParam(UpX, UpY));
            System.Threading.Thread.Sleep(500);
            WinApi.PostMessage(hWnd, (uint)WinApi.MouseMessages.WM_LBUTTONUP, (int)WinApi.VirtualKeyShort.LBUTTON, WinApi.MakeLParam(UpX, UpY));
            System.Threading.Thread.Sleep(100);

            AllowControlEmulator = true;
        }

        static public void PressButton(int Button)
        {
            while (!AllowControlEmulator)
            {
                System.Threading.Thread.Sleep(20);
            }
            AllowControlEmulator = false;

            WinApi.PostMessage(hWnd, (uint)WinApi.KeyboardMessages.WM_IME_KEYDOWN, Button, 0);
            System.Threading.Thread.Sleep(100);
            WinApi.PostMessage(hWnd, (uint)WinApi.KeyboardMessages.WM_IME_KEYUP, Button, 0);
            System.Threading.Thread.Sleep(100);

            AllowControlEmulator = true;
        }
    }
}
