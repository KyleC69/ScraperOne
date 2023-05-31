// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// NewTumblBlogFiles.csNewTumblBlogFiles.cs032320233:28 AM


using System.Runtime.Serialization;

namespace ScraperOne.DataModels.Files;

[DataContract]
public class NewTumblBlogFiles : Files
{
    public NewTumblBlogFiles(string name, string location) : base(name, location)
    {
        BlogType = BlogTypes.Newtumbl;
    }
}