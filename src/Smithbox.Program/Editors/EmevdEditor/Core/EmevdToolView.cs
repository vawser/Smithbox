using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.EventScriptEditorNS;
using StudioCore.Interface;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor.Core;

/// <summary>
/// Handles the tool view for this editor.
/// </summary>
public class EmevdToolView
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdToolView(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_EmevdEditor"))
        {
            Editor.Selection.SwitchWindowContext(EmevdEditorContext.ToolWindow);

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

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
