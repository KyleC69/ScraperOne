using System.Runtime.Serialization;

namespace ScraperOne.DataModels.TumblrApi;

[DataContract]
public class TumblrApiJson
{
    [DataMember(Name = "tumblelog", EmitDefaultValue = false)]
    public TumbleLog TumbleLog { get; set; }

    [DataMember(Name = "posts-start", EmitDefaultValue = false)]
    public int PostsStart { get; set; }

    [DataMember(Name = "posts-total", EmitDefaultValue = false)]
    public int PostsTotal { get; set; }

    [DataMember(Name = "posts-type", EmitDefaultValue = false)]
    public bool PostsType { get; set; }

    [DataMember(Name = "posts", EmitDefaultValue = false)]
    public List<Post> Posts { get; set; }
}