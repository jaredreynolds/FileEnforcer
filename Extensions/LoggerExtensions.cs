using System;
using Microsoft.Extensions.Logging;

namespace FileEnforcer.Extensions
{
    public static class LoggerExtensions
    {
        public static void LogDebug(this ILogger logger, Func<string> messageFactory)
            => logger.Log(LogLevel.Debug, messageFactory);

        public static void LogDebug(this ILogger logger, Exception exception, Func<string> messageFactory)
            => logger.Log(LogLevel.Debug, exception, messageFactory);

        public static void LogTrace(this ILogger logger, Func<string> messageFactory)
            => logger.Log(LogLevel.Trace, messageFactory);

        public static void Log(this ILogger logger, LogLevel logLevel, Func<string> messageFactory)
        {
            if (logger?.IsEnabled(logLevel) == true)
            {
                logger.Log(logLevel, messageFactory?.Invoke());
            }
        }

        public static void Log(this ILogger logger, LogLevel logLevel, Exception exception, Func<string> messageFactory)
        {
            if (logger?.IsEnabled(logLevel) == true)
            {
                logger.Log(logLevel, exception, messageFactory?.Invoke());
            }
        }
    }
}
