// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// BlogCollection.csBlogCollection.cs032320233:28 AM

#nullable disable


using System.Runtime.Serialization;

namespace ScraperOne.DataModels;

[DataContract]
public class BlogCollection : Model
{
    private string i_downloadLocation;
    private int i_id;
    private bool? i_isOnline;
    private string i_name;
    private bool i_offlineDuplicateCheck;


    [DataMember]
    public string DownloadLocation
    {
        get => i_downloadLocation;
        set => SetProperty(ref i_downloadLocation, value);
    }


    [DataMember]
    public int Id
    {
        get => i_id;
        set => SetProperty(ref i_id, value);
    }


    public bool? IsOnline
    {
        get => i_isOnline;
        set => SetProperty(ref i_isOnline, value);
    }


    [DataMember]
    public string Name
    {
        get => i_name;
        set => SetProperty(ref i_name, value);
    }


    [DataMember]
    public bool OfflineDuplicateCheck
    {
        get => i_offlineDuplicateCheck;
        set => SetProperty(ref i_offlineDuplicateCheck, value);
    }


    public BlogCollection Clone()
    {
        return new BlogCollection
        {
            Id = Id,
            Name = Name,
            DownloadLocation = DownloadLocation,
            OfflineDuplicateCheck = OfflineDuplicateCheck,
            IsOnline = IsOnline
        };
    }
}