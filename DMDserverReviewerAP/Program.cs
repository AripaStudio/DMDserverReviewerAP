// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Timers;
using DMDserverReviewerAP;
using Timer = System.Timers.Timer;

HashSet<int> processIdsHashSet = new HashSet<int>();
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

    int currentLeft = Console.CursorLeft;
    int currentTop = Console.CursorTop;

    if (getByName.Length == 0)
    {
        Console.SetCursorPosition(0, 0);
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.WriteLine("dmd server not found.");
        Console.ResetColor();
        DMDserverExists = false;
        return;
    }
    UserInterfaceManager.DrawHeader(MaxSizeMB);
    DMDserverExists = true;

    int index = 0;
    foreach (Process process in getByName)
    {


        int pid = process.Id;
        string processName = process.ProcessName;


        if (processIdsHashSet.Contains(pid))
        {
            CheckMemoryLimit(process, index);
            index++;
            continue;
        }

        processIdsHashSet.Add(pid);
        try
        {
            process.EnableRaisingEvents = true;
            process.Exited += (sender, eventArgs) =>
            {
                processIdsHashSet.Remove(pid);
                Console.WriteLine($"Process {pid} has exited.");
                Console.WriteLine($"Process name : {processName} has exited.");
            };
        }
        catch (Exception exception)
        {
            Console.WriteLine($"err = {exception.Message}");
        }

        CheckMemoryLimit(process, index);
        index++;



    }
    UserInterfaceManager.DrawFooter(getByName.Length);

    Console.SetCursorPosition(currentLeft, currentTop);

    void CheckMemoryLimit(Process process, int idx)
    {

        try
        {
            long workingSetMB = process.WorkingSet64 / (1024 * 1024);
            var getCpuUsage = GetCpuUsage(process);
            UserInterfaceManager.Create(process.ProcessName, process.Id, idx, workingSetMB, getCpuUsage);


            if (workingSetMB > MaxSizeMB)
            {
                if (getCpuUsage > 10 && workingSetMB < MaxSizeInWorkingTime)
                {
                    return;
                }

                process.Kill();



            }
        }
        catch (Exception e)
        { }

    }

}



void ChangeMaxSize(long input)
{
    MaxSizeMB = input;
    MaxSizeInWorkingTime = MaxSizeMB + 1024;
}

Console.Clear();
UserInterfaceManager.DrawHeader(MaxSizeMB);

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

Console.SetCursorPosition(0, 18);
Console.ForegroundColor = ConsoleColor.Gray;
Console.WriteLine("==================================================================");
Console.WriteLine("Command: enter 'exit' to close, or a number to change limit:      ");
Console.ForegroundColor = ConsoleColor.DarkGray;
Console.Write("> ");
Console.ResetColor();
while (true)
{
    Console.SetCursorPosition(2, 20);
    var a = Console.ReadLine();

    Console.SetCursorPosition(2, 20);
    Console.Write(new string(' ', Console.WindowWidth - 3));

    if (a == "exit")
    {
        return;
    }

    if (long.TryParse(a, out long newLimit) && newLimit > 0)
    {
        ChangeMaxSize(newLimit);
        UserInterfaceManager.DrawHeader(MaxSizeMB);

        Console.SetCursorPosition(0, 18);
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("==================================================================");
        Console.WriteLine("Command: enter 'exit' to close, or a number to change limit:      ");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("> ");
        Console.ResetColor();
    }
}
