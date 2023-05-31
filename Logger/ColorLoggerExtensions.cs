// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ColorLoggerExtensions.csColorLoggerExtensions.cs032320233:29 AM


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace ScraperOne.Logger;

public static class ColorLoggerExtensions
{
    public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder)
    {
        // builder.AddConfiguration();
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ColorLoggerProvider>());
        // LoggerProviderOptions.RegisterProviderOptions<ColorLoggerConfiguration, ColorLoggerProvider>(builder.Services);
        return builder;
    }


    public static ILoggingBuilder AddColorConsoleLogger(
        this ILoggingBuilder builder,
        Action<ColorLoggerConfiguration> configure)
    {
        builder.AddColorConsoleLogger();
        builder.Services.Configure(configure);
        return builder;
    }
}