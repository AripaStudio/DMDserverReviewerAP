using System;
using System.Security.Principal;

namespace DMDserverReviewerAP
{
    public static class UserInterfaceManager
    {

        private static void ClearLine(int top)
        {
            Console.SetCursorPosition(0 , top);
            Console.Write(new string(' ', Console.WindowWidth));
        }
        public static void Create(string nameProcess, int processId, int index, long ramUsage, double cpuUsage)
        {
            int targetLine = 5 + index;

            ClearLine(targetLine);
            Console.SetCursorPosition(0, targetLine);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{processId,-6}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("]");

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"  {nameProcess,-15}");

            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.Write($"  RAM: {ramUsage,4} MB");

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write($"  CPU: {cpuUsage,5:F1} %");

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("  [");
            Console.ForegroundColor = ramUsage > 1024 ? ConsoleColor.Red : ConsoleColor.Green;
            Console.Write(ramUsage > 1024 ? "LIMIT!" : "ACTIVE");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("]");

            Console.Write(new string(' ', 10));
            Console.ResetColor();
        }

        public static void DrawHeader(long maxLimit, bool IsDMDRun = true)
        {
            ClearLine(0);
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("==================================================================");
            ClearLine(1);
            Console.SetCursorPosition(0, 1);
            if (!SecurityAdmin.IsAdministrator())
            {
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                Console.Write("  Please Run Administrate ");
            }
            ClearLine(2);
            Console.SetCursorPosition(0, 2);
            if (!IsDMDRun)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.Write("  DMD not Found! ");
            }

            ClearLine(3);
            Console.SetCursorPosition(0, 3);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"  DMDServer Watchdog Active | Max Limit: {maxLimit} MB");

            ClearLine(4);
            Console.SetCursorPosition(0, 4);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("==================================================================");

            Console.ResetColor();
        }

        public static void DrawFooter(int processCount)
        {
            int footerLine = 5 + processCount;

            ClearLine(footerLine);
            Console.SetCursorPosition(0, footerLine);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.Write("==================================================================");

            ClearLine(footerLine + 1);

            Console.ResetColor();
        }

        public static void DrawMessages(string message , ConsoleColor color, int processCount = 1)
        {
            int messageLine = 22 + processCount;
            ClearLine(messageLine);
            Console.SetCursorPosition(0, messageLine);
            Console.ForegroundColor = color;
            Console.Write($"Message : {message}");
            Console.ResetColor();

        }
    }

    public static class SecurityAdmin
    {
        public static bool IsAdministrator()
        {
            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }

}
