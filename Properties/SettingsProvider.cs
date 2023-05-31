// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// SettingsProvider.csSettingsProvider.cs032320233:30 AM

#pragma warning restore


using System.Runtime.Serialization.Json;
using System.Text;

namespace ScraperOne.Properties;

public interface ISettingsProvider
{
    T LoadSettings<T>(string fileName) where T : class, new();

    void SaveSettings(string fileName, object settings);
}

public class SettingsProvider : ISettingsProvider
{
    private static readonly Type[] sro_KnownTypes = { typeof(string[]) };


    public T LoadSettings<T>(string fileName) where T : class, new()
    {
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException(Resources.No_Empty_String, nameof(fileName));
        if (!Path.IsPathRooted(fileName))
            throw new ArgumentException(Resources.ThePathIsNotRooted, nameof(fileName));
        if (File.Exists(fileName))
        {
            using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            var serializer = new DataContractJsonSerializer(typeof(T), sro_KnownTypes);
            return (T)serializer.ReadObject(stream) ?? new T();
        }

        return new T();
    }


    public void SaveSettings(string fileName, object settings)
    {
        if (settings == null) throw new ArgumentNullException(nameof(settings));
        if (string.IsNullOrEmpty(fileName))
            throw new ArgumentException(Resources.No_Empty_String, nameof(fileName));
        if (!Path.IsPathRooted(fileName))
            throw new ArgumentException(Resources.ThePathIsNotRooted, nameof(fileName));
        var directory = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(directory)) _ = Directory.CreateDirectory(directory);
        using var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
        using var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true, "  ");
        var serializer = new DataContractJsonSerializer(settings.GetType(), sro_KnownTypes);
        serializer.WriteObject(writer, settings);
        writer.Flush();
    }
}