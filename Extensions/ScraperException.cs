// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ScraperException.csScraperException.cs0323202311:13 PM

#nullable enable
// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ScraperException.csScraperException.cs032320232:14 AM


using System.Runtime.Serialization;

namespace Scraper;

[Serializable]
internal class ScraperException : Exception
{
    public ScraperException()
    {
    }


    public ScraperException(string? message) : base(message)
    {
    }


    public ScraperException(string? message, Exception? innerException) : base(message, innerException)
    {
    }


    protected ScraperException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}