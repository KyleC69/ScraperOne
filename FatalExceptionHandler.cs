// 
// Program: Scraper One
// Author:  Kyle Crowder
// License : Open Source
// Portions of code taken from TumblrThree
// 
// 052023

namespace ScraperOne;



internal class FatalExceptionHandler
{
    internal static void Handle(UnobservedTaskExceptionEventArgs e)
    {
        System.Console.WriteLine(e.Exception.Message);
    }



    internal static void Handle(Exception e)
    {
        Console.WriteLine($"GLOBAL::EXCEPTION--{e.Message}");
        //Console.WriteLine($"GLOBAL::EXCEPTION--{Result}");
        Console.WriteLine($"GLOBAL::EXCEPTION--{e.Message}");
        Console.WriteLine($"GLOBAL::EXCEPTION--{e.Message}");
        Console.WriteLine($"GLOBAL::EXCEPTION--{e.Message}");
    }



    internal static void Handle(object value, Exception huh)
    {
        throw new NotImplementedException();
    }
}