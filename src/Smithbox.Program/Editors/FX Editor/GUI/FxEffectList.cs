using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.FxEditor;

/// <summary>Every effect id in the project's sfx binders, with alias names and a search filter.</summary>
public class FxEffectList
{
    private readonly FxEditorScreen Editor;

    private string Filter = "";
    private bool ExactFilter = false;

    // Filtered list cache, rebuilt when the filter, toggle or bank changes
    private List<FxEntry> Cache = new();
    private string CacheFilter;
    private bool CacheExact;
    private int CacheVersion = -1;
    private bool CacheAliases;

    public FxEffectList(FxEditorScreen editor)
    {
        Editor = editor;
    }

    private FxBank Bank => Editor.Data.Bank;

    public void Display()
    {
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
        if (Editor.Selection.FocusEntry && Editor.Selection.Entry != null && Filter != "" && !Cache.Contains(Editor.Selection.Entry))
        {
            Filter = "";
            RebuildCache();
        }

        if (CFG.Current.FxEditor_EffectList_Group_By_Binder)
            DisplayGrouped();
        else
            DisplayRows(Cache, true);

        ImGui.EndChild();
    }

    /// <summary>A clipped list of rows; <paramref name="focus"/> lets it scroll to a jumped-to effect.</summary>
    private void DisplayRows(List<FxEntry> rows, bool focus)
    {
        var clipper = new ImGuiListClipper();
        clipper.Begin(rows.Count);

        // Keep the selected row in range when a jump asked for it
        int focusIndex = focus && Editor.Selection.FocusEntry ? rows.IndexOf(Editor.Selection.Entry) : -1;
        if (focusIndex >= 0)
            clipper.IncludeItemByIndex(focusIndex);
        else if (focus)
            Editor.Selection.FocusEntry = false;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                DisplayRow(rows[i], i == focusIndex);
        }
        clipper.End();
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
                DisplayRows(rows, focusBinder != null && path == focusBinder);
                ImGui.TreePop();
            }
        }

        if (focusEntry != null && focusBinder == null)
            Editor.Selection.FocusEntry = false;
        OpenGroupsOnce = false;
    }

    private void RebuildCache()
    {
        bool aliases = CFG.Current.FxEditor_EffectList_Display_Aliases;
        if (CacheVersion == Bank.Version && CacheFilter == Filter && CacheExact == ExactFilter && CacheAliases == aliases)
            return;

        // Searching opens every group once, so the matches are visible; they can be collapsed after
        if (CacheFilter != Filter && !string.IsNullOrEmpty(Filter))
            OpenGroupsOnce = true;

        CacheVersion = Bank.Version;
        CacheFilter = Filter;
        CacheExact = ExactFilter;
        CacheAliases = aliases;
        Cache = Bank.Entries.Values
            .Where(e => EditorFilters.IsMatch(Filter, e.Id.ToString(), ExactFilter, e.Alias, false, true,
                string.Join(" ", e.Binders.Select(b => b.Binder.Filename))))
            .ToList();

        // The same effects by binder, binders in name order, effects in id order within each
        Groups = Cache
            .SelectMany(e => e.Binders.Select(b => (binder: b.Binder, entry: e)))
            .GroupBy(x => x.binder.Path, StringComparer.OrdinalIgnoreCase)
            .Select(g => (path: g.Key, name: g.First().binder.Filename, rows: g.Select(x => x.entry).Distinct().ToList()))
            .OrderBy(g => g.name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    // Grouped view cache: built with the flat one
    private List<(string path, string name, List<FxEntry> rows)> Groups = new();
    private bool OpenGroupsOnce;

    private void DisplayRow(FxEntry entry, bool focus)
    {
        var label = entry.IsDirty ? $"{entry.Stem}*" : entry.Stem;
        if (entry.Binders.Count > 1)
            label += $"  [{LOC.Get("FX_EffectList_Binder_Count", entry.Binders.Count)}]";

        // The ## suffix keeps the ID stable when the dirty marker changes
        if (ImGui.Selectable($" {label}##fxEffect_{entry.Id}", Editor.Selection.IsSelected(entry)))
        {
            Editor.Selection.SelectEntry(entry);
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(string.Join("\n", entry.Binders.Select(b => b.Binder.Path)));
        }

        if (focus)
        {
            Editor.Selection.FocusEntry = false;
            ImGui.SetScrollHereY(0.5f);
        }

        if (CFG.Current.FxEditor_EffectList_Display_Aliases && !string.IsNullOrEmpty(entry.Alias))
        {
            GUI.DisplayAlias(entry.Alias);
        }

        // Context menu
        if (ImGui.BeginPopupContextItem($"##fxEffectContext_{entry.Id}"))
        {
            if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Copy_Id")}##fxCopyId"))
                ImGui.SetClipboardText(entry.Id.ToString());
            GUI.Tooltip(LOC.Get("FX_EffectList_Copy_Id_TT"));

            if (ImGui.MenuItem($"{LOC.Get("FX_EffectList_Reload")}##fxReload", "", false, entry.IsLoaded))
                Editor.ReloadEntry(entry);
            GUI.Tooltip(LOC.Get("FX_EffectList_Reload_TT"));

            ImGui.EndPopup();
        }
    }
}
