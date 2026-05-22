using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

/// <summary>
/// The 'rows' view: each of the entries for the selected individual category
/// </summary>
public class MsbEntryView
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    private string EntryListFilter = "";
    private bool ExactEntryListFilter = false;

    private MapDataSelection Selection => View.Selection;

    private Type _lastSubType;
    private List<(int idx, object entry, string label)> _cachedEntries = new();

    public MsbEntryView(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        // Rebuild entry cache when the active sub-category changes.
        if (_lastSubType != Selection.SelectedSubCategoryType)
        {
            RebuildEntryCache();
            _lastSubType = Selection.SelectedSubCategoryType;
        }

        string header = Selection.SelectedSubCategory is not null
            ? $"{Selection.SelectedBaseCategory} › {Selection.SelectedSubCategory}"
            : Selection.SelectedBaseCategory ?? "";

        UIHelper.SimpleHeader("Entries", "");

        EditorFilters.DisplayFramedListFilter("EntryListFilter", ref EntryListFilter, ref ExactEntryListFilter);

        float listHeight = ImGui.GetContentRegionAvail().Y;
        ImGui.BeginChild("##EntryList", new Vector2(0, listHeight), ImGuiChildFlags.Borders);

        if (Selection.SelectedMap is null || Selection.SelectedSubCategoryType is null)
        {
            ImGui.TextDisabled("No category selected.");
        }
        else if (_cachedEntries.Count == 0)
        {
            ImGui.TextDisabled("No entries.");
        }
        else
        {
            int visibleIndex = 0; // index within the filtered view for range-select

            for(int i = 0; i < _cachedEntries.Count; i++)
            {
                var rawIndex = _cachedEntries[i].idx;
                var entry = _cachedEntries[i].entry;
                var label = _cachedEntries[i].label;

                if (!EditorFilters.IsMatch(EntryListFilter, label, ExactEntryListFilter))
                {
                    visibleIndex++;
                    continue;
                }

                bool isSelected = Selection.SelectedEntries.ContainsKey(rawIndex);

                if (ImGui.Selectable($"{label}##{rawIndex}", isSelected,
                        ImGuiSelectableFlags.AllowDoubleClick))
                {
                    Selection.HandleMsbEntrySelection(
                        selectedIndex: GetFirstSelectedIndex(),
                        curListIndex: rawIndex,
                        entry: entry);
                }

                // Tooltip with the entry index
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip($"Index: {rawIndex}");
                }

                // Right-click context menu placeholder
                if (ImGui.BeginPopupContextItem($"##EntryCtx_{rawIndex}"))
                {
                    if(ImGui.Selectable("Duplicate"))
                    {
                        var action = new MsbEntryDuplicate(View, Project);
                        View.ActionManager.ExecuteAction(action);
                    }

                    if (ImGui.Selectable("Delete"))
                    {
                        var action = new MsbEntryDelete(View, Project);
                        View.ActionManager.ExecuteAction(action);
                    }

                    ImGui.EndPopup();
                }

                visibleIndex++;
            }
        }

        ImGui.EndChild();
    }

    public void RebuildEntryCache()
    {
        _cachedEntries.Clear();
        _lastSubType = null;

        if (Selection.SelectedMap is null || Selection.SelectedSubCategoryType is null)
            return;

        object paramObject = GetParamObject();
        if (paramObject is null)
            return;

        IList entries = FindMutableEntryList(paramObject);
        if (entries is null)
            return;

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (entry is null)
                continue;

            if (!Selection.SelectedSubCategoryType.IsInstanceOfType(entry))
                continue;

            string label = GetEntryLabel(entry, i);
            _cachedEntries.Add((i, entry, label));
        }

        _lastSubType = Selection.SelectedSubCategoryType;
    }

    public object GetParamObject()
    {
        if (Selection.SelectedBaseCategoryType is null || Selection.SelectedMap is null)
            return null;

        var targetType = Selection.SelectedBaseCategoryType;

        foreach (var prop in Selection.SelectedMap.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Exact match first — the concrete property (e.g. ModelParam Models).
            if (prop.PropertyType == targetType)
                return prop.GetValue(Selection.SelectedMap);
        }

        // Fallback: accept a subclass in case a game-specific MSB subclasses the param.
        foreach (var prop in Selection.SelectedMap.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (targetType.IsAssignableFrom(prop.PropertyType))
                return prop.GetValue(Selection.SelectedMap);
        }

        return null;
    }


    public IList FindMutableEntryList(object paramObject)
    {
        var targetType = Selection.SelectedSubCategoryType;

        foreach (var prop in paramObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propType = prop.PropertyType;
            if (!propType.IsGenericType)
                continue;

            var args = propType.GetGenericArguments();
            if (args.Length != 1)
                continue;

            if (!targetType.IsAssignableFrom(args[0]) && !args[0].IsAssignableFrom(targetType))
                continue;

            // Only accept a property that is itself mutable.
            if (prop.GetValue(paramObject) is IList list)
                return list;
        }

        return null;
    }


    private int GetFirstSelectedIndex()
    {
        foreach (var key in Selection.SelectedEntries.Keys)
        {
            return key;
        }

        return -1;
    }

    private string GetEntryLabel(object entry, int index)
    {
        var nameProp = entry.GetType().GetProperty("Name",
            BindingFlags.Public | BindingFlags.Instance);

        if (nameProp is not null)
        {
            var nameVal = nameProp.GetValue(entry) as string;
            if (!string.IsNullOrWhiteSpace(nameVal))
            {
                return nameVal;
            }
        }

        var str = entry.ToString();
        if (!string.IsNullOrWhiteSpace(str) && str != entry.GetType().FullName)
        {
            return str;
        }

        return $"[{index}]";
    }
}
