using SoulsFormats;
using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamSelection
{
    public Project Project;
    public GparamEditor Editor;

    public GPARAM _selectedGparam;
    public string _selectedGparamKey;

    public GPARAM.Param _selectedParamGroup;
    public int _selectedParamGroupKey;

    public GPARAM.IField _selectedParamField;
    public int _selectedParamFieldKey;

    public GPARAM.IFieldValue _selectedFieldValue = null;
    public int _selectedFieldValueKey;

    public int _duplicateValueRowId = 0;

    public bool AutoSelectGparam = false;
    public bool AutoSelectGroup = false;
    public bool AutoSelectField = false;

    public GparamSelection(Project project, GparamEditor editor)
    {
        Project = project;
        Editor = editor;
    }

    public bool HasValidSelectionForQuickEdit()
    {
        if (HasGparamSelected() &&
            HasGroupSelected() &&
            HasFieldSelected() &&
            HasFieldValueSelected())
        {
            return true;
        }

        return false;
    }

    public bool HasGparamSelected()
    {
        if (_selectedGparamKey != "")
            return true;

        return false;
    }

    public bool IsGparamSelected(string key)
    {
        if (_selectedGparamKey == key)
            return true;

        return false;
    }

    public void SelectGparam(string key, GPARAM gparam)
    {
        _selectedGparamKey = key;
        _selectedGparam = gparam;
    }

    public bool IsGroupSelected(int index)
    {
        if (_selectedParamGroupKey == index)
            return true;

        return false;
    }

    public bool HasGroupSelected()
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
    public void SelectGroup(int index, GPARAM.Param entry)
    {
        ResetGparamFieldSelection();
        ResetGparamFieldValueSelection();

        _selectedParamGroup = entry;
        _selectedParamGroupKey = index;
    }

    public bool IsFieldSelected(int index)
    {
        if (_selectedParamFieldKey == index)
            return true;

        return false;
    }

    /// <summary>
    /// Has the selected GPARAM field.
    /// </summary>
    public bool HasFieldSelected()
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
    public void SelectField(int index, GPARAM.IField entry)
    {
        ResetGparamFieldValueSelection();

        _selectedParamField = entry;
        _selectedParamFieldKey = index;
        Editor.QuickEdit.targetParamField = entry;
    }

    public bool IsFieldValueSelected(int index)
    {
        if (_selectedFieldValueKey == index)
            return true;

        return false;
    }

    /// <summary>
    /// Has the selected GPARAM field value.
    /// </summary>
    public bool HasFieldValueSelected()
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
    public void SelectFieldValue(int index, GPARAM.IFieldValue entry)
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

}
