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
            "GPARAM_ValueList_TimeOfDay_Column_Toggle_Hide",
            "GPARAM_ValueList_TimeOfDay_Column_Toggle_Show",
            "GPARAM_ValueList_TimeOfDay_Column_Toggle_TT");

        // Toggle: Information Column
        GUI.DisplayToggleButton("infoColumnToggle", Icons.Info,
            ref CFG.Current.GparamEditor_Value_List_Display_Information_Column,
            "GPARAM_ValueList_Info_Column_Toggle_Hide",
            "GPARAM_ValueList_Info_Column_Toggle_Show",
            "GPARAM_ValueList_Info_Column_Toggle_TT");

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

        // The field's information is one block for the whole field, which a table row can't span, so it sits in a
        // column of its own beside the values
        var showInfo = CFG.Current.GparamEditor_Value_List_Display_Information_Column;

        if (showInfo)
        {
            var layoutFlags = ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp;

            if (ImGui.BeginTable("gparamValueListLayout", 2, layoutFlags))
            {
                ImGui.TableSetupColumn("##gparamValueListValues", ImGuiTableColumnFlags.WidthStretch, 3.0f);
                ImGui.TableSetupColumn("##gparamValueListInfo", ImGuiTableColumnFlags.WidthStretch, 1.0f);

                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                DisplayValueRows(fileEntry, data, group, field);

                ImGui.TableNextColumn();
                DisplayInfo(field);

                ImGui.EndTable();
            }
        }
        else
        {
            DisplayValueRows(fileEntry, data, group, field);
        }

        Shortcuts(data, group, field);
    }

    /// <summary>
    /// The field's values, one row each: the row's selectable, its ID, its time of day and its value
    /// </summary>
    private void DisplayValueRows(FileDictionaryEntry fileEntry, GPARAM data, GPARAM.Param group, IField field)
    {
        var showTimeOfDay = CFG.Current.GparamEditor_Value_List_Display_Time_Of_Day_Column;
        var columnCount = showTimeOfDay ? 4 : 3;

        var tableFlags = ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV |
                         ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingStretchProp;

        if (!ImGui.BeginTable("gparamValueListTable", columnCount, tableFlags))
            return;

        var scale = DPI.UIScale();

        ImGui.TableSetupScrollFreeze(0, 1);

        ImGui.TableSetupColumn(LOC.Get("GPARAM_ValueList_Row_Header"), ImGuiTableColumnFlags.WidthFixed, 80.0f * scale);
        ImGui.TableSetupColumn(LOC.Get("GPARAM_ValueList_Row_ID_Header"), ImGuiTableColumnFlags.WidthFixed, 100.0f * scale);

        if (showTimeOfDay)
        {
            ImGui.TableSetupColumn(LOC.Get("GPARAM_ValueList_Row_Time_of_Day_Header"), ImGuiTableColumnFlags.WidthFixed, 100.0f * scale);
        }

        ImGui.TableSetupColumn(LOC.Get("GPARAM_ValueList_Row_Value_Header"), ImGuiTableColumnFlags.WidthStretch);

        // The headers, each with its tooltip
        ImGui.TableNextRow(ImGuiTableRowFlags.Headers);

        HeaderCell("GPARAM_ValueList_Row_Header");
        HeaderCell("GPARAM_ValueList_Row_ID_Header");

        if (showTimeOfDay)
        {
            HeaderCell("GPARAM_ValueList_Row_Time_of_Day_Header");
        }

        HeaderCell("GPARAM_ValueList_Row_Value_Header");

        for (int i = 0; i < field.Values.Count; i++)
        {
            var value = field.Values[i];

            if (value == null)
                continue;

            if (!EditorFilters.IsMatch(ValueListFilter, value.ID.ToString(), ExactValueListFilter))
                continue;

            ImGui.TableNextRow();

            // The values' widgets share their IDs between columns, so each row gets its own ID scope
            ImGui.PushID(i);

            // Row
            ImGui.TableNextColumn();
            GparamProperty_Row(fileEntry, data, group, field, value, i);

            // ID
            ImGui.TableNextColumn();
            GparamProperty_ID(data, group, field, value, i);

            // Time of Day
            if (showTimeOfDay)
            {
                ImGui.TableNextColumn();
                GparamProperty_TimeOfDay(data, group, field, value, i);
            }

            // Value
            ImGui.TableNextColumn();
            GparamProperty_Value(data, group, field, value, i);

            ImGui.PopID();
        }

        if (field.Values.Count == 0)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();

            DisplayDummySelectable(fileEntry, data, group, field);
        }

        ImGui.EndTable();
    }

    private static void HeaderCell(string key)
    {
        ImGui.TableNextColumn();
        ImGui.TableHeader(LOC.Get(key));
        GUI.Tooltip(LOC.Get($"{key}_TT"));
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

        // Scroll to a value jumped to from a tool
        if (View.Selection.FocusValue && View.Selection._selectedFieldValueIndex == index)
        {
            View.Selection.FocusValue = false;
            ImGui.SetScrollHereY(0.5f);
        }

        // Right-click selects the row too, so its context menu opens on it.
        // A row already in the multi-selection keeps it, so the menu can act on all of them.
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && !isSelected)
        {
            View.Selection.SelectSingleFieldValue(index, value);
        }

        ContextMenu(fileEntry, data, group, field, value, index);
    }

    public void GparamProperty_ID(GPARAM data, GPARAM.Param group,
        IField field, IFieldValue value, int index)
    {
        ImGui.AlignTextToFramePadding();
        View.PropertyEditor.IdField(data, group, field, value, index);
    }

    public void GparamProperty_TimeOfDay(GPARAM data, Param group, IField field, IFieldValue value, int index)
    {
        ImGui.AlignTextToFramePadding();
        View.PropertyEditor.TimeOfDayField(data, group, field, value, index);
    }

    public void GparamProperty_Value(GPARAM data, Param group, IField field, IFieldValue value, int index)
    {
        ImGui.AlignTextToFramePadding();
        View.PropertyEditor.ValueField(data, group, field, value, index);
    }

    /// <summary>
    /// The field's type, description and enum options, beside the values
    /// </summary>
    private void DisplayInfo(IField field)
    {
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

        // Paste, Paste Special: copied values go into the field even when it has none yet
        if (ImGui.BeginPopupContextItem($"##Gparam_PropId_Empty_Context"))
        {
            View.ToolView.DataTransferTool.Commands.PasteMenuItems();

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Give an empty field a default value. It is added to the existing field, rather than the field being rebuilt,
    /// so the field object stays the one older undo steps refer to and the add itself can be undone.
    /// </summary>
    private void AddNewValue(FileDictionaryEntry fileEntry, GPARAM data, Param group, IField field)
    {
        if (data == null || group == null || field == null)
            return;

        // The lowest ID the group's other fields use, so the value lines up with the group's existing sets; else 0
        var otherIds = group.Fields
            .Where(e => e != null && e != field)
            .SelectMany(e => e.Values)
            .Select(e => e.ID)
            .ToList();

        var id = otherIds.Count > 0 ? otherIds.Min() : 0;

        var newValue = field.CreateDefaultValue(id);

        var action = new AddValueAction(Project, data, group, field, new List<IFieldValue>() { newValue });
        View.ActionManager.ExecuteAction(action);
    }

    public void ContextMenu(FileDictionaryEntry fileEntry, GPARAM data, GPARAM.Param group, IField field, IFieldValue value, int index)
    {
        if (View.Selection.IsValueSelected(index))
        {
            if (ImGui.BeginPopupContextItem($"##Gparam_PropId_Context"))
            {
                // Duplicate and Delete act on the whole multi-selection when the clicked row is part of it,
                // like their keybinds do
                var targets = ContextTargets(field, value);

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
                        AddValues(data, group, field, targets, false);
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Dupe_Submit_Action_TT"));

                    ImGui.EndMenu();
                }

                // Delete
                if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Context_Delete_Action")}##deleteAction"))
                {
                    DeleteValues(data, group, field, targets);

                    ImGui.CloseCurrentPopup();
                }
                GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Delete_Action_TT"));

                ImGui.Separator();

                // Copy, Paste, Paste Special, Export to File, Import from File. Copy and Export take the same
                // values as Duplicate and Delete.
                View.ToolView.DataTransferTool.Commands.ContextMenuItems(GparamTransferKind.Values,
                    GparamTransferTarget.FromSelection(View.Selection).WithValues(targets));

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
                    // Invariant, so a copied 0.5 reads the same everywhere (not 0,5 in some locales)
                    ImGui.SetClipboardText(System.Convert.ToString(value.Value, System.Globalization.CultureInfo.InvariantCulture) ?? "");
                }
                GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Copy_Value_Action_TT"));

                ImGui.Separator();

                // Quick Target
                if (ImGui.BeginMenu($"{LOC.Get("GPARAM_ValueList_Context_Target_Header")}##targetHeader"))
                {
                    // Target in Local Search
                    if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Context_Target_In_Local_Search")}##targetLocalSearch"))
                    {
                        View.ToolView.LocalSearchTool.TargetValue(group.Key, field.Key, value.ID);
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Target_In_Local_Search_TT"));

                    // Target in Global Search
                    if (ImGui.Selectable($"{LOC.Get("GPARAM_ValueList_Context_Target_In_Global_Search")}##targetGlobalSearch"))
                    {
                        View.ToolView.GlobalSearchTool.TargetValue(group.Key, field.Key, value.ID);
                    }
                    GUI.Tooltip(LOC.Get("GPARAM_ValueList_Context_Target_In_Global_Search_TT"));

                    ImGui.EndMenu();
                }

                ImGui.EndPopup();
            }
        }
    }

    private void Shortcuts(GPARAM data, GPARAM.Param group, IField field)
    {
        if (FocusManager.IsFocus(EditorFocusContext.GparamEditor_Properties))
        {
            // Duplicate
            if (InputManager.IsPressed(KeybindID.Duplicate))
            {
                AddValuesShortcut();
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                DeleteValuesShortcut();
            }

            // Copy, Paste
            View.ToolView.DataTransferTool.Commands.Shortcuts(GparamTransferKind.Values);
        }
    }

    /// <summary>
    /// What a context-menu action applies to: the multi-selection if the clicked value is in it, else that value
    /// </summary>
    private List<IFieldValue> ContextTargets(IField field, IFieldValue clicked)
    {
        var selected = ValuesInField(field, View.Selection.GetSelectedValues());

        if (selected.Count > 1 && selected.Any(e => ReferenceEquals(e, clicked)))
            return selected;

        return new List<IFieldValue>() { clicked };
    }

    /// <summary>
    /// The entries that are really values of the field: drops nulls and values a stale selection still holds
    /// </summary>
    private static List<IFieldValue> ValuesInField(IField field, List<IFieldValue> entries)
    {
        if (field == null || entries == null)
            return new List<IFieldValue>();

        return entries.Where(e => e != null && field.IndexOfValue(e) >= 0).ToList();
    }

    public void AddValues(GPARAM data, Param group, IField field, List<IFieldValue> entries, bool useDuplicateOffset)
    {
        if (data == null || group == null || field == null)
            return;

        var targets = ValuesInField(field, entries);

        // Nothing to duplicate: don't put an empty action on the undo stack or mark the file as modified
        if (targets.Count == 0)
            return;

        var duplicateID = View.Selection.DuplicateValueID;
        var duplicateOffset = View.Selection.DuplicateValueOffset;

        var action = new AddValueAction(Project, data, group, field, targets, duplicateID, duplicateOffset, useDuplicateOffset);
        View.ActionManager.ExecuteAction(action);
    }

    public void DeleteValues(GPARAM data, Param group, IField field, List<IFieldValue> entries)
    {
        if (data == null || group == null || field == null)
            return;

        // A target no longer in the field would be inserted again by the undo
        var targets = ValuesInField(field, entries);

        if (targets.Count == 0)
            return;

        var action = new DeleteValueAction(Project, data, group, field, targets);
        View.ActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// A field is selected and at least one of its values is selected
    /// </summary>
    public bool CanAffectValues()
    {
        var field = View.Selection.GetSelectedField();

        if (View.Selection.GetSelectedGparam() == null || View.Selection.GetSelectedGroup() == null || field == null)
            return false;

        return ValuesInField(field, View.Selection.GetSelectedValues()).Count > 0;
    }

    public void AddValuesShortcut()
    {
        if (!CanAffectValues())
            return;

        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();
        var field = View.Selection.GetSelectedField();
        var values = View.Selection.GetSelectedValues();

        AddValues(data, group, field, values, true);
    }

    public void DeleteValuesShortcut()
    {
        if (!CanAffectValues())
            return;

        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();
        var field = View.Selection.GetSelectedField();
        var values = View.Selection.GetSelectedValues();

        DeleteValues(data, group, field, values);
    }
}


