// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ServiceNotLoadedException.csServiceNotLoadedException.cs032320233:30 AM


using System.Runtime.Serialization;

namespace ScraperOne.Services;

[Serializable]
internal class ServiceNotLoadedException : Exception
{
    public ServiceNotLoadedException()
    {
    }


    public ServiceNotLoadedException(string message) : base(message)
    {
    }


    public ServiceNotLoadedException(string message, Exception innerException) : base(message, innerException)
    {
    }


    protected ServiceNotLoadedException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}