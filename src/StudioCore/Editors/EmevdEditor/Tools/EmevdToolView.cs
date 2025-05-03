using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editors.EmevdEditor.Enums;
using StudioCore.Editors.EmevdEditor.Framework;
using StudioCore.EmevdEditor;
using StudioCore.Interface;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the tool view for this editor.
/// </summary>
public class EmevdToolView
{
    private EmevdEditorScreen Screen;
    private EmevdActionHandler ActionHandler;
    private EmevdSelectionManager Selection;

    public EmevdToolView(EmevdEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ActionHandler = screen.ActionHandler;
    }

    public void OnProjectChanged()
    {

    }


    public void Display()
    {
        if (Screen.Project.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_EmevdEditor"))
        {
            Selection.SwitchWindowContext(EmevdEditorContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

            List<string> loggedInstructions = new List<string>();

            if (ImGui.CollapsingHeader("Debug Tool"))
            {
                if (ImGui.Button("Log Unknown Instructions", defaultButtonSize))
                {
                    ActionHandler.LogUnknownInstructions();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
