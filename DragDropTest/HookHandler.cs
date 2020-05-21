using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace DragDropTest
{
    public class KeyboardListener : IDisposable
    {
        private static IntPtr hookId = IntPtr.Zero;
        private InterceptKeys.LowLevelKeyboardProc proc;//

        [MethodImpl(MethodImplOptions.NoInlining)]
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                return HookCallbackInner(nCode, wParam, lParam);
            }
            catch
            {
                Console.WriteLine("There was some error somewhere...");
            }
            return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        private IntPtr HookCallbackInner(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                if (wParam == (IntPtr)InterceptKeys.WM_KEYDOWN)                   
                    KeyDown?.Invoke(this, new RawKeyEventArgs(vkCode, false));

                else if (wParam == (IntPtr)InterceptKeys.WM_KEYUP)        
                    KeyUp?.Invoke(this, new RawKeyEventArgs(vkCode, false));           
            }
            return InterceptKeys.CallNextHookEx(hookId, nCode, wParam, lParam);
        }

        public event RawKeyEventHandler KeyDown;
        public event RawKeyEventHandler KeyUp;

        public KeyboardListener()
        {
            proc = HookCallback;//
            hookId = InterceptKeys.SetHook(proc);
            //hookId = InterceptKeys.SetHook((InterceptKeys.LowLevelKeyboardProc)HookCallback);
        }
        ~KeyboardListener()
        {
            Dispose();
        }

        public void Dispose()
        {
            InterceptKeys.UnhookWindowsHookEx(hookId);
        }
    }

    internal static class InterceptKeys
    {
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        public static int WH_KEYBOARD_LL = 13;
        public static int WM_KEYDOWN = 0x0100;
        public static int WM_KEYUP = 0x0101;

        public static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
    }

    public class RawKeyEventArgs : EventArgs
    {
        public int VKCode;
        public Key Key;
        public bool IsSysKey;

        public RawKeyEventArgs(int VKCode, bool isSysKey)
        {
            this.VKCode = VKCode;
            this.IsSysKey = isSysKey;
            this.Key = System.Windows.Input.KeyInterop.KeyFromVirtualKey(VKCode);
        }
    }

    public delegate void RawKeyEventHandler(object sender, RawKeyEventArgs args);

    //public class HookHandler
    //{

    //    private const int WH_KEYBOARD_LL = 13;
    //    private const int WM_KEYDOWN = 0x0100;
    //    public static HookProc proc = HookCallback;
    //    public static IntPtr hook = IntPtr.Zero;

    //    public HookHandler(Application app)
    //    {
    //        hook = SetHook(proc);
    //        app.Run();
    //        UnhookWindowsHookEx(hook);
    //    }
    //    //public static void Main()
    //    //{
    //    //    hook = SetHook(proc);
    //    //    // Application.Run();
    //    //    UnhookWindowsHookEx(hook);
    //    //}
    //    public static IntPtr SetHook(HookProc proc)
    //    {
    //        using (Process curProcess = Process.GetCurrentProcess())
    //        using (ProcessModule curModule = curProcess.MainModule)
    //        {
    //            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
    //        }
    //    }
    //    public delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);

    //    private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    //    {
    //        if ((nCode >= 0) && (wParam == (IntPtr)WM_KEYDOWN))
    //        {
    //            int vkCode = Marshal.ReadInt32(lParam);
    //            if (((ConsoleKey)vkCode == ConsoleKey.LeftWindows) || ((ConsoleKey)vkCode == ConsoleKey.RightWindows))
    //            {
    //                Console.WriteLine("{0} blocked!", (ConsoleKey)vkCode);
    //                return (IntPtr)1;
    //            }
    //            Console.WriteLine((ConsoleKey)vkCode);
    //        }
    //        return CallNextHookEx(hook, nCode, wParam, lParam);
    //    }

    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    public static extern bool UnhookWindowsHookEx(IntPtr hhk);
    //    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
    //    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    //    private static extern IntPtr GetModuleHandle(string lpModuleName);
    //}
}
