// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ThrottledStream.csThrottledStream.cs032320233:30 AM


using System.Timers;
using Timer = System.Timers.Timer;

namespace ScraperOne.Modules.Downloaders;

public class ThrottledStream : Stream
{
    private static long s_processed;
    private readonly Stream i_parent;
    private readonly Timer i_resettimer;
    private readonly AutoResetEvent i_wh = new(true);
    private long i_maxBytesPerSecond;


    /// <summary>
    ///     Creates a new Stream with Databandwith cap.
    /// </summary>
    /// <param name="parentStream"></param>
    /// <param name="maxBytesPerSecond"></param>
    public ThrottledStream(Stream parentStream, long maxBytesPerSecond = long.MaxValue)
    {
        SetMaxBytesPerSecond(maxBytesPerSecond);
        i_parent = parentStream;
        s_processed = 0;
        i_resettimer = new Timer { Interval = 1000 };
        i_resettimer.Elapsed += resettimer_Elapsed;
        i_resettimer.Start();
    }


    public override bool CanRead => i_parent.CanRead;

    public override bool CanSeek => i_parent.CanSeek;

    public override bool CanWrite => i_parent.CanWrite;

    public override long Length => i_parent.Length;


    public override long Position
    {
        get => i_parent.Position;
        set => i_parent.Position = value;
    }

    private void resettimer_Elapsed(object sender, ElapsedEventArgs e)
    {
        s_processed = 0;
        _ = i_wh.Set();
    }


    public override void Close()
    {
        i_resettimer.Stop();
        i_resettimer.Close();
        base.Close();
    }


    public override void Flush()
    {
        i_parent.Flush();
    }


    public override int Read(byte[] buffer, int offset, int count)
    {
        Throttle(count);
        return i_parent.Read(buffer, offset, count);
    }


    public override long Seek(long offset, SeekOrigin origin)
    {
        return i_parent.Seek(offset, origin);
    }


    public override void SetLength(long value)
    {
        i_parent.SetLength(value);
    }


    public override void Write(byte[] buffer, int offset, int count)
    {
        Throttle(count);
        i_parent.Write(buffer, offset, count);
    }


    /// <summary>
    ///     Number of Bytes that are allowed per second.
    /// </summary>
    private long GetMaxBytesPerSecond()
    {
        return i_maxBytesPerSecond;
    }


    /// <summary>
    ///     Number of Bytes that are allowed per second.
    /// </summary>
    private void SetMaxBytesPerSecond(long value)
    {
        if (value < 1)
        {
            // throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.MaxBytePerSecond));
        }

        i_maxBytesPerSecond = value;
    }


    private void Throttle(long bytes)
    {
        try
        {
            s_processed += bytes;
            if (s_processed >= i_maxBytesPerSecond) _ = i_wh.WaitOne();
        }
        catch
        {
        }
    }


    protected override void Dispose(bool disposing)
    {
        i_resettimer.Dispose();
        base.Dispose(disposing);
    }
}