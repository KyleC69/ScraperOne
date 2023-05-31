// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ResponseDetails.csResponseDetails.cs032320233:29 AM


using System.Net;

namespace ScraperOne.Modules.Crawlers;

public class ResponseDetails
{
    public HttpStatusCode HttpStatusCode { get; set; }
    public string RedirectUrl { get; set; }
    public string Response { get; set; }
}