using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;

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
        UIHelper.SimpleHeader("Groups", "");

        Parent.Filters.DisplayGroupFilterSearch();

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.CircleO}##emptyGroupToggle"))
        {
            CFG.Current.GparamEditor_Group_List_Display_Empty_Group = !CFG.Current.GparamEditor_Group_List_Display_Empty_Group;
        }
        UIHelper.Tooltip("Toggle the display of empty groups.");

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##addGroupToggle"))
        {
            CFG.Current.GparamEditor_Group_List_Display_Group_Add = !CFG.Current.GparamEditor_Group_List_Display_Group_Add;
        }
        UIHelper.Tooltip("Toggle the display of the add group buttons.");

        ImGui.BeginChild("GparamGroupsSection");

        if (Parent.Selection.IsFileSelected())
        {
            GPARAM data = Parent.Selection.GetSelectedGparam();

            // Available groups
            for (int i = 0; i < data.Params.Count; i++)
            {
                GPARAM.Param entry = data.Params[i];

                var name = entry.Key;
                if (CFG.Current.GparamEditor_Group_List_Display_Aliases)
                    name = FormatInformationUtils.GetReferenceName(Project.Handler.GparamData.GparamInformation, entry.Key, entry.Name);

                var display = false;

                if (!CFG.Current.GparamEditor_Group_List_Display_Empty_Group)
                {
                    foreach (var fieldEntry in entry.Fields)
                    {
                        if (fieldEntry.Values.Count > 0)
                        {
                            display = true;
                        }
                    }
                }
                else
                {
                    display = true;
                }

                if (Parent.Filters.IsGroupFilterMatch(entry.Name, ""))
                {
                    if (display)
                    {
                        // Group row
                        if (ImGui.Selectable($@" {name}##{entry.Key}", i == Parent.Selection._selectedParamGroupKey))
                        {
                            Parent.Selection.SetGparamGroup(i, entry);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Parent.Selection.SelectGparamGroup)
                        {
                            Parent.Selection.SelectGparamGroup = false;
                            Parent.Selection.SetGparamGroup(i, entry);
                        }

                        if (ImGui.IsItemFocused())
                        {
                            if (InputManager.HasArrowSelection())
                            {
                                Parent.Selection.SelectGparamGroup = true;
                            }
                        }
                    }
                }

                Parent.ContextMenu.GroupContextMenu(i);
            }

            if (CFG.Current.GparamEditor_Group_List_Display_Group_Add)
            {
                ImGui.Separator();

                AddMissingGroupSection();
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Groups List: add buttons
    /// </summary>
    public void AddMissingGroupSection()
    {
        GPARAM data = Parent.Selection.GetSelectedGparam();

        List<FormatReference> missingGroups = new List<FormatReference>();

        if (Project.Handler.GparamData.GparamInformation.list == null)
            return;

        // Get source Format Reference
        foreach (var entry in Project.Handler.GparamData.GparamInformation.list)
        {
            bool isPresent = false;

            foreach (var param in data.Params)
            {
                if (entry.id == param.Key)
                {
                    isPresent = true;
                }
            }

            if (!isPresent)
            {
                missingGroups.Add(entry);
            }
        }

        foreach (var missing in missingGroups)
        {
            if (ImGui.Button($"Add##{missing.id}", DPI.StandardButtonSize))
            {
                AddMissingGroup(missing);
            }
            ImGui.SameLine();
            ImGui.Text($"{missing.name}");
        }
    }

    /// <summary>
    /// Add missing param group to target GPARAM
    /// </summary>
    /// <param name="missingGroup"></param>
    public void AddMissingGroup(FormatReference missingGroup)
    {
        var newGroup = new GPARAM.Param();
        newGroup.Key = missingGroup.id;
        newGroup.Name = missingGroup.name;
        newGroup.Fields = new List<GPARAM.IField>();

        Parent.Selection._selectedGparam.Params.Add(newGroup);
    }
}
