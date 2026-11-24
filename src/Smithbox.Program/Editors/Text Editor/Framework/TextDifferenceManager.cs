using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

public class TextDifferenceManager
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public TextDifferenceManager(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// Entries of the compared FMG that are unique to the project (no vanilla entry with their ID).
    /// </summary>
    /// <remarks>
    /// Keyed by the entry object. These were string-keyed by ID ($"{entry.ID}", plus the FMG
    /// name and sub-category for DS2), which allocated a string for every lookup: several per
    /// row per frame in the entry list. The cache only ever describes one FMG, so the entry
    /// itself is enough, and a duplicate-ID entry no longer shares its twin's colour.
    /// </remarks>
    private HashSet<FMG.Entry> AdditionCache = new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// Entries of the compared FMG whose text differs from vanilla (unique entries included).
    /// </summary>
    private HashSet<FMG.Entry> DifferenceCache = new(ReferenceEqualityComparer.Instance);

    private bool CacheFilled = false;

    /// <summary>
    /// The compared vanilla FMG's text by ID, for Revert to Vanilla and the vanilla tooltip
    /// </summary>
    private Dictionary<int, string> VanillaTextById = new();

    /// <summary>
    /// Vanilla entries whose ID the mod's FMG doesn't have (deleted by the mod), in vanilla order.
    /// Listed by the entry list's 'deleted' filter command.
    /// </summary>
    public IReadOnlyList<FMG.Entry> DeletedEntries => _deletedEntries;
    private List<FMG.Entry> _deletedEntries = new();

    /// <summary>
    /// Changes on every rebuild, so caches built from the 'modified' / 'unique' state
    /// (the entry list's filtered rows) know when to rebuild too.
    /// </summary>
    public int Version { get; private set; }

    // Batching: inside a grouped action every sub-action called TrackFmgDifferences,
    // rebuilding the whole cache each time (5k duplicated entries = 5k full rebuilds).
    // While a batch is open, requests are recorded and run once when it closes.
    // UI thread only, like all action execution.
    private static int BatchDepth = 0;
    private static readonly HashSet<TextDifferenceManager> PendingManagers = new();

    public static void BeginBatch()
    {
        BatchDepth++;
    }

    public static void EndBatch()
    {
        if (BatchDepth == 0)
            return;

        BatchDepth--;

        if (BatchDepth > 0 || PendingManagers.Count == 0)
            return;

        var pending = PendingManagers.ToList();
        PendingManagers.Clear();

        foreach (var manager in pending)
        {
            manager.TrackFmgDifferences();
        }
    }

    /// <summary>
    /// Generate difference truth for currently selected FMG
    /// </summary>
    public void TrackFmgDifferences(int setFmgId = -1)
    {
        // Defer selection-based rebuilds while a batch is open. Explicit FMG requests
        // (from the exporter) still run immediately, as their caller reads the result.
        if (BatchDepth > 0 && setFmgId == -1)
        {
            PendingManagers.Add(this);
            return;
        }

        Version++;

        CacheFilled = false;
        AdditionCache = new(ReferenceEqualityComparer.Instance);
        DifferenceCache = new(ReferenceEqualityComparer.Instance);
        VanillaTextById = new();
        _deletedEntries = new();

        var selectedContainer = Parent.Selection.SelectedContainerWrapper;

        if (selectedContainer == null)
            return;

        // No FMG selected (and none passed in): nothing to compare.
        // Undo/redo with no FMG selected used to throw here.
        if (Parent.Selection.SelectedFmgWrapper == null && setFmgId == -1)
            return;

        // Leave empty if disabled
        if(!CFG.Current.TextEditor.TextEditor_Include_Vanilla_Cache)
        {
            return;
        }

        var containerCategory = selectedContainer.ContainerDisplayCategory;
        var containerSubCategory = selectedContainer.ContainerDisplaySubCategory;
        var containerName = selectedContainer.FileEntry.Filename;

        // Fmg ID for comparison is selected FMG
        var fmgID = Parent.Selection.SelectedFmgWrapper?.ID ?? -1;

        // Set the ID to passed instead of selected if called from the FmgExporter
        if(setFmgId != -1)
        {
            fmgID = setFmgId;
        }

        // Get vanilla container and entries
        var isDS2 = Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S;

        var vanillaContainer = Project.Handler.TextData.VanillaBank.Containers
            .Where(e => e.Value.ContainerDisplayCategory == containerCategory)
            .Where(e => !isDS2 || e.Value.ContainerDisplaySubCategory == containerSubCategory)
            .Where(e => e.Value.FileEntry.Filename == containerName)
            .FirstOrDefault();

        if (vanillaContainer.Value?.FmgWrappers == null)
            return;

        var vanillaFmg = vanillaContainer.Value.FmgWrappers
            .FirstOrDefault(e => e.ID == fmgID);

        if (vanillaFmg == null)
            return;

        // Vanilla text by ID (the last entry wins if vanilla repeats an ID, as before)
        var vanillaEntries = new Dictionary<int, string>(vanillaFmg.File.Entries.Count);

        foreach (var entry in vanillaFmg.File.Entries)
        {
            vanillaEntries[entry.ID] = entry.Text;
        }

        // The primary side is the selected container itself. It used to be looked up again by
        // category and filename, which for DES (compressed and uncompressed copies share a
        // filename) could compare a different container than the one on screen.
        var primaryFmg = selectedContainer.FmgWrappers?
            .FirstOrDefault(e => e.ID == fmgID);

        if (primaryFmg == null)
            return;

        var primaryIds = new HashSet<int>(primaryFmg.File.Entries.Count);

        foreach (var entry in primaryFmg.File.Entries)
        {
            primaryIds.Add(entry.ID);

            if (vanillaEntries.TryGetValue(entry.ID, out var vanillaText))
            {
                // A null vanilla entry with project text counts as different
                if (vanillaText != null ? !vanillaText.Equals(entry.Text) : entry.Text != null)
                {
                    DifferenceCache.Add(entry);
                }
            }
            // Is a mod-unique row, there it is a difference
            else
            {
                AdditionCache.Add(entry);
                DifferenceCache.Add(entry);
            }
        }

        // Vanilla entries the mod no longer has (the first entry of a repeated vanilla ID only)
        var listedDeleted = new HashSet<int>();

        foreach (var vanillaEntry in vanillaFmg.File.Entries)
        {
            if (!primaryIds.Contains(vanillaEntry.ID) && listedDeleted.Add(vanillaEntry.ID))
            {
                _deletedEntries.Add(vanillaEntry);
            }
        }

        VanillaTextById = vanillaEntries;

        CacheFilled = true;
    }

    /// <summary>
    /// The vanilla text for an entry of the compared FMG. False for entries unique to the mod,
    /// and when the vanilla comparison is off or unavailable.
    /// </summary>
    public bool TryGetVanillaText(FMG.Entry entry, out string text)
    {
        text = null;

        if (!CacheFilled || entry == null)
            return false;

        return VanillaTextById.TryGetValue(entry.ID, out text);
    }

    public bool IsDifferentToVanilla(FMG.Entry entry)
    {
        // Ignore if the cache is being filled, or the bank isn't loaded
        if (!CacheFilled || entry == null)
            return false;

        return DifferenceCache.Contains(entry);
    }

    public bool IsUniqueToProject(FMG.Entry entry)
    {
        // Ignore if the cache is being filled, or the bank isn't loaded
        if (!CacheFilled || entry == null)
            return false;

        return AdditionCache.Contains(entry);
    }

    /// <summary>The shared status of an entry of the compared FMG, for VanillaMarkers</summary>
    public VanillaStatus EntryStatus(FMG.Entry entry)
    {
        if (IsUniqueToProject(entry))
            return VanillaStatus.Added;

        return IsDifferentToVanilla(entry) ? VanillaStatus.Modified : VanillaStatus.Unchanged;
    }

    // Per FMG of any container, the status worked out at a Version: every edit and selection rebuilds the compared
    // FMG (Version goes up), which is when another FMG's status may have changed too
    private readonly Dictionary<TextFmgWrapper, (int Version, VanillaStatus Status)> FmgStatusCache =
        new(ReferenceEqualityComparer.Instance);

    /// <summary>
    /// How an FMG compares with the game's FMG of the same ID in the same container, for the FMG list: Added when
    /// the game has no such container or FMG, Modified when any entry is added, removed or has different text,
    /// else Unchanged. None when the vanilla comparison is off.
    /// </summary>
    public VanillaStatus FmgStatus(TextContainerWrapper container, TextFmgWrapper fmg)
    {
        if (!CFG.Current.TextEditor.TextEditor_Include_Vanilla_Cache || container == null || fmg?.File == null)
            return VanillaStatus.None;

        if (FmgStatusCache.TryGetValue(fmg, out var cached) && cached.Version == Version)
            return cached.Status;

        var status = ComputeFmgStatus(container, fmg);
        FmgStatusCache[fmg] = (Version, status);
        return status;
    }

    private VanillaStatus ComputeFmgStatus(TextContainerWrapper container, TextFmgWrapper fmg)
    {
        var vanillaContainer = FindVanillaContainer(container);
        if (vanillaContainer?.FmgWrappers == null)
            return VanillaStatus.Added;

        var vanillaFmg = vanillaContainer.FmgWrappers.FirstOrDefault(e => e.ID == fmg.ID);
        if (vanillaFmg?.File == null)
            return VanillaStatus.Added;

        var vanillaEntries = vanillaFmg.File.Entries;
        var entries = fmg.File.Entries;

        var vanillaText = new Dictionary<int, string>(vanillaEntries.Count);
        foreach (var e in vanillaEntries)
            vanillaText[e.ID] = e.Text;

        var seen = new HashSet<int>();
        foreach (var e in entries)
        {
            if (!vanillaText.TryGetValue(e.ID, out var text))
                return VanillaStatus.Modified;

            if (text != null ? !text.Equals(e.Text) : e.Text != null)
                return VanillaStatus.Modified;

            seen.Add(e.ID);
        }

        // An entry the mod removed
        return seen.Count < vanillaText.Count ? VanillaStatus.Modified : VanillaStatus.Unchanged;
    }

    /// <summary>The vanilla bank's container matching a project container (category, DS2 sub-category, file name)</summary>
    private TextContainerWrapper FindVanillaContainer(TextContainerWrapper container)
    {
        var isDS2 = Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S;

        return Project.Handler.TextData.VanillaBank.Containers
            .Where(e => e.Value.ContainerDisplayCategory == container.ContainerDisplayCategory)
            .Where(e => !isDS2 || e.Value.ContainerDisplaySubCategory == container.ContainerDisplaySubCategory)
            .Where(e => e.Value.FileEntry.Filename == container.FileEntry.Filename)
            .Select(e => e.Value)
            .FirstOrDefault();
    }
}
