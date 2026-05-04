using Google.Protobuf.WellKnownTypes;
using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamValueList
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    public GparamValueList(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the event parameter view
    /// </summary>
    public void Display()
    {
        DisplayHeader();

        // Values
        ImGui.BeginChild("valueListTable", ImGuiChildFlags.Borders);

        DisplayValueTable();

        ImGui.EndChild();
    }
    public void DisplayHeader()
    {
        UIHelper.SimpleHeader("Values", "");

        // Search
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("GparamFieldSearchSection", searchHeight, ImGuiChildFlags.Borders);

        Parent.Filters.DisplayFieldValueFilterSearch();

        // Time of Day Toggle
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.ClockO}##todColumnToggle"))
        {
            CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column = !CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column;
        }

        var todColumnMode = "Displaying Time of Day column.";
        if (!CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column)
        {
            todColumnMode = "Hiding Time of Day column.";
        }
        UIHelper.Tooltip($"Toggle the display of the Time of Day column.\nCurrent Mode: {todColumnMode}");

        // Information Toggle
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Info}##infoColumnToggle"))
        {
            CFG.Current.GparamEditor_Value_List_Display_Information_Column = !CFG.Current.GparamEditor_Value_List_Display_Information_Column;
        }

        var infoColumnMode = "Displaying Information column.";
        if (!CFG.Current.GparamEditor_Value_List_Display_Information_Column)
        {
            infoColumnMode = "Hiding Information column.";
        }
        UIHelper.Tooltip($"Toggle the display of the Information column.\nCurrent Mode: {infoColumnMode}");

        ImGui.EndChild();

    }
    private void DisplayValueTable()
    {
        if (!Parent.Selection.IsGparamFieldSelected())
            return;

        var data = Parent.Selection.GetSelectedGparam();
        var group = Parent.Selection.GetSelectedGroup();
        var field = Parent.Selection.GetSelectedField();

        if (data == null)
            return;

        if (group == null)
            return;

        if (field == null)
            return;

        var columnCount = 2;

        if (CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column)
            columnCount++;

        if (CFG.Current.GparamEditor_Value_List_Display_Information_Column)
            columnCount++;

        ImGui.Columns(columnCount);

        DisplayColumn_ID(data, group, field);
        DisplayColumn_TimeOfDay(data, group, field);
        DisplayColumn_Value(data, group, field);
        DisplayColumn_Info(data, group, field);

        ImGui.Columns(1);

        Shortcuts(data, group, field);
    }

    // ID
    private void DisplayColumn_ID(GPARAM data, GPARAM.Param group, IField field)
    {
        ImGui.BeginChild("GparamPropList_ID");
        UIHelper.SimpleHeader("ID", "");

        for (int i = 0; i < field.Values.Count; i++)
        {
            var value = field.Values[i];
            var display = Parent.Filters.IsFieldValueFilterMatch(value.ID.ToString(), "");

            if (!display)
                continue;

            GparamProperty_ID(data, group, field, value, i);
        }

        ImGui.EndChild();
    }

    public void GparamProperty_ID(GPARAM data, GPARAM.Param group, 
        IField field, IFieldValue value, int index)
    {
        var isSelected = index == Parent.Selection._selectedFieldValueIndex;

        string name = value.ID.ToString();

        ImGui.AlignTextToFramePadding();
        if (ImGui.Selectable($"{name}##{index}", isSelected))
        {
            Parent.Selection.SetGparamFieldValue(index, value);
        }

        ContextMenu(data, group, field, value, index);
    }

    // Time of Day
    private void DisplayColumn_TimeOfDay(GPARAM data, GPARAM.Param group, IField field)
    {
        if (!CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column)
            return;

        ImGui.NextColumn();

        ImGui.BeginChild("GparamPropList_TimeOfDay");
        UIHelper.SimpleHeader("Time of Day", "");

        for (int i = 0; i < field.Values.Count; i++)
        {
            var value = field.Values[i];
            var display = Parent.Filters.IsFieldValueFilterMatch(value.ID.ToString(), "");

            if (!display)
                continue;

            GparamProperty_TimeOfDay(i, field, value);
        }

        ImGui.EndChild();
    }
    public void GparamProperty_TimeOfDay(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        Parent.PropertyEditor.TimeOfDayField(index, field, value);
    }

    // Value
    private void DisplayColumn_Value(GPARAM data, GPARAM.Param group, IField field)
    {
        ImGui.NextColumn();

        ImGui.BeginChild("GparamPropList_Value");
        UIHelper.SimpleHeader("Value", "");

        for (int i = 0; i < field.Values.Count; i++)
        {
            var value = field.Values[i];
            var display = Parent.Filters.IsFieldValueFilterMatch(value.ID.ToString(), "");

            if (!display)
                continue;

            GparamProperty_Value(i, field, value);
        }

        ImGui.EndChild();
    }
    public void GparamProperty_Value(int index, IField field, IFieldValue value)
    {
        ImGui.AlignTextToFramePadding();
        Parent.PropertyEditor.ValueField(index, field, value);
    }

    // Information
    private void DisplayColumn_Info(GPARAM data, GPARAM.Param group, IField field)
    {
        if (!CFG.Current.GparamEditor_Value_List_Display_Information_Column)
            return;

        ImGui.NextColumn();

        ImGui.BeginChild("GparamPropList_Info");
        UIHelper.SimpleHeader("Information", "");

        GparamProperty_Info(field);

        ImGui.EndChild();
    }

    public void GparamProperty_Info(IField field)
    {
        ImGui.AlignTextToFramePadding();

        var groupId = Parent.Selection.GetSelectedGroup().Key;
        var fieldId = field.Key;
        var fieldDescription = GparamMetaUtils.GetFieldDescription(Project, groupId, fieldId);

        UIHelper.WrappedText($"Type: {GparamUtils.GetReadableObjectTypeName(field)}");
        UIHelper.WrappedText($"");

        // Skip if empty
        if (fieldDescription != "")
        {
            UIHelper.WrappedText($"{fieldDescription}");
        }

        var fieldEnum = GparamMetaUtils.GetFieldEnum(Project, groupId, fieldId);

        if (fieldEnum != null)
        {
            var enums = Project.Handler.GparamData.Enums.List;

            if (enums.Any(e => e.Key == fieldEnum))
            {
                var targetEnum = enums.FirstOrDefault(e => e.Key == fieldEnum);

                foreach (var entry in targetEnum.Options)
                {
                    var name = entry.Names.FirstOrDefault(e => e.Language == CFG.Current.GparamEditor_Annotation_Language);

                    if (name != null)
                    {
                        UIHelper.WrappedText($"{entry.Key} - {name}");
                    }
                    else
                    {
                        UIHelper.WrappedText($"{entry.Key}");
                    }
                }
            }
        }
    }

    public void ContextMenu(GPARAM data, GPARAM.Param group, IField field, IFieldValue value, int index)
    {
        if (index == Parent.Selection._selectedFieldValueIndex)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_PropId_Context"))
            {
                // Duplicate
                if (ImGui.BeginMenu("Duplicate"))
                {
                    // Input
                    ImGui.InputInt("ID##duplicateInput_ID", ref Parent.Selection.DuplicateValueID);

                    if (Parent.Selection.DuplicateValueID < 0)
                    {
                        Parent.Selection.DuplicateValueID = 0;
                    }

                    ImGui.InputInt("ID##duplicateInput_Offset", ref Parent.Selection.DuplicateValueOffset);

                    // Submit
                    if (ImGui.Selectable("Submit"))
                    {
                        AddValues(data, group, field, new List<GPARAM.IFieldValue>() { value }, false);
                    }
                    UIHelper.Tooltip("Duplicate the selected value row, assigning the specified ID below as the new id.");

                    ImGui.EndMenu();
                }

                // Delete
                if (ImGui.Selectable("Delete"))
                {
                    DeleteValues(data, group, field, new List<GPARAM.IFieldValue>() { value });

                    ImGui.CloseCurrentPopup();
                }
                UIHelper.Tooltip("Delete the value row.");


                ImGui.Separator();

                // Quick Edit
                if (ImGui.BeginMenu("Quick Edit"))
                {
                    // Target
                    if (ImGui.Selectable("Target"))
                    {
                        var fieldIndex = -1;
                        for (int i = 0; i < field.Values.Count; i++)
                        {
                            if (field.Values[i] == value)
                            {
                                fieldIndex = i;
                                break;
                            }
                        }

                        if (fieldIndex != -1)
                        {
                            Parent.QuickEditHandler.UpdateValueRowFilter(fieldIndex);
                        }

                        ImGui.CloseCurrentPopup();
                    }
                    UIHelper.Tooltip("Add this field to the Field Filter in the Quick Edit window.");

                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }
        }
    }

    private void Shortcuts(GPARAM data, GPARAM.Param group, IField field)
    {
        var value = Parent.Selection.GetSelectedValue();

        if (FocusManager.IsFocus(EditorFocusContext.GparamEditor_Properties))
        {
            // Duplicate
            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                AddValues(data, group, field, new List<IFieldValue>() { value }, true);
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                DeleteValues(data, group, field, new List<IFieldValue>() { value });
            }
        }
    }

    public void AddValues(GPARAM data, Param group, IField field, List<IFieldValue> entries, bool useDuplicateOffset)
    {
        var duplicateID = Parent.Selection.DuplicateValueID;
        var duplicateOffset = Parent.Selection.DuplicateValueOffset;

        var action = new AddValueAction(Project, data, group, field, entries, duplicateID, duplicateOffset, useDuplicateOffset);
        Parent.ActionManager.ExecuteAction(action);
    }

    public void DeleteValues(GPARAM data, Param group, IField field, List<IFieldValue> entries)
    {
        var action = new DeleteValueAction(Project, data, group, field, entries);
        Parent.ActionManager.ExecuteAction(action);
    }

    public void AddValuesShortcut()
    {
        var data = Parent.Selection.GetSelectedGparam();
        var group = Parent.Selection.GetSelectedGroup();
        var field = Parent.Selection.GetSelectedField();
        var value = Parent.Selection.GetSelectedValue();

        AddValues(data, group, field, new List<IFieldValue>() { value }, true);
    }

    public void DeleteValuesShortcut()
    {
        var data = Parent.Selection.GetSelectedGparam();
        var group = Parent.Selection.GetSelectedGroup();
        var field = Parent.Selection.GetSelectedField();
        var value = Parent.Selection.GetSelectedValue();

        DeleteValues(data, group, field, new List<IFieldValue>() { value });
    }
}


