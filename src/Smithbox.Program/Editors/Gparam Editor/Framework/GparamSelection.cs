using Google.Protobuf.WellKnownTypes;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.GparamEditor;

public class GparamSelection
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    public FileDictionaryEntry SelectedFileEntry;

    public GPARAM _selectedGparam;
    public string _selectedGparamKey;

    public string _selectedParamGroupKey;
    public int _selectedParamGroupIndex;

    public string _selectedParamFieldKey;
    public int _selectedParamFieldIndex;

    public int _selectedFieldValueKey;
    public int _selectedFieldValueIndex;

    // The anchor value itself, so its index can be found again after values are added or removed
    private GPARAM.IFieldValue _selectedFieldValue;

    public SortedDictionary<int, GPARAM.IFieldValue> SelectedFieldValues = new();

    public int DuplicateValueID = 0;
    public int DuplicateValueOffset = 0;

    public bool SelectGparamFile = false;
    public bool SelectGparamGroup = false;
    public bool SelectGparamField = false;

    public bool FocusFile = false;
    public bool FocusGroup = false;
    public bool FocusField = false;
    public bool FocusValue = false;

    public GparamSelection(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void ResetSelection()
    {
        ResetGparamFileSelection();
        ResetGparamGroupSelection();
        ResetGparamFieldSelection();
        ResetGparamFieldValueSelection();

        SelectedFieldValues.Clear();
    }

    public bool CanAffectSelection()
    {
        if(IsFileSelected() && 
            IsGparamGroupSelected() && 
            IsGparamFieldSelected() && 
            HasValidFieldValueSelection())
        {
            return true;
        }

        return false;
    }

    public void ResetGparamFileSelection()
    {
        _selectedGparam = null;
        _selectedGparamKey = "";

        SelectedFieldValues.Clear();
    }

    public void ResetGparamGroupSelection()
    {
        _selectedParamGroupKey = null;
        _selectedParamGroupIndex = -1;

        SelectedFieldValues.Clear();
    }

    public void ResetGparamFieldSelection()
    {
        _selectedParamFieldKey = null;
        _selectedParamFieldIndex = -1;

        SelectedFieldValues.Clear();
    }

    public void ResetGparamFieldValueSelection()
    {
        _selectedFieldValueKey = -1;
        _selectedFieldValueIndex = -1;
        _selectedFieldValue = null;

        SelectedFieldValues.Clear();
    }

    /// <summary>
    /// Bring the selection back in line with the data after it has changed underneath it (add, delete, undo, redo,
    /// import). The row highlight and context menus go by index while the lookups go by key, so after a change the
    /// indices are found again from the keys, and any level whose entry is gone is cleared.
    /// </summary>
    public void Reconcile()
    {
        // File
        if (SelectedFileEntry == null || _selectedGparam == null)
            return;

        var bank = Project.Handler.GparamData.PrimaryBank;

        if (!bank.Entries.TryGetValue(SelectedFileEntry, out var wrapper) || wrapper.Gparam == null)
        {
            ResetSelection();
            SelectedFileEntry = null;
            return;
        }

        // An import or its undo can swap the file's GPARAM for another instance
        if (wrapper.Gparam != _selectedGparam)
        {
            _selectedGparam = wrapper.Gparam;
        }

        // Group
        if (_selectedParamGroupKey == null)
            return;

        var groupIndex = _selectedGparam.Params.FindIndex(e => e != null && e.Key == _selectedParamGroupKey);

        if (groupIndex == -1)
        {
            ResetGparamGroupSelection();
            ResetGparamFieldSelection();
            ResetGparamFieldValueSelection();
            return;
        }

        _selectedParamGroupIndex = groupIndex;

        var group = _selectedGparam.Params[groupIndex];

        // Field
        if (_selectedParamFieldKey == null)
            return;

        var fieldIndex = group.Fields.FindIndex(e => e != null && e.Key == _selectedParamFieldKey);

        if (fieldIndex == -1)
        {
            ResetGparamFieldSelection();
            ResetGparamFieldValueSelection();
            return;
        }

        _selectedParamFieldIndex = fieldIndex;

        var field = group.Fields[fieldIndex];

        // Values: re-key the multi-selection by each value's current index, dropping the ones no longer in the field
        if (SelectedFieldValues.Count > 0)
        {
            var moved = false;
            var remapped = new SortedDictionary<int, GPARAM.IFieldValue>();

            foreach (var entry in SelectedFieldValues)
            {
                var curIndex = field.IndexOfValue(entry.Value);

                if (curIndex != entry.Key)
                    moved = true;

                if (curIndex >= 0)
                    remapped[curIndex] = entry.Value;
            }

            if (moved)
            {
                SelectedFieldValues.Clear();

                foreach (var entry in remapped)
                {
                    SelectedFieldValues.Add(entry.Key, entry.Value);
                }
            }
        }

        // The anchor value
        if (_selectedFieldValue != null)
        {
            var anchorIndex = field.IndexOfValue(_selectedFieldValue);

            if (anchorIndex == -1)
            {
                _selectedFieldValue = null;
                _selectedFieldValueKey = -1;
                _selectedFieldValueIndex = -1;
            }
            else
            {
                _selectedFieldValueIndex = anchorIndex;
                _selectedFieldValueKey = _selectedFieldValue.ID;
            }
        }
    }

    /// <summary>
    /// Has the selected GPARAM file.
    /// </summary>
    public bool IsFileSelected()
    {
        if(_selectedGparam != null && _selectedGparamKey != "")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the selected GPARAM file.
    /// </summary>
    /// <remarks>
    /// Loads the file on the calling thread, so the selection is complete when this returns: callers that go on to
    /// select a group (the command queue, the data finder) see this file, not the previous one.
    /// </remarks>
    public void SetFileSelection(FileDictionaryEntry entry)
    {
        ResetGparamFileSelection();
        ResetGparamGroupSelection();
        ResetGparamFieldSelection();
        ResetGparamFieldValueSelection();

        SelectedFileEntry = entry;

        if (entry == null)
            return;

        var bank = Project.Handler.GparamData.PrimaryBank;

        bank.LoadGraphicsParamNow(entry);

        if (bank.Entries.TryGetValue(entry, out var wrapper) && wrapper.Gparam != null)
        {
            _selectedGparamKey = entry.Filename;
            _selectedGparam = wrapper.Gparam;
        }
    }

    /// <summary>
    /// Whether this is the selected file. Compares the whole entry: for BB, x.gparam and x.gparambnd share a name.
    /// </summary>
    public bool IsSelectedFile(FileDictionaryEntry entry)
    {
        return entry != null && SelectedFileEntry != null && entry.Equals(SelectedFileEntry);
    }

    /// <summary>
    /// Select just this value, ignoring held modifier keys (for jumping to a value from a tool)
    /// </summary>
    public void SelectSingleFieldValue(int index, GPARAM.IFieldValue entry)
    {
        if (entry == null)
            return;

        SelectedFieldValues.Clear();
        SelectedFieldValues.Add(index, entry);

        _selectedFieldValueKey = entry.ID;
        _selectedFieldValueIndex = index;
        _selectedFieldValue = entry;

        DuplicateValueID = GparamUtils.NextFreeValueId(GetSelectedField(), entry.ID);
    }

    /// <summary>
    /// Has the selected GPARAM group.
    /// </summary>
    public bool IsGparamGroupSelected()
    {
        if (_selectedParamGroupKey != null && _selectedParamGroupIndex != -1)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the selected GPARAM group.
    /// </summary>
    public void SetGparamGroup(int index, GPARAM.Param entry)
    {
        ResetGparamFieldSelection();
        ResetGparamFieldValueSelection();

        _selectedParamGroupKey = entry.Key;
        _selectedParamGroupIndex = index;
    }

    /// <summary>
    /// Has the selected GPARAM field.
    /// </summary>
    public bool IsGparamFieldSelected()
    {
        if (_selectedParamFieldKey != null && _selectedParamFieldIndex != -1)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the selected GPARAM field.
    /// </summary>
    public void SetGparamField(int index, GPARAM.IField entry)
    {
        ResetGparamFieldValueSelection();

        _selectedParamFieldKey = entry.Key;
        _selectedParamFieldIndex = index;
    }

    public bool HasSpecificFieldValueSelection(int index)
    {
        return SelectedFieldValues.ContainsKey(index);
    }

    public bool HasValidFieldValueSelection()
    {
        if (SelectedFieldValues.Count < 1)
        {
            return false;
        }

        return true;
    }

    public void SetGparamFieldValue(int index, GPARAM.IFieldValue entry)
    {
        HandleMultiselection(_selectedFieldValueIndex, index, entry);

        _selectedFieldValueKey = entry.ID;
        _selectedFieldValueIndex = index;
        _selectedFieldValue = entry;

        // Pre-fill the duplicate's ID with the next one the field doesn't use, as two values can't share an ID
        DuplicateValueID = GparamUtils.NextFreeValueId(GetSelectedField(), entry.ID);
    }

    public bool IsValueSelected(int index)
    {
        if (HasSpecificFieldValueSelection(index) || _selectedFieldValueIndex == index)
        {
            return true;
        }

        return false;
    }

    public void HandleMultiselection(int currentSelectionIndex, int currentIndex, GPARAM.IFieldValue entry)
    {
        var fieldList = GetSelectedField();

        if (fieldList == null)
            return;

        // Multi-Select: Range Select
        // Needs an anchor: with none yet (-1, e.g. just after picking the field) it falls through to a normal click
        if (InputManager.HasShiftDown() && currentSelectionIndex >= 0)
        {
            var start = currentSelectionIndex;
            var end = currentIndex;

            if (end < start)
            {
                start = currentIndex;
                end = currentSelectionIndex;
            }

            for (int k = start; k <= end; k++)
            {
                if (!SelectedFieldValues.ContainsKey(k) && k >= 0 && k < fieldList.Values.Count)
                {
                    var curValue = fieldList.Values.ElementAt(k);

                    var isMatch = EditorFilters.IsMatch(
                        Parent.FieldValueListView.ValueListFilter,
                        curValue.ID.ToString(),
                        Parent.FieldValueListView.ExactValueListFilter);

                    if (isMatch)
                    {
                        if (!SelectedFieldValues.ContainsKey(k))
                        {
                            SelectedFieldValues.Add(k, curValue);
                        }
                    }
                }
            }
        }
        // Multi-Select Mode
        else if (InputManager.HasCtrlDown())
        {
            if (SelectedFieldValues.ContainsKey(currentIndex) && SelectedFieldValues.Count > 1)
            {
                SelectedFieldValues.Remove(currentIndex);
            }
            else
            {
                if (!SelectedFieldValues.ContainsKey(currentIndex))
                {
                    if (currentIndex < fieldList.Values.Count)
                    {
                        var curEntry = fieldList.Values[currentIndex];

                        if (!SelectedFieldValues.ContainsKey(currentIndex))
                        {
                            SelectedFieldValues.Add(currentIndex, curEntry);
                        }
                    }
                }
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            SelectedFieldValues.Clear();

            if (currentIndex < fieldList.Values.Count)
            {
                var curEntry = fieldList.Values[currentIndex];

                if (!SelectedFieldValues.ContainsKey(currentIndex))
                {
                    SelectedFieldValues.Add(currentIndex, curEntry);
                }
            }
        }
    }

    /// <summary>
    /// Get currently selected GPARAM
    /// </summary>
    public GPARAM GetSelectedGparam()
    {
        return _selectedGparam;
    }

    /// <summary>
    /// Get currently selected GPARAM.Param
    /// </summary>
    public GPARAM.Param GetSelectedGroup()
    {
        if (_selectedGparam != null)
        {
            if (_selectedGparam.Params.Any(e => e.Key == _selectedParamGroupKey))
            {
                return _selectedGparam.Params.First(e => e.Key == _selectedParamGroupKey);
            }
        }

        return null;
    }

    /// <summary>
    /// Get currently selected GPARAM.IField
    /// </summary>
    public GPARAM.IField GetSelectedField()
    {
        var group = GetSelectedGroup();

        if (group == null)
            return null;

        if (group.Fields.Any(e => e.Key == _selectedParamFieldKey))
        {
            return group.Fields.First(e => e.Key == _selectedParamFieldKey);
        }

        return null;
    }

    /// <summary>
    /// Get currently selected GPARAM.IFieldValue
    /// </summary>
    public GPARAM.IFieldValue GetSelectedValue()
    {
        var group = GetSelectedGroup();
        var field = GetSelectedField();

        if (group == null)
            return null;

        if (field == null)
            return null;
        
        if (field.Values.Any(e => e.ID == _selectedFieldValueKey))
        {
            return field.Values.First(e => e.ID == _selectedFieldValueKey);
        }

        return null;
    }

    public List<GPARAM.IFieldValue> GetSelectedValues()
    {
        return SelectedFieldValues.Values.ToList();
    }
}
