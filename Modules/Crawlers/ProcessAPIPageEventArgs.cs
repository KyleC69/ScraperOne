// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ProcessAPIPageEventArgs.csProcessAPIPageEventArgs.cs032320233:29 AM


using ScraperOne.DataModels.NewTumbl;

namespace ScraperOne.Modules.Crawlers
{
    internal class ProcessApiPageEventArgs
    {
        public List<ARow> CurrentPage { get; internal set; }
        public bool IsComplete { get; internal set; }
        public int PageNumber { get; internal set; }
        public string RawJson { get; internal set; }
    }
}