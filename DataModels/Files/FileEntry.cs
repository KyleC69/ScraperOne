// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// FileEntry.csFileEntry.cs032320233:28 AM

#nullable disable


using System.Runtime.Serialization;

namespace ScraperOne.DataModels.Files;

[DataContract]
public class FileEntry
{
    public string Filename
    {
        get => FilenameSer ?? Link;
        set => FilenameSer = value == Link ? null : value;
    }


    [DataMember(Name = "F", EmitDefaultValue = false)]
    public string FilenameSer { get; set; }

    [DataMember(Name = "L")] public string Link { get; set; }


    public string OriginalLink
    {
        get => string.IsNullOrEmpty(OriginalLinkSer) ? null : OriginalLinkSer;
        set => OriginalLinkSer = string.IsNullOrEmpty(value) || value == Link ? null : value;
    }


    [DataMember(Name = "O", EmitDefaultValue = false)]
    public string OriginalLinkSer { get; set; }


    [DataMember(Name = "postId", EmitDefaultValue = false)]
    public string PostId { get; set; }
}