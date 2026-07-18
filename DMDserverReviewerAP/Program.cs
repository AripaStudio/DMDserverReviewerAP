// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

long MaxSizeMB = 1024;
long MaxSizeInWorkingTime = MaxSizeMB + 1024;
bool DMDserverExists = true;

// CPU Usage % = (cpuUsedMs / (totalMsPassed * ProcessorCount)) * 100
double GetCpuUsage(Process process)
{

    TimeSpan startCpuTime = TimeSpan.Zero;
    DateTime startTime = new DateTime();

    TimeSpan endCpuTime = TimeSpan.Zero;
    DateTime endTime = new DateTime();

    try
    {

        startCpuTime = process.TotalProcessorTime;
        startTime = DateTime.UtcNow;

        Thread.Sleep(1000);

        process.Refresh();

        endCpuTime = process.TotalProcessorTime;
        endTime = DateTime.UtcNow;
    }
    catch (Exception e)
    {
        Console.WriteLine("Error : " + e);
        return 0;
    }


    double cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
    double totalMsPassed = (endTime - startTime).TotalMilliseconds;

    double getDenominator = Environment.ProcessorCount * totalMsPassed;

    return (cpuUsedMs / getDenominator) * 100;


}
void ReviewDMD()
{

    var getByName = Process.GetProcessesByName("dmdserver");
    if (getByName.Length == 0)
    {
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("dmd server not found.");
        Console.ResetColor();
        DMDserverExists = false;
        return;
    }
    DMDserverExists = true;
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("dmd server is Run.");
    Console.ResetColor();
    foreach (Process process in getByName)
    {
        using (process)
        {
            if (process.HasExited) continue;

            long workingSetMB = process.WorkingSet64 / (1024 * 1024);
            var getCpuUsage = GetCpuUsage(process);

            if (process.HasExited) continue;

            if (workingSetMB > MaxSizeMB)
            {
                if (getCpuUsage > 10 && workingSetMB < MaxSizeInWorkingTime)
                {

                    continue;
                }
                process.Kill();
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Killed due to high memory allocation and low CPU usage.");
                Console.ResetColor();


            }
        }
    }




}

void ChangeMaxSize(long input)
{
    MaxSizeMB = input;
    MaxSizeInWorkingTime = MaxSizeMB + 1024;
}

Timer timer = new Timer();
timer.AutoReset = true;
timer.Interval = 1000;
timer.Elapsed += TimerOnElapsed;
timer.Start();

void TimerOnElapsed(object? sender, ElapsedEventArgs e)
{
    timer.Stop();
    ReviewDMD();
    if (!DMDserverExists)
    {
        timer.Interval = 5000;
    }
    else
    {
        timer.Interval = 1000;
    }
    timer.Start();
}

Console.WriteLine("enter a number for change max size limit");
var ChangeMaxSizeLimit = Console.ReadLine();
if (ChangeMaxSizeLimit != null)
{
    try
    {
        if (Convert.ToInt32(ChangeMaxSizeLimit) != 0)
        {
            ChangeMaxSize(Convert.ToInt32(ChangeMaxSizeLimit));
        }
    }
    catch (Exception e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("input not valid.");
        Console.ResetColor();
    }
}

Console.WriteLine($"Max size limit changed to {MaxSizeMB} MB. Watchdog is running...");
while (true)
{
    Console.WriteLine("enter a number for change max size limit or for exit (enter 'exit') ");

    var a = Console.ReadLine();
    if (a == "exit")
    {
        return 1;
    }
}