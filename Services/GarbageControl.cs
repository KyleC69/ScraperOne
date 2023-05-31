






namespace ScraperOne.Services;


public class GarbageControl
{

    public static void Collect()
    {
        GCMemoryInfo info = GC.GetGCMemoryInfo();
        System.Console.Write("Frag Bytes");
        System.Console.WriteLine(info.FragmentedBytes);

        GC.WaitForPendingFinalizers();
        GC.Collect();
        GC.WaitForFullGCComplete();
        Console.WriteLine("Garbage Cleanup is complete;");

    }

    internal static void ThreadProc(object state)
    {
        throw new NotImplementedException();
    }
}