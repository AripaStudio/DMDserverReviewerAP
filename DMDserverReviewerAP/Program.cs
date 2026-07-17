// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

long MaxSizeMB = 1024;

void ReviewDMD()
{

    var getByName = Process.GetProcessesByName("dmdserver");

    foreach (Process process in getByName)
    {
        using (process)
        {
            long workingSetMB = process.WorkingSet64 / (1024 * 1024);
            if (workingSetMB > MaxSizeMB)
            {
                process.Kill();
                Console.WriteLine("Killed.");
            }
        }
    }



}

void ChangeMaxSize(long input)
{
    MaxSizeMB = input;
}

Timer timer = new Timer();
timer.AutoReset = true;
timer.Interval = 500;
timer.Elapsed += TimerOnElapsed;
timer.Start();

void TimerOnElapsed(object? sender, ElapsedEventArgs e)
{
    timer.Stop();
    ReviewDMD();
    timer.Start();
}

Console.WriteLine("enter a number for change max size limit");
var ChangeMaxSizeLimit = Console.ReadLine();
if (ChangeMaxSizeLimit != null)
{
    if (Convert.ToInt32(ChangeMaxSizeLimit) != 0)
    {
        ChangeMaxSize(Convert.ToInt32(ChangeMaxSizeLimit));
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