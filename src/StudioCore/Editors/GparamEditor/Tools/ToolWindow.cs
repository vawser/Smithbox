using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editors.GparamEditor.Actions;
using StudioCore.GraphicsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Tools;


public class ToolWindow
{
    private GparamEditorScreen Screen;
    public ActionHandler Handler;

    public ToolWindow(GparamEditorScreen screen)
    {
        Screen = screen;
        Handler = new ActionHandler(screen);
    }

    public void OnProjectChanged()
    {

    }

    public void OnGui(GparamQuickEdit handler)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_GparamEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
            var halfButtonSize = new Vector2(windowWidth * 0.975f / 2, 32);

            if (ImGui.CollapsingHeader("Quick Edit"))
            {
                handler.DisplayInputWindow();
            }
            if (ImGui.CollapsingHeader("Quick Edit Commands"))
            {
                handler.DisplayCheatSheet();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
