using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.GparamEditor.Utils;
using StudioCore.Interface;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamFieldEntryView
{
    public GparamEditor Editor;
    public Project Project;

    private bool[] displayTruth;

    public GparamFieldEntryView(Project curPoject, GparamEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }

    public void Draw()
    {
        ImGui.Begin("Values##GparamValues");
        Editor.EditorFocus.SetFocusContext(GparamEditorContext.FieldValue);

        Editor.Filters.DisplayFieldValueFilterSearch();

        if (Editor.Selection.HasFieldSelected())
        {
            GPARAM.IField field = Editor.Selection.GetSelectedGparamField();

            ResetDisplayTruth(field);

            ImGui.Columns(4);

            // ID
            ImGui.BeginChild("IdList##GparamPropertyIds");
            ImGui.Text($"ID");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                GPARAM.IFieldValue entry = field.Values[i];

                displayTruth[i] = Editor.Filters.IsFieldValueFilterMatch(entry.Id.ToString(), "");

                if (displayTruth[i])
                {
                    GparamProperty_ID(i, field, entry);
                }
            }

            // Display "Add" button if field has no value rows.
            if (field.Values.Count <= 0)
            {
                if (ImGui.Button("Add"))
                {
                    Editor.FieldInput.AddValueField(field);
                    ResetDisplayTruth(field);
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            // Time of Day
            ImGui.BeginChild("IdList##GparamTimeOfDay");
            ImGui.Text($"Time of Day");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (displayTruth[i])
                {
                    GPARAM.IFieldValue entry = field.Values[i];
                    GparamProperty_TimeOfDay(i, field, entry);
                }
            }

            ImGui.EndChild();

            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("ValueList##GparamPropertyValues");
            ImGui.Text($"Value");
            ImGui.Separator();

            for (int i = 0; i < field.Values.Count; i++)
            {
                if (displayTruth[i])
                {
                    GPARAM.IFieldValue entry = field.Values[i];
                    GparamProperty_Value(i, field, entry);
                }
            }

            ImGui.EndChild();

            // Information
            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("InfoList##GparamPropertyInfo");
            ImGui.Text($"Information");
            ImGui.Separator();

            // Only show once
            GparamProperty_Info(field);

            ImGui.EndChild();
        }

        ImGui.End();
    }

    /// <summary>
    /// Reset the Values display truth list
    /// </summary>
    /// <param name="field"></param>
    public void ResetDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count];

        for (int i = 0; i < field.Values.Count; i++)
        {
            displayTruth[i] = true;
        }
    }

    /// <summary>
    /// Extend the Values display truth list in preparation for value row addition.
    /// </summary>
    /// <param name="field"></param>
    public void ExtendDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count + 1];

        for (int i = 0; i < field.Values.Count + 1; i++)
        {
            displayTruth[i] = true;
        }
    }
    /// <summary>
    /// REduce the Values display truth list in preparation for value row removal.
    /// </summary>
    /// <param name="field"></param>
    public void ReduceDisplayTruth(IField field)
    {
        displayTruth = new bool[field.Values.Count + -1];

        for (int i = 0; i < field.Values.Count + -1; i++)
        {
            displayTruth[i] = true;
        }
    }

    /// <summary>
    /// Values table: ID column
    /// </summary>
    /// <param name="index"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void GparamProperty_ID(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();

        string name = value.Id.ToString();

        if (ImGui.Selectable($"{name}##{index}", index == Editor.Selection._selectedFieldValueKey))
        {
            Editor.Selection.SelectFieldValue(index, value);
        }

        // Context menu

        if (index == Editor.Selection._selectedFieldValueKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_PropId_Context"))
            {
                if (ImGui.Selectable("Target in Quick Edit"))
                {
                    var fieldIndex = -1;
                    for (int i = 0; i < Editor.Selection._selectedParamField.Values.Count; i++)
                    {
                        if (Editor.Selection._selectedParamField.Values[i] == Editor.Selection._selectedFieldValue)
                        {
                            fieldIndex = i;
                            break;
                        }
                    }

                    if (fieldIndex != -1)
                    {
                        Editor.QuickEdit.UpdateValueRowFilter(fieldIndex);
                    }

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Add this field to the Field Filter in the Quick Edit window.");

                if (ImGui.Selectable("Remove"))
                {
                    Editor.ActionHandler.DeleteValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Delete the value row.");

                if (ImGui.Selectable("Duplicate"))
                {
                    Editor.ActionHandler.DuplicateValueRow();

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Duplicate the selected value row, assigning the specified ID below as the new id.");

                ImGui.InputInt("##valueIdInput", ref Editor.Selection._duplicateValueRowId);

                if (Editor.Selection._duplicateValueRowId < 0)
                {
                    Editor.Selection._duplicateValueRowId = 0;
                }

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// Values table: Time of Day column
    /// </summary>
    /// <param name="index"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void GparamProperty_TimeOfDay(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        Editor.FieldInput.TimeOfDayField(index, field, value);
    }

    /// <summary>
    /// Values table: Value column
    /// </summary>
    /// <param name="index"></param>
    /// <param name="field"></param>
    /// <param name="value"></param>
    public void GparamProperty_Value(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        Editor.FieldInput.ValueField(index, field, value);
    }

    /// <summary>
    /// Values table: Information column
    /// </summary>
    /// <param name="field"></param>
    public void GparamProperty_Info(IField field)
    {
        ImGui.AlignTextToFramePadding();

        string desc = Project.GparamData.Meta.GetReferenceDescription(Editor.Selection._selectedParamGroup.Key, Editor.Selection._selectedParamField.Key);

        UIHelper.WrappedText($"Type: {GparamUtils.GetReadableObjectTypeName(field)}");
        UIHelper.WrappedText($"");

        // Skip if empty
        if (desc != "")
        {
            UIHelper.WrappedText($"{desc}");
        }

        // Show enum list if they exist
        var propertyEnum = Project.GparamData.Meta.GetEnumForProperty(field.Key);
        if (propertyEnum != null)
        {
            foreach (var entry in propertyEnum.members)
            {
                UIHelper.WrappedText($"{entry.id} - {entry.name}");
            }
        }
    }
}
