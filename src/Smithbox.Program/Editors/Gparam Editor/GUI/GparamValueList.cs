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
        GUI.TitleHeader(
            LOC.Get("GPARAM_ValueList_Header"),
            LOC.Get("GPARAM_ValueList_Header_TT"));

        // Search
        ImGui.BeginChild("GparamFieldSearchSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("gparamEditor_ValueList",
            ref ValueListFilter, ref ExactValueListFilter);

        // Toggle: Time of Day Column
        GUI.DisplayToggleButton("timeOfDayColumnToggle", Icons.CalendarTimesO,
            ref CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column,
            "GPARAM_GroupList_TimeOfDay_Column_Toggle_Hide",
            "GPARAM_ValueList_TimeOfDay_Column_Toggle_Show",
            "GPARAM_GroupList_TimeOfDay_Column_Toggle_TT");

        // Toggle: Information Column
        GUI.DisplayToggleButton("infoColumnToggle", Icons.Info,
            ref CFG.Current.GparamEditor_Value_List_Display_Information_Column,
            "GPARAM_GroupList_Info_Column_Toggle_Hide",
            "GPARAM_ValueList_Info_Column_Toggle_Show",
            "GPARAM_GroupList_Info_Column_Toggle_TT");

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

        GUI.SimpleHeader(
            LOC.Get("GPARAM_ValueList_Row_Header"),
            LOC.Get("GPARAM_ValueList_Row_Header_TT"));

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
        if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Row_Selectable", index)}##{index}", isSelected))
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

        GUI.SimpleHeader(
            LOC.Get("GPARAM_ValueList_Row_ID_Header"),
            LOC.Get("GPARAM_ValueList_Row_ID_Header_TT"));

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

        GUI.SimpleHeader(
            LOC.Get("GPARAM_ValueList_Row_Time_of_Day_Header"),
            LOC.Get("GPARAM_ValueList_Row_Time_of_Day_Header_TT"));

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

        GUI.SimpleHeader(
            LOC.Get("GPARAM_ValueList_Row_Value_Header"),
            LOC.Get("GPARAM_ValueList_Row_Value_Header_TT"));

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

        GUI.SimpleHeader(
            LOC.Get("GPARAM_ValueList_Row_Information_Header"),
            LOC.Get("GPARAM_ValueList_Row_Information_Header_TT"));

        GparamProperty_Info(field);

        ImGui.EndChild();
    }

    public void GparamProperty_Info(IField field)
    {
        ImGui.AlignTextToFramePadding();

        var groupId = View.Selection.GetSelectedGroup().Key;
        var fieldId = field.Key;
        var fieldDescription = GparamMetaUtils.GetFieldDescription(Project, groupId, fieldId);

        GUI.WrappedText($"{LOC.Get("GPARAM_ValueList_InfoCol_Type", GparamUtils.GetReadableObjectTypeName(field))}");
        GUI.Spacer();

        // Skip if empty
        if (fieldDescription != "")
        {
            GUI.WrappedText($"{fieldDescription}");
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
                        GUI.WrappedText($"{entry.Key} - {name.Text}");
                    }
                    else
                    {
                        GUI.WrappedText($"{entry.Key}");
                    }
                }
            }
        }
    }

    private void DisplayDummySelectable(FileDictionaryEntry fileEntry, GPARAM data, Param group, IField field)
    {
        ImGui.BeginGroup();

        if (ImGui.Selectable($@"{LOC.Get("GPARAM_ValueList_Empty_Selectable")}##addValueDummy"))
        {
            AddNewValue(fileEntry, data, group, field);
        }
        GUI.Tooltip(LOC.Get("GPARAM_ValueList_Empty_Selectable_TT"));

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
            if (ImGui.BeginPopupContextItem($"##Gparam_PropId_Context"))
            {
                // Duplicate
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_ValueList_Context_Duplicate_Header")}##duplicateHeader"))
                {
                    // Input
                    ImGui.InputInt($"{LOC.Get("GPARAM_ValueList_Context_DupeID")}##duplicateInput_ID", ref View.Selection.DuplicateValueID);

                    if (View.Selection.DuplicateValueID < 0)
                    {
                        View.Selection.DuplicateValueID = 0;
                    }

                    ImGui.InputInt($"{LOC.Get("GPARAM_ValueList_Context_DupeOffset")}##duplicateInput_Offset", ref View.Selection.DuplicateValueOffset);

                    // Submit
                    if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Context_Dupe_Submit_Action")}##submitAction"))
                    {
                        AddValues(data, group, field, new List<GPARAM.IFieldValue>() { value }, false);
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Dupe_Submit_Action_TT"));

                    ImGui.EndMenu();
                }

                // Delete
                if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Context_Delete_Action")}##deleteAction"))
                {
                    DeleteValues(data, group, field, new List<GPARAM.IFieldValue>() { value });

                    ImGui.CloseCurrentPopup();
                }
                GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Delete_Action_TT"));

                ImGui.Separator();

                // Import
                if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Context_Import_Action")}"))
                {
                    View.ToolView.DataTransferTool.ImportValue(Project, View, fileEntry, data, group, field, value, overwrite);
                }
                GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Import_Action_TT"));

                // Export
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_ValueList_Context_Export_Header")}##exportHeader"))
                {
                    ImGui.InputTextWithHint("##overrideFilename", 
                        LOC.Get("GPARAM_ValueList_Context_Export_Filename_Hint"), 
                        ref OverrideFileName, 255);
                    GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Export_Filename_TT"));

                    if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Context_Export_File_Action")}##exportFileAction"))
                    {
                        View.ToolView.DataTransferTool.ExportValueFile(fileEntry, data, group, field, value, OverrideFileName);
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Export_File_Action_TT"));

                    ImGui.EndMenu();
                }

                ImGui.Separator();

                // Copy ID
                if (ImGui.MenuItem($"{LOC.Get("GPARAM_ValueList_Context_Copy_ID_Action")}##copyIdAction"))
                {
                    ImGui.SetClipboardText($"{value.ID}");
                }
                GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Copy_ID_Action_TT"));

                // Copy Value
                if (ImGui.MenuItem($"{LOC.Get("GPARAM_ValueList_Context_Copy_Value_Action")}##copyValueAction"))
                {
                    ImGui.SetClipboardText($"{value.Value.ToString()}");
                }
                GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Copy_Value_Action_TT"));

                ImGui.Separator();

                // Quick Edit
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_ValueList_Context_QuickEdit_Header")}##quickEditHeader"))
                {
                    // Target in Quick Edit
                    if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Context_Target_In_Quick_Edit")}##targetQuickEdit"))
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
                    GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Target_In_Quick_Edit_TT"));

                    // Target in Data Finder
                    if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Context_Target_In_Data_Finder")}##targetDataFinder"))
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
                    GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Target_In_Data_Finder_TT"));

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


