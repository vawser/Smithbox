using HKLib.hk2018.hkaiCollisionAvoidance;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Banks.FormatBank;
using StudioCore.Configuration;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamGroupListView
{
    private GparamEditorScreen Screen;
    private GparamFilters Filters;
    private GparamSelectionManager Selection;
    private GparamContextMenu ContextMenu;

    public GparamGroupListView(GparamEditorScreen screen)
    {
        Screen = screen;
        Filters = screen.Filters;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
    }
    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the event parameter view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Groups##GparamGroups");

        Filters.DisplayGroupFilterSearch();

        ImGui.BeginChild("GparamGroupsSection");

        if (Selection.IsFileSelected())
        {
            GPARAM data = Selection.GetSelectedGparam();

            // Available groups
            for (int i = 0; i < data.Params.Count; i++)
            {
                GPARAM.Param entry = data.Params[i];

                var name = entry.Key;
                if (CFG.Current.Gparam_DisplayParamGroupAlias)
                    name = Smithbox.BankHandler.GPARAM_Info.GetReferenceName(entry.Key, entry.Name);

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

                if (Filters.IsGroupFilterMatch(entry.Name, ""))
                {
                    if (display)
                    {
                        // Group row
                        if (ImGui.Selectable($@" {name}##{entry.Key}", i == Selection._selectedParamGroupKey))
                        {
                            Selection.SetGparamGroup(i, entry);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Selection.SelectGparamGroup)
                        {
                            Selection.SelectGparamGroup = false;
                            Selection.SetGparamGroup(i, entry);
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Selection.SelectGparamGroup = true;
                        }
                    }
                }

                ContextMenu.GroupContextMenu(i);
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
        GPARAM data = Selection.GetSelectedGparam();

        List<FormatReference> missingGroups = new List<FormatReference>();

        // Get source Format Reference
        foreach (var entry in Smithbox.BankHandler.GPARAM_Info.Information.list)
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
                Selection.ToggleSelectedFileModifiedState(true);
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

        Selection._selectedGparam.Params.Add(newGroup);
    }
}
