using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace StudioCore.Renderer;

public enum ResourceLoadState
{
    Waiting,
    Loading,
    Done
}

/// <summary>
///     The progress of one category of resources, across every job that loads it.
/// </summary>
public readonly record struct ResourceLoadStatus(string Name, ResourceLoadState State, int Loaded, int Total, bool IsEstimate);

/// <summary>
///     Totals up the progress of resource jobs by category. A category is the name of a job: a single load
///     is split into many short-lived jobs, so jobs that load the same kind of resource should share a name.
///     Finished jobs stay in the totals until everything has loaded, so a category's progress covers the whole load.
/// </summary>
public static class ResourceLoadTracker
{
    /// <summary>
    ///     Loads shorter than this (in seconds) are not displayed, as they would only cause the viewport to flicker
    /// </summary>
    private const double DisplayDelay = 0.25;

    private sealed class Category
    {
        public string Name;

        // True once the load of this category has been started, even if it turned out there was nothing to load
        public bool Requested;

        // Size of the jobs that have finished
        public int Retired;

        // Sum of the running jobs, recalculated by Update
        public int Loaded;
        public int Total;
        public bool Active;
        public bool IsEstimate;
    }

    private static readonly object _lock = new();
    private static readonly List<Category> _categories = new();
    private static readonly List<string> _sessions = new();
    private static readonly Stopwatch _loadTime = new();

    private static string _currentFile;

    /// <summary>
    ///     The categories of the current load, in the order they were first seen. Empty while nothing is displayed.
    /// </summary>
    public static IReadOnlyList<ResourceLoadStatus> Statuses { get; private set; } = Array.Empty<ResourceLoadStatus>();

    public static string CurrentFile => Volatile.Read(ref _currentFile);

    /// <summary>
    ///     Marks the start of a load that is made up of several steps. The totals are kept until every session
    ///     has ended, including across any gaps between the steps in which no job is running.
    /// </summary>
    public static void BeginSession(string name)
    {
        lock (_lock)
        {
            _sessions.Add(name);
        }
    }

    public static void EndSession(string name)
    {
        lock (_lock)
        {
            _sessions.Remove(name);
        }
    }

    /// <summary>
    ///     Gets the name of the first session, and how many others are running alongside it.
    /// </summary>
    public static string GetSessionName(out int otherSessions)
    {
        lock (_lock)
        {
            otherSessions = Math.Max(_sessions.Count - 1, 0);
            return _sessions.Count > 0 ? _sessions[0] : null;
        }
    }

    /// <summary>
    ///     Lists a category that is going to be loaded, so it is displayed as waiting until its load begins.
    /// </summary>
    public static void Reserve(string name)
    {
        lock (_lock)
        {
            GetCategory(name);
        }
    }

    /// <summary>
    ///     Notes that the load of a category has been started. Any work it has is now covered by its jobs.
    /// </summary>
    public static void MarkRequested(string name)
    {
        lock (_lock)
        {
            GetCategory(name).Requested = true;
        }
    }

    /// <summary>
    ///     Sets the file displayed as being worked on. Called from the job threads, so the latest one wins.
    /// </summary>
    internal static void SetCurrentFile(string file)
    {
        Volatile.Write(ref _currentFile, file);
    }

    /// <summary>
    ///     Moves a finished job into the totals of its category. Anything that failed to load is
    ///     counted as well, as there is nothing left to wait for.
    /// </summary>
    internal static void Retire(ResourceJob job)
    {
        if (job.Hidden)
        {
            return;
        }

        lock (_lock)
        {
            GetCategory(job.Name).Retired += Math.Max(job.Progress, job.GetEstimateTaskSize());
        }
    }

    /// <summary>
    ///     Recalculates the statuses. Runs once per frame on the main thread, after the jobs have been processed.
    /// </summary>
    internal static void Update()
    {
        lock (_lock)
        {
            if (ResourceManager.ActiveJobProgress.IsEmpty && _sessions.Count == 0)
            {
                if (_loadTime.IsRunning)
                {
                    Clear();
                }

                return;
            }

            if (!_loadTime.IsRunning)
            {
                _loadTime.Start();
            }

            foreach (Category category in _categories)
            {
                category.Loaded = category.Retired;
                category.Total = category.Retired;
                category.Active = false;
                category.IsEstimate = false;
            }

            foreach (KeyValuePair<ResourceJob, int> entry in ResourceManager.ActiveJobProgress)
            {
                ResourceJob job = entry.Key;

                if (job.Hidden)
                {
                    continue;
                }

                Category category = GetCategory(job.Name);
                var size = job.GetEstimateTaskSize();

                category.Loaded += Math.Min(job.Progress, size);
                category.Total += size;
                category.Active = true;
                category.IsEstimate |= job.IsEstimating;
            }

            Statuses = _loadTime.Elapsed.TotalSeconds < DisplayDelay ? Array.Empty<ResourceLoadStatus>() : BuildStatuses();
        }
    }

    private static ResourceLoadStatus[] BuildStatuses()
    {
        var statuses = new ResourceLoadStatus[_categories.Count];

        for (var i = 0; i < statuses.Length; i++)
        {
            Category category = _categories[i];
            ResourceLoadState state = ResourceLoadState.Done;

            if (category.Active)
            {
                state = ResourceLoadState.Loading;
            }
            else if (!category.Requested && category.Retired == 0)
            {
                state = ResourceLoadState.Waiting;
            }

            statuses[i] = new ResourceLoadStatus(category.Name, state, category.Loaded, category.Total, category.IsEstimate);
        }

        return statuses;
    }

    private static Category GetCategory(string name)
    {
        foreach (Category category in _categories)
        {
            if (category.Name == name)
            {
                return category;
            }
        }

        var newCategory = new Category { Name = name };
        _categories.Add(newCategory);
        return newCategory;
    }

    private static void Clear()
    {
        _categories.Clear();
        _loadTime.Reset();
        Statuses = Array.Empty<ResourceLoadStatus>();
        Volatile.Write(ref _currentFile, null);
    }
}
