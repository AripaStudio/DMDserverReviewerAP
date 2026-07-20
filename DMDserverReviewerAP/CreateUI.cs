using System;

namespace DMDserverReviewerAP
{
    public static class UserInterfaceManager
    {
        public static void Create(string nameProcess, int processId, int index, long ramUsage, double cpuUsage)
        {
            int targetLine = 4 + index;

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

        public static void DrawHeader(long maxLimit)
        {
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("==================================================================");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"  DMDServer Watchdog Active | Max Limit: {maxLimit} MB");
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("==================================================================");
            Console.ResetColor();
        }

        public static void DrawFooter(int processCount)
        {
            Console.SetCursorPosition(0, 4 + processCount);
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine("==================================================================");
            Console.ResetColor();
        }
    }
}
