using Microsoft.Extensions.Logging;
using System;

namespace StudioCore.Logger
{
    public sealed record LogEvent(
        DateTime Time,
        LogLevel Level,
        LogPriority Priority,
        string Category,
        string Message,
        Exception? Exception
    );
}