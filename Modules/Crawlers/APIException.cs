// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// NewTumblCrawler.csNewTumblCrawler.cs032420232:59 PM

using System.Runtime.Serialization;

namespace ScraperOne.Modules
{
    [Serializable]
    internal class APIException : Exception
    {

        public APIException(Exception innerException) : base(innerException?.Message, innerException) { }
        public APIException()        {        }

        public APIException(string message) : base(message)        {        }

        public APIException(string message, Exception innerException) : base(message, innerException)        {        }

        protected APIException(SerializationInfo info, StreamingContext context) : base(info, context)        {        }
    }
}




