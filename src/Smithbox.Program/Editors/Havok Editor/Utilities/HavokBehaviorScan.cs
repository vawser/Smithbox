using Hexa.NET.ImGui;
using Andre.IO.VFS;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Which behavior files a scoped tool (Problems, Search) works on
/// </summary>
public enum HavokBehaviorScope { File, Binder, Modified, All }

/// <summary>
/// Where a result leads when clicked: the object in the Outline, or the event / variable of that index
/// </summary>
public enum HavokScanJump { Outline, Event, Variable }

/// <summary>
/// Finds an object again in a file that is read anew: its type, m_name, and its place among the objects of that type
/// and name in the symbols' walk (the same for the same bytes)
/// </summary>
public record HavokObjectLocator(string TypeName, string Name, int Ordinal)
{
    public object Resolve(HavokBehaviorSymbols symbols)
    {
        if (symbols == null)
            return null;

        var seen = 0;

        foreach (var obj in symbols.Objects)
        {
            if (obj.GetType().Name != TypeName || (HavokBehaviorSymbols.GetValue(obj, "m_name") as string ?? "") != Name)
                continue;

            if (seen++ == Ordinal)
                return obj;
        }

        return null;
    }
}

/// <summary>
/// Makes locators for the objects of one file, numbering each (type, name) once
/// </summary>
public class HavokLocatorIndex
{
    private readonly HavokBehaviorSymbols Symbols;
    private Dictionary<object, int> Ordinals;

    public HavokLocatorIndex(HavokBehaviorSymbols symbols)
    {
        Symbols = symbols;
    }

    public HavokObjectLocator Get(object obj)
    {
        if (obj == null)
            return null;

        if (Ordinals == null)
        {
            Ordinals = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);
            var counts = new Dictionary<(string, string), int>();

            foreach (var o in Symbols.Objects)
            {
                var key = (o.GetType().Name, HavokBehaviorSymbols.GetValue(o, "m_name") as string ?? "");
                var n = counts.GetValueOrDefault(key);
                Ordinals[o] = n;
                counts[key] = n + 1;
            }
        }

        return Ordinals.TryGetValue(obj, out var ordinal)
            ? new HavokObjectLocator(obj.GetType().Name, HavokBehaviorSymbols.GetValue(obj, "m_name") as string ?? "", ordinal)
            : null;
    }
}

/// <summary>
/// One result: the file it's in, what it says, and where a click leads. A result of the File scope keeps the live
/// object (<see cref="Live"/>); the others keep a locator, resolved once their file is open.
/// </summary>
public record HavokScanResult(FileDictionaryEntry Binder, string FilePath, string Text, HavokScanJump Jump, int Index,
    HavokObjectLocator Target, object Live)
{
    public string FileLabel => $"{Binder?.Filename}/{Path.GetFileName(FilePath.Replace('\\', '/'))}";
}

/// <summary>
/// What a scoped tool does with one file: adds its results through the callback
/// </summary>
public delegate void HavokScanWork(HavokBehaviorSymbols symbols, ProjectEntry project, HavokScanSink sink);

/// <summary>
/// Collects one file's results
/// </summary>
public class HavokScanSink
{
    private readonly HavokBehaviorScan Scan;
    private readonly FileDictionaryEntry Binder;
    private readonly string FilePath;
    private readonly HavokLocatorIndex Locators;
    private readonly bool KeepLive;
    private readonly int Run;

    internal HavokScanSink(HavokBehaviorScan scan, FileDictionaryEntry binder, string filePath, HavokBehaviorSymbols symbols, bool keepLive)
    {
        Scan = scan;
        Run = scan.CurrentRun;
        Binder = binder;
        FilePath = filePath;
        Locators = new HavokLocatorIndex(symbols);
        KeepLive = keepLive;
    }

    public bool IsFull => Scan.IsFull;

    public void Add(string text, object target, HavokScanJump jump = HavokScanJump.Outline, int index = -1)
    {
        Scan.AddResult(Run, new HavokScanResult(Binder, FilePath, text, jump, index,
            jump is HavokScanJump.Outline ? Locators.Get(target) : null, KeepLive ? target : null));
    }
}

/// <summary>
/// Runs a scoped tool over behavior files: the open file live, files with unsaved edits from the editor's copy (on
/// the UI thread), and the rest from their stored copy on worker threads, with progress and cancel.
/// See Claude/Documentation/Behavior.md (Havok behavior tools).
/// </summary>
public class HavokBehaviorScan
{
    public const int MaxResults = 5000;

    private readonly object ResultLock = new();
    private List<HavokScanResult> Results = new();
    private CancellationTokenSource Cancel;

    private int DoneCount;
    private int TotalCount;

    public volatile bool IsRunning;

    /// <summary>Whether the last run has finished (results are complete, or it was cancelled)</summary>
    public bool HasRun { get; private set; }

    public bool WasCancelled { get; private set; }

    public int Done => Volatile.Read(ref DoneCount);
    public int Total => Volatile.Read(ref TotalCount);

    public bool IsFull
    {
        get { lock (ResultLock) return Results.Count >= MaxResults; }
    }

    /// <summary>Files that couldn't be read in the last run</summary>
    public List<string> Failed { get; } = new();

    public IReadOnlyList<HavokScanResult> Snapshot()
    {
        lock (ResultLock) return Results.ToList();
    }

    /// <summary>Counts runs: a run that was cleared or replaced adds nothing more, and doesn't end the next one</summary>
    internal int CurrentRun => Volatile.Read(ref RunCount);
    private int RunCount;

    internal void AddResult(int run, HavokScanResult result)
    {
        lock (ResultLock)
        {
            if (run != RunCount)
                return;

            if (Results.Count < MaxResults)
                Results.Add(result);
        }
    }

    public void Clear()
    {
        CancelRun();

        // A run still finishing a binder can't add to the new results
        lock (ResultLock)
            RunCount++;

        IsRunning = false;

        lock (ResultLock) Results = new();

        Failed.Clear();
        HasRun = false;
        WasCancelled = false;
    }

    public void CancelRun()
    {
        Cancel?.Cancel();
        WasCancelled = IsRunning;
    }

    /// <summary>
    /// Starts a run. The File scope runs at once on the open file; the others also run on the open binder's
    /// unsaved files at once, then read the remaining files in the background.
    /// </summary>
    public void Start(HavokEditorView view, ProjectEntry project, HavokBehaviorScope scope, HavokScanWork work)
    {
        Clear();

        var bank = view.HavokBank;
        var currentBinder = view.Selection.BinderFileEntry;
        var currentFile = view.Selection.FilePath;
        var behaviorView = view.PropertyView.BehaviorView;

        if (scope is HavokBehaviorScope.File)
        {
            if (behaviorView.Symbols != null && currentBinder != null && currentFile != null)
                work(behaviorView.Symbols, project, new HavokScanSink(this, currentBinder, currentFile, behaviorView.Symbols, true));

            HasRun = true;
            return;
        }

        // The binders in scope
        var binders = scope switch
        {
            HavokBehaviorScope.Binder => currentBinder == null ? new List<FileDictionaryEntry>() : new() { currentBinder },
            HavokBehaviorScope.Modified => project.Locator.HavokBehaviorFiles.Entries
                .Where(b => project.VFS.FileStatus(b.Path) is VanillaStatus.Modified or VanillaStatus.Added).ToList(),
            _ => project.Locator.HavokBehaviorFiles.Entries.ToList()
        };

        // Files with unsaved edits: from the editor's copy, now (the worker must not read a graph the UI edits)
        var fromEditor = new HashSet<(FileDictionaryEntry, string)>();

        foreach (var (binder, files) in bank.BehaviorBank)
        {
            if (scope is HavokBehaviorScope.Binder && !ReferenceEquals(binder, currentBinder))
                continue;

            foreach (var (path, wrapper) in files)
            {
                if (!wrapper.IsDirty || !wrapper.IsLoaded() || !IsBehaviorGraphPath(path))
                    continue;

                var symbols = HavokBehaviorSymbols.Build(wrapper.Root, view.PropertyCache.GetCachedHavokFields);

                if (symbols != null)
                    work(symbols, project, new HavokScanSink(this, binder, path, symbols, false));

                fromEditor.Add((binder, path));

                if (!binders.Contains(binder))
                    binders.Add(binder);
            }
        }

        binders = binders.OrderBy(b => b.Filename, StringComparer.OrdinalIgnoreCase).ToList();

        TotalCount = binders.Count;
        DoneCount = 0;
        IsRunning = true;
        Cancel = new CancellationTokenSource();

        var token = Cancel.Token;
        var onlyChanged = scope is HavokBehaviorScope.Modified;

        var run = CurrentRun;

        Task.Run(async () =>
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2),
                CancellationToken = token
            };

            try
            {
                await Parallel.ForEachAsync(binders, options, (binder, ct) =>
                {
                    // The editor's field cache isn't thread-safe: one per binder, with each file's own profile
                    var cache = new HavokPropertyCache();
                    Havok.HavokProfile profile = null;
                    cache.ProfileSource = () => profile;

                    try
                    {
                        var failed = bank.ReadStoredBehaviorFiles(binder, onlyChanged,
                            path => !fromEditor.Contains((binder, path)),
                            (path, document) =>
                            {
                                if (IsFull)
                                    return;

                                profile = document.Profile;
                                var symbols = HavokBehaviorSymbols.Build(document.Root, cache.GetCachedHavokFields);

                                if (symbols != null)
                                    work(symbols, project, new HavokScanSink(this, binder, path, symbols, false));
                            }, ct);

                        if (failed.Count > 0 && run == CurrentRun)
                        {
                            lock (Failed)
                                Failed.AddRange(failed.Select(f => $"{binder.Filename}/{Path.GetFileName(f.Replace('\\', '/'))}"));
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        if (run == CurrentRun)
                        {
                            lock (Failed)
                                Failed.Add(binder.Filename);
                        }
                    }

                    Interlocked.Increment(ref DoneCount);

                    return ValueTask.CompletedTask;
                });
            }
            catch (OperationCanceledException)
            {
                if (run == CurrentRun)
                    WasCancelled = true;
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_Scan_Failed"), ex);
            }

            // A run cleared meanwhile leaves the state to the run after it
            if (run == CurrentRun)
            {
                IsRunning = false;
                HasRun = true;
            }
        });
    }

    /// <summary>
    /// A behbnd's behavior graphs are in its Behaviors folder. Full paths in most games; DB_NT's binder names are
    /// relative (Behaviors\c0000.hkx), hence the leading slash.
    /// </summary>
    public static bool IsBehaviorGraphPath(string path)
    {
        var name = "/" + path.Replace('\\', '/');
        return name.Contains("/Behaviors/", StringComparison.OrdinalIgnoreCase) && name.Contains(".hkx", StringComparison.OrdinalIgnoreCase);
    }

    #region UI
    /// <summary>
    /// The scope radio buttons. False (with the reason shown) when the scope needs an open file there isn't.
    /// </summary>
    public static bool DrawScope(string id, ref HavokBehaviorScope scope, bool hasFile)
    {
        foreach (var value in Enum.GetValues<HavokBehaviorScope>())
        {
            if (value != HavokBehaviorScope.File)
                ImGui.SameLine();

            if (ImGui.RadioButton($"{LOC.Get($"HAVOK_Scope_{value}")}##{id}_{value}", scope == value))
                scope = value;

            GUI.Tooltip(LOC.Get($"HAVOK_Scope_{value}_TT"));
        }

        if (scope is HavokBehaviorScope.File or HavokBehaviorScope.Binder && !hasFile)
        {
            GUI.WrappedText(LOC.Get("HAVOK_Scope_Needs_File"));
            return false;
        }

        return true;
    }

    /// <summary>
    /// The progress line of a running scan, with Cancel; and the cap / failure notes of a finished one
    /// </summary>
    public void DrawStatus(string id)
    {
        if (IsRunning)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("HAVOK_Scan_Progress", Done, Total));
            ImGui.SameLine();

            if (ImGui.SmallButton($"{LOC.Get("HAVOK_Scan_Cancel")}##{id}_cancel"))
                CancelRun();

            return;
        }

        if (WasCancelled)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Scan_Cancelled"));

        if (IsFull)
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Scan_Capped", MaxResults));

        int failed;
        lock (Failed) failed = Failed.Count;

        if (failed > 0)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("HAVOK_Scan_Failed_Files", failed));

            string list;
            lock (Failed) list = string.Join("\n", Failed.Take(30));
            GUI.Tooltip(list);
        }
    }

    /// <summary>
    /// The results, one row each (file, then text), clipped. Clicking a row jumps to it.
    /// </summary>
    public static void DrawResults(string id, IReadOnlyList<HavokScanResult> results, HavokBehaviorView owner, bool showFile)
    {
        ImGui.BeginChild($"{id}_results");

        var clipper = new ImGuiListClipper();
        clipper.Begin(results.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var r = results[i];

                if (ImGui.Selectable($"##{id}_row{i}", false, ImGuiSelectableFlags.AllowOverlap))
                    owner.JumpTo(r);

                GUI.Tooltip(LOC.Get("HAVOK_Scan_Result_TT"));

                ImGui.SameLine(0, 0);

                if (showFile)
                {
                    ImGui.TextDisabled(r.FileLabel);
                    ImGui.SameLine();
                }

                ImGui.Text(r.Text);
            }
        }

        clipper.End();

        ImGui.EndChild();
    }
    #endregion
}
