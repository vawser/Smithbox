using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;

public class GparamGroupList
{
    private GparamEditorView View;
    private ProjectEntry Project;

    private string GroupListFilter = "";
    private bool ExactGroupListFilter = false;

    public GparamGroupList(GparamEditorView view, ProjectEntry project)
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

        // Groups
        ImGui.BeginChild("GparamGroupsSection", ImGuiChildFlags.Borders);

        DisplayGroupList();

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        GUI.TitleHeader(
            LOC.Get("GPARAM_GroupList_Header"),
            LOC.Get("GPARAM_GroupList_Header_TT"));

        // Search
        ImGui.BeginChild("GparamGroupSearchSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("gparamEditor_GroupList",
            ref GroupListFilter, ref ExactGroupListFilter);

        // Toggle: Display Empty Groups
        GUI.DisplayToggleButton("emptyGroupToggle", Icons.Book,
            ref CFG.Current.GparamEditor_Group_List_Display_Empty_Group,
            "GPARAM_GroupList_Empty_Groups_Toggle_Hide",
            "GPARAM_GroupList_Empty_Groups_Toggle_Show",
            "GPARAM_GroupList_Empty_Groups_Toggle_TT");

        ImGui.EndChild();
    }

    private void DisplayGroupList()
    {
        if (!View.Selection.IsFileSelected())
            return;

        var fileEntry = View.Selection.SelectedFileEntry;
        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();

        if (data == null)
            return;

        // Available groups
        for (int i = 0; i < data.Params.Count; i++)
        {
            var curGroup = data.Params[i];

            DisplayGroupSelectable(fileEntry, data, curGroup, i);
        }

        if(data.Params.Count == 0)
        {
            DisplayDummySelectable(fileEntry, data);
        }

        Shortcuts(data, group);
    }

    private void DisplayGroupSelectable(FileDictionaryEntry fileEntry, GPARAM data, Param group, int index)
    {
        var selected = index == View.Selection._selectedParamGroupIndex;
        var groupName = GetGroupName(group);
        var groupDesc = GetGroupDescription(group);

        if (!DisplayEmptyGroup(group))
            return;

        var isMatch = EditorFilters.IsMatch(GroupListFilter, group.Name, ExactGroupListFilter, groupName);

        if (!isMatch)
            return;

        ImGui.BeginGroup();

        // Group row
        if (ImGui.Selectable($@" {groupName}##{group.Key}", selected))
        {
            View.Selection.SetGparamGroup(index, group);
            View.FieldListView.InvalidateAddOptions();
        }

        if (CFG.Current.GparamEditor_Group_List_Display_Descriptions)
        {
            if (groupDesc != "")
            {
                GUI.Tooltip(groupDesc);
            }
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && View.Selection.SelectGparamGroup)
        {
            View.Selection.SelectGparamGroup = false;
            View.Selection.SetGparamGroup(index, group);
            View.FieldListView.InvalidateAddOptions();
        }

        if (ImGui.IsItemFocused())
        {
            if (InputManager.HasArrowSelection())
            {
                View.Selection.SelectGparamGroup = true;
            }
        }

        ImGui.EndGroup();

        ContextMenu(fileEntry, data, group, index);
    }

    private void DisplayDummySelectable(FileDictionaryEntry fileEntry, GPARAM data)
    {
        ImGui.BeginGroup();

        if (ImGui.Selectable($@"{LOC.Get("GPARAM_GroupList_Empty_Selectable")}##addGroupDummy"))
        {

        }
        GUI.Tooltip(LOC.Get("GPARAM_GroupList_Empty_Selectable_TT"));

        ImGui.EndGroup();

        DummyContextMenu(fileEntry, data);
    }

    private string OverrideFileName = "";

    private void ContextMenu(FileDictionaryEntry fileEntry, GPARAM data, GPARAM.Param group, int index)
    {
        var groupIndex = View.Selection._selectedParamGroupIndex;
        bool overwrite = CFG.Current.GparamEditor_Data_Import_Overwrite;

        if (index != groupIndex)
            return;

        if (ImGui.BeginPopupContextItem($"##Gparam_Group_Context"))
        {
            // Add
            if (ImGui.BeginMenu($"{LOC.Get("GPARAM_GroupList_Context_Add_Header")}##addHeader"))
            {
                AddGroupMenu(data);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("GPARAM_GroupList_Context_Add_Header_TT"));

            // Delete
            if (ImGui.Selectable($"{LOC.Get("GPARAM_GroupList_Context_Delete_Action")}##deleteAction"))
            {
                DeleteGroups(data, new List<GPARAM.Param>() { group });
            }
            GUI.Tooltip(LOC.Get("GPARAM_GroupList_Context_Delete_Action_TT"));

            ImGui.Separator();

            // Import
            if (ImGui.Selectable($"{LOC.Get("GPARAM_GroupList_Context_Import_Action")}##importAction"))
            {
                View.ToolView.DataTransferTool.ImportGroup(Project, View, fileEntry, data, group);
            }
            GUI.Tooltip(LOC.Get("GPARAM_GroupList_Context_Import_Action_TT"));

            // Export
            if (ImGui.BeginMenu($"{LOC.Get("GPARAM_GroupList_Context_Export_Header")}##exportHeader"))
            {
                ImGui.InputTextWithHint("##overrideFilename", 
                    LOC.Get("GPARAM_GroupList_Context_Export_Filename_Hint"),
                    ref OverrideFileName, 255);
                GUI.Tooltip(LOC.Get("GPARAM_GroupList_Context_Export_Filename_TT"));

                // Export File
                if (ImGui.Selectable($"{LOC.Get("GPARAM_GroupList_Context_Export_File_Action")}##exportFile"))
                {
                    View.ToolView.DataTransferTool.ExportGroupFile(fileEntry, data, group, OverrideFileName);
                }
                GUI.Tooltip(LOC.Get("GPARAM_GroupList_Context_Export_File_Action_TT"));

                ImGui.EndMenu();
            }

            ImGui.Separator();

            // Copy Key
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_GroupList_Context_Copy_Key_Action")}##copyKey"))
            {
                ImGui.SetClipboardText(group.Key);
            }
            GUI.Tooltip(LOC.Get("GPARAM_GroupList_Context_Copy_Key_Action_TT"));

            // Copy Name
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_GroupList_Context_Copy_Name_Action")}##copyName"))
            {
                var groupName = GetGroupName(group);
                ImGui.SetClipboardText(groupName);
            }
            GUI.Tooltip(LOC.Get("GPARAM_GroupList_Context_Copy_Name_Action_TT"));

            ImGui.Separator();

            // Quick Target
            if (ImGui.BeginMenu($"{LOC.Get("GPARAM_GroupList_Context_QuickEdit_Header")}##quickEditHeader"))
            {
                // Target in Quick Edit
                if (ImGui.Selectable($"{LOC.Get("GPARAM_GroupList_Context_Target_In_Quick_Edit")}##quickEditTarget"))
                {
                    View.QuickEditHandler.UpdateGroupFilter(group.Key);
                }
                GUI.Tooltip(LOC.Get("GPARAM_GroupList_Context_Target_In_Quick_Edit_TT"));

                // Target in Data Finder
                if (ImGui.Selectable($"{LOC.Get("GPARAM_GroupList_Context_Target_In_Data_Finder")}##dataFinderTarget"))
                {
                    View.ToolView.DataFinder.UpdateGroupFilter(group.Key);
                }
                GUI.Tooltip(LOC.Get("GPARAM_GroupList_Context_Target_In_Data_Finder_TT"));

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    private void DummyContextMenu(FileDictionaryEntry fileEntry, GPARAM data)
    {
        if (ImGui.BeginPopupContextItem($"##Gparam_Group_Context"))
        {
            // Add
            AddGroupMenu(data);

            ImGui.EndPopup();
        }
    }

    public void Shortcuts(GPARAM data, GPARAM.Param entry)
    {
        if (FocusManager.IsFocus(EditorFocusContext.GparamEditor_GroupList))
        {
            // Add
            if (InputManager.IsPressed(KeybindID.Add))
            {
                PopulateAddOptions();

                for (int i = 0; i < AddOptions.Count; i++)
                {
                    var curOption = AddOptions[i];
                    var curAnnotation = curOption.Annotation;

                    // Ignore existing groups, we only want to allow adding missing groups
                    if (data.Params.Any(e => e.Key == curAnnotation.ID))
                        continue;

                    curOption.ToAdd = true;
                }

                AddNewGroups(data, AddOptions);
            }

            // Delete
            if (InputManager.IsPressed(KeybindID.Delete))
            {
                DeleteGroups(data, new List<GPARAM.Param>() { entry });
            }
        }
    }

    private string GetGroupName(GPARAM.Param entry)
    {
        var name = entry.Key;
        var groupId = entry.Key;
        var groupName = GparamMetaUtils.GetGroupName(Project, groupId);

        if (groupName != null)
        {
            name = groupName;
        }

        return name;
    }

    private string GetGroupDescription(GPARAM.Param entry)
    {
        var name = entry.Key;
        var groupId = entry.Key;
        var groupName = GparamMetaUtils.GetGroupDescription(Project, groupId);

        if (groupName != null)
        {
            name = groupName;
        }

        return name;
    }

    private bool DisplayEmptyGroup(GPARAM.Param entry)
    {
        if (!CFG.Current.GparamEditor_Group_List_Display_Empty_Group)
        {
            foreach (var fieldEntry in entry.Fields)
            {
                if (fieldEntry.Values.Count > 0)
                {
                    return true;
                }
            }
        }
        else
        {
            return true;
        }

        return false;
    }

    private List<GroupAddEntry> AddOptions = null;
    public void AddGroupMenu(GPARAM data)
    {
        PopulateAddOptions();

        var listSize = new Vector2(350, 250) * DPI.UIScale();

        ImGui.BeginChild("##addGroupList", listSize);

        GUI.SimpleHeader(
            LOC.Get("GPARAM_GroupList_Groups_To_Add_Header"),
            LOC.Get("GPARAM_GroupList_Groups_To_Add_Header_TT"));

        // Toggle All
        if (ImGui.Selectable($"{LOC.Get("GPARAM_GroupList_Groups_To_Add_Toggle_All")}##toggleAll"))
        {
            for (int i = 0; i < AddOptions.Count; i++)
            {
                var curOption = AddOptions[i];

                var curAnnotation = curOption.Annotation;

                // Ignore existing groups, we only want to allow adding missing groups
                if (data.Params.Any(e => e.Key == curAnnotation.ID))
                    continue;

                curOption.ToAdd = !curOption.ToAdd;
            }
        }

        for (int i = 0; i < AddOptions.Count; i++)
        {
            var curOption = AddOptions[i];
            var curState = curOption.ToAdd;

            var curAnnotation = curOption.Annotation;

            // Ignore existing groups, we only want to allow adding missing groups
            if (data.Params.Any(e => e.Key == curAnnotation.ID))
                continue;

            ImGui.Checkbox($"{curAnnotation.Name}", ref curState);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                curOption.ToAdd = curState;
            }

            if (CFG.Current.GparamEditor_Group_List_Display_Descriptions)
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

        // Submit
        if(ImGui.Selectable($"{LOC.Get("GPARAM_GroupList_Field_Submit")}##submitAction"))
        {
            AddNewGroups(data, AddOptions);
        }
    }

    private void PopulateAddOptions()
    {
        // We only ever build the add options once.
        if (AddOptions == null)
        {
            var potentialGroups = Project.Handler.GparamData.Annotations.Entries.FirstOrDefault(e => e.Key.Name == CFG.Current.GparamEditor_Annotation_Language);

            if (potentialGroups.Value == null)
                return;

            AddOptions = new();

            var options = potentialGroups.Value.Params.Where(e => e.IsObsolete == false).ToList();
            foreach (var optEntry in options)
            {
                var addEntry = new GroupAddEntry();
                addEntry.Annotation = optEntry;
                addEntry.ToAdd = false;

                AddOptions.Add(addEntry);
            }
        }

    }

    public void AddNewGroups(GPARAM data, List<GroupAddEntry> groups)
    {
        var newGroups = groups.Where(e => e.ToAdd == true).ToList();

        var entries = new List<GparamAnnotationEntry>();

        foreach(var entry in newGroups)
        {
            if(entry.ToAdd)
            {
                var annotationEntry = entry.Annotation;
                entries.Add(annotationEntry);
            }
        }

        AddGroups(data, entries);

        // Reset the to add state so we don't add the already present entries on secondary usages
        foreach(var entry in AddOptions)
        {
            entry.ToAdd = false;
        }
    }

    public void AddGroups(GPARAM data, List<GparamAnnotationEntry> entries)
    {
        var action = new AddGroupAction(Project, data, entries);
        View.ActionManager.ExecuteAction(action);
    }

    public void DeleteGroups(GPARAM data, List<GPARAM.Param> entries)
    {
        var action = new DeleteGroupAction(Project, data, entries);
        View.ActionManager.ExecuteAction(action);
    }

    public void AddGroupsShortcut()
    {
        var data = View.Selection.GetSelectedGparam();

        PopulateAddOptions();

        for (int i = 0; i < AddOptions.Count; i++)
        {
            var curOption = AddOptions[i];
            var curAnnotation = curOption.Annotation;

            // Ignore existing groups, we only want to allow adding missing groups
            if (data.Params.Any(e => e.Key == curAnnotation.ID))
                continue;

            curOption.ToAdd = true;
        }

        AddNewGroups(data, AddOptions);
    }

    public void DeleteGroupsShortcut()
    {
        var data = View.Selection.GetSelectedGparam();
        var group = View.Selection.GetSelectedGroup();

        DeleteGroups(data, new List<GPARAM.Param>() { group });
    }
}

public class GroupAddEntry
{
    public GparamAnnotationEntry Annotation { get; set; }

    public bool ToAdd { get; set; }
}