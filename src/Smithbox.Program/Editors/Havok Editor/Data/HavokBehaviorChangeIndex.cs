using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The steps of reading the game's copy of a behavior file and comparing it, in order, for the progress bar
/// </summary>
public enum HavokReadStage { ReadBinder, Decompress, Parse, Index, Compare }

/// <summary>
/// The shown behavior file's changes against the game's copy (HavokBehaviorDiff), kept up to date for the behavior
/// lists' vanilla colours (VanillaMarkers: Modified green, Added blue) and the Changes tool.
/// <para>
/// The game's copy is only read for a file the mod changes (its stored bytes differ, or it has unsaved edits), in the
/// background. The comparison reads the live objects, so it runs on the UI thread: when the game's copy is ready,
/// and again half a second after the last edit, undo or redo (so a run of edits costs one comparison).
/// A file the game doesn't have is Added throughout.
/// </para>
/// </summary>
public class HavokBehaviorChangeIndex
{
    private readonly HavokEditorView View;
    private readonly ProjectEntry Project;

    private const double SettleSeconds = 0.5;

    private HavokFileWrapper File;
    private HavokBehaviorSymbols VanillaSymbols;
    private volatile bool Loading;
    private volatile bool Loaded;
    private bool Subscribed;

    private bool Dirty = true;
    private double DirtySince;
    private HavokBehaviorSymbols DiffSymbols;

    // The symbols seen last frame, to notice when they're rebuilt
    private HavokBehaviorSymbols SeenSymbols;

    private Dictionary<object, VanillaStatus> ObjectStatuses = new(ReferenceEqualityComparer.Instance);
    private VanillaStatus[] EventStatuses = Array.Empty<VanillaStatus>();
    private VanillaStatus[] VariableStatuses = Array.Empty<VanillaStatus>();

    /// <summary>The comparison of the shown file, or null if there is none (yet, or the file is unchanged)</summary>
    public HavokBehaviorDiff Diff { get; private set; }

    /// <summary>The game has no copy of the shown file: everything in it is Added</summary>
    public bool IsNewFile { get; private set; }

    /// <summary>The shown file is the game's (stored bytes the same, no unsaved edits): nothing has changed</summary>
    public bool IsUnchanged { get; private set; }

    /// <summary>The game's copy is being read, or the comparison is waiting for edits to settle</summary>
    public bool IsPending => Loading || (Loaded && (Dirty || Diff == null) && !IsNewFile && !IsUnchanged);

    /// <summary>Whether the game's copy is being read (the progress bar is shown)</summary>
    public bool IsLoading => Loading;

    // The read's current stage, when the read and that stage started (Environment.TickCount64, as the worker sets them)
    private volatile int Stage;
    private long LoadStartedMs;

    private void SetStage(HavokReadStage stage)
    {
        Stage = (int)stage;
    }

    /// <summary>
    /// The read's progress: a bar filled by the stages done, labelled with the current stage, the file and how long
    /// it has run. Parsing a large file (ER c0000) is one long step, so the elapsed time shows it's still going.
    /// </summary>
    public void DrawProgress(string id)
    {
        if (!Loading)
            return;

        var stage = (HavokReadStage)Stage;
        var stageCount = Enum.GetValues<HavokReadStage>().Length;
        var seconds = (Environment.TickCount64 - Interlocked.Read(ref LoadStartedMs)) / 1000.0;
        var fileName = Path.GetFileName((File?.Key ?? "").Replace('\\', '/'));

        ImGui.ProgressBar((float)Stage / stageCount, new Vector2(-1, 0),
            LOC.Get("HAVOK_Changes_Progress", LOC.Get($"HAVOK_Changes_Stage_{stage}"), fileName, seconds.ToString("0.0")));

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(LOC.Get("HAVOK_Changes_Progress_TT", (int)stage + 1, stageCount));
    }

    public HavokBehaviorChangeIndex(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Called once a frame, after the shown file's symbols are built
    /// </summary>
    public void Update(HavokBehaviorSymbols symbols)
    {
        var file = View.Selection.SelectedFileWrapper;

        if (symbols == null || file == null)
        {
            Reset(null);
            return;
        }

        if (!Subscribed && View.ActionManager != null)
        {
            View.ActionManager.ActionPerformed += MarkDirty;
            Subscribed = true;
        }

        if (!ReferenceEquals(file, File))
            Reset(file);

        // New symbols (another file, or rebuilt after an edit): compare again once they've settled. Only on the change:
        // marking every frame until the comparison ran would keep pushing the settle time back, so it never ran.
        if (!ReferenceEquals(symbols, SeenSymbols))
        {
            SeenSymbols = symbols;
            MarkDirty();
        }

        var stored = View.HavokBank.EntryStatus(View.Selection.BinderFileEntry, false, file.Key);

        IsNewFile = stored is VanillaStatus.Added;

        // Still comparing the binders: wait
        if (stored is VanillaStatus.None && !file.IsDirty)
            return;

        IsUnchanged = stored is VanillaStatus.Unchanged && !file.IsDirty;

        if (IsNewFile || IsUnchanged)
        {
            ClearStatuses();
            DiffSymbols = symbols;
            Dirty = false;
            return;
        }

        if (!Loaded)
        {
            StartLoad(file);
            return;
        }

        if (VanillaSymbols == null)
        {
            // The game's copy couldn't be read: no colours
            ClearStatuses();
            Loading = false;
            return;
        }

        if (!Dirty || ImGui_Time() - DirtySince < SettleSeconds)
            return;

        Dirty = false;
        DiffSymbols = symbols;
        Diff = HavokBehaviorDiff.Build(Project, symbols, VanillaSymbols, View.PropertyCache.GetCachedHavokFields);
        BuildStatuses(symbols);

        // The first comparison ends the read's progress bar
        Loading = false;
    }

    private static double ImGui_Time() => Hexa.NET.ImGui.ImGui.GetTime();

    private void MarkDirty()
    {
        Dirty = true;
        DirtySince = ImGui_Time();
    }

    private void Reset(HavokFileWrapper file)
    {
        File = file;
        VanillaSymbols = null;
        Loaded = false;
        Loading = false;
        IsNewFile = false;
        IsUnchanged = false;
        DiffSymbols = null;
        SeenSymbols = null;
        Dirty = true;
        DirtySince = 0;
        ClearStatuses();
    }

    private void ClearStatuses()
    {
        Diff = null;
        ObjectStatuses = new(ReferenceEqualityComparer.Instance);
        EventStatuses = Array.Empty<VanillaStatus>();
        VariableStatuses = Array.Empty<VanillaStatus>();
    }

    private void StartLoad(HavokFileWrapper file)
    {
        if (Loading)
            return;

        Loading = true;
        Interlocked.Exchange(ref LoadStartedMs, Environment.TickCount64);
        SetStage(HavokReadStage.ReadBinder);

        var bank = View.HavokBank;
        var binder = View.Selection.BinderFileEntry;
        var key = file.Key;

        Task.Run(() =>
        {
            HavokBehaviorSymbols symbols = null;

            try
            {
                var document = bank.ReadVanillaDocument(binder, false, key, SetStage);

                if (document != null)
                {
                    SetStage(HavokReadStage.Index);

                    // A field cache of its own: the editor's isn't thread-safe
                    var cache = new HavokPropertyCache { ProfileSource = () => document.Profile };
                    symbols = HavokBehaviorSymbols.Build(document.Root, cache.GetCachedHavokFields);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_Changes_Read_Failed", key), e);
            }

            // Dropped if another file was shown meanwhile
            if (!ReferenceEquals(File, file))
                return;

            // The comparison runs on the UI thread next frame, which ends the read (Loading stays set until then)
            SetStage(HavokReadStage.Compare);
            VanillaSymbols = symbols;
            Loaded = true;
        });
    }

    /// <summary>
    /// The per-object, per-event and per-variable statuses the lists colour with
    /// </summary>
    private void BuildStatuses(HavokBehaviorSymbols symbols)
    {
        ObjectStatuses = new(ReferenceEqualityComparer.Instance);

        foreach (var change in Diff.Changes)
        {
            if (change.Kind is HavokChangeKind.Object && change.Live != null)
                ObjectStatuses[change.Live] = change.Status is HavokChangeStatus.Added ? VanillaStatus.Added : VanillaStatus.Modified;
        }

        // Events by name: a name's occurrences beyond the game's count are Added (DS3 has duplicate names)
        var vanillaCounts = Count(VanillaSymbols.EventNames);
        var seen = new Dictionary<string, int>();
        var eventNames = symbols.EventNames;
        EventStatuses = new VanillaStatus[eventNames?.Count ?? 0];

        for (var i = 0; i < EventStatuses.Length; i++)
        {
            var name = eventNames[i] as string ?? "";
            var n = seen.GetValueOrDefault(name) + 1;
            seen[name] = n;
            EventStatuses[i] = n > vanillaCounts.GetValueOrDefault(name) ? VanillaStatus.Added : VanillaStatus.Unchanged;
        }

        // Variables by name
        var vanillaVariables = Count(VanillaSymbols.VariableNames);
        var modifiedVariables = Diff.Changes
            .Where(c => c.Kind is HavokChangeKind.Variable && c.Status is HavokChangeStatus.Modified)
            .Select(c => c.Name).ToHashSet();
        var variableNames = symbols.VariableNames;
        VariableStatuses = new VanillaStatus[variableNames?.Count ?? 0];

        for (var i = 0; i < VariableStatuses.Length; i++)
        {
            var name = variableNames[i] as string ?? "";
            VariableStatuses[i] = !vanillaVariables.ContainsKey(name) ? VanillaStatus.Added
                : modifiedVariables.Contains(name) ? VanillaStatus.Modified
                : VanillaStatus.Unchanged;
        }
    }

    private static Dictionary<string, int> Count(System.Collections.IList names)
    {
        var counts = new Dictionary<string, int>();

        if (names == null)
            return counts;

        foreach (var name in names)
        {
            var text = name as string ?? "";
            counts[text] = counts.GetValueOrDefault(text) + 1;
        }

        return counts;
    }

    #region Statuses
    public VanillaStatus ObjectStatus(object obj)
    {
        if (obj == null)
            return VanillaStatus.None;

        if (IsNewFile)
            return VanillaStatus.Added;

        return ObjectStatuses.TryGetValue(obj, out var status) ? status : VanillaStatus.None;
    }

    public VanillaStatus EventStatus(int index)
    {
        if (IsNewFile)
            return VanillaStatus.Added;

        return index >= 0 && index < EventStatuses.Length ? EventStatuses[index] : VanillaStatus.None;
    }

    public VanillaStatus VariableStatus(int index)
    {
        if (IsNewFile)
            return VanillaStatus.Added;

        return index >= 0 && index < VariableStatuses.Length ? VariableStatuses[index] : VanillaStatus.None;
    }
    #endregion
}
