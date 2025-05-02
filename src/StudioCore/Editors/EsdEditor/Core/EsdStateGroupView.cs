using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editors.EsdEditor.Enums;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the state group selection, viewing and editing.
/// </summary>
public class EsdStateGroupView
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;
    private EsdFilters Filters;
    private EsdContextMenu ContextMenu;

    public EsdStateGroupView(EsdEditorScreen screen)
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
        ImGui.Begin("State Group Selection##EsdStateGroupSelectView");
        Selection.SwitchWindowContext(EsdEditorContext.StateGroup);

        var script = Selection._selectedEsdScript;
        var stateGroupKey = Selection._selectedStateGroupKey;

        Filters.DisplayStateGroupFilterSearch();

        ImGui.BeginChild("StateGroupSection");
        Selection.SwitchWindowContext(EsdEditorContext.StateGroup);

        if (script != null)
        {
            foreach (var entry in script.StateGroups)
            {
                var stateId = entry.Key;
                var stateGroups = entry.Value;

                var displayName = $"{entry.Key}";
                var aliasName = displayName;

                if (Filters.IsStateGroupFilterMatch(displayName, aliasName))
                {
                    if (ImGui.Selectable($@" {stateId}", stateGroupKey == stateId))
                    {
                        Selection.ResetStateGroupNode();

                        Selection.SetStateGroup(stateId, stateGroups);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectNextStateGroup)
                    {
                        Selection.SelectNextStateGroup = false;
                        Selection.SetStateGroup(stateId, stateGroups);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectNextStateGroup = true;
                    }

                    // Only apply to selection
                    if (Selection._selectedStateGroupKey != -1)
                    {
                        if (Selection._selectedStateGroupKey == entry.Key)
                        {
                            ContextMenu.StateGroupContextMenu(entry);
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
