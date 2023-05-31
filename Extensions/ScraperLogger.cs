// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ScraperLogger.csScraperLogger.cs032320233:29 AM


using System.Text;
using Splat;

namespace ScraperOne.Extensions;

public class ScraperLogger : TextWriter
{
    private readonly List<(LogLevel logLevel, string message)> i_logs = new();


    public override Encoding Encoding => throw new NotImplementedException();


    //public override Encoding Encoding => Encoding.UTF8;

    // public ICollection<(LogLevel logLevel, string message)> Logs => _logs;


    public override void WriteLine(string value)
    {
        var colonIndex = value!.IndexOf(":", StringComparison.InvariantCulture);
        _ = (LogLevel)Enum.Parse(typeof(LogLevel), value.AsSpan(0, colonIndex));
        _ = value[(colonIndex + 1)..].Trim();
        base.WriteLine(value);
    }
}