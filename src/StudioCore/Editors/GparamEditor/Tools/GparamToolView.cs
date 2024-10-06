using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editors.GparamEditor.Framework;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Tools;


public class GparamToolView
{
    private GparamEditorScreen Screen;
    public GparamActionHandler ActionHandler;

    public GparamToolView(GparamEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = new GparamActionHandler(screen);
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for this view
    /// </summary>
    public void Display()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_GparamEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
            var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);

            if (ImGui.CollapsingHeader("Quick Edit"))
            {
                Screen.QuickEditHandler.DisplayInputWindow();
            }
            if (ImGui.CollapsingHeader("Quick Edit Commands"))
            {
                Screen.QuickEditHandler.DisplayCheatSheet();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
