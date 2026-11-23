using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>What the Effect List shows, besides the search (FX Effect List Actions Plan, phase C).</summary>
public enum FxListFilter
{
    All,
    Modified,
    Added,
    Unsaved,
    Favorites,
    Recent,
    UsesResource,
    Problems,
}

/// <summary>
/// Every effect id in the project's sfx binders, with alias names, a search and filters, and the actions on whole
/// effects (FX Effect List Actions Plan): duplicating, binder membership, deleting, reverting, Used By, comparing,
/// importing and exporting, changing an id and the Project Check. The dialogs are in FxEffectListDialogs.cs.
/// </summary>
public partial class FxEffectList
{
    private readonly FxEditorView Editor;

    private string Filter = "";
    private bool ExactFilter = false;
    private FxListFilter Mode = FxListFilter.All;
    private string ResourceText = "";

    // Filtered list cache, rebuilt when the filter, toggle or bank changes (and once a second while a filter that
    // depends on edits is on)
    private List<FxEntry> Cache = new();
    private object CacheKey;
    private DateTime CacheTime;

    /// <summary>Bumped by every edit, undo and redo.</summary>
    private int Edits => Editor.Data?.EditVersion ?? 0;

    public FxUsage Usage;
    public FxExternalReferences External;
    public FxFavorites Favorites;
    public FxProjectCheck ProjectCheck;

    /// <summary>The effects picked with Ctrl / Shift + click. The selected effect is always one of them.</summary>
    private readonly HashSet<FxEntry> Picked = new();
    private FxEntry Anchor;

    public FxEffectList(FxEditorView editor)
    {
        Editor = editor;
        // Shared by every view: one scan, one favorites list
        Usage = editor.Editor.Usage;
        External = editor.Editor.External;
        Favorites = editor.Editor.Favorites;
        ProjectCheck = editor.Editor.ProjectCheck;
    }

    private FxBank Bank => Editor.Data.Bank;
    private FxData Data => Editor.Data;

    /// <summary>Shows the effects using a texture or model (the Resources tool's link).</summary>
    public void ShowUsesResource(string resource)
    {
        Mode = FxListFilter.UsesResource;
        ResourceText = resource;
        Usage.StartScan();
    }

    public void Display()
    {
        Usage.Update();
        External.Update();

        GUI.TitleHeader(LOC.Get("FX_EffectList_Header"), LOC.Get("FX_EffectList_Header_TT"));

        // Search
        ImGui.BeginChild("FxEffectSearchSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("fxEditor_EffectList", ref Filter, ref ExactFilter);

        // Toggle: Aliases
        GUI.DisplayToggleButton("fxAliasToggle", Icons.StickyNote,
            ref CFG.Current.FxEditor_EffectList_Display_Aliases,
            "FX_EffectList_Alias_Toggle_Hide",
            "FX_EffectList_Alias_Toggle_Show",
            "FX_EffectList_Alias_Toggle_TT");

        // Toggle: Flat list / grouped by binder
        GUI.DisplayToggleButton("fxGroupToggle", Icons.FolderOpen,
            ref CFG.Current.FxEditor_EffectList_Group_By_Binder,
            "FX_EffectList_Group_Toggle_Flat",
            "FX_EffectList_Group_Toggle_Grouped",
            "FX_EffectList_Group_Toggle_TT");
        ImGui.EndChild();

        // Filters and the list-wide actions
        ImGui.BeginChild("FxEffectFilterSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        DisplayFilterBar();
        ImGui.EndChild();

        ImGui.BeginChild("FxEffectListSection", ImGuiChildFlags.Borders);

        if (!Bank.IsIndexed)
        {
            // Indexing runs on a background task; show how far it is
            GUI.WrappedText(LOC.Get("FX_EffectList_Indexing"));
            float fraction = Bank.TotalBinders == 0 ? 0 : (float)Bank.IndexedBinders / Bank.TotalBinders;
            ImGui.ProgressBar(fraction, new Vector2(-1, 0), $"{Bank.IndexedBinders} / {Bank.TotalBinders}");
            ImGui.EndChild();
            return;
        }

        if (Bank.Entries.Count == 0)
        {
            GUI.WrappedText(LOC.Get("FX_EffectList_Empty"));
            ImGui.EndChild();
            return;
        }

        RebuildCache();

        // A jump from another editor to an effect the filter hides: clear the filter so the list can show it
        if (Editor.Selection.FocusEntry && Editor.Selection.Entry != null && (Filter != "" || Mode != FxListFilter.All)
            && !Cache.Contains(Editor.Selection.Entry))
        {
            Filter = "";
            Mode = FxListFilter.All;
            RebuildCache();
        }

        if (Mode == FxListFilter.UsesResource && !Usage.HasScan)
            ScanProgress();

        DrawnLists.Clear();
        if (CFG.Current.FxEditor_EffectList_Group_By_Binder)
            DisplayGrouped();
        else
            DisplayRows(Cache, true, "");

        // A step's scroll waits for its list; a group collapsed meanwhile drops it
        if (StepScrollKey != null && !DrawnLists.Any(l => l.Key == StepScrollKey))
            StepScrollKey = null;

        HandleKeys();

        ImGui.EndChild();

        DisplayDialogs();
        RunDeferred();
    }

    // ---------------------------------------------------------------- filters

    private void DisplayFilterBar()
    {
        ImGui.SetNextItemWidth(150 * DPI.UIScale());
        if (ImGui.BeginCombo("##fxListFilter", LOC.Get($"FX_EffectList_Filter_{Mode}")))
        {
            foreach (var mode in Enum.GetValues<FxListFilter>())
            {
                if (ImGui.Selectable(LOC.Get($"FX_EffectList_Filter_{mode}"), mode == Mode))
                {
                    Mode = mode;
                    if (mode == FxListFilter.UsesResource)
                        Usage.StartScan();
                    if (mode == FxListFilter.Problems && ProjectCheck.RanAt == null)
                        ProjectCheck.Run();
                }
                GUI.Tooltip(LOC.Get($"FX_EffectList_Filter_{mode}_TT"));
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("FX_EffectList_Filter_TT"));

        if (Mode == FxListFilter.UsesResource)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(100 * DPI.UIScale());
            ImGui.InputTextWithHint("##fxListResource", LOC.Get("FX_EffectList_Resource_Hint"), ref ResourceText, 32);
            GUI.Tooltip(LOC.Get("FX_EffectList_Resource_TT"));
        }

        // Import an effect file: every format
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.Download}##fxImport", DPI.IconButtonSize))
            RequestImport();
        GUI.Tooltip(LOC.Get("FX_EffectList_Import_TT", Ext));

        // Project Check: FXR3 only (its checks read FXR nodes)
        bool structure = Data.CanEditStructure;
        ImGui.BeginDisabled(!structure);
        ImGui.SameLine();
        if (ImGui.Button($"{Icons.CheckSquare}##fxProjectCheck", DPI.IconButtonSize))
        {
            ProjectCheck.Run();
            ProjectCheckToOpen = true;
        }
        TooltipEvenDisabled(LOC.Get(!structure ? "FX_Structure_TT" : "FX_EffectList_Project_Check_TT"));
        ImGui.EndDisabled();
    }

    /// <summary>A tooltip that also shows over a disabled item, so it can say why it's disabled.</summary>
    private static void TooltipEvenDisabled(string text)
    {
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            ImGui.SetTooltip(text);
    }

    private void ScanProgress()
    {
        if (!Usage.IsScanning)
        {
            Usage.StartScan();
            return;
        }
        float fraction = Usage.ScanTotal == 0 ? 0 : (float)Usage.ScanDone / Usage.ScanTotal;
        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("FX_EffectList_Scanning", Usage.ScanDone, Usage.ScanTotal));
    }

    private bool PassesMode(FxEntry e) => Mode switch
    {
        FxListFilter.Modified => e.CombinedVanilla == FxVanillaState.Modified,
        FxListFilter.Added => e.CombinedVanilla == FxVanillaState.Added,
        FxListFilter.Unsaved => e.AnyUnsaved,
        FxListFilter.Favorites => Favorites.IsFavorite(e.Id),
        FxListFilter.Recent => Favorites.Recent.Contains(e.Id),
        FxListFilter.UsesResource => Usage.UsesResource(e, ResourceText),
        FxListFilter.Problems => ProjectCheck.WithProblems.Contains(e.Id),
        _ => true,
    };

    private void RebuildCache()
    {
        bool aliases = CFG.Current.FxEditor_EffectList_Display_Aliases;
        var key = (Bank.Version, Filter, ExactFilter, aliases, Mode, ResourceText, Edits, Usage.HasScan,
            ProjectCheck.RanAt, Favorites.Count, Favorites.Recent.Count > 0 ? Favorites.Recent[0] : -1);

        // Unsaved, Recent and the resource filter follow edits and saves that don't change the key, so they're
        // refreshed once a second too
        bool timed = Mode is FxListFilter.Unsaved or FxListFilter.Recent or FxListFilter.UsesResource
            && (DateTime.Now - CacheTime).TotalSeconds > 1;
        // The markers and the dirty star follow saves too
        timed |= (DateTime.Now - CacheTime).TotalSeconds > 2;
        if (key.Equals(CacheKey) && !timed)
            return;

        // Searching opens every group once, so the matches are visible; they can be collapsed after
        if (PreviousFilter != Filter && !string.IsNullOrEmpty(Filter))
            OpenGroupsOnce = true;
        PreviousFilter = Filter;

        CacheKey = key;
        CacheTime = DateTime.Now;
        var entries = Bank.Entries.Values
            .Where(e => EditorFilters.IsMatch(Filter, e.Id.ToString(), ExactFilter, () => e.Vanilla.ToStatus(), e.Alias, false, true,
                string.Join(" ", e.Binders.Select(b => b.Binder.Filename))))
            .Where(PassesMode);

        // Recent lists most recent first
        Cache = Mode == FxListFilter.Recent
            ? entries.OrderBy(e => Favorites.Recent.IndexOf(e.Id)).ToList()
            : entries.ToList();

        // The same effects by binder, binders in name order, effects in id order within each
        Groups = Cache
            .SelectMany(e => e.Binders.Select(b => (binder: b.Binder, entry: e)))
            .GroupBy(x => x.binder.Path, StringComparer.OrdinalIgnoreCase)
            .Select(g => (path: g.Key, name: g.First().binder.Filename, rows: g.Select(x => x.entry).Distinct().ToList()))
            .OrderBy(g => g.name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Picked effects that left the bank (deleted, or an undone duplicate) aren't picked any more
        Picked.RemoveWhere(e => !Bank.Entries.TryGetValue(e.Id, out var x) || x != e);
    }

    private string PreviousFilter;

    // Grouped view cache: built with the flat one
    private List<(string path, string name, List<FxEntry> rows)> Groups = new();
    private bool OpenGroupsOnce;

    // ---------------------------------------------------------------- rows

    /// <summary>
    /// A clipped list of rows; <paramref name="focus"/> lets it scroll to a jumped-to effect. <paramref name="key"/>
    /// names the list for the arrow keys: the binder's path when grouped, "" for the flat list.
    /// </summary>
    private void DisplayRows(List<FxEntry> rows, bool focus, string key)
    {
        DrawnLists.Add((key, rows));

        var clipper = new ImGuiListClipper();
        clipper.Begin(rows.Count);

        // Keep the selected row in range when a jump asked for it
        int focusIndex = focus && Editor.Selection.FocusEntry ? rows.IndexOf(Editor.Selection.Entry) : -1;
        if (focusIndex >= 0)
            clipper.IncludeItemByIndex(focusIndex);
        else if (focus)
            Editor.Selection.FocusEntry = false;

        // ... and the row the arrow keys stepped to, so it can be scrolled to
        int stepIndex = StepEntry != null && key == StepScrollKey ? rows.IndexOf(StepEntry) : -1;
        if (stepIndex >= 0)
            clipper.IncludeItemByIndex(stepIndex);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                DisplayRow(rows, rows[i], i == focusIndex, i == stepIndex);
        }
        clipper.End();
    }

    // ---------------------------------------------------------------- arrow keys

    /// <summary>The lists drawn this frame, top to bottom: the flat list, or each open binder group.</summary>
    private readonly List<(string Key, List<FxEntry> Rows)> DrawnLists = new();

    /// <summary>The effect the arrow keys last stepped to, and the list it was in (an effect can be in several groups).</summary>
    private FxEntry StepEntry;
    private string StepKey;

    /// <summary>The list holding a row just stepped to, until it is scrolled into view; null once done.</summary>
    private string StepScrollKey;

    /// <summary>Up / Down step through the effects shown (collapsed groups are skipped) while the list has focus.</summary>
    private void HandleKeys()
    {
        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) || ImGui.IsAnyItemActive())
            return;

        var dir = GUI.ListStepKey();
        if (dir == 0)
            return;

        var flat = DrawnLists.SelectMany(l => l.Rows.Select(e => (l.Key, Entry: e))).ToList();
        if (flat.Count == 0)
            return;

        // Where we are: the row last stepped to while it's still selected, else the selected effect's first row
        var selected = Editor.Selection.Entry;
        int at = -1;
        if (selected != null && StepEntry == selected)
            at = flat.FindIndex(r => r.Key == StepKey && r.Entry == selected);
        if (at < 0 && selected != null)
            at = flat.FindIndex(r => r.Entry == selected);

        var (key, entry) = flat[Math.Clamp(at + dir, 0, flat.Count - 1)];

        // A plain step, like a plain click: the picked set is just this effect
        Picked.Clear();
        Anchor = entry;
        Select(entry);

        if (Editor.Selection.Entry == entry)
        {
            StepEntry = entry;
            StepKey = key;
            StepScrollKey = key;
        }
    }

    /// <summary>
    /// The filtered effects under the binder files they come from. An effect stored in several binders is listed
    /// under each (it is still one effect: an edit shows in every group and saves to every binder).
    /// </summary>
    private void DisplayGrouped()
    {
        var focusEntry = Editor.Selection.FocusEntry ? Editor.Selection.Entry : null;
        // A jump opens the group of the binder the effect is read from
        var focusBinder = focusEntry?.Binders.FirstOrDefault()?.Binder.Path;

        foreach (var (path, name, rows) in Groups)
        {
            if (OpenGroupsOnce)
                ImGui.SetNextItemOpen(true);
            if (focusBinder != null && path == focusBinder)
                ImGui.SetNextItemOpen(true);

            bool hasDirty = rows.Any(e => e.IsDirty);
            var label = $"{name}{(hasDirty ? "*" : "")}  ({rows.Count})";
            bool open = ImGui.TreeNodeEx($"{label}##fxBinder_{path}", ImGuiTreeNodeFlags.SpanAvailWidth);
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip(path);

            if (open)
            {
                DisplayRows(rows, focusBinder != null && path == focusBinder, path);
                ImGui.TreePop();
            }
        }

        if (focusEntry != null && focusBinder == null)
            Editor.Selection.FocusEntry = false;
        OpenGroupsOnce = false;
    }

    private bool IsPicked(FxEntry entry) => Picked.Contains(entry) || Editor.Selection.IsSelected(entry);

    /// <summary>The effects an action from <paramref name="entry"/>'s context menu applies to: every picked one when it's picked.</summary>
    private List<FxEntry> TargetsFor(FxEntry entry)
    {
        if (!IsPicked(entry))
            return new() { entry };
        var set = Picked.ToHashSet();
        if (Editor.Selection.Entry != null)
            set.Add(Editor.Selection.Entry);
        return set.Where(e => Bank.Entries.TryGetValue(e.Id, out var x) && x == e).OrderBy(e => e.Id).ToList();
    }

    private void DisplayRow(List<FxEntry> rows, FxEntry entry, bool focus, bool stepped)
    {
        // Marks: favorite (modified and added effects are shown by the label's colour alone)
        var marks = "";
        if (Favorites.IsFavorite(entry.Id))
            marks += $"{Icons.Star} ";

        // An id with several versions (DES) counts as one row: unsaved when any version is, all their binders
        bool unsaved = entry.AnyUnsaved;
        var label = $"{marks}{entry.Stem}{(unsaved ? "*" : "")}";
        int binderCount = entry.AllVersions.Sum(v => v.Binders.Count);
        if (binderCount > 1)
            label += $"  [{LOC.Get("FX_EffectList_Binder_Count", binderCount)}]";

        // The ## suffix keeps the ID stable when the label changes. Modified / added effects take the shared colour.
        var coloured = VanillaMarkers.PushColour(entry.CombinedVanilla.ToStatus());
        var clicked = ImGui.Selectable($" {label}##fxEffect_{entry.Id}", IsPicked(entry));
        VanillaMarkers.PopColour(coloured);

        if (clicked)
            Click(rows, entry);

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(RowTooltip(entry));

        if (focus)
        {
            Editor.Selection.FocusEntry = false;
            ImGui.SetScrollHereY(0.5f);
        }
        else if (stepped)
        {
            // An arrow-key step scrolls only when the row is out of view, so the list holds still until the edge
            StepScrollKey = null;
            if (!GUI.IsLastItemFullyInView())
                ImGui.SetScrollHereY(0.5f);
        }

        if (CFG.Current.FxEditor_EffectList_Display_Aliases && !string.IsNullOrEmpty(entry.Alias))
        {
            GUI.DisplayAlias(entry.Alias);
        }

        if (ImGui.BeginPopupContextItem($"##fxEffectContext_{entry.Id}"))
        {
            ContextMenu(entry);
            ImGui.EndPopup();
        }
    }

    private string RowTooltip(FxEntry entry)
    {
        var lines = entry.Versions == null
            ? entry.Binders.Select(b => b.Binder.Path).ToList()
            : entry.Versions.Select((v, i) => LOC.Get("FX_EffectList_Version_Binders", i + 1, string.Join(", ", v.Binders.Select(b => b.Binder.Path)))).ToList();
        var leaving = entry.OnDisk.Values.Where(r => !entry.InBinder(r.Binder.Path)).ToList();
        foreach (var r in leaving)
            lines.Add(LOC.Get("FX_EffectList_Leaving_Binder", r.Binder.Path));
        foreach (var b in entry.Binders.Where(b => !entry.OnDisk.ContainsKey(b.Binder.Path) && !entry.IsNew))
            lines.Add(LOC.Get("FX_EffectList_Joining_Binder", b.Binder.Filename));
        if (entry.CombinedVanilla != FxVanillaState.Same)
            lines.Add(LOC.Get($"FX_EffectList_Vanilla_{entry.CombinedVanilla}"));
        // DES keeps different effects under one id in different binders: each is a version, picked in the Effect Tree
        if (entry.Versions != null)
            lines.Add(LOC.Get("FX_EffectList_Copies_Differ", entry.Versions.Count));
        return string.Join("\n", lines);
    }

    /// <summary>Click selects; Ctrl+click adds or takes one away; Shift+click picks the run from the last click.</summary>
    private void Click(List<FxEntry> rows, FxEntry entry)
    {
        var io = ImGui.GetIO();
        if (io.KeyShift && Anchor != null && rows.Contains(Anchor))
        {
            int a = rows.IndexOf(Anchor), b = rows.IndexOf(entry);
            if (!io.KeyCtrl)
                Picked.Clear();
            for (int i = Math.Min(a, b); i <= Math.Max(a, b); i++)
                Picked.Add(rows[i]);
        }
        else if (io.KeyCtrl)
        {
            // The selected effect counts as picked, so it joins the set before another is added
            if (Editor.Selection.Entry != null)
                Picked.Add(Editor.Selection.Entry);
            if (!Picked.Remove(entry))
                Picked.Add(entry);
            Anchor = entry;
            if (!Picked.Contains(entry))
                return;
        }
        else
        {
            Picked.Clear();
            Anchor = entry;
        }
        Select(entry);
    }

    private void Select(FxEntry entry)
    {
        Editor.Selection.SelectEntry(entry);
        if (Editor.Selection.Entry == entry)
            Favorites.Visited(entry.Id);
    }

    // ---------------------------------------------------------------- context menu

    private void ContextMenu(FxEntry entry)
    {
        var targets = TargetsFor(entry);
        bool single = targets.Count == 1;
        if (!single)
            ImGui.TextDisabled(LOC.Get("FX_EffectList_Targets", targets.Count));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Copy_Id")}##fxCopyId"))
            ImGui.SetClipboardText(string.Join("\n", targets.Select(t => t.Id)));
        GUI.Tooltip(LOC.Get("FX_EffectList_Copy_Id_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Reload")}##fxReload", "", false, single && entry.IsLoaded))
            Editor.ReloadEntry(entry);
        GUI.Tooltip(LOC.Get("FX_EffectList_Reload_TT"));

        ImGui.Separator();

        // Every format (DS2 / DS2S keep their .sfxparam records in step on save, FFX Older Games Plan §22.8).
        // An id with two versions (DES) keeps each in its own binders: its binders and id aren't changed as one
        bool versions = targets.Any(HasVersions);

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Duplicate")}##fxDuplicate", InputManager.GetHint(KeybindID.Duplicate), false, single))
            RequestDuplicate(entry);
        GUI.Tooltip(LOC.Get("FX_EffectList_Duplicate_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Binders")}##fxBinders", "", false, !versions))
            RequestBinders(targets);
        TooltipEvenDisabled(versions ? LOC.Get("FX_EffectList_Versions_TT") : LOC.Get("FX_EffectList_Binders_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Delete")}##fxDelete", InputManager.GetHint(KeybindID.Delete)))
            RequestDelete(targets);
        GUI.Tooltip(LOC.Get("FX_EffectList_Delete_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Change_Id")}##fxChangeId", "", false, single && !versions))
            RequestChangeId(entry);
        TooltipEvenDisabled(versions ? LOC.Get("FX_EffectList_Versions_TT") : LOC.Get("FX_EffectList_Change_Id_TT"));

        ImGui.Separator();

        bool canRevert = targets.Any(CanRevertAny);
        if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Revert")}##fxRevert", "", false, canRevert))
            Deferred.Add(() => Revert(targets.Where(CanRevertAny).ToList()));
        GUI.Tooltip(LOC.Get(canRevert ? "FX_EffectList_Revert_TT" : "FX_EffectList_Revert_None_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Compare_Vanilla")}##fxCompareVanilla", "", false, single && CanRevert(Shown(entry))))
            RequestCompareVanilla(entry);
        GUI.Tooltip(LOC.Get("FX_EffectList_Compare_Vanilla_TT"));

        if (targets.Count == 2)
        {
            if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Compare_Pair")}##fxComparePair"))
                RequestCompare(targets[0], targets[1]);
            GUI.Tooltip(LOC.Get("FX_EffectList_Compare_Pair_TT"));
        }
        else if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Compare_With")}##fxCompareWith", "", false, single))
        {
            RequestCompareWith(entry);
        }
        GUI.Tooltip(LOC.Get("FX_EffectList_Compare_With_TT"));

        ImGui.Separator();

        if (single)
        {
            if (ImGui.BeginMenu($"{LOC.Get("FX_EffectList_Used_By")}##fxUsedBy"))
            {
                UsedByMenu(entry);
                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("FX_EffectList_Used_By_TT"));

            if (ImGui.BeginMenu($"{LOC.Get("FX_EffectList_Starts")}##fxStarts"))
            {
                StartsMenu(entry);
                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("FX_EffectList_Starts_TT"));

            ImGui.Separator();
        }

        bool allFavorite = targets.All(t => Favorites.IsFavorite(t.Id));
        if (ImGui.MenuItem($"{LOC.Get(allFavorite ? "FX_EffectList_Unfavorite" : "FX_EffectList_Favorite")}##fxFavorite"))
            Favorites.Set(targets.Select(t => t.Id), !allFavorite);
        GUI.Tooltip(LOC.Get("FX_EffectList_Favorite_TT"));

        if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Edit_Alias")}##fxEditAlias", "", false, single))
            RequestAlias(entry);
        GUI.Tooltip(LOC.Get("FX_EffectList_Edit_Alias_TT"));

        if (ImGui.MenuItem($"{LOC.Get(single ? "FX_EffectList_Export" : "FX_EffectList_Export_Many", Ext)}##fxExport"))
            Deferred.Add(() => Export(targets));
        GUI.Tooltip(LOC.Get("FX_EffectList_Export_TT", Ext));
    }

    private void UsedByMenu(FxEntry entry)
    {
        // Effects that start it
        ImGui.TextDisabled(LOC.Get("FX_EffectList_Used_By_Effects"));
        if (!Usage.HasScan)
        {
            ImGui.TextDisabled(Usage.IsScanning
                ? LOC.Get("FX_EffectList_Scanning", Usage.ScanDone, Usage.ScanTotal)
                : LOC.Get("FX_EffectList_Scan_Needed"));
            if (!Usage.IsScanning && ImGui.MenuItem($"{LOC.Get("FX_EffectList_Scan")}##fxScan", "", false, true))
                Usage.StartScan();
        }
        else
        {
            var hosts = Usage.StartedBy(entry.Id);
            if (hosts.Count == 0)
                ImGui.TextDisabled(LOC.Get("FX_EffectList_None"));
            foreach (var host in hosts.Take(50))
                if (ImGui.MenuItem($"{host.Stem}{AliasSuffix(host)}##fxUsedByEffect_{host.Id}"))
                    Editor.JumpTo(host.Id);
            if (hosts.Count > 50)
                ImGui.TextDisabled(LOC.Get("FX_EffectList_More", hosts.Count - 50));
        }

        ImGui.Separator();

        // Param fields holding its id
        ImGui.TextDisabled(LOC.Get("FX_EffectList_Used_By_Params"));
        if (!Usage.ParamsAvailable)
        {
            ImGui.TextDisabled(LOC.Get("FX_EffectList_Params_Unavailable"));
        }
        else
        {
            var uses = Usage.ParamsUsing(entry.Id);
            if (uses.Count == 0)
                ImGui.TextDisabled(LOC.Get("FX_EffectList_None"));
            foreach (var use in uses.Take(50))
            {
                var name = string.IsNullOrEmpty(use.RowName) ? "" : $" {use.RowName}";
                if (ImGui.MenuItem($"{use.Param} {use.RowId}{name} : {use.Field}##fxUsedByParam_{use.Param}_{use.RowId}_{use.Field}"))
                    FxUsage.JumpToParam(use);
            }
            if (uses.Count > 50)
                ImGui.TextDisabled(LOC.Get("FX_EffectList_More", uses.Count - 50));
            if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Rescan_Params")}##fxRescanParams"))
                Usage.ScanParams();
            GUI.Tooltip(LOC.Get("FX_EffectList_Rescan_Params_TT"));
        }

        // Time acts and maps, from the References tool's scan
        ImGui.Separator();
        ImGui.TextDisabled(LOC.Get("FX_ReferencesTool_External_Header"));
        if (!External.HasScan)
        {
            ImGui.TextDisabled(LOC.Get("FX_ReferencesTool_External_Not_Scanned"));
            return;
        }
        var external = External.UsesOf(entry.Id);
        if (external.Count == 0)
            ImGui.TextDisabled(LOC.Get("FX_EffectList_None"));
        for (int i = 0; i < Math.Min(external.Count, 50); i++)
        {
            var u = external[i];
            if (ImGui.MenuItem($"[{u.Kind}]  {u.Label}##fxUsedByExternal{i}", "", false, u.Command != null))
                EditorCommandQueue.AddCommand(u.Command);
        }
        if (external.Count > 50)
            ImGui.TextDisabled(LOC.Get("FX_EffectList_More", external.Count - 50));
    }

    private void StartsMenu(FxEntry entry)
    {
        var targets = Usage.Starts(entry);
        if (targets.Count == 0)
            ImGui.TextDisabled(LOC.Get("FX_EffectList_None"));
        foreach (var id in targets)
        {
            bool exists = Bank.Entries.TryGetValue(id, out var target);
            var label = exists ? $"{target.Stem}{AliasSuffix(target)}" : LOC.Get("FX_EffectList_Missing_Effect", $"f{id:D9}");
            if (ImGui.MenuItem($"{label}##fxStarts_{id}", "", false, exists))
                Editor.JumpTo(id);
        }
    }

    private static string AliasSuffix(FxEntry e) => string.IsNullOrEmpty(e.Alias) ? "" : $"  {e.Alias}";

    // ---------------------------------------------------------------- shortcuts

    /// <summary>Duplicate and Delete on the selected effect(s), while the Effect List has focus.</summary>
    public void Shortcuts()
    {
        if (!FocusManager.IsFocus(EditorFocusContext.FxEditor_EffectList) || ImGui.GetIO().WantTextInput)
            return;
        var entry = Editor.Selection.Entry;
        if (entry == null || !Bank.Entries.ContainsKey(entry.Id))
            return;

        if (InputManager.IsPressed(KeybindID.Duplicate))
            RequestDuplicate(entry);
        else if (InputManager.IsPressed(KeybindID.Delete))
            RequestDelete(TargetsFor(entry));
    }

    // ---------------------------------------------------------------- deferred

    /// <summary>Changes made from a menu, run once the list is drawn (they change the bank the list is drawn from).</summary>
    private readonly List<Action> Deferred = new();

    private void RunDeferred()
    {
        if (Deferred.Count == 0)
            return;
        var run = Deferred.ToList();
        Deferred.Clear();
        foreach (var action in run)
            action();
    }

    private void Execute(EditorAction action)
    {
        Editor.Inspector.FlushPending();
        Editor.CurveEditor.FlushPending();
        Editor.ActionManager.ExecuteAction(action);
    }

    /// <summary>After effects were added, removed or replaced: drop a selection that points at what's gone.</summary>
    private void OnEffectsChanged()
    {
        var selected = Editor.Selection.Entry;
        // A version (DES) stays selected while its id is in the bank
        if (selected != null && (!Bank.Entries.TryGetValue(selected.Id, out var e) || e != selected.Main))
            Editor.Selection.SelectEntry(null);
        else
            Editor.Selection.ClearItem();
        Picked.RemoveWhere(p => !Bank.Entries.TryGetValue(p.Id, out var x) || x != p);
    }
}
