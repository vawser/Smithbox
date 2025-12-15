using StudioCore.Utilities;

namespace StudioCore.Renderer;

/// <summary>
/// Separate handling for logging here so we can keep the log statements in situ and just toggle the appearance in the Logger.
/// </summary>
public static class ResourceLog
{
    public static void AddLog(string text)
    {
#if DEBUG
        TaskLogs.AddLog(text);
#endif
    }
    public static void AddLog(string text, Microsoft.Extensions.Logging.LogLevel level)
    {
#if DEBUG
        TaskLogs.AddLog(text, level);
#endif
    }
}