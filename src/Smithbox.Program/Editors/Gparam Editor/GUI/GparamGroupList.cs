using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Numerics;

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

        if (Parent.Selection.IsFileSelected())
        {
            GPARAM data = Parent.Selection.GetSelectedGparam();

            // Available groups
            for (int i = 0; i < data.Params.Count; i++)
            {
                GPARAM.Param entry = data.Params[i];

                var name = entry.Key;
                var groupId = entry.Key;
                var groupName = GparamMetaUtils.GetGroupName(Project, groupId);

                if (groupName != null)
                {
                    name = groupName;
                }

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
        }

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
}
