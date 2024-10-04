using ImGuiNET;
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

    public EsdStateNodeView(EsdEditorScreen screen)
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
        ImGui.Begin("State Node Selection##EsdStateNodeSelectView");

        var stateGroups = Selection._selectedStateGroups;
        var stateNodeKey = Selection._selectedStateGroupNodeKey;

        if (stateGroups != null)
        {
            foreach (var (key, entry) in stateGroups)
            {
                if (ImGui.Selectable($@" {key}", key == stateNodeKey))
                {
                    Selection.SetStateGroupNode(key, entry);
                }
            }
        }

        ImGui.End();
    }
}
