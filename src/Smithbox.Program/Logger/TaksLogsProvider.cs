#nullable enable
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace StudioCore.Logger
{
    public class TaskLogsProvider : ILoggerProvider, ISupportExternalScope
{
    private IExternalScopeProvider? _scopes;
    
    public ILogger CreateLogger(string categoryName)
        => new TaskLogsLogger(categoryName, () => _scopes);
    
    public void SetScopeProvider(IExternalScopeProvider? scopes)
        => _scopes = scopes;

    public void Dispose() { }

    private sealed class TaskLogsLogger : ILogger
    {
        private readonly string _category;
        private readonly Func<IExternalScopeProvider?> _getScopes;

        public TaskLogsLogger(string category, Func<IExternalScopeProvider?> getScopes)
        {
            _category = category;
            _getScopes = getScopes;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var scopes = _getScopes();
            return scopes?.Push(state) ?? NullScope.Instance;
        }
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            var msg = formatter(state, exception);
            var prio = ReadPriorityFromScopes() ?? DefaultPriority(logLevel);

            TaskLogs.Publish(new(
                Time: DateTime.Now,
                Level: logLevel,
                Priority: prio,
                Category: _category,
                Message: msg,
                Exception: exception
            ));
        }

        private LogPriority? ReadPriorityFromScopes()
        {
            var scopes = _getScopes();
            if (scopes is null) return null;

            LogPriority? found = null;

            scopes.ForEachScope((scopeObj, _) =>
            {
                // Common pattern: scope is IEnumerable<KeyValuePair<string, object>>
                if (scopeObj is IEnumerable<KeyValuePair<string, object>> kvps)
                {
                    foreach (var kv in kvps)
                    {
                        if (kv.Key == LoggerPriorityExtensions.PriorityKey && kv.Value is LogPriority p)
                            found = p;
                    }
                }
            }, state: (object?)null);

            return found;
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
    public static LogPriority DefaultPriority(LogLevel level) =>
        level switch
        {
            LogLevel.Error or LogLevel.Critical => LogPriority.High,
            LogLevel.Warning => LogPriority.Normal,
            _ => LogPriority.Low
        };
}

}