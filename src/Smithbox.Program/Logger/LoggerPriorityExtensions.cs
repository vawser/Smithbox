#nullable enable
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace StudioCore.Logger
{
    public static class LoggerPriorityExtensions
    {
        public const string PriorityKey = "smithbox.log_priority";
        public static void LogWithPriority(this ILogger logger, LogLevel level, string message, LogPriority priority, Exception? ex, params object?[] args)
        {
            using (logger.BeginScope(new[] { new KeyValuePair<string, object>(PriorityKey, priority)}))
            {
                //this warning is incorrectly detected here, ignore it
#pragma warning disable CA2254
                logger.Log(level, ex, message, args);
#pragma warning restore CA2254
            }
        }

        public static void LogWithPriority(this ILogger logger, LogLevel level, string message, LogPriority priority, params object?[] args)
        {
            logger.LogWithPriority(level, message, priority, null, args);
        }
    }
}