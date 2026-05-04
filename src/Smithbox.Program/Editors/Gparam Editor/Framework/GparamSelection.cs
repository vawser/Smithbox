using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
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

    public int DuplicateValueID = 0;
    public int DuplicateValueOffset = 0;

    public bool SelectGparamFile = false;
    public bool SelectGparamGroup = false;
    public bool SelectGparamField = false;

    public GparamSelection(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public bool CanAffectSelection()
    {
        if(IsFileSelected() && 
            IsGparamGroupSelected() && 
            IsGparamFieldSelected() && 
            IsGparamFieldValueSelected())
        {
            return true;
        }

        return false;
    }

    public void ResetGparamFileSelection()
    {
        _selectedGparam = null;
        _selectedGparamKey = "";
    }

    public void ResetGparamGroupSelection()
    {
        _selectedParamGroupKey = null;
        _selectedParamGroupIndex = -1;
    }

    public void ResetGparamFieldSelection()
    {
        _selectedParamFieldKey = null;
        _selectedParamFieldIndex = -1;
    }

    public void ResetGparamFieldValueSelection()
    {
        _selectedFieldValueKey = -1;
        _selectedFieldValueIndex = -1;
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
         == entry.Filename);

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

    /// <summary>
    /// Has the selected GPARAM field value.
    /// </summary>
    public bool IsGparamFieldValueSelected()
    {
        if (_selectedFieldValueKey != -1 && _selectedFieldValueIndex != -1)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Set the selected GPARAM field value.
    /// </summary>
    public void SetGparamFieldValue(int index, GPARAM.IFieldValue entry)
    {
        _selectedFieldValueKey = entry.ID;
        _selectedFieldValueIndex = index;
        DuplicateValueID = entry.ID;
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
        if(_selectedGparam.Params.Any(e => e.Key == _selectedParamGroupKey))
        {
            return _selectedGparam.Params.First(e => e.Key == _selectedParamGroupKey);
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
