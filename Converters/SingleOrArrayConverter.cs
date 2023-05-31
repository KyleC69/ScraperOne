// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// SingleOrArrayConverter.csSingleOrArrayConverter.cs032320233:30 AM


using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ScraperOne.Modules;

public class SingleOrArrayConverter<T> : JsonConverter
{
    public override bool CanWrite => true;


    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(List<T>);
    }


    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        return token.Type == JTokenType.Array ? token.ToObject<List<T>>() : (object)new List<T> { token.ToObject<T>() };
    }


    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var list = (List<T>)value;
        if (list.Count == 1) value = list[0];
        serializer.Serialize(writer, value);
    }
}