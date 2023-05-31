// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// QueueSettings.csQueueSettings.cs032320233:30 AM


using System.Runtime.Serialization;
using ScraperOne.DataModels;

namespace ScraperOne.Properties;

[DataContract]
public sealed class QueueSettings : IExtensibleDataObject
{
    [DataMember(Name = "Names")] private readonly List<string> i_names;

    [DataMember(Name = "Types")] private readonly List<BlogTypes> i_types;


    public QueueSettings()
    {
        i_names = new List<string>();
        i_types = new List<BlogTypes>();
    }

    [DataMember] public string LastCrawledBlogName { get; set; }

    [DataMember] public BlogTypes LastCrawledBlogType { get; set; }

    public IReadOnlyList<string> Names => i_names;

    public IReadOnlyList<BlogTypes> Types => i_types;


    ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }


    public void ReplaceAll(IEnumerable<string> newBlogNames, IEnumerable<BlogTypes> newBlogTypes)
    {
        i_names.Clear();
        i_names.AddRange(newBlogNames);
        i_types.Clear();
        i_types.AddRange(newBlogTypes);
    }
}