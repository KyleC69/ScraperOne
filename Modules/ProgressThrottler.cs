// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ProgressThrottler.csProgressThrottler.cs032320233:30 AM


using System.Timers;
using Timer = System.Timers.Timer;

namespace ScraperOne.Modules;

public class ProgressThrottler<T> : IProgress<T>
{
    private readonly IProgress<T> i_progress;
    private bool i_reportProgressAfterThrottling = true;


    public ProgressThrottler(IProgress<T> progress, double interval)
    {
        var resetTimer = new Timer { Interval = interval };
        resetTimer.Elapsed += resetTimer_Elapsed;
        resetTimer.Start();

        i_progress = progress ?? throw new ArgumentNullException(nameof(progress));
    }


    public void Report(T value)
    {
        if (!i_reportProgressAfterThrottling) return;

        i_progress.Report(value);
        i_reportProgressAfterThrottling = false;
    }

    private void resetTimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        i_reportProgressAfterThrottling = true;
    }
}