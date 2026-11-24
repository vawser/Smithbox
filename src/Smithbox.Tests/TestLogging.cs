using Microsoft.Extensions.Logging;
using StudioCore;
using StudioCore.Application;

namespace SmithboxTests;

/// <summary>
///     Sets up what code that logs or localizes needs outside the app: localization in "Test Language" (LOC.Get
///     returns the key, without its arguments) and a logger that records, per thread, what was logged, so a golden
///     test can include it without picking up what tests on other threads log.
/// </summary>
internal static class TestLogging
{
    [ThreadStatic] private static List<string>? _captured;

    private static readonly object InitLock = new();
    private static bool _initialized;

    public static void Init()
    {
        lock (InitLock)
        {
            if (_initialized)
                return;

            Startup.Current ??= new Startup();
            Startup.Current.Program_Language = "Test Language";

            Smithbox.SbLoggerFactory ??= LoggerFactory.Create(builder => builder.AddProvider(new CaptureProvider()));

            _initialized = true;
        }
    }

    /// <summary>What this thread logged since the last call, cleared</summary>
    public static List<string> Take()
    {
        var taken = _captured ?? new List<string>();
        _captured = null;
        return taken;
    }

    private sealed class CaptureProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName) => new CaptureLogger();
        public void Dispose() { }
    }

    private sealed class CaptureLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            (_captured ??= new List<string>()).Add($"{logLevel}: {formatter(state, exception)}");
        }
    }
}
