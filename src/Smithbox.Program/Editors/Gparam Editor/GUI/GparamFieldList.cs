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
public class GparamFieldList
{
    private GparamEditorView View;
    private ProjectEntry Project;

    private string FieldListFilter = "";
    private bool ExactFieldListFilter = false;

    public GparamFieldList(GparamEditorView view, ProjectEntry project)
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

        // Fields
        ImGui.BeginChild("GparamFieldsSection", ImGuiChildFlags.Borders);

        DisplayFieldList();

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        GUI.TitleHeader(
            LOC.Get("GPARAM_FieldList_Header"),
            LOC.Get("GPARAM_FieldList_Header_TT"));

        // Search
        ImGui.BeginChild("GparamFieldSearchSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("gparamEditor_FieldList",
            ref FieldListFilter, ref ExactFieldListFilter);

        ImGui.EndChild();
    }

    private void DisplayFieldList()
    {
        if (!View.Selection.IsGparamGroupSelected())
            return;

        var fileEntry = View.Selection.SelectedFileEntry;
        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();
        var field = View.Selection.GetSelectedField();

        if (data == null)
            return;

        if (group == null)
            return;

        var shownCount = 0;

        for (int i = 0; i < group.Fields.Count; i++)
        {
            var curField = group.Fields[i];

            if (DisplayFieldSelectable(fileEntry, data, group, curField, i))
                shownCount++;
        }

        // With no rows, there would be nowhere to right-click to add fields
        if (shownCount == 0)
        {
            DisplayDummySelectable(fileEntry, data, group);
        }

        DisplayAddPopup(data, group);

        Shortcuts(data, group, field);
    }

    private const string AddPopupId = "##gparamFieldAddPopup";

    /// <summary>
    /// The add menu, opened at the mouse by the Add key
    /// </summary>
    private void DisplayAddPopup(GPARAM data, Param group)
    {
        if (ImGui.BeginPopup(AddPopupId))
        {
            AddFieldMenu(data, group);

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Returns whether the row was shown (it may be hidden by the search)
    /// </summary>
    private bool DisplayFieldSelectable(FileDictionaryEntry fileEntry, GPARAM data, Param group, IField field, int index)
    {
        var selected = index == View.Selection._selectedParamFieldIndex;

        if (field == null)
            return false;

        var fieldName = GetFieldName(field);
        var fieldDesc = GetFieldDescription(field);

        var isMatch = EditorFilters.IsMatch(
            FieldListFilter, field.Name, ExactFieldListFilter, fieldName);

        if (!isMatch)
            return false;

        ImGui.BeginGroup();

        // Fields the viewport's renderer reads are coloured, so the user knows editing them can be previewed
        var isRendered = CFG.Current.GparamEditor_Highlight_Viewport_Fields &&
                         View.ViewportWindow.LightingBank.IsRenderedField(group.Key, field.Key);

        if (isRendered)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_GparamPreviewable_Text);
        }

        // Field row
        if (ImGui.Selectable($@"[{index}] {fieldName}##{field.Key}{index}", selected))
        {
            View.Selection.SetGparamField(index, field);
        }

        // Scroll to a field jumped to from a tool
        if (selected && View.Selection.FocusField)
        {
            View.Selection.FocusField = false;
            ImGui.SetScrollHereY(0.5f);
        }

        // Right-click selects the row too, so its context menu opens on it
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && !selected)
        {
            View.Selection.SetGparamField(index, field);
        }

        if (isRendered)
        {
            ImGui.PopStyleColor();
        }

        var tooltip = CFG.Current.GparamEditor_Field_List_Display_Descriptions ? fieldDesc : "";

        if (isRendered)
        {
            var note = LOC.Get("GPARAM_FieldList_Shown_In_Viewport");
            tooltip = tooltip != "" ? $"{tooltip}\n\n{note}" : note;
        }

        if (tooltip != "")
        {
            GUI.Tooltip(tooltip);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && View.Selection.SelectGparamField)
        {
            View.Selection.SelectGparamField = false;
            View.Selection.SetGparamField(index, field);
        }

        if (ImGui.IsItemFocused())
        {
            if (InputManager.HasArrowSelection())
            {
                View.Selection.SelectGparamField = true;
            }
        }

        ImGui.EndGroup();

        ContextMenu(fileEntry, data, group, field, index);

        return true;
    }

    private void DisplayDummySelectable(FileDictionaryEntry fileEntry, GPARAM data, Param group)
    {
        ImGui.BeginGroup();

        if (ImGui.Selectable($@" {LOC.Get("GPARAM_FieldList_Empty_Selectable")}##addFieldDummy"))
        {

        }
        GUI.Tooltip(LOC.Get("GPARAM_FieldList_Empty_Selectable_TT"));

        ImGui.EndGroup();

        DummyContextMenu(fileEntry, data, group);
    }

    private string OverrideFileName = "";

    public void ContextMenu(FileDictionaryEntry fileEntry, GPARAM data, GPARAM.Param param, GPARAM.IField field, int index)
    {
        var fieldIndex = View.Selection._selectedParamFieldIndex;

        if (index != fieldIndex)
            return;

        if (ImGui.BeginPopupContextItem($"##Gparam_Field_Context"))
        {
            // Add
            if (ImGui.BeginMenu($"{LOC.Get("GPARAM_FieldList_Context_Add_Header")}##addHeader"))
            {
                AddFieldMenu(data, param);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("GPARAM_FieldList_Context_Add_Header_TT"));

            // Delete
            if (ImGui.Selectable($"{LOC.Get("GPARAM_FieldList_Context_Delete_Action")}##deleteAction"))
            {
                DeleteFields(data, param, new List<GPARAM.IField>() { field });
            }
            GUI.Tooltip(LOC.Get("GPARAM_FieldList_Context_Delete_Action_TT"));

            ImGui.Separator();

            // Import
            if (ImGui.Selectable($"{LOC.Get("GPARAM_FieldList_Context_Import_Action")}"))
            {
                View.ToolView.DataTransferTool.ImportField(Project, View, fileEntry, data, param, field);
            }
            GUI.Tooltip(LOC.Get("GPARAM_FieldList_Context_Import_Action_TT"));

            // Export
            if (ImGui.BeginMenu($"{LOC.Get("GPARAM_FieldList_Context_Export_Header")}"))
            {
                ImGui.InputTextWithHint("##overrideFilename", 
                    LOC.Get("GPARAM_FieldList_Context_Export_Filename_Hint"),
                    ref OverrideFileName, 255);
                GUI.Tooltip(LOC.Get("GPARAM_FieldList_Context_Export_Filename_TT"));

                if (ImGui.Selectable($"{LOC.Get("GPARAM_FieldList_Context_Export_File_Action")}##exportFile"))
                {
                    View.ToolView.DataTransferTool.ExportFieldFile(fileEntry, data, param, field, OverrideFileName);
                }
                GUI.Tooltip(LOC.Get("GPARAM_FieldList_Context_Export_File_Action_TT"));

                ImGui.EndMenu();
            }

            ImGui.Separator();

            // Copy Key
            if (ImGui.Selectable($"{LOC.Get("GPARAM_FieldList_Context_Copy_Key_Action")}##copyKey"))
            {
                ImGui.SetClipboardText(field.Key);
            }
            GUI.Tooltip(LOC.Get("GPARAM_FieldList_Context_Copy_Key_Action_TT"));

            // Copy Name
            if (ImGui.Selectable($"{LOC.Get("GPARAM_FieldList_Context_Copy_Name_Action")}##copyName"))
            {
                var fieldName = GetFieldName(field);
                ImGui.SetClipboardText(fieldName);
            }
            GUI.Tooltip(LOC.Get("GPARAM_FieldList_Context_Copy_Name_Action_TT"));

            ImGui.Separator();
            
            // Quick Target
            if (ImGui.BeginMenu($"{LOC.Get("GPARAM_FieldList_Context_Target_Header")}##targetHeader"))
            {
                // Target in Local Search
                if (ImGui.Selectable($"{LOC.Get("GPARAM_FieldList_Context_Target_In_Local_Search")}##targetLocalSearch"))
                {
                    View.ToolView.LocalSearchTool.TargetField(field.Key);
                }
                GUI.Tooltip(LOC.Get("GPARAM_FieldList_Context_Target_In_Local_Search_TT"));

                // Target in Global Search
                if (ImGui.Selectable($"{LOC.Get("GPARAM_FieldList_Context_Target_In_Global_Search")}##targetGlobalSearch"))
                {
                    View.ToolView.GlobalSearchTool.TargetField(field.Key);
                }
                GUI.Tooltip(LOC.Get("GPARAM_FieldList_Context_Target_In_Global_Search_TT"));

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    private void DummyContextMenu(FileDictionaryEntry fileEntry, GPARAM data, Param group)
    {
        if (ImGui.BeginPopupContextItem($"##Gparam_Field_Context"))
        {
            // Add
            AddFieldMenu(data, group);

            ImGui.EndPopup();
        }
    }

    public void Shortcuts(GPARAM data, GPARAM.Param param, GPARAM.IField field)
    {
        if (FocusManager.IsFocus(EditorFocusContext.GparamEditor_FieldList))
        {
            // Add: open the add menu, to pick which fields to add (Edit > Fields adds all of them)
            if (InputManager.IsPressed(KeybindID.Add) && CanAddFields())
            {
                ImGui.OpenPopup(AddPopupId);
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                DeleteFieldsShortcut();
            }
        }
    }

    private string GetFieldName(IField entry)
    {
        var name = entry.Key;
        var groupId = View.Selection.GetSelectedGroup().Key;
        var fieldId = entry.Key;
        var fieldName = GparamMetaUtils.GetFieldName(Project, groupId, fieldId);

        if (fieldName != null)
        {
            name = fieldName;
        }

        return name;
    }

    private string GetFieldDescription(IField entry)
    {
        var desc = entry.Key;
        var groupId = View.Selection.GetSelectedGroup().Key;
        var fieldId = entry.Key;
        var fieldDesc = GparamMetaUtils.GetFieldDescription(Project, groupId, fieldId);

        if (fieldDesc != null)
        {
            desc = fieldDesc;
        }

        return desc;
    }

    private Dictionary<string, List<FieldAddEntry>> AddOptions = new();

    // What the add options were built for: they are rebuilt when the group or annotation language changes
    private string AddOptionsGroupKey = null;
    private string AddOptionsLanguage = null;

    public void AddFieldMenu(GPARAM data, GPARAM.Param param)
    {
        PopulateAddOptions(param);

        var listSize = new Vector2(350, 250) * DPI.UIScale();

        if (!AddOptions.ContainsKey(param.Key))
            return;

        var targetFields = AddOptions[param.Key];

        ImGui.BeginChild("##addFieldList", listSize);

        GUI.SimpleHeader(
            LOC.Get("GPARAM_FieldList_Fields_To_Add_Header"),
            LOC.Get("GPARAM_FieldList_Fields_To_Add_Header_TT"));

        // Toggle All: ticks every missing field, or unticks them all if they all are ticked already
        var missingOptions = targetFields
            .Where(e => !param.Fields.Any(f => f != null && f.Key == e.Annotation.ID))
            .ToList();

        if (ImGui.Selectable($"{LOC.Get("GPARAM_FieldList_Fields_To_Add_Toggle_All")}##toggleAll", false, ImGuiSelectableFlags.NoAutoClosePopups))
        {
            var tickAll = missingOptions.Any(e => !e.ToAdd);

            foreach (var option in missingOptions)
            {
                option.ToAdd = tickAll;
            }
        }
        GUI.Tooltip(LOC.Get("GPARAM_FieldList_Fields_To_Add_Toggle_All_TT"));

        for (int i = 0; i < targetFields.Count; i++)
        {
            var curOption = targetFields[i];

            var curAnnotation = curOption.Annotation;
            var curState = curOption.ToAdd;

            // Ignore existing fields, we only want to allow adding missing fields
            if (param.Fields.Any(e => e != null && e.Key == curAnnotation.ID))
                continue;

            // The ID keeps two annotations with the same display name apart
            ImGui.Checkbox($"{curAnnotation.Name}##{curAnnotation.ID}", ref curState);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                curOption.ToAdd = curState;
            }

            if (CFG.Current.GparamEditor_Field_List_Display_Descriptions)
            {
                var desc = "";

                if (curOption.Annotation.Description != "")
                    desc = curOption.Annotation.Description;

                if (desc != "")
                {
                    GUI.Tooltip(desc);
                }
            }
        }

        ImGui.EndChild();

        if (ImGui.Selectable($"{LOC.Get("GPARAM_FieldList_Field_Submit")}##fieldSubmit"))
        {
            AddNewFields(data, param);
        }
    }

    public void InvalidateAddOptions()
    {
        AddOptionsGroupKey = null;
        AddOptionsLanguage = null;
        AddOptions = new();
    }

    /// <summary>
    /// Build the add options for the passed group from the annotations.
    /// Always leaves a dictionary behind: with no entry for the group if it has no annotated fields.
    /// </summary>
    private void PopulateAddOptions(GPARAM.Param param)
    {
        if (param == null)
            return;

        var language = CFG.Current.GparamEditor_Annotation_Language;

        if (AddOptionsGroupKey == param.Key && AddOptionsLanguage == language)
            return;

        AddOptionsGroupKey = param.Key;
        AddOptionsLanguage = language;
        AddOptions = new();

        var potentialGroups = Project.Handler.GparamData.Annotations.Entries.FirstOrDefault(e => e.Key.Name == language);

        if (potentialGroups.Value == null)
            return;

        var groups = potentialGroups.Value.Params.ToList();
        foreach (var group in groups)
        {
            if(group.ID == param.Key)
            {
                foreach(var field in group.Fields)
                {
                    var addEntry = new FieldAddEntry();
                    addEntry.Annotation = field;
                    addEntry.ToAdd = false;

                    if (!AddOptions.ContainsKey(group.ID))
                    {
                        AddOptions.Add(group.ID, new List<FieldAddEntry>() { addEntry });
                    }
                    else
                    {
                        AddOptions[group.ID].Add(addEntry);
                    }
                }
            }
        }
    }
    public void AddNewFields(GPARAM data, GPARAM.Param param)
    {
        if (data == null || param == null)
            return;

        if (!AddOptions.ContainsKey(param.Key))
            return;

        var fields = AddOptions[param.Key];

        var entries = new List<GparamAnnotationFieldEntry>();

        foreach (var entry in fields)
        {
            if (!entry.ToAdd)
                continue;

            // Never add a field the group already has
            if (param.Fields.Any(e => e != null && e.Key == entry.Annotation.ID))
                continue;

            entries.Add(entry.Annotation);
        }

        AddFields(data, param, entries);

        // Reset the to add state so we don't add the already present entries on secondary usages
        foreach (var entry in AddOptions)
        {
            if (entry.Key == param.Key)
            {
                foreach(var field in entry.Value)
                {
                    field.ToAdd = false;
                }
            }
        }
    }

    public void AddFields(GPARAM data, Param group, List<GparamAnnotationFieldEntry> entries)
    {
        // Nothing to add: don't put an empty action on the undo stack or mark the file as modified
        if (data == null || group == null || entries == null || entries.Count == 0)
            return;

        var action = new AddFieldAction(Project, data, group, entries);
        View.ActionManager.ExecuteAction(action);
    }

    public void DeleteFields(GPARAM data, Param group, List<GPARAM.IField> entries)
    {
        if (data == null || group == null || entries == null)
            return;

        // Only fields actually in the group: a null or stale target would be re-inserted by the undo
        var targets = entries.Where(e => e != null && group.Fields.Contains(e)).ToList();

        if (targets.Count == 0)
            return;

        var action = new DeleteFieldAction(Project, data, group, targets);
        View.ActionManager.ExecuteAction(action);
    }

    /// <summary>
    /// A group is selected, so there is somewhere to add missing fields to
    /// </summary>
    public bool CanAddFields()
    {
        return View.Selection.GetSelectedGparam() != null && View.Selection.GetSelectedGroup() != null;
    }

    /// <summary>
    /// A field is selected and still in its group
    /// </summary>
    public bool CanDeleteFields()
    {
        return CanAddFields() && View.Selection.GetSelectedField() != null;
    }

    public void AddFieldsShortcut()
    {
        if (!CanAddFields())
            return;

        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();

        PopulateAddOptions(group);

        if (AddOptions.ContainsKey(group.Key))
        {
            var targetFields = AddOptions[group.Key];

            for (int i = 0; i < targetFields.Count; i++)
            {
                var curOption = targetFields[i];

                var curAnnotation = curOption.Annotation;
                var curState = curOption.ToAdd;

                // Ignore existing fields, we only want to allow adding missing fields
                if (group.Fields.Any(e => e != null && e.Key == curAnnotation.ID))
                    continue;

                curOption.ToAdd = true;
            }

            AddNewFields(data, group);
        }
    }

    public void DeleteFieldsShortcut()
    {
        if (!CanDeleteFields())
            return;

        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();
        var field = View.Selection.GetSelectedField();

        DeleteFields(data, group, new List<GPARAM.IField>() { field });
    }
}

public class FieldAddEntry
{
    public GparamAnnotationFieldEntry Annotation { get; set; }

    public bool ToAdd { get; set; }
}

