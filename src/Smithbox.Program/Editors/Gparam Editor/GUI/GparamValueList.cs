using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamValueList
{
    private GparamEditorView View;
    private ProjectEntry Project;

    public string ValueListFilter = "";
    public bool ExactValueListFilter = false;

    public GparamValueList(GparamEditorView view, ProjectEntry project)
    {
        View = view;
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

        EditorFilters.DisplayListFilter("gparamEditor_ValueList",
            ref ValueListFilter, ref ExactValueListFilter);

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
        if (!View.Selection.IsGparamFieldSelected())
            return;

        var fileEntry = View.Selection.SelectedFileEntry;
        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();
        var field = View.Selection.GetSelectedField();

        if (data == null)
            return;

        if (group == null)
            return;

        if (field == null)
            return;

        var columnCount = 3;

        if (CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column)
            columnCount++;

        if (CFG.Current.GparamEditor_Value_List_Display_Information_Column)
            columnCount++;

        ImGui.Columns(columnCount);

        DisplayColumn_Row(fileEntry, data, group, field);
        DisplayColumn_ID(data, group, field);
        DisplayColumn_TimeOfDay(data, group, field);
        DisplayColumn_Value(data, group, field);
        DisplayColumn_Info(data, group, field);

        ImGui.Columns(1);

        Shortcuts(data, group, field);
    }

    // Row
    private void DisplayColumn_Row(FileDictionaryEntry fileEntry, GPARAM data, GPARAM.Param group, IField field)
    {
        ImGui.BeginChild("GparamPropList_Row");
        UIHelper.SimpleHeader("Row", "");

        for (int i = 0; i < field.Values.Count; i++)
        {
            var value = field.Values[i];
            if (value == null)
                continue;

            var isMatch = EditorFilters.IsMatch(ValueListFilter, value.ID.ToString(), ExactValueListFilter);

            if (!isMatch)
                continue;

            GparamProperty_Row(fileEntry, data, group, field, value, i);
        }


        if (field.Values.Count == 0)
        {
            DisplayDummySelectable(fileEntry, data, group, field);
        }

        ImGui.EndChild();
    }
    public void GparamProperty_Row(FileDictionaryEntry fileEntry, GPARAM data, GPARAM.Param group,
        IField field, IFieldValue value, int index)
    {
        var isSelected = View.Selection.IsValueSelected(index);

        ImGui.AlignTextToFramePadding();
        if (ImGui.Selectable($"Row {index}##{index}", isSelected))
        {
            View.Selection.SetGparamFieldValue(index, value);
        }

        ContextMenu(fileEntry, data, group, field, value, index);
    }

    // ID
    private void DisplayColumn_ID(GPARAM data, GPARAM.Param group, IField field)
    {
        ImGui.NextColumn();

        ImGui.BeginChild("GparamPropList_ID");
        UIHelper.SimpleHeader("ID", "");

        for (int i = 0; i < field.Values.Count; i++)
        {
            var value = field.Values[i];
            if (value == null)
                continue;

            var isMatch = EditorFilters.IsMatch(ValueListFilter, value.ID.ToString(), ExactValueListFilter);

            if (!isMatch)
                continue;

            GparamProperty_ID(data, group, field, value, i);
        }

        ImGui.EndChild();
    }

    public void GparamProperty_ID(GPARAM data, GPARAM.Param group, 
        IField field, IFieldValue value, int index)
    {
        ImGui.AlignTextToFramePadding();
        View.PropertyEditor.IdField(data, group, field, value, index);
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
            if (value == null)
                continue;

            var isMatch = EditorFilters.IsMatch(ValueListFilter, value.ID.ToString(), ExactValueListFilter);

            if (!isMatch)
                continue;

            GparamProperty_TimeOfDay(data, group, field, value, i);
        }

        ImGui.EndChild();
    }
    public void GparamProperty_TimeOfDay(GPARAM data, Param group, IField field, IFieldValue value, int index)
    {
        ImGui.AlignTextToFramePadding();
        View.PropertyEditor.TimeOfDayField(data, group, field, value, index);
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
            if (value == null)
                continue;

            var isMatch = EditorFilters.IsMatch(ValueListFilter, value.ID.ToString(), ExactValueListFilter);

            if (!isMatch)
                continue;

            GparamProperty_Value(data, group, field, value, i);
        }

        ImGui.EndChild();
    }
    public void GparamProperty_Value(GPARAM data, Param group, IField field, IFieldValue value, int index)
    {
        ImGui.AlignTextToFramePadding();
        View.PropertyEditor.ValueField(data, group, field, value, index);
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

        var groupId = View.Selection.GetSelectedGroup().Key;
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
                        UIHelper.WrappedText($"{entry.Key} - {name.Text}");
                    }
                    else
                    {
                        UIHelper.WrappedText($"{entry.Key}");
                    }
                }
            }
        }
    }

    private void DisplayDummySelectable(FileDictionaryEntry fileEntry, GPARAM data, Param group, IField field)
    {
        ImGui.BeginGroup();

        if (ImGui.Selectable($@" Empty##addValueDummy"))
        {
            AddNewValue(fileEntry, data, group, field);
        }
        UIHelper.Tooltip("Click to add new value.");

        ImGui.EndGroup();
    }

    private void AddNewValue(FileDictionaryEntry fileEntry, GPARAM data, Param group, IField field)
    {
        // Get the annotation for this field so we can seed to new value properly
        var potentialGroups = Project.Handler.GparamData.Annotations.Entries.FirstOrDefault(
            e => e.Key.Name == CFG.Current.GparamEditor_Annotation_Language);

        if (potentialGroups.Value == null)
            return;

        GparamAnnotationFieldEntry addValueAnnotation = null;

        var groups = potentialGroups.Value.Params.ToList();
        foreach (var curGroup in groups)
        {
            if (curGroup.ID == group.Key)
            {
                foreach (var curField in curGroup.Fields)
                {
                    if (curField.ID == field.Key)
                    {
                        addValueAnnotation = curField;
                    }
                }
            }
        }

        if (addValueAnnotation != null)
        {
            var action = new ReplaceFieldAction(Project, data, group, new List<GparamAnnotationFieldEntry>() { addValueAnnotation } );
            View.ActionManager.ExecuteAction(action);
        }
    }

    private string OverrideFileName = "";

    public void ContextMenu(FileDictionaryEntry fileEntry, GPARAM data, GPARAM.Param group, IField field, IFieldValue value, int index)
    {
        bool overwrite = CFG.Current.GparamEditor_Data_Import_Overwrite;

        if (index == View.Selection._selectedFieldValueIndex)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_PropId_Context"))
            {
                // Duplicate
                if (ImGui.BeginMenu("Duplicate"))
                {
                    // Input
                    ImGui.InputInt("ID##duplicateInput_ID", ref View.Selection.DuplicateValueID);

                    if (View.Selection.DuplicateValueID < 0)
                    {
                        View.Selection.DuplicateValueID = 0;
                    }

                    ImGui.InputInt("ID##duplicateInput_Offset", ref View.Selection.DuplicateValueOffset);

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

                // Import
                if (ImGui.Selectable("Import"))
                {
                    View.ToolView.DataTransferTool.ImportValue(Project, View, fileEntry, data, group, field, value, overwrite);
                }
                UIHelper.Tooltip("Import a GPARAM Value json to overwrite this entry.");

                // Export
                if (ImGui.BeginMenu("Export"))
                {
                    ImGui.InputText("##overrideFilename", ref OverrideFileName, 255);
                    UIHelper.Tooltip("Define the filename for the exported GPARAM Value file.");

                    if (ImGui.Selectable("Export File"))
                    {
                        View.ToolView.DataTransferTool.ExportValueFile(fileEntry, data, group, field, value, OverrideFileName);
                    }

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("Export this currently selected GPARAM Value to JSON.");

                ImGui.Separator();

                if (ImGui.MenuItem("Copy ID"))
                {
                    ImGui.SetClipboardText($"{value.ID}");
                }

                if (ImGui.MenuItem("Copy Value"))
                {
                    ImGui.SetClipboardText($"{value.Value.ToString()}");
                }

                ImGui.Separator();

                if (ImGui.BeginMenu("Target"))
                {
                    if (ImGui.Selectable("Quick Edit"))
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
                            View.QuickEditHandler.UpdateValueRowFilter(fieldIndex);
                        }
                    }
                    UIHelper.Tooltip("Add this value to the Value Filter in the Quick Edit window.");

                    if (ImGui.Selectable("Data Finder"))
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
                            View.ToolView.DataFinder.UpdateValueRowFilter(fieldIndex);
                        }
                    }
                    UIHelper.Tooltip("Add this value to the Value Filter in the Data Finder window.");

                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }
        }
    }

    private void Shortcuts(GPARAM data, GPARAM.Param group, IField field)
    {
        var values = View.Selection.GetSelectedValues();

        if (FocusManager.IsFocus(EditorFocusContext.GparamEditor_Properties))
        {
            // Duplicate
            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                AddValues(data, group, field, values, true);
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                DeleteValues(data, group, field, values);
            }
        }
    }

    public void AddValues(GPARAM data, Param group, IField field, List<IFieldValue> entries, bool useDuplicateOffset)
    {
        var duplicateID = View.Selection.DuplicateValueID;
        var duplicateOffset = View.Selection.DuplicateValueOffset;

        var action = new AddValueAction(Project, data, group, field, entries, duplicateID, duplicateOffset, useDuplicateOffset);
        View.ActionManager.ExecuteAction(action);
    }

    public void DeleteValues(GPARAM data, Param group, IField field, List<IFieldValue> entries)
    {
        var action = new DeleteValueAction(Project, data, group, field, entries);
        View.ActionManager.ExecuteAction(action);
    }

    public void AddValuesShortcut()
    {
        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();
        var field = View.Selection.GetSelectedField();
        var values = View.Selection.GetSelectedValues();

        AddValues(data, group, field, values, true);
    }

    public void DeleteValuesShortcut()
    {
        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();
        var field = View.Selection.GetSelectedField();
        var values = View.Selection.GetSelectedValues();

        DeleteValues(data, group, field, values);
    }
}


