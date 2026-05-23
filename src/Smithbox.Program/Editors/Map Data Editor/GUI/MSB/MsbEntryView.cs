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
/// The 'rows' view: each of the entries for the selected individual category.
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
    private Dictionary<Type, int> _mapEntryCountByType = new();
    private object _lastScannedMap = null;

    private const string DragDropId = "MSB_ENTRY_DRAG";
    private int _dragSourceRawIndex = -1;

    public MsbEntryView(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (_lastScannedMap != Selection.SelectedMap)
        {
            RebuildMapEntryCountCache();
        }

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
            object paramObject = GetParamObject();
            IList liveList = paramObject is not null ? FindMutableEntryList(paramObject) : null;

            int visibleIndex = 0;

            for (int i = 0; i < _cachedEntries.Count; i++)
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

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip($"Index: {rawIndex}");
                }

                bool canReorder = liveList is not null && string.IsNullOrEmpty(EntryListFilter);

                if (canReorder && ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
                {
                    if (!Selection.SelectedEntries.ContainsKey(rawIndex))
                    {
                        Selection.ResetMsbEntrySelection();
                        Selection.SelectedEntries.Add(rawIndex, entry);
                    }

                    unsafe
                    {
                        int payload = rawIndex;
                        ImGui.SetDragDropPayload(DragDropId, &payload, sizeof(int));
                    }
                    _dragSourceRawIndex = rawIndex;

                    int selCount = Selection.SelectedEntries.Count;
                    ImGui.Text(selCount > 1 ? $"Moving {selCount} entries" : $"Moving: {label}");
                    ImGui.EndDragDropSource();
                }

                if (canReorder && ImGui.BeginDragDropTarget())
                {
                    unsafe
                    {
                        var payload = ImGui.AcceptDragDropPayload(DragDropId);
                        if (!payload.IsNull && payload.DataSize == sizeof(int))
                        {
                            int toIndex = rawIndex;
                            var selectedIndices = Selection.SelectedEntries.Keys.ToList();

                            if (!selectedIndices.Contains(toIndex))
                            {
                                var action = new MsbEntryReorder(View, liveList, selectedIndices, toIndex);
                                View.ActionManager.ExecuteAction(action);

                                RemapSelectionAfterReorder(action);

                                RebuildEntryCache();
                            }

                            _dragSourceRawIndex = -1;
                        }
                    }

                    ImGui.EndDragDropTarget();
                }

                if (ImGui.BeginPopupContextItem($"##EntryCtx_{rawIndex}"))
                {
                    if (ImGui.Selectable("Duplicate"))
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

    public void RebuildMapEntryCountCache()
    {
        _mapEntryCountByType.Clear();

        if (Selection.SelectedMap is null)
            return;

        foreach (var mapProp in Selection.SelectedMap.GetType()
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip indexed properties (e.g. IList.Item indexers)
            if (mapProp.GetIndexParameters().Length > 0)
                continue;

            var paramObject = mapProp.GetValue(Selection.SelectedMap);
            if (paramObject is null)
                continue;

            // Skip flat lists sitting directly on the MSB (e.g. PartPoses)
            if (paramObject is IList)
                continue;

            foreach (var prop in paramObject.GetType()
                         .GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Same guard for indexed properties on the param object
                if (prop.GetIndexParameters().Length > 0)
                    continue;

                if (prop.GetValue(paramObject) is not IList list)
                    continue;

                foreach (var entry in list)
                {
                    if (entry is null) continue;
                    var t = entry.GetType();
                    _mapEntryCountByType.TryGetValue(t, out int n);
                    _mapEntryCountByType[t] = n + 1;
                }
            }
        }

        _lastScannedMap = Selection.SelectedMap;
    }

    public int GetCachedEntryCount(Type subCategoryType)
    {
        if (subCategoryType is null)
            return 0;

        int total = 0;
        foreach (var (type, count) in _mapEntryCountByType)
        {
            if (subCategoryType.IsAssignableFrom(type))
                total += count;
        }

        return total;
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
        var mapType = Selection.SelectedMap.GetType();

        // Normal path: find a param property whose type matches.
        foreach (var prop in mapType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            if (prop.PropertyType == targetType)
                return prop.GetValue(Selection.SelectedMap);
        }

        foreach (var prop in mapType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            if (targetType.IsAssignableFrom(prop.PropertyType))
                return prop.GetValue(Selection.SelectedMap);
        }

        // Flat-list fallback: the MSB root itself owns a List<T> whose element
        // type matches (e.g. List<PartPose>). Return the map as the param object
        // so FindMutableEntryList can locate the list directly on it.
        foreach (var prop in mapType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            var pt = prop.PropertyType;
            if (!pt.IsGenericType) continue;
            var args = pt.GetGenericArguments();
            if (args.Length == 1 && targetType.IsAssignableFrom(args[0]))
                return Selection.SelectedMap;
        }

        return null;
    }

    public IList FindMutableEntryList(object paramObject)
    {
        var targetType = Selection.SelectedSubCategoryType;

        foreach (var prop in paramObject.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var propType = prop.PropertyType;

            if (prop.GetIndexParameters().Length > 0) 
                continue;

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

    private void RemapSelectionAfterReorder(MsbEntryReorder action)
    {
        var newIndices = action.MovedIndices;
        if (newIndices is null || newIndices.Count == 0) return;

        // Pair each new index with the live list item at that position.
        var remapped = new SortedDictionary<int, object>();
        foreach (int idx in newIndices)
        {
            object item = View.Selection.SelectedMap is not null
                ? GetLiveItemAt(idx)
                : null;
            remapped[idx] = item;
        }

        Selection.SelectedEntries.Clear();
        foreach (var kv in remapped)
            Selection.SelectedEntries[kv.Key] = kv.Value;
    }

    private object GetLiveItemAt(int index)
    {
        object paramObject = GetParamObject();
        if (paramObject is null) return null;
        IList list = FindMutableEntryList(paramObject);
        if (list is null || index < 0 || index >= list.Count) return null;
        return list[index];
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