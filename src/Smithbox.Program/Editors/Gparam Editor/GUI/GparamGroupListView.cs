using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.GparamEditor;

public class GparamGroupListView
{
    private GparamEditorScreen Editor;
    private ProjectEntry Project;

    public GparamGroupListView(GparamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the event parameter view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Groups##GparamGroups");
        Editor.Selection.SwitchWindowContext(GparamEditorContext.Group);

        Editor.Filters.DisplayGroupFilterSearch();

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.CircleO}##emptyGroupToggle"))
        {
            CFG.Current.Gparam_DisplayEmptyGroups = !CFG.Current.Gparam_DisplayEmptyGroups;
        }
        UIHelper.Tooltip("Toggle the display of empty groups.");

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##addGroupToggle"))
        {
            CFG.Current.Gparam_DisplayAddGroups = !CFG.Current.Gparam_DisplayAddGroups;
        }
        UIHelper.Tooltip("Toggle the display of the add group buttons.");

        ImGui.BeginChild("GparamGroupsSection");
        Editor.Selection.SwitchWindowContext(GparamEditorContext.Group);

        if (Editor.Selection.IsFileSelected())
        {
            GPARAM data = Editor.Selection.GetSelectedGparam();

            // Available groups
            for (int i = 0; i < data.Params.Count; i++)
            {
                GPARAM.Param entry = data.Params[i];

                var name = entry.Key;
                if (CFG.Current.Gparam_DisplayParamGroupAlias)
                    name = FormatInformationUtils.GetReferenceName(Project.GparamData.GparamInformation, entry.Key, entry.Name);

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
                    if (display)
                    {
                        // Group row
                        if (ImGui.Selectable($@" {name}##{entry.Key}", i == Editor.Selection._selectedParamGroupKey))
                        {
                            Editor.Selection.SetGparamGroup(i, entry);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Editor.Selection.SelectGparamGroup)
                        {
                            Editor.Selection.SelectGparamGroup = false;
                            Editor.Selection.SetGparamGroup(i, entry);
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Editor.Selection.SelectGparamGroup = true;
                        }
                    }
                }

                Editor.ContextMenu.GroupContextMenu(i);
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

        List<FormatReference> missingGroups = new List<FormatReference>();

        if (Project.GparamData.GparamInformation.list == null)
            return;

        // Get source Format Reference
        foreach (var entry in Project.GparamData.GparamInformation.list)
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

        Editor.Selection._selectedGparam.Params.Add(newGroup);
    }
}
