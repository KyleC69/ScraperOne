using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace ScraperOne.Logger
{
    public sealed class ColorLoggerConfiguration
    {
        public int EventId { get; set; }

        public Dictionary<LogLevel, ConsoleColor> LogLevelToColorMap { get; set; } = new()
        {
            [LogLevel.Information] = ConsoleColor.Green,
            [LogLevel.Debug] = ConsoleColor.DarkYellow,
            [LogLevel.Critical] = ConsoleColor.DarkRed,
            [LogLevel.Error] = ConsoleColor.Red
        };
    }

    public sealed class ColorLogger : ILogger
    {
        private readonly Func<ColorLoggerConfiguration> _igetCurrentConfig;
        private readonly string _iName;


        public ColorLogger(string name, Func<ColorLoggerConfiguration> getCurrentConfig)
        {
            (_iName, _igetCurrentConfig) = (name, getCurrentConfig);
        }


        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return default!;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _igetCurrentConfig().LogLevelToColorMap.ContainsKey(logLevel);
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            ColorLoggerConfiguration config = _igetCurrentConfig();
            if (config.EventId == 0 || config.EventId == eventId.Id)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
                Console.Write($"[{eventId.Id,2}: {logLevel,-12}]");
                Debug.Write($"[{eventId.Id,2}: {logLevel,-12}]");

                Console.ForegroundColor = originalColor;
                Console.Write($"::{_iName} - ");
                Debug.Write($"::{_iName} - ");

                Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
                Console.Write($"{formatter(state, exception)}");
                Debug.Write($"{formatter(state, exception)}");

                Console.ForegroundColor = originalColor;
                Console.WriteLine();
                Debug.WriteLine("");
            }
        }

        public void LogDebug(string msg)
        {
            Console.WriteLine(msg);
            Debug.Write(msg);
        }


        public void LogDebug<TState>(LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            ColorLoggerConfiguration config = _igetCurrentConfig();
            if (config.EventId == 0 || config.EventId == eventId.Id)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
                Console.Write($"[{eventId.Id,2}: {logLevel,-12}]");
                Debug.Write($"[{eventId.Id,2}: {logLevel,-12}]");

                Console.ForegroundColor = originalColor;
                Console.Write($"::{_iName} - ");
                Debug.Write($"::{_iName} - ");

                Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
                Console.Write($"{formatter(state, exception)}");
                Debug.Write($"{formatter(state, exception)}");

                Console.ForegroundColor = originalColor;
                Console.WriteLine();
                Debug.WriteLine("");
            }
        }


        public void LogError<TState>(LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            ColorLoggerConfiguration config = _igetCurrentConfig();
            if (config.EventId == 0 || config.EventId == eventId.Id)
            {
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
                Console.Write($"[{eventId.Id,2}: {logLevel,-12}]");
                Debug.Write($"[{eventId.Id,2}: {logLevel,-12}]");

                Console.ForegroundColor = originalColor;
                Console.Write($"::{_iName} - ");
                Debug.Write($"::{_iName} - ");

                Console.ForegroundColor = config.LogLevelToColorMap[logLevel];
                Console.Write($"{formatter(state, exception)}");
                Debug.Write($"{formatter(state, exception)}");

                Console.ForegroundColor = originalColor;
                Console.WriteLine();
                Debug.WriteLine("");
            }
        }


        public void Write([Localizable(false)] string message, LogLevel logLevel)
        {
            Debug.WriteLine(message);
        }

        internal void LogError(string v)
        {
            Console.WriteLine(v);
        }

        public void LogInformation(string s)
        {
            Console.WriteLine(s);
        }
    }
}