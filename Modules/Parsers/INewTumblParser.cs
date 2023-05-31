// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// INewTumblParser.csINewTumblParser.cs032320233:30 AM


using System.Text.RegularExpressions;

namespace ScraperOne.Modules.Parsers;

public interface INewTumblParser
{
    Regex GetGenericPhotoUrlRegex();

    Regex GetPhotoUrlRegex();

    bool IsNewTumblUrl(string url);

    IEnumerable<string> SearchForGenericPhotoUrl(string searchableText);

    IEnumerable<string> SearchForPhotoUrl(string searchableText);
}