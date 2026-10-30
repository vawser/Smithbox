using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The 'rows' view: each of the entries for the selected individual category.
/// </summary>
public class MsbEntryView
{
    public MapEditorView View;
    public ProjectEntry Project;

    private string EntryListFilter = "";
    private bool ExactEntryListFilter = false;

    private MapDataSelection Selection => View.MapDataView.Selection;

    private MapContainer _lastMap;
    private Type _lastSubType;
    private bool _isDirty = true;

    // The entity is null for entries in a category without entities
    private List<(object entry, MsbEntity ent, string label, string alias)> _cachedEntries = new();
    private Dictionary<Type, int> _mapEntryCountByType = new();

    private int _lastClickedIndex = -1;

    private bool _arrowKeyPressed = false;

    public MsbEntryView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Rebuilds the entry list on the next display, as map objects have been added, removed or reordered.
    /// </summary>
    public void Invalidate()
    {
        _isDirty = true;
    }

    public void Display()
    {
        if (InputManager.HasArrowSelection())
        {
            _arrowKeyPressed = true;
        }

        UpdateCaches();

        GUI.TitleHeader("Entries", "");

        EditorFilters.DisplayFramedListFilter("EntryListFilter", ref EntryListFilter, ref ExactEntryListFilter);

        float listHeight = ImGui.GetContentRegionAvail().Y;
        ImGui.BeginChild("##EntryList", new Vector2(0, listHeight), ImGuiChildFlags.Borders);

        if (Selection.SelectedSubCategoryType is null)
        {
            ImGui.TextDisabled("No category selected.");
        }
        else if (_cachedEntries.Count == 0)
        {
            ImGui.TextDisabled("No entries.");
        }
        else
        {
            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.ScrollY;

            if (ImGui.BeginTable($"msbEntryTable", 2, tblFlags))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch, 0.5f);
                ImGui.TableSetupColumn("Alias", ImGuiTableColumnFlags.WidthStretch, 0.5f);

                ImGui.TableSetupScrollFreeze(2, 1);
                ImGui.TableHeadersRow();

                for (int i = 0; i < _cachedEntries.Count; i++)
                {
                    var (entry, ent, cachedLabel, alias) = _cachedEntries[i];

                    // Read directly, since renames from the other Map Editor windows do not rebuild this list
                    var label = entry is IMsbEntry msbEntry && !string.IsNullOrWhiteSpace(msbEntry.Name)
                        ? msbEntry.Name
                        : cachedLabel;

                    if (!EditorFilters.IsMatch(EntryListFilter, label, ExactEntryListFilter))
                    {
                        continue;
                    }

                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    if (ent != null)
                    {
                        DisplayEntityRow(i, label, ent);
                    }
                    else
                    {
                        ImGui.TextUnformatted(label);
                        GUI.Tooltip($"Index: {i}\nThis entry is not a map object, so it has no properties to edit.");
                    }

                    ImGui.TableSetColumnIndex(1);

                    if (alias != null && alias != "")
                    {
                        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);
                    }
                }

                ImGui.EndTable();
            }
        }

        ImGui.EndChild();
    }

    private void UpdateCaches()
    {
        var map = Selection.SelectedMap;

        if (_lastMap != map)
        {
            Selection.ResetCategories();

            _lastMap = map;
            _isDirty = true;
        }

        if (!_isDirty && _lastSubType == Selection.SelectedSubCategoryType)
            return;

        _isDirty = false;
        _lastSubType = Selection.SelectedSubCategoryType;

        RebuildMapEntryCountCache();
        RebuildEntryCache();
    }

    private void DisplayEntityRow(int listIndex, string label, MsbEntity ent)
    {
        bool isSelected = View.ViewportSelection.IsSelected(ent);

        if (ImGui.Selectable($"{label}##{listIndex}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
        {
            HandleEntrySelection(listIndex, ent);
        }

        if (_arrowKeyPressed && ImGui.IsItemFocused() && !isSelected)
        {
            HandleEntrySelection(listIndex, ent);

            _arrowKeyPressed = false;
        }

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip($"Index: {listIndex}");
        }

        // The context actions apply to the selection, so a right-click must select the entry first
        if (!isSelected && ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            HandleEntrySelection(listIndex, ent);
        }

        if (ent.Type != MsbEntityType.Models && ImGui.BeginPopupContextItem($"##EntryCtx_{listIndex}"))
        {
            View.ReorderAction.OnContext(ent);
            View.DuplicateAction.OnContext();
            View.DeleteAction.OnContext();

            ImGui.EndPopup();
        }
    }

    private void HandleEntrySelection(int listIndex, MsbEntity ent)
    {
        var selection = View.ViewportSelection;

        // Multi-Select: Range Select
        if (InputManager.HasShiftDown() && _lastClickedIndex != -1)
        {
            var start = Math.Min(_lastClickedIndex, listIndex);
            var end = Math.Max(_lastClickedIndex, listIndex);

            for (int k = start; k <= end; k++)
            {
                selection.AddSelection(_cachedEntries[k].ent);
            }
        }
        // Multi-Select Mode
        else if (InputManager.HasCtrlDown())
        {
            if (selection.IsSelected(ent))
            {
                selection.RemoveSelection(ent);
            }
            else
            {
                selection.AddSelection(ent);
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            selection.ClearSelection();
            selection.AddSelection(ent);
        }

        _lastClickedIndex = listIndex;
    }

    public void RebuildMapEntryCountCache()
    {
        _mapEntryCountByType.Clear();

        var map = Selection.SelectedMap;

        if (map is null)
            return;

        foreach (var obj in map.Objects.Concat(map.Models))
        {
            if (obj is MsbEntity { WrappedObject: not null } ent)
            {
                AddEntryCount(ent.WrappedObject);
            }
        }

        if (Selection.SourceMsb is null)
            return;

        foreach (var mapProp in Selection.SourceMsb.GetType()
                     .GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip indexed properties (e.g. IList.Item indexers)
            if (mapProp.GetIndexParameters().Length > 0)
                continue;

            var paramObject = mapProp.GetValue(Selection.SourceMsb);
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
                    // These are counted from the entities, which reflect the current state of the map
                    if (entry is null or IMsbModel or IMsbPart or IMsbRegion or IMsbEvent)
                        continue;

                    AddEntryCount(entry);
                }
            }
        }
    }

    private void AddEntryCount(object entry)
    {
        var t = entry.GetType();
        _mapEntryCountByType.TryGetValue(t, out int n);
        _mapEntryCountByType[t] = n + 1;
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
        _lastClickedIndex = -1;

        var map = Selection.SelectedMap;
        var subType = Selection.SelectedSubCategoryType;

        if (map is null || subType is null)
            return;

        if (!Selection.IsEntityCategory)
        {
            object paramObject = GetParamObject();
            if (paramObject is null)
                return;

            IList entries = FindEntryList(paramObject);
            if (entries is null)
                return;

            foreach (var entry in entries)
            {
                if (subType.IsInstanceOfType(entry))
                {
                    AddCachedEntry(map, entry, null);
                }
            }
        }
        else
        {
            var entities = Selection.SelectedBaseCategory is "Model" ? map.Models : map.Objects;

            foreach (var obj in entities)
            {
                if (obj is MsbEntity ent && subType.IsInstanceOfType(ent.WrappedObject))
                {
                    AddCachedEntry(map, ent.WrappedObject, ent);
                }
            }
        }
    }

    private void AddCachedEntry(MapContainer map, object entry, MsbEntity ent)
    {
        string label = GetEntryLabel(entry, _cachedEntries.Count);
        string alias = Project.Handler.MapData.GetMapObjectName(map.Name, label);

        _cachedEntries.Add((entry, ent, label, alias));
    }

    private object GetParamObject()
    {
        if (Selection.SelectedBaseCategoryType is null || Selection.SourceMsb is null)
            return null;

        var targetType = Selection.SelectedBaseCategoryType;
        var mapType = Selection.SourceMsb.GetType();

        // Normal path: find a param property whose type matches.
        foreach (var prop in mapType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            if (prop.PropertyType == targetType)
                return prop.GetValue(Selection.SourceMsb);
        }

        foreach (var prop in mapType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            if (targetType.IsAssignableFrom(prop.PropertyType))
                return prop.GetValue(Selection.SourceMsb);
        }

        // Flat-list fallback: the MSB root itself owns a List<T> whose element
        // type matches. Return the map as the param object so FindEntryList
        // can locate the list directly on it.
        foreach (var prop in mapType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            var pt = prop.PropertyType;
            if (!pt.IsGenericType) continue;
            var args = pt.GetGenericArguments();
            if (args.Length == 1 && targetType.IsAssignableFrom(args[0]))
                return Selection.SourceMsb;
        }

        return null;
    }

    private IList FindEntryList(object paramObject)
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

            if (prop.GetValue(paramObject) is IList list)
                return list;
        }

        return null;
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
