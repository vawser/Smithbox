using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core.ProjectNS;
using StudioCore.Interface;
using StudioCore.Resources.JSON;
using System.Collections.Generic;

namespace StudioCore.Editors.GparamEditorNS;

public class GparamGroupView
{
    public GparamEditor Editor;
    public Project Project;
    public GparamGroupView(Project curPoject, GparamEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }

    public void Draw()
    {
        ImGui.Begin("Groups##GparamGroups");
        Editor.EditorFocus.SetFocusContext(GparamEditorContext.Group);

        Editor.Filters.DisplayGroupFilterSearch();

        ImGui.BeginChild("GparamGroupsSection");

        if (Editor.Selection.HasGparamSelected())
        {
            var gparam = Editor.Selection._selectedGparam;

            // Available groups
            for (int i = 0; i < gparam.Params.Count; i++)
            {
                GPARAM.Param entry = gparam.Params[i];

                var name = entry.Key;

                if (CFG.Current.Gparam_DisplayParamGroupAlias)
                {
                    name = Project.GparamData.Meta.GetReferenceName(entry.Key, entry.Name);
                }

                var display = false;

                if (!CFG.Current.Gparam_DisplayEmptyGroups)
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

                if (Editor.Filters.IsGroupFilterMatch(entry.Name, ""))
                {
                    var isSelected = Editor.Selection.IsGroupSelected(i);

                    if (display)
                    {
                        // Group row
                        if (ImGui.Selectable($@" {name}##{entry.Key}", isSelected))
                        {
                            Editor.Selection.SelectGroup(i, entry);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Editor.Selection.AutoSelectGroup)
                        {
                            Editor.Selection.AutoSelectGroup = false;
                            Editor.Selection.SelectGroup(i, entry);
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Editor.Selection.AutoSelectGroup = false;
                        }
                    }
                }

                // Context menu
                if (i == Editor.Selection._selectedParamGroupKey)
                {
                    if (ImGui.BeginPopupContextItem($"Options##Gparam_Group_Context"))
                    {
                        if (ImGui.Selectable("Target in Quick Edit"))
                        {
                            Editor.QuickEdit.UpdateGroupFilter(Editor.Selection._selectedParamGroup.Key);

                            ImGui.CloseCurrentPopup();
                        }
                        UIHelper.Tooltip("Add this group to the Group Filter in the Quick Edit window.");

                        if (ImGui.Selectable("Remove"))
                        {
                            Editor.Selection._selectedGparam.Params.Remove(entry);

                            ImGui.CloseCurrentPopup();
                        }
                        UIHelper.Tooltip("Delete the selected group.");

                        ImGui.EndPopup();
                    }
                }
            }

            if (CFG.Current.Gparam_DisplayAddGroups)
            {
                ImGui.Separator();

                AddMissingGroupSection();
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }

    /// <summary>
    /// Groups List: add buttons
    /// </summary>
    public void AddMissingGroupSection()
    {
        GPARAM data = Editor.Selection.GetSelectedGparam();

        List<GparamFormatReference> missingGroups = new List<GparamFormatReference>();

        // Get source Format Reference
        foreach (var entry in Project.GparamData.Meta.Information.list)
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
            if (ImGui.Button($"Add##{missing.id}"))
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
    public void AddMissingGroup(GparamFormatReference missingGroup)
    {
        var newGroup = new GPARAM.Param();
        newGroup.Key = missingGroup.id;
        newGroup.Name = missingGroup.name;
        newGroup.Fields = new List<GPARAM.IField>();

        Editor.Selection._selectedGparam.Params.Add(newGroup);
    }
}
