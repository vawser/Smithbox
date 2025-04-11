using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.Editors.GparamEditor.Enums;
using StudioCore.Editors.TextEditor;
using StudioCore.GraphicsEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.GparamEditor.Data.GparamParamBank;

namespace StudioCore.Editors.GparamEditor;

public class GparamSelectionManager
{
    private GparamEditorScreen Screen;

    public GparamParamBank.GparamInfo _selectedGparamInfo;
    public GPARAM _selectedGparam;
    public string _selectedGparamKey;

    public GPARAM.Param _selectedParamGroup;
    public int _selectedParamGroupKey;

    public GPARAM.IField _selectedParamField;
    public int _selectedParamFieldKey;

    public GPARAM.IFieldValue _selectedFieldValue = null;
    public int _selectedFieldValueKey;

    public int _duplicateValueRowId = 0;

    public bool SelectGparamFile = false;
    public bool SelectGparamGroup = false;
    public bool SelectGparamField = false;



    public GparamSelectionManager(GparamEditorScreen screen)
    {
        Screen = screen;
    }

    public void ToggleSelectedFileModifiedState(bool state)
    {
        _selectedGparamInfo.WasModified = state;
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
        _selectedParamGroup = null;
        _selectedParamGroupKey = -1;
    }

    public void ResetGparamFieldSelection()
    {
        _selectedParamField = null;
        _selectedParamFieldKey = -1;
    }

    public void ResetGparamFieldValueSelection()
    {
        _selectedFieldValue = null;
        _selectedFieldValueKey = -1;
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
    public void SetFileSelection(GparamInfo info)
    {
        ResetGparamGroupSelection();
        ResetGparamFieldSelection();
        ResetGparamFieldValueSelection();

        _selectedGparamKey = info.Name;
        _selectedGparamInfo = info;
        _selectedGparam = info.Gparam;

    }

    /// <summary>
    /// Has the selected GPARAM group.
    /// </summary>
    public bool IsGparamGroupSelected()
    {
        if (_selectedParamGroup != null && _selectedParamGroupKey != -1)
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

        _selectedParamGroup = entry;
        _selectedParamGroupKey = index;
    }

    /// <summary>
    /// Has the selected GPARAM field.
    /// </summary>
    public bool IsGparamFieldSelected()
    {
        if (_selectedParamField != null && _selectedParamFieldKey != -1)
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

        _selectedParamField = entry;
        _selectedParamFieldKey = index;
        Screen.QuickEditHandler.targetParamField = entry;
    }

    /// <summary>
    /// Has the selected GPARAM field value.
    /// </summary>
    public bool IsGparamFieldValueSelected()
    {
        if (_selectedFieldValue != null && _selectedFieldValueKey != -1)
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
        _selectedFieldValue = entry;
        _selectedFieldValueKey = index;
        _duplicateValueRowId = entry.Id;
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
    public GPARAM.Param GetSelectedGparamGroup()
    {
        return _selectedParamGroup;
    }

    /// <summary>
    /// Get currently selected GPARAM.IField
    /// </summary>
    public GPARAM.IField GetSelectedGparamField()
    {
        return _selectedParamField;
    }

    /// <summary>
    /// Get currently selected GPARAM.IFieldValue
    /// </summary>
    public GPARAM.IFieldValue GetSelectedGparamFieldValue()
    {
        return _selectedFieldValue;
    }

    public GparamEditorContext CurrentWindowContext = GparamEditorContext.None;

    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SwitchWindowContext(GparamEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            CurrentWindowContext = newContext;
            //TaskLogs.AddLog($"Context: {newContext.GetDisplayName()}");
        }
    }
}
