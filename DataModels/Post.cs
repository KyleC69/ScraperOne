// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// Post.csPost.cs032320233:29 AM


using System.Runtime.Serialization;

using Newtonsoft.Json;

using ScraperOne.DataModels.NewTumbl;

namespace ScraperOne.DataModels
{
    public static class PostType
    {
        public static byte Answer => 3;
        public static byte Audio => 99;
        public static byte Comment => 8;
        public static byte Link => 4;
        public static byte Photo => 5;
        public static byte Quote => 2;
        public static byte Text => 1;
        public static byte Video => 7;
    }

    [DataContract]
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class Post
    {
        public Post()
        {
            DownloadedUrls = new List<string>();
            DownloadedFilenames = new List<string>();
        }


        public byte? BPostTypeIx { get; set; }
        public byte? BRatingIx { get; set; }
        public byte? BState { get; set; }
        public byte BStatus { get; set; }

        public List<string> DownloadedFilenames { get; private set; }

        public List<string> DownloadedUrls { get; private set; }
        public DateTime? DtActive { get; set; }

        public DateTime DtCreated { get; set; }
        public DateTime? DtDeleted { get; set; }
        public DateTime? DtFavorite { get; set; }
        public DateTime? DtFlag { get; set; }
        public DateTime? DtLike { get; set; }
        public DateTime? DtModified { get; set; }
        public DateTime? DtScheduled { get; set; }
        public int? DwBlogIx { get; set; }
        public int? DwBlogIxFrom { get; set; }
        public int? DwBlogIxOrig { get; set; }
        public int? DwBlogIxSubmit { get; set; }
        public long? DwChecksum { get; set; }
        public int? NCountComment { get; set; }
        public int? NCountLike { get; set; }
        public byte? NCountMark { get; set; }
        public int? NCountPost { get; set; }
        public byte? NTierIz { get; set; }

        public List<Part> Parts { get; set; }
        public long? QwPostIx { get; set; }
        public long? QwPostIxFrom { get; set; }
        public long? QwPostIxOrig { get; set; }
        public string SzExternal { get; set; }
        public string SzSource { get; set; }
        public string SzUrl { get; set; }
        public List<Tag> Tags { get; set; }


        public object Clone()
        {
            return MemberwiseClone();
        }


        public static Post Create(ARow post)
        {
            return JsonConvert.DeserializeObject<Post>(JsonConvert.SerializeObject(post));
        }
    }

    [DataContract]
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class Part
    {
        public byte? BOrder { get; set; }
        public byte? BPartTypeIx { get; set; }
        public DateTime? DtScheduled { get; set; }
        public int? DwBlogIxFrom { get; set; }

        public List<Media> Medias { get; set; }
        public short? NPartIz { get; set; }
        public long? QwPartIx { get; set; }
        public long? QwPostIx { get; set; }
        public long? QwPostIxFrom { get; set; }


        public static Part Create(ARow part)
        {
            return JsonConvert.DeserializeObject<Part>(JsonConvert.SerializeObject(part));
        }
    }

    [DataContract]
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class Media
    {
        public byte? BMediaTypeIx { get; set; }
        public byte? BPartTypeIx { get; set; }
        public byte BStatus { get; set; }
        public DateTime DtCreated { get; set; }
        public int DwUserIx { get; set; }
        public short? NHeight { get; set; }
        public int? NLength { get; set; }
        public int? NSize { get; set; }
        public short? NWidth { get; set; }
        public long? QwMediaIx { get; set; }
        public long? QwPartIx { get; set; }
        public string SzBody { get; set; }
        public string SzIpAddress { get; set; }
        public string SzSub { get; set; }


        public static Media Create(ARow media)
        {
            return JsonConvert.DeserializeObject<Media>(JsonConvert.SerializeObject(media));
        }
    }

    [DataContract]
    [JsonObject(MemberSerialization = MemberSerialization.OptOut)]
    public class Tag
    {
        public byte? BOrder { get; set; }
        public long? QwPostIx { get; set; }
        public string SzTagId { get; set; }


        public static Tag Create(ARow tag)
        {
            return new Tag { QwPostIx = tag.qwPostIx, BOrder = tag.bOrder, SzTagId = tag.szTagId };
        }
    }
}