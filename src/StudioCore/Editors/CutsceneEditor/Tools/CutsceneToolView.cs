using Hexa.NET.ImGui;
using StudioCore.Core.Project;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.CutsceneEditor.Framework;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutsceneToolView
{
    private CutsceneEditorScreen Screen;
    private CutsceneActionHandler ActionHandler;

    public CutsceneToolView(CutsceneEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void OnProjectChanged()
    {

    }
    public void Display()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_CutsceneEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
