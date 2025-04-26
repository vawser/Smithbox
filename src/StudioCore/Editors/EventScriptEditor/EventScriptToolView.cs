using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.EventScriptEditorNS;

/// <summary>
/// Handles the tool view for this editor.
/// </summary>
public class EventScriptToolView
{
    public EventScriptEditor Editor;
    public Project Project;

    public EventScriptToolView(Project curProject, EventScriptEditor editor)
    {
        Project = curProject;
        Editor = editor;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(EventScriptEditorContext.ToolWindow);

        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        List<string> loggedInstructions = new List<string>();

        if (ImGui.CollapsingHeader("Debug Tool"))
        {
            if (ImGui.Button("Log Unknown Instructions", defaultButtonSize))
            {
                Editor.ActionHandler.LogUnknownInstructions();
            }
        }
    }
}
