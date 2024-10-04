using ImGuiNET;
using SoulsFormats;
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

    public EsdStateGroupView(EsdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
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

        var script = Selection._selectedEsdScript;
        var stateGroupKey = Selection._selectedStateGroupKey;

        if (script != null)
        {
            foreach (var entry in script.StateGroups)
            {
                var stateId = entry.Key;
                var stateGroups = entry.Value;

                if (ImGui.Selectable($@" {stateId}", stateGroupKey == stateId))
                {
                    Selection.ResetStateGroupNode();

                    Selection.SetStateGroup(stateId, stateGroups);
                }
            }
        }

        ImGui.End();
    }
}
