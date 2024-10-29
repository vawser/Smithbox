using HKLib.hk2018.hkaiCollisionAvoidance;
using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the state node selection, viewing and editing.
/// </summary>
public class EsdStateNodeView
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;
    private EsdFilters Filters;
    private EsdContextMenu ContextMenu;

    public EsdStateNodeView(EsdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("State Node Selection##EsdStateNodeSelectView");

        var stateGroups = Selection._selectedStateGroups;
        var stateNodeKey = Selection._selectedStateGroupNodeKey;

        Filters.DisplayStateFilterSearch();

        ImGui.BeginChild("StateNodeSection");

        if (stateGroups != null)
        {
            foreach (var (key, entry) in stateGroups)
            {
                var displayName = $"{key}";
                var aliasName = displayName;

                if (Filters.IsStateFilterMatch(displayName, aliasName))
                {
                    if (ImGui.Selectable($@" {key}", key == stateNodeKey))
                    {
                        Selection.SetStateGroupNode(key, entry);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectNextStateNode)
                    {
                        Selection.SelectNextStateNode = false;
                        Selection.SetStateGroupNode(key, entry);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectNextStateNode = true;
                    }

                    // Only apply to selection
                    if (Selection._selectedStateGroupNodeKey != -1)
                    {
                        if (Selection._selectedStateGroupNodeKey == key)
                        {
                            ContextMenu.StateNodeContextMenu(entry);
                        }
                    }

                    UIHelper.DisplayAlias(aliasName);
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
