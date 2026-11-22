using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Application;

/// <summary>
/// File-level vanilla status for any path in the project's VFS, shared by every editor's file lists:
/// <list type="bullet">
/// <item>not in the project folder: Unchanged if the game has it, None if neither has it;</item>
/// <item>in the project folder only: Added;</item>
/// <item>in both: Modified or Unchanged by comparing bytes with the game's copy (a byte-identical override counts as
/// Unchanged). When both are DCX and the bytes differ, the decompressed data is compared too, so a file re-saved
/// with different compression still counts as Unchanged.</item>
/// </list>
/// Comparisons run on one background worker and are cached by (path, size, write time), so <see cref="Get" /> is
/// cheap enough for the draw loop. Until a compare finishes the file reports None (draws uncoloured). A write through
/// ProjectFS drops that file's entry. <see cref="Version" /> goes up whenever a result changes, for lists that cache
/// per-row state. Plan: Claude/Documentation/Vanilla Modified-Added Parity.md (Phase 0, item 4).
/// </summary>
public sealed class ProjectFileStatus : IDisposable
{
    /// <summary>How long a cached entry is trusted before the project file is stat'ed again</summary>
    private const long RevalidateMs = 5000;

    private readonly ProjectVFS Vfs;
    private readonly string ProjectRoot;
    private readonly VirtualFileSystem ProjectFS;

    private sealed class Entry
    {
        /// <summary>-1 when the project folder has no such file</summary>
        public long Length;
        public DateTime WriteTime;
        public VanillaStatus Status;
        public bool Pending;
        public long CheckedAt;

        /// <summary>The game's path for the file (usually the same as the project path)</summary>
        public string VanillaKey;
    }

    private readonly ConcurrentDictionary<string, Entry> Cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly BlockingCollection<string> Queue = new();
    private readonly CancellationTokenSource Cancel = new();
    private Task? Worker;
    private int _workerStarted;

    private int _version;

    /// <summary>Goes up whenever any file's status changes</summary>
    public int Version => Volatile.Read(ref _version);

    public ProjectFileStatus(ProjectVFS vfs, string projectRoot)
    {
        Vfs = vfs;
        ProjectRoot = projectRoot;
        ProjectFS = vfs.ProjectFS;
        ProjectFS.FileWritten += OnFileWritten;
    }

    /// <summary>The worker starts on the first compare, so a VFS no list ever asks about costs no thread</summary>
    private void EnsureWorker()
    {
        if (Interlocked.Exchange(ref _workerStarted, 1) == 0)
            Worker = Task.Factory.StartNew(Run, Cancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }

    /// <summary>
    /// The file's status against the game's copy. Never blocks: a file whose compare hasn't finished is None, and
    /// is queued for the background worker. <paramref name="vanillaPath" /> is for a file the project keeps at a
    /// different path from the game's (the Sound Editor's sd files: /sd/x.bnk in the project, /x.bnk in the game).
    /// </summary>
    public VanillaStatus Get(string path, string? vanillaPath = null)
    {
        if (string.IsNullOrWhiteSpace(path) || Cancel.IsCancellationRequested)
            return VanillaStatus.None;

        var key = Normalize(path);
        var vanillaKey = string.IsNullOrWhiteSpace(vanillaPath) ? key : Normalize(vanillaPath);
        var now = Environment.TickCount64;

        if (Cache.TryGetValue(key, out var cached) && cached.VanillaKey != vanillaKey)
            cached = null;

        if (cached != null && now - cached.CheckedAt < RevalidateMs)
            return cached.Status;

        var (length, writeTime) = Stat(key);

        if (cached != null && cached.Length == length && cached.WriteTime == writeTime)
        {
            cached.CheckedAt = now;
            return cached.Status;
        }

        var entry = new Entry { Length = length, WriteTime = writeTime, CheckedAt = now, VanillaKey = vanillaKey };

        if (length < 0)
        {
            // Not in the project: the game's own file, or nothing at all
            entry.Status = VanillaExists(vanillaKey) ? VanillaStatus.Unchanged : VanillaStatus.None;
        }
        else
        {
            entry.Status = VanillaStatus.None;
            entry.Pending = true;
        }

        Cache[key] = entry;
        Interlocked.Increment(ref _version);

        if (entry.Pending)
        {
            try
            {
                Queue.TryAdd(key);
                EnsureWorker();
            }
            catch (Exception e) when (e is InvalidOperationException or ObjectDisposedException)
            {
                // Disposed while drawing (the project is closing): the file just stays uncoloured
            }
        }

        return entry.Status;
    }

    /// <summary>Drops a file's cached status (after writing it other than through ProjectFS, or deleting it)</summary>
    public void Invalidate(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        if (Cache.TryRemove(Normalize(path), out _))
            Interlocked.Increment(ref _version);
    }

    /// <summary>Drops every cached status</summary>
    public void InvalidateAll()
    {
        Cache.Clear();
        Interlocked.Increment(ref _version);
    }

    private void OnFileWritten(string path)
    {
        // Writes outside the project (backups use absolute paths) aren't project files
        if (string.IsNullOrWhiteSpace(path) || Path.IsPathRooted(path.TrimStart('/', '\\')))
            return;

        Invalidate(path);
    }

    private void Run()
    {
        try
        {
            foreach (var key in Queue.GetConsumingEnumerable(Cancel.Token))
            {
                if (!Cache.TryGetValue(key, out var entry) || !entry.Pending)
                    continue;

                VanillaStatus status;
                try
                {
                    status = Compare(key, entry.VanillaKey);
                }
                catch (Exception e)
                {
                    if (Cancel.IsCancellationRequested)
                        return;

                    Smithbox.LogError(this, $"Failed to compare {key} with the game's copy", LogPriority.Low, e);
                    status = VanillaStatus.None;
                }

                // Only store it if the file hasn't changed (or been invalidated) while it was compared
                if (Cache.TryGetValue(key, out var current) && ReferenceEquals(current, entry))
                {
                    var (length, writeTime) = Stat(key);
                    if (length == entry.Length && writeTime == entry.WriteTime)
                    {
                        entry.Status = status;
                        entry.Pending = false;
                        Interlocked.Increment(ref _version);
                    }
                    else
                    {
                        Cache.TryRemove(key, out _);
                        Interlocked.Increment(ref _version);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private VanillaStatus Compare(string key, string vanillaKey)
    {
        var vanillaFS = Vfs.VanillaFS;

        if (!vanillaFS.TryGetFile(vanillaKey, out var vanillaFile))
            return VanillaStatus.Added;

        var projectPath = Path.Combine(ProjectRoot, key);
        if (!File.Exists(projectPath))
            return VanillaStatus.Unchanged;

        var project = File.ReadAllBytes(projectPath);
        var vanilla = vanillaFile.GetData();

        if (project.Length == vanilla.Length && project.AsSpan().SequenceEqual(vanilla.Span))
            return VanillaStatus.Unchanged;

        // Same data under different compression (a re-save of an untouched file) is still unchanged
        if (DCX.Is(project) && DCX.Is(vanilla))
        {
            var a = DCX.Decompress(project);
            var b = DCX.Decompress(vanilla);

            if (a.Length == b.Length && a.Span.SequenceEqual(b.Span))
                return VanillaStatus.Unchanged;
        }

        return VanillaStatus.Modified;
    }

    private bool VanillaExists(string key)
    {
        try
        {
            return Vfs.VanillaFS.FileExists(key);
        }
        catch
        {
            return false;
        }
    }

    private (long Length, DateTime WriteTime) Stat(string key)
    {
        if (string.IsNullOrEmpty(ProjectRoot) || ProjectFS is EmptyVirtualFileSystem)
            return (-1, default);

        try
        {
            var info = new FileInfo(Path.Combine(ProjectRoot, key));
            return info.Exists ? (info.Length, info.LastWriteTimeUtc) : (-1, default);
        }
        catch
        {
            return (-1, default);
        }
    }

    /// <summary>VFS paths are relative, '/'-separated; the project folder and the game's data share them</summary>
    private static string Normalize(string path) => path.Replace('\\', '/').TrimStart('/');

    public void Dispose()
    {
        if (Cancel.IsCancellationRequested)
            return;

        ProjectFS.FileWritten -= OnFileWritten;
        Cancel.Cancel();
        Queue.CompleteAdding();

        try
        {
            Worker?.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
        }

        Queue.Dispose();
        Cancel.Dispose();
    }
}
