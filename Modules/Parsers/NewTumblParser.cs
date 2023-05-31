// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// NewTumblParser.csNewTumblParser.cs032320233:30 AM


using System.Text.RegularExpressions;

namespace ScraperOne.Modules.Parsers;

public class NewTumblParser : INewTumblParser
{
    public Regex GetPhotoUrlRegex()
    {
        return MyRegex1();
    }


    public Regex GetGenericPhotoUrlRegex()
    {
        return MyRegex2();
    }


    public IEnumerable<string> SearchForPhotoUrl(string searchableText)
    {
        var regex = GetPhotoUrlRegex();
        foreach (var match in regex.Matches(searchableText).Cast<Match>())
        {
            var imageUrl = match.Groups[1].Value;
            yield return imageUrl;
        }
    }


    public IEnumerable<string> SearchForGenericPhotoUrl(string searchableText)
    {
        var regex = GetGenericPhotoUrlRegex();
        foreach (var match in regex.Matches(searchableText).Cast<Match>())
        {
            var imageUrl = match.Groups[1].Value;
            yield return imageUrl;
        }
    }


    public bool IsNewTumblUrl(string url)
    {
        var regex = MyRegex();
        return regex.IsMatch(url);
    }


    private static Regex MyRegex()
    {
        return new Regex("/nT_[\\w]*");
    }


    private static Regex MyRegex1()
    {
        return new Regex("\"(http[A-Za-z0-9_/:.]*newtumbl\\.com[A-Za-z0-9_/:.-]*(?<!_150)\\.(jpg|jpeg|png|gif))\"");
    }


    private static Regex MyRegex2()
    {
        return new Regex(
            "\"(https?://(?:[a-z0-9\\-]+\\.)+[a-z]{2,6}(?:/[^/#?]+)+\\.(?:jpg|jpeg|tiff|tif|heif|heic|png|gif|webp))\"");
    }
}