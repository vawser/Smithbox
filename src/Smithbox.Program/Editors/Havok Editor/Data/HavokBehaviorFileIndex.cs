using StudioCore.Editors.Common;
using System.Collections.Concurrent;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The file names in every behbnd of the project, so the Behavior Editor can list the behavior files themselves
/// instead of their binders. Read in the background the first time the list is shown (each behbnd is read once),
/// and kept for the session: once a binder is opened, the list follows the bank's copy of its files instead,
/// so staged file actions (rename, insert, delete ...) show.
/// </summary>
public class HavokBehaviorFileIndex
{
    private readonly ProjectEntry Project;
    private readonly HavokBank Bank;

    private readonly ConcurrentDictionary<FileDictionaryEntry, List<string>> Files = new();

    private int Started;
    private int CompletedCount;
    private int FailedCount;

    /// <summary>The behbnds, sorted by name</summary>
    public List<FileDictionaryEntry> Binders { get; }

    public int Total => Binders.Count;
    public int Completed => Volatile.Read(ref CompletedCount);
    public int Failed => Volatile.Read(ref FailedCount);

    /// <summary>Set once every behbnd has been read (or failed to)</summary>
    public volatile bool IsReady;

    public HavokBehaviorFileIndex(ProjectEntry project, HavokBank bank)
    {
        Project = project;
        Bank = bank;

        Binders = project.Locator.HavokBehaviorFiles.Entries
            .OrderBy(e => e.Filename, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>Start reading the behbnds, the first time it's called</summary>
    public void EnsureStarted()
    {
        if (Interlocked.Exchange(ref Started, 1) != 0)
            return;

        if (Binders.Count == 0)
        {
            IsReady = true;
            return;
        }

        Task.Run(Load);
    }

    private async Task Load()
    {
        // Each read decompresses a whole behbnd, so not every core at once
        var options = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2) };

        try
        {
            await Parallel.ForEachAsync(Binders, options, (binder, _) =>
            {
                var names = Bank.ReadBinderFileNames(binder);

                // A list already recorded from the bank is newer than the read
                if (names != null)
                    Files.TryAdd(binder, names);
                else
                    Interlocked.Increment(ref FailedCount);

                Interlocked.Increment(ref CompletedCount);

                return ValueTask.CompletedTask;
            });
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("BHV_Havok_Index_Failed"), ex);
        }

        IsReady = true;

        if (Failed > 0)
            Smithbox.LogError(this, LOC.Get("BHV_Havok_Index_Unreadable", Failed));
    }

    /// <summary>
    /// Record the bank's file list of an opened binder, so the list stays right after the bank lets the binder go
    /// (a saved rename, insert or delete would otherwise show the names first read)
    /// </summary>
    public void Update(FileDictionaryEntry binder, IEnumerable<string> names)
    {
        Files[binder] = names.ToList();
    }

    /// <summary>The binder's file names as read by the index, or false if it hasn't been read (yet)</summary>
    public bool TryGetFiles(FileDictionaryEntry binder, out List<string> files)
    {
        return Files.TryGetValue(binder, out files);
    }
}
