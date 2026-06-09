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

    public SortedDictionary<int, int> SelectedFieldValues = new();

    public int DuplicateValueID = 0;
    public int DuplicateValueOffset = 0;

    public bool SelectGparamFile = false;
    public bool SelectGparamGroup = false;
    public bool SelectGparamField = false;

    public bool FocusFile = false;
    public bool FocusGroup = false;
    public bool FocusField = false;

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

        SelectedFieldValues.Clear();
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
    public async void SetFileSelection(FileDictionaryEntry entry)
    {
        ResetGparamGroupSelection();
        ResetGparamFieldSelection();
        ResetGparamFieldValueSelection();

        await Project.Handler.GparamData.PrimaryBank.LoadGraphicsParam(entry);

        SelectedFileEntry = entry;
        var targetEntry = Project.Handler.GparamData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename
         == entry.Filename && e.Key.Extension == entry.Extension);

        _selectedGparamKey = targetEntry.Key.Filename;
        _selectedGparam = targetEntry.Value;
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
        Parent.QuickEditHandler.targetParamField = entry;
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
        DuplicateValueID = entry.ID;
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
        if (InputManager.HasShiftDown())
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
                if (!SelectedFieldValues.ContainsKey(k) && k < fieldList.Values.Count)
                {
                    foreach (var val in fieldList.Values)
                    {
                        var isMatch = EditorFilters.IsMatch(
                            Parent.FieldValueListView.ValueListFilter,
                            val.ID.ToString(),
                            Parent.FieldValueListView.ExactValueListFilter);

                        if (isMatch)
                        {
                            if (!SelectedFieldValues.ContainsKey(k))
                            {
                                SelectedFieldValues.Add(k, val.ID);
                            }
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
                            SelectedFieldValues.Add(currentIndex, curEntry.ID);
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
                    SelectedFieldValues.Add(currentIndex, curEntry.ID);
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
}
