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
public partial class MsbEntryView
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

    // The field columns of the selected category, and their text for each cached entry (by the same index).
    // The text is read again whenever the map's history changes, as an edit or undo can change any value.
    private List<string> _columns = new();
    private string[][] _cells = Array.Empty<string[]>();
    private bool _cellsDirty = true;
    private int _cellsHistoryVersion = -1;

    // The cached entries' indexes in the order they are shown, and each entry's place in that order.
    // Sorted again when the header is clicked, or when the entries or their values have been read again.
    private int[] _order = Array.Empty<int>();
    private int[] _orderPosition = Array.Empty<int>();
    private bool _orderDirty = true;
    private int _sortColumn = 0;
    private bool _sortAscending = true;

    // The cell being edited: its cached entry and column, and the text in its input. -1 when none is.
    private int _editRow = -1;
    private int _editColumn = -1;
    private string _editText = "";
    private bool _editFocus = false;
    private bool _editDrawn = false;

    private readonly MsbQueryMatcher _fieldLister;
    private object _pickerFieldsFor;
    private List<MsbQueryField> _pickerFields = new();
    private string _pickerFilter = "";
    private bool _pickerExactFilter = false;

    public MsbEntryView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        _fieldLister = new MsbQueryMatcher(project);
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
        // Not while a cell is typed in, where the arrows move the text cursor
        if (InputManager.HasArrowSelection() && !ImGui.GetIO().WantTextInput)
        {
            _arrowKeyPressed = true;
        }

        UpdateFollowSelection();

        UpdateCaches();

        GUI.TitleHeader("Entries", "");

        ImGui.BeginChild("framedListFilter_EntryListFilter", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("EntryListFilter", ref EntryListFilter, ref ExactEntryListFilter);

        DisplayColumnsButton();

        DisplayQuickFilterButtons();

        ImGui.EndChild();

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
            // Hideable: the columns can be hidden from the header's context menu. Sortable: a header click sorts by it.
            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersOuter | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Hideable | ImGuiTableFlags.Sortable;

            // Per entry type, so each type keeps its own column widths
            var tableId = $"msbEntryTable_{Selection.SelectedSubCategoryType.FullName}";
            var aliasColumn = FirstFieldColumn + _columns.Count;

            if (ImGui.BeginTable(tableId, aliasColumn + 1, tblFlags))
            {
                // The entry's position in this category's list, which reordering changes
                ImGui.TableSetupColumn("#", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.DefaultSort);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.NoHide, 0.5f);

                // The Save Check's issues, and how many objects refer to the entry
                ImGui.TableSetupColumn($"{Icons.ExclamationTriangle}##issues", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Used By", ImGuiTableColumnFlags.WidthFixed);

                foreach (var path in _columns)
                {
                    ImGui.TableSetupColumn($"{MsbEntryColumns.GetHeader(path)}##{path}", ImGuiTableColumnFlags.WidthFixed);
                }

                ImGui.TableSetupColumn("Alias", ImGuiTableColumnFlags.WidthStretch, 0.5f);

                ImGui.TableSetupScrollFreeze(0, 1);

                UpdateSort(aliasColumn);

                ImGui.TableHeadersRow();

                _editDrawn = false;

                foreach (var i in _order)
                {
                    var (label, alias) = GetCurrentLabel(i);
                    var ent = _cachedEntries[i].ent;

                    if (!IsShown(label, alias) || !PassesQuickFilters(i))
                    {
                        continue;
                    }

                    ImGui.TableNextRow();

                    if (ImGui.TableSetColumnIndex(IndexColumn))
                    {
                        ImGui.TextDisabled(i.ToString());
                    }

                    ImGui.TableSetColumnIndex(NameColumn);

                    if (ent != null)
                    {
                        DisplayEntityRow(i, label, ent);
                    }
                    else
                    {
                        ImGui.TextUnformatted(label);
                        GUI.Tooltip($"Index: {i}\nThis entry is not a map object, so it has no properties to edit.");
                    }

                    if (ImGui.TableSetColumnIndex(IssuesColumn))
                    {
                        DisplayIssuesCell(ent);
                    }

                    if (ImGui.TableSetColumnIndex(UsedByColumn))
                    {
                        DisplayUsedByCell(i);
                    }

                    var cells = i < _cells.Length ? _cells[i] : null;

                    for (int c = 0; c < _columns.Count; c++)
                    {
                        if (ImGui.TableSetColumnIndex(FirstFieldColumn + c) && cells != null && c < cells.Length)
                        {
                            DisplayCell(i, c, cells[c], ent);
                        }
                    }

                    if (ImGui.TableSetColumnIndex(aliasColumn) && !string.IsNullOrEmpty(alias))
                    {
                        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);
                    }
                }

                // The edited cell is gone (filtered out, or its column hidden), so its edit is dropped
                if (_editRow != -1 && !_editDrawn)
                {
                    _editRow = -1;
                    _editColumn = -1;
                }

                // The followed entity's row is filtered out, so it isn't scrolled to later
                _scrollTarget = null;

                ImGui.EndTable();
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Returns the entry's current name and alias. The name is read directly, since renames from the
    /// other Map Editor windows do not rebuild this list; the alias is looked up again only when the name changed.
    /// </summary>
    private (string label, string alias) GetCurrentLabel(int index)
    {
        var (entry, ent, cachedLabel, alias) = _cachedEntries[index];

        if (entry is IMsbEntry msbEntry && !string.IsNullOrWhiteSpace(msbEntry.Name) && msbEntry.Name != cachedLabel)
        {
            cachedLabel = msbEntry.Name;
            alias = Project.Handler.MapData.GetMapObjectName(Selection.SelectedMap?.Name, cachedLabel);

            _cachedEntries[index] = (entry, ent, cachedLabel, alias);
        }

        return (cachedLabel, alias);
    }

    /// <summary>
    /// Whether the filter matches the entry's name or its alias, as both are shown in the table
    /// </summary>
    private bool IsShown(string label, string alias)
    {
        return EditorFilters.IsMatch(EntryListFilter, label, ExactEntryListFilter, alias ?? "");
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

        if (_isDirty || _lastSubType != Selection.SelectedSubCategoryType)
        {
            _isDirty = false;
            _lastSubType = Selection.SelectedSubCategoryType;

            RebuildMapEntryCountCache();
            RebuildEntryCache();

            _cellsDirty = true;
        }

        var historyVersion = View.ViewportActionManager.Version;

        if (_cellsDirty || _cellsHistoryVersion != historyVersion)
        {
            _cellsDirty = false;
            _cellsHistoryVersion = historyVersion;

            RebuildCells();
        }
    }

    /// <summary>
    /// Reads the column fields of every cached entry
    /// </summary>
    private void RebuildCells()
    {
        var sample = _cachedEntries.Count > 0 ? _cachedEntries[0].entry : null;

        _columns = MsbEntryColumns.GetColumns(Selection.SelectedSubCategoryType, sample);
        _cells = new string[_cachedEntries.Count][];

        for (int i = 0; i < _cachedEntries.Count; i++)
        {
            var row = new string[_columns.Count];

            for (int c = 0; c < _columns.Count; c++)
            {
                row[c] = MsbEntryColumns.GetText(_cachedEntries[i].entry, _columns[c]);
            }

            _cells[i] = row;
        }

        RebuildChecks();

        _orderDirty = true;
    }

    /// <summary>
    /// A field cell: its text, or an input while it is being edited. A double-click starts the edit, which is
    /// applied when the input is left (not by Escape), as an undoable change.
    /// </summary>
    private void DisplayCell(int row, int column, string text, MsbEntity ent)
    {
        if (row == _editRow && column == _editColumn)
        {
            _editDrawn = true;

            if (_editFocus)
            {
                ImGui.SetKeyboardFocusHere();
                _editFocus = false;
            }

            ImGui.SetNextItemWidth(-1);
            ImGui.InputText($"##cellEdit_{row}_{column}", ref _editText, 512);

            if (GUI.InputCommitted())
            {
                ApplyCellEdit(row, column, _editText);
            }

            // Left by Enter, Tab, a click elsewhere or Escape
            if (ImGui.IsItemDeactivated())
            {
                _editRow = -1;
                _editColumn = -1;
            }

            return;
        }

        ImGui.TextUnformatted(text);

        if (!ImGui.IsItemHovered())
            return;

        // Only map objects can be changed, as entries without an entity aren't saved from the editor
        if (ent == null)
            return;

        if (!MsbEntryColumns.TryGetField(_cachedEntries[row].entry, _columns[column], out var field) || !field.CanSet)
            return;

        var targets = GetEditTargets(row).Count;

        GUI.Tooltip(targets > 1
            ? $"Double-click to edit. The new value is set on all {targets} selected entries."
            : "Double-click to edit.");

        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            _editRow = row;
            _editColumn = column;
            _editText = MsbEntryColumns.GetEditText(field.Value);
            _editFocus = true;
        }
    }

    /// <summary>
    /// The entries a cell edit changes: every selected entry of this list when the edited one is selected,
    /// otherwise only the edited one
    /// </summary>
    private List<int> GetEditTargets(int row)
    {
        var ent = _cachedEntries[row].ent;

        if (ent == null || !View.ViewportSelection.IsSelected(ent))
            return new List<int> { row };

        var targets = new List<int>();

        for (int i = 0; i < _cachedEntries.Count; i++)
        {
            var other = _cachedEntries[i].ent;

            if (other != null && View.ViewportSelection.IsSelected(other))
                targets.Add(i);
        }

        return targets;
    }

    /// <summary>
    /// Sets the field of the column to the text on each target entry, as one undoable action. The text is read as
    /// the Mass Edit tool's Set reads it, so numbers are range-checked and enums take a name or a number.
    /// </summary>
    private void ApplyCellEdit(int row, int column, string text)
    {
        var path = _columns[column];
        var step = new MsbMassEditStep(path, MsbMassEditOperator.Set, text, null);

        var actions = new List<ViewportAction>();
        var identifierChanges = new List<(MsbEntity ent, object oldValue, object newValue)>();
        string refusal = null;

        foreach (var target in GetEditTargets(row))
        {
            var ent = _cachedEntries[target].ent;

            if (ent == null || !MsbEntryColumns.TryGetField(_cachedEntries[target].entry, path, out var field) || !field.CanSet)
                continue;

            if (!step.TryApply(field.ValueType, field.Value, null, out var newValue, out var reason))
            {
                refusal ??= reason;
                continue;
            }

            if (Equals(field.Value, newValue))
                continue;

            var action = new PropChangeAction(ent, field.Owner);
            action.AddPropertyChange(field.Property, newValue, field.Index);

            // References to other objects are worked out from the fields, so they are worked out again
            action.SetPostExecutionAction(_ => ent.BuildReferenceMap());

            actions.Add(action);

            if (IsEntityIdentifier(field))
                identifierChanges.Add((ent, field.Value, newValue));
        }

        if (refusal != null)
        {
            Smithbox.Log(this, $"Map Data: {path} was not set to \"{text}\": {refusal}", Microsoft.Extensions.Logging.LogLevel.Warning);
        }

        if (actions.Count == 0)
            return;

        View.ViewportActionManager.ExecuteAction(actions.Count == 1 ? actions[0] : new ViewportCompoundAction(actions));

        // As the Properties window does when an entity ID is committed
        foreach (var (ent, oldValue, newValue) in identifierChanges)
        {
            View.EntityIdentifierTool.UpdateEntityCache(ent, oldValue, newValue);
        }
    }

    private bool IsEntityIdentifier(MsbEntryField field)
    {
        try
        {
            return Project.Handler.MapData.Meta?.GetFieldMeta(field.Property.Name, field.Owner.GetType()) is { EntityIdentifierProperty: true };
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Takes the header's sort, then sorts the rows if it or the rows have changed
    /// </summary>
    private void UpdateSort(int aliasColumn)
    {
        var specs = ImGui.TableGetSortSpecs();

        if (!specs.IsNull && specs.SpecsDirty)
        {
            if (specs.SpecsCount > 0)
            {
                var spec = specs.Specs[0];

                _sortColumn = spec.ColumnIndex;
                _sortAscending = spec.SortDirection != ImGuiSortDirection.Descending;
            }

            specs.SpecsDirty = false;
            _orderDirty = true;
        }

        if (_orderDirty || _order.Length != _cachedEntries.Count)
        {
            _orderDirty = false;

            SortRows(aliasColumn);
        }
    }

    private void SortRows(int aliasColumn)
    {
        var count = _cachedEntries.Count;
        var order = Enumerable.Range(0, count).ToArray();

        // The index column is the order the entries are in already
        if (_sortColumn > 0)
        {
            var keys = new SortKey[count];

            for (int i = 0; i < count; i++)
            {
                keys[i] = new SortKey(GetSortText(i, aliasColumn));
            }

            // Ties keep their index order, so the sort is stable
            Array.Sort(order, (a, b) =>
            {
                var result = keys[a].CompareTo(keys[b]);
                return result != 0 ? result : a.CompareTo(b);
            });
        }

        if (!_sortAscending)
        {
            Array.Reverse(order);
        }

        _order = order;
        _orderPosition = new int[count];

        for (int position = 0; position < count; position++)
        {
            _orderPosition[order[position]] = position;
        }
    }

    private string GetSortText(int index, int aliasColumn)
    {
        if (_sortColumn == NameColumn)
            return GetCurrentLabel(index).label;

        if (_sortColumn == aliasColumn)
            return GetCurrentLabel(index).alias;

        // Issues and uses sort as numbers: errors above warnings, and the most used first when descending
        if (_sortColumn == IssuesColumn)
            return GetIssueRank(_cachedEntries[index].ent).ToString();

        if (_sortColumn == UsedByColumn)
            return index < _usedBy.Length && _usedBy[index] >= 0 ? _usedBy[index].ToString() : "";

        var c = _sortColumn - FirstFieldColumn;

        return index < _cells.Length && c >= 0 && c < _cells[index].Length ? _cells[index][c] : "";
    }

    /// <summary>
    /// A cell as it is sorted: numbers (and vectors, by their components) by value, before text,
    /// and empty cells last
    /// </summary>
    private readonly struct SortKey : IComparable<SortKey>
    {
        private readonly string Text;
        private readonly double[] Numbers;

        public SortKey(string text)
        {
            Text = text ?? "";
            Numbers = MsbQueryMatcher.ParseNumbers(Text);
        }

        private int Rank => Numbers != null ? 0 : Text.Length > 0 ? 1 : 2;

        public int CompareTo(SortKey other)
        {
            var rank = Rank.CompareTo(other.Rank);

            if (rank != 0)
                return rank;

            if (Numbers != null)
            {
                for (int i = 0; i < Math.Min(Numbers.Length, other.Numbers.Length); i++)
                {
                    var result = Numbers[i].CompareTo(other.Numbers[i]);

                    if (result != 0)
                        return result;
                }

                return Numbers.Length.CompareTo(other.Numbers.Length);
            }

            return string.Compare(Text, other.Text, StringComparison.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// The button that opens the column picker: the fields of the category's first entry, to show or hide
    /// </summary>
    private void DisplayColumnsButton()
    {
        var subType = Selection.SelectedSubCategoryType;

        ImGui.SameLine();

        ImGui.BeginDisabled(subType == null || _cachedEntries.Count == 0);

        if (ImGui.Button($"{Icons.Columns}##entryColumns", DPI.IconButtonSize))
        {
            ImGui.OpenPopup("##entryColumnsPopup");
        }

        ImGui.EndDisabled();

        GUI.Tooltip("Choose the fields shown as columns for this category.");

        ImGui.SetNextWindowSize(new Vector2(420, 420) * DPI.UIScale(), ImGuiCond.Appearing);

        if (subType != null && _cachedEntries.Count > 0 && ImGui.BeginPopup("##entryColumnsPopup"))
        {
            DisplayColumnPicker(subType, _cachedEntries[0].entry);

            ImGui.EndPopup();
        }
    }

    private void DisplayColumnPicker(Type subType, object sample)
    {
        if (_pickerFieldsFor != sample)
        {
            _pickerFieldsFor = sample;
            _pickerFields = _fieldLister.GetFields(sample);
        }

        // Shown columns, in order
        ImGui.TextDisabled("Shown columns");

        if (_columns.Count == 0)
        {
            ImGui.TextDisabled("None.");
        }

        for (int c = 0; c < _columns.Count; c++)
        {
            var path = _columns[c];

            ImGui.BeginDisabled(c == 0);
            if (ImGui.ArrowButton($"##columnUp_{path}", ImGuiDir.Up))
            {
                MoveColumn(subType, c, c - 1);
            }
            ImGui.EndDisabled();

            ImGui.SameLine();

            ImGui.BeginDisabled(c == _columns.Count - 1);
            if (ImGui.ArrowButton($"##columnDown_{path}", ImGuiDir.Down))
            {
                MoveColumn(subType, c, c + 1);
            }
            ImGui.EndDisabled();

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Times}##columnRemove_{path}"))
            {
                SetColumn(subType, path, false);
            }
            GUI.Tooltip("Hides this column.");

            ImGui.SameLine();
            ImGui.TextUnformatted(path);
        }

        if (MsbEntryColumns.HasCustomColumns(subType))
        {
            if (ImGui.Button("Reset to Defaults"))
            {
                MsbEntryColumns.ResetColumns(subType);
                _cellsDirty = true;
            }
            GUI.Tooltip("Shows the default columns for this category again.");
        }

        ImGui.Separator();

        // Every field of the sample entry, to tick on or off
        ImGui.TextDisabled($"Fields of {GetEntryLabel(sample, 0)}");

        EditorFilters.DisplayFramedListFilter("entryColumnsFilter", ref _pickerFilter, ref _pickerExactFilter);

        ImGui.BeginChild("##entryColumnsFields", new Vector2(0, 0), ImGuiChildFlags.Borders);

        for (int i = 0; i < _pickerFields.Count; i++)
        {
            var field = _pickerFields[i];

            if (!EditorFilters.IsMatch(_pickerFilter, field.Path, _pickerExactFilter, field.DisplayName))
                continue;

            var shown = _columns.Contains(field.Path);
            var label = field.DisplayName != field.Name ? $"{field.Path}  ({field.DisplayName})" : field.Path;

            if (ImGui.Checkbox($"{label}  = {field.Shown}##entryColumnField_{i}", ref shown))
            {
                SetColumn(subType, field.Path, shown);
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Shows or hides a column. The columns are copied first, so the defaults become this type's own columns.
    /// </summary>
    private void SetColumn(Type subType, string path, bool shown)
    {
        var columns = new List<string>(_columns);

        if (shown && !columns.Contains(path))
        {
            columns.Add(path);
        }
        else if (!shown)
        {
            columns.Remove(path);
        }

        MsbEntryColumns.SetColumns(subType, columns);
        _cellsDirty = true;
    }

    private void MoveColumn(Type subType, int from, int to)
    {
        var columns = new List<string>(_columns);

        (columns[from], columns[to]) = (columns[to], columns[from]);

        MsbEntryColumns.SetColumns(subType, columns);
        _cellsDirty = true;
    }

    private void DisplayEntityRow(int listIndex, string label, MsbEntity ent)
    {
        bool isSelected = View.ViewportSelection.IsSelected(ent);

        // Added and modified entries are tinted as in the Map Content list
        var vanillaColour = View.MapContentView.PushVanillaColour(ent);

        var clicked = ImGui.Selectable($"{label}##{listIndex}", isSelected, ImGuiSelectableFlags.AllowDoubleClick);

        if (vanillaColour)
        {
            ImGui.PopStyleColor();
        }

        ScrollToIfTarget(ent);

        if (clicked)
        {
            HandleEntrySelection(listIndex, ent);

            // Selected here, so there is nothing to follow
            _lastFollowed = ent;

            // Double-click frames the entry in the viewport, as in the Map Content list
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && ent.RenderSceneMesh != null)
            {
                View.ViewportHandler.ActiveViewport.Viewport.FrameBox(ent.RenderSceneMesh.GetBounds(), new Vector3());
            }
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

        if (ImGui.BeginPopupContextItem($"##EntryCtx_{listIndex}"))
        {
            DisplayCopyActions(label, ent);

            // Models are not map objects, so the map object actions don't apply to them
            if (ent.Type != MsbEntityType.Models)
            {
                ImGui.Separator();

                View.MapContentView.DisplayMapObjectContextActions(ent);
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Copies the entry's name, and its entity ID if it has one, for use in scripts such as EMEVD
    /// </summary>
    private void DisplayCopyActions(string label, MsbEntity ent)
    {
        if (ImGui.Selectable("Copy Name"))
        {
            ImGui.SetClipboardText(label);
        }
        GUI.Tooltip("Copies the name of this entry to the clipboard.");

        // Read by field, as the entity ID's type differs between games
        var entityId = ent.WrappedObject?.GetType().GetProperty("EntityID")?.GetValue(ent.WrappedObject);

        if (entityId != null)
        {
            if (ImGui.Selectable("Copy Entity ID"))
            {
                ImGui.SetClipboardText(entityId.ToString());
            }
            GUI.Tooltip($"Copies the entity ID of this entry ({entityId}) to the clipboard.");
        }
    }

    private void HandleEntrySelection(int listIndex, MsbEntity ent)
    {
        var selection = View.ViewportSelection;

        // Multi-Select: Range Select
        if (InputManager.HasShiftDown() && _lastClickedIndex != -1 &&
            _lastClickedIndex < _orderPosition.Length && listIndex < _orderPosition.Length)
        {
            // The range is of the rows as they are shown, so it follows the sort
            var from = _orderPosition[_lastClickedIndex];
            var to = _orderPosition[listIndex];

            var start = Math.Min(from, to);
            var end = Math.Max(from, to);

            // Only the rows the filter shows, so hidden entries are not selected
            for (int position = start; position <= end; position++)
            {
                var k = _order[position];
                var rangeEnt = _cachedEntries[k].ent;

                var (rangeLabel, rangeAlias) = GetCurrentLabel(k);

                if (rangeEnt != null && IsShown(rangeLabel, rangeAlias))
                {
                    selection.AddSelection(rangeEnt);
                }
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

    /// <summary>
    /// The number of entries that are any of the given types. Each entry is counted once, though sub-categories
    /// can overlap (a type and its derived type), so a base category's count isn't the sum of its sub-categories.
    /// </summary>
    public int GetCachedEntryCount(ICollection<Type> types)
    {
        int total = 0;
        foreach (var (type, count) in _mapEntryCountByType)
        {
            if (types.Any(t => t.IsAssignableFrom(type)))
                total += count;
        }

        return total;
    }

    public void RebuildEntryCache()
    {
        _cachedEntries.Clear();

        // The rows are about to change, so an edit in progress no longer has its row
        _editRow = -1;
        _editColumn = -1;
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
