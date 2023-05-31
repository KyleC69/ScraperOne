// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// NewTumblBlog.csNewTumblBlog.cs032320233:29 AM

using System.Runtime.Serialization;
using ScraperOne.DataModels.Files;

namespace ScraperOne.DataModels;

[DataContract]
public class NewTumblBlog : Blog
{
    public static Blog Create(string url, string location, string filenameTemplate)
    {
        var blog = new NewTumblBlog
        {
            Url = ExtractUrl(url),
            Name = ExtractName(url),
            Location = location,
            Online = true,
            Version = "4",
            DateAdded = DateTime.Now,
            FilenameTemplate = filenameTemplate
        };
        _ = Directory.CreateDirectory(location);
        blog.ChildId = Path.Combine(location, blog.Name + "_files." + blog.BlogType);
        if (!File.Exists(blog.ChildId))
        {
            IFiles files = new NewTumblBlogFiles(blog.Name, blog.Location);
            files.Save();
        }

        return blog;
    }


    protected static string ExtractUrl(string url)
    {
        return "https://" + ExtractSubDomain(url) + ".newtumbl.com/";
    }
}