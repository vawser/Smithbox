using Hexa.NET.ImGui;
using HKLib.hk2018.hkReflect;
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

public class GparamGroupList
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    public GparamGroupList(GparamEditorView view, ProjectEntry project)
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

        // Groups
        ImGui.BeginChild("GparamGroupsSection", ImGuiChildFlags.Borders);

        DisplayGroupList();

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        UIHelper.SimpleHeader("Groups", "");

        // Search
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("GparamGroupSearchSection", searchHeight, ImGuiChildFlags.Borders);

        Parent.Filters.DisplayGroupFilterSearch();

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##emptyGroupToggle"))
        {
            CFG.Current.GparamEditor_Group_List_Display_Empty_Group = !CFG.Current.GparamEditor_Group_List_Display_Empty_Group;
        }

        var emptyGroupMode = "Displaying empty groups.";
        if (!CFG.Current.GparamEditor_Group_List_Display_Empty_Group)
        {
            emptyGroupMode = "Hiding empty groups.";
        }
        UIHelper.Tooltip($"Toggle the display of empty groups.\nCurrent Mode: {emptyGroupMode}");

        ImGui.EndChild();
    }

    private void DisplayGroupList()
    {
        if (!Parent.Selection.IsFileSelected())
            return;

        var data = Parent.Selection.GetSelectedGparam();
        var group = Parent.Selection.GetSelectedGroup();

        if (data == null)
            return;

        // Available groups
        for (int i = 0; i < data.Params.Count; i++)
        {
            var curGroup = data.Params[i];

            DisplayGroupSelectable(data, curGroup, i);
        }

        Shortcuts(data, group);
    }

    private void DisplayGroupSelectable(GPARAM data, Param group, int index)
    {
        var selected = index == Parent.Selection._selectedParamGroupIndex;
        var groupName = GetGroupName(group);
        var groupDesc = GetGroupDescription(group);

        if (!DisplayEmptyGroup(group))
            return;

        if (!Parent.Filters.IsGroupFilterMatch(group.Name, ""))
            return;

        ImGui.BeginGroup();

        // Group row
        if (ImGui.Selectable($@" {groupName}##{group.Key}", selected))
        {
            Parent.Selection.SetGparamGroup(index, group);
        }

        if (groupDesc != "")
        {
            UIHelper.Tooltip(groupDesc);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && Parent.Selection.SelectGparamGroup)
        {
            Parent.Selection.SelectGparamGroup = false;
            Parent.Selection.SetGparamGroup(index, group);
        }

        if (ImGui.IsItemFocused())
        {
            if (InputManager.HasArrowSelection())
            {
                Parent.Selection.SelectGparamGroup = true;
            }
        }

        ImGui.EndGroup();

        ContextMenu(data, group, index);
    }

    private void ContextMenu(GPARAM data, GPARAM.Param group, int index)
    {
        var groupIndex = Parent.Selection._selectedParamGroupIndex;

        if (index != groupIndex)
            return;

        if (ImGui.BeginPopupContextItem($"Options##Gparam_Group_Context"))
        {
            // Add
            if (ImGui.BeginMenu("Add"))
            {
                AddGroupMenu(data);

                ImGui.EndMenu();
            }

            // Delete
            if (ImGui.Selectable("Delete"))
            {
                DeleteGroups(data, new List<GPARAM.Param>() { group });
            }
            UIHelper.Tooltip("Delete the selected group.");

            ImGui.Separator();

            // Quick Edit
            if (ImGui.BeginMenu("Quick Edit"))
            {
                // Target
                if (ImGui.Selectable("Target"))
                {
                    Parent.QuickEditHandler.UpdateGroupFilter(Parent.Selection._selectedParamGroupKey);
                }
                UIHelper.Tooltip("Add this file to the Group Filter in the Quick Edit window.");

                ImGui.EndMenu();
            }

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

        UIHelper.SimpleHeader("Groups to Add", "");

        if (ImGui.Selectable("Toggle All"))
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
        }

        ImGui.EndChild();

        if(ImGui.Selectable("Submit"))
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
        Parent.ActionManager.ExecuteAction(action);
    }

    public void DeleteGroups(GPARAM data, List<GPARAM.Param> entries)
    {
        var action = new DeleteGroupAction(Project, data, entries);
        Parent.ActionManager.ExecuteAction(action);
    }

    public void AddGroupsShortcut()
    {
        var data = Parent.Selection.GetSelectedGparam();

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
        var data = Parent.Selection.GetSelectedGparam();
        var group = Parent.Selection.GetSelectedGroup();

        DeleteGroups(data, new List<GPARAM.Param>() { group });
    }
}

public class GroupAddEntry
{
    public GparamAnnotationEntry Annotation { get; set; }

    public bool ToAdd { get; set; }
}