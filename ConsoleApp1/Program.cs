﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Runtime.InteropServices;
//using System.Windows.Forms;
using System.Diagnostics;
using System.Windows;

namespace ConsoleApp1
{
    class Program
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static HookProc proc = HookCallback;
        private static IntPtr hook = IntPtr.Zero;

        public static void Main()
        {
            hook = SetHook(proc);
           // Application.Run();
            UnhookWindowsHookEx(hook);
        }
        private static IntPtr SetHook(HookProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if ((nCode >= 0) && (wParam == (IntPtr)WM_KEYDOWN))
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (((ConsoleKey)vkCode == ConsoleKey.LeftWindows) || ((ConsoleKey)vkCode == ConsoleKey.RightWindows))
                {
                    Console.WriteLine("{0} blocked!", (ConsoleKey)vkCode);
                    return (IntPtr)1;
                }
          
                    Console.WriteLine("key={0}", (ConsoleKey)vkCode);
                

             
            }
            return CallNextHookEx(hook, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}


