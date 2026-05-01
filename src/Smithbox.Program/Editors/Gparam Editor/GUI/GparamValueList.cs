using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Linq;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamValueList
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    private bool[] displayTruth;

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

        if (Parent.Selection.IsGparamFieldSelected())
        {
            GPARAM.IField field = Parent.Selection.GetSelectedGparamField();

            ResetDisplayTruth(field);

            var columnCount = 2;

            if (CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column)
                columnCount++;

            if (CFG.Current.GparamEditor_Value_List_Display_Information_Column)
                columnCount++;

            ImGui.Columns(columnCount);

            // ID
            ImGui.BeginChild("IdList##GparamPropertyIds");
            UIHelper.SimpleHeader("ID", "");

            for (int i = 0; i < field.Values.Count; i++)
            {
                GPARAM.IFieldValue entry = field.Values[i];

                displayTruth[i] = Parent.Filters.IsFieldValueFilterMatch(entry.ID.ToString(), "");

                if (displayTruth[i])
                {
                    GparamProperty_ID(i, field, entry);
                }
            }

            ImGui.EndChild();

            if (CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column)
            {
                ImGui.NextColumn();

                // Time of Day
                ImGui.BeginChild("IdList##GparamTimeOfDay");
                UIHelper.SimpleHeader("Time of Day", "");

                for (int i = 0; i < field.Values.Count; i++)
                {
                    if (displayTruth[i])
                    {
                        GPARAM.IFieldValue entry = field.Values[i];
                        GparamProperty_TimeOfDay(i, field, entry);
                    }
                }

                ImGui.EndChild();
            }

            ImGui.NextColumn();

            // Value
            ImGui.BeginChild("ValueList##GparamPropertyValues");
            UIHelper.SimpleHeader("Value", "");

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
            if (CFG.Current.GparamEditor_Value_List_Display_Information_Column)
            {
                ImGui.NextColumn();

                // Value
                ImGui.BeginChild("InfoList##GparamPropertyInfo");
                UIHelper.SimpleHeader("Information", "");

                // Only show once
                GparamProperty_Info(field);

                ImGui.EndChild();
            }
        }

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

        string name = value.ID.ToString();

        if (ImGui.Selectable($"{name}##{index}", index == Parent.Selection._selectedFieldValueKey))
        {
            Parent.Selection.SetGparamFieldValue(index, value);
        }

        Parent.ContextMenu.FieldValueContextMenu(index);
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
        Parent.PropertyEditor.TimeOfDayField(index, field, value);
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
        Parent.PropertyEditor.ValueField(index, field, value);
    }

    /// <summary>
    /// Values table: Information column
    /// </summary>
    /// <param name="field"></param>
    public void GparamProperty_Info(IField field)
    {
        ImGui.AlignTextToFramePadding();

        var groupId = Parent.Selection.GetSelectedGparamGroup().Key;
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

                foreach(var entry in targetEnum.Options)
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
}


