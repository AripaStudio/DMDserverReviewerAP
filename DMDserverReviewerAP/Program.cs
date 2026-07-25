// See https://aka.ms/new-console-template for more information

using DMDserverReviewerAP;
using System;
using System.Diagnostics;
using System.Timers;
using Timer = System.Timers.Timer;

HashSet<int> processIdsHashSet = new HashSet<int>();
long MaxSizeMB = 1024;
long MaxSizeInWorkingTime = MaxSizeMB + 1024;
bool DMDserverExists = true;
byte ShowMessages = 0;


// CPU Usage % = (cpuUsedMs / (totalMsPassed * ProcessorCount)) * 100
async Task<double> GetCpuUsage(Process process)
{

    TimeSpan startCpuTime = TimeSpan.Zero;
    DateTime startTime = new DateTime();

    TimeSpan endCpuTime = TimeSpan.Zero;
    DateTime endTime = new DateTime();

    try
    {

        startCpuTime = process.TotalProcessorTime;
        startTime = DateTime.UtcNow;

        await Task.Delay(1000);

        process.Refresh();

        endCpuTime = process.TotalProcessorTime;
        endTime = DateTime.UtcNow;
    }
    catch (Exception e)
    {
        Console.WriteLine("Error : " + e.Message);
        ShowMessages += 1;
        return 0;
    }


    double cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
    double totalMsPassed = (endTime - startTime).TotalMilliseconds;

    double getDenominator = Environment.ProcessorCount * totalMsPassed;

    return (cpuUsedMs / getDenominator) * 100;


}

var mb = MaxSizeMB;

async Task ReviewDMD()
{


    var getByName = Process.GetProcessesByName("dmdserver");


    int currentLeft = Console.CursorLeft;
    int currentTop = Console.CursorTop;

    if (getByName.Length == 0)
    {
        UserInterfaceManager.DrawHeader(mb, false);
        DMDserverExists = false;
        return;
    }
    UserInterfaceManager.DrawHeader(mb);
    DMDserverExists = true;

    int index = 0;
    foreach (Process process in getByName)
    {

        int pid = process.Id;
        string processName = process.ProcessName;


        if (processIdsHashSet.Contains(pid))
        {
            await CheckMemoryLimit(process, index);
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
                UserInterfaceManager.DrawMessages($"Process {pid} has exited.", ConsoleColor.DarkCyan, getByName.Length);
                UserInterfaceManager.DrawMessages($"Process name : {processName} has exited.", ConsoleColor.DarkCyan, getByName.Length);
                ShowMessages += 1;
            };
        }
        catch (Exception exception)
        {
            UserInterfaceManager.DrawMessages($"err = {exception.Message}", ConsoleColor.DarkRed, getByName.Length);
            ShowMessages += 1;
        }

        await CheckMemoryLimit(process, index);
        index++;



    }
    UserInterfaceManager.DrawFooter(getByName.Length);

    Console.SetCursorPosition(currentLeft, currentTop);

    async Task CheckMemoryLimit(Process process, int idx)
    {   

        try
        {
            if (process.HasExited)
            {
                processIdsHashSet.Remove(process.Id);
                return;
            }

            long workingSetMB = process.WorkingSet64 / (1024 * 1024);
            var getCpuUsage = await GetCpuUsage(process);
            if (process.HasExited)
            {
                processIdsHashSet.Remove(process.Id);
                return;
            }

            UserInterfaceManager.Create(process.ProcessName, process.Id, idx, workingSetMB, getCpuUsage);


            if (workingSetMB > mb)
            {
                if (getCpuUsage > 10 && workingSetMB < MaxSizeInWorkingTime)
                {
                    return;
                }

                process.Kill();



            }
        }
        catch (InvalidOperationException)
        {
            processIdsHashSet.Remove(process.Id);
        }
        catch (Exception e)
        {
            UserInterfaceManager.DrawMessages($"err in CheckMemoryLimit = {e.Message}", ConsoleColor.DarkRed, getByName.Length);
            ShowMessages += 1;
        }

    }

}



void ChangeMaxSize(long input)
{
    MaxSizeMB = input;
    MaxSizeInWorkingTime = MaxSizeMB + 1024;
}


Console.Clear();
UserInterfaceManager.DrawHeader(MaxSizeMB);

_ = Task.Run(async () =>
{
    while (true)
    {
        if (ShowMessages > 10)
        {
            Console.Clear();
            ShowMessages = 0;
            Console.SetCursorPosition(0, 18);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("==================================================================");
            Console.WriteLine("Command: enter 'exit' to close, or a number to change limit:      ");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("> ");
            Console.ResetColor();

        }


        await ReviewDMD();
        await Task.Delay(DMDserverExists ? 1000 : 5000);
    }
});

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
    string? a = Console.ReadLine();

    Console.SetCursorPosition(2, 20);
    Console.Write(new string(' ', Console.WindowWidth - 2));
    if (a == "exit")
    {
        return;
    }
    if (long.TryParse(a, out long newLimit) && newLimit > 0)
    {
        ChangeMaxSize(newLimit);

        ShowMessages += 15;
    }
    else if (!string.IsNullOrWhiteSpace(a))
    {
        Console.SetCursorPosition(2, 21);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("Invalid command or limit number!");
        Console.ResetColor();

        ShowMessages += 15;
    }
}
