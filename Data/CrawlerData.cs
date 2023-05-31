// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// CrawlerData.csCrawlerData.cs032320233:28 AM

namespace ScraperOne.Data;

public interface ICrawlerData<T>
{
    T Data { get; }
    string Filename { get; }
}

public class CrawlerData<T> : ICrawlerData<T>
{
    public CrawlerData(string filename, T data)
    {
        Filename = filename;
        Data = data;
    }


    public T Data { get; protected set; }

    public string Filename { get; protected set; }
}