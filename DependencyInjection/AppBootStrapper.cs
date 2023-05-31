// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// AppBootStrapper.csAppBootStrapper.cs0324202311:25 PM


using ScraperOne.Logger;
using ScraperOne.Services;
using Splat;

namespace ScraperOne.DependencyInjection;

public class AppBootStrapper
{
    public static void RegisterServices(IMutableDependencyResolver services, IReadonlyDependencyResolver resolver)
    {
        Locator.CurrentMutable.RegisterConstant(new CookieService());
        Locator.CurrentMutable.RegisterConstant(new ColorLoggerProvider());
        ColorLoggerProvider logfactory = resolver.GetService<ColorLoggerProvider>();

        //services.RegisterConstant(new BlogService(logfactory.CreateLogger("BlogService")));
        services.RegisterConstant(new CrawlerController(logfactory.CreateLogger("CrawlerController")));
        services.RegisterConstant(new QueueController(logfactory.CreateLogger("QueueController")));
        services.RegisterConstant(new QueueManager(logfactory.CreateLogger("QueueManager")));
        services.RegisterConstant(new ManagerService(logfactory.CreateLogger("ManagerService")));
        services.RegisterConstant(new DiServiceLoader(logfactory.CreateLogger("DIServiceLoader"),
            resolver.GetService<ManagerService>(),
            resolver.GetService<CrawlerController>(),
            resolver.GetService<QueueController>(),
            resolver.GetService<QueueManager>()));
    }
}