using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace YugiohCardManager
{
    static class Program
    {
        private static Thread? consoleThread;
        private static bool consoleVisible = false;
        private static object consoleLock = new object();

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;

        public static void ToggleConsole()
        {
            lock (consoleLock)
            {
                if (consoleVisible)
                {
                    FreeConsole();
                }
                else
                {
                    AllocConsole();
                    IntPtr consoleWindow = GetConsoleWindow();
                    ShowWindow(consoleWindow, SW_SHOW);
                }
                consoleVisible = !consoleVisible;
            }
        }
    }
}
