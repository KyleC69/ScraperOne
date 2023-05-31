// ScraperOne 2023
// All Rights Reserved
// Kyle Crowder
// Lawrence Enterprises 2023
// 
// ColorLoggerProvider.csColorLoggerProvider.cs032320233:29 AM


using System.Collections.Concurrent;
using System.Runtime.Versioning;
using Microsoft.Extensions.Logging;

namespace ScraperOne.Logger
{
    [UnsupportedOSPlatform("browser")]
    [Microsoft.Extensions.Logging.ProviderAlias("ColorLogger")]
    public sealed class ColorLoggerProvider : ILoggerProvider
    {
        private readonly ColorLoggerConfiguration i_currentConfig;

        private readonly ConcurrentDictionary<string, ColorLogger> i_loggers = new(StringComparer.OrdinalIgnoreCase);

        private readonly IDisposable i_onChangeToken;


        public ColorLoggerProvider()
        {
            ColorLoggerConfiguration config = new ColorLoggerConfiguration();
            i_currentConfig = config;
            i_onChangeToken = null;
        }


        public void Dispose()
        {
            i_loggers.Clear();
            i_onChangeToken?.Dispose();
        }

        ILogger ILoggerProvider.CreateLogger(string categoryName)
        {
            return i_loggers.GetOrAdd(categoryName, name => new ColorLogger(name, GetCurrentConfig));
        }


        public ColorLogger CreateLogger(string categoryName)
        {
            return i_loggers.GetOrAdd(categoryName, name => new ColorLogger(name, GetCurrentConfig));
        }


        private ColorLoggerConfiguration GetCurrentConfig()
        {
            return i_currentConfig;
        }
    }
}