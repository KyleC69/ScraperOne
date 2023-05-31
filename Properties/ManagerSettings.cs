// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ManagerSettings.csManagerSettings.cs032320233:30 AM


using System.Runtime.Serialization;

namespace ScraperOne.Properties;

[DataContract]
public sealed class ManagerSettings : IExtensibleDataObject
{
    ExtensionDataObject IExtensibleDataObject.ExtensionData { get; set; }
}