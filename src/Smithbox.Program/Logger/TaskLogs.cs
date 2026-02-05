#nullable enable
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Logger;

/// <summary>
/// Used to log and display information for the user.
/// Must be registered with an ILoggerFactory to be used.
/// </summary>
public static class TaskLogs
{
    private static ImmutableArray<LogSubscription> _subs = ImmutableArray<LogSubscription>.Empty;

    public static LogSubscription Subscribe(Func<LogEvent, bool> filter)
    {
        var sub = new LogSubscription(filter);
        ImmutableInterlocked.Update(ref _subs, arr => arr.Add(sub));
        return sub;
    }

    public static void Unsubscribe(LogSubscription sub)
    {
        ImmutableInterlocked.Update(ref _subs, arr => arr.Remove(sub));
    }

    public static void Publish(LogEvent evt)
    {
        var snapshot = _subs; // atomic read
        foreach (var s in snapshot)
        {
            if (s.Filter(evt)) s.Queue.Enqueue(evt);
        }
    }
}

public sealed class LogSubscription
{
    internal readonly ConcurrentQueue<LogEvent> Queue = new();
    internal readonly Func<LogEvent, bool> Filter;

    public LogSubscription(Func<LogEvent, bool> filter) => Filter = filter;

    public bool TryDequeue([MaybeNullWhen(false)] out LogEvent evt) => Queue.TryDequeue(out evt);
}