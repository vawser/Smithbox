using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.EmevdEditor;
using StudioCore.Platform;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor.Tools;

public class ToolWindow
{
    private EmevdEditorScreen Screen;

    public ToolWindow(EmevdEditorScreen screen)
    {
        Screen = screen;
    }


    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

    }


    public void OnGui()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * Smithbox.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_EmevdEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

            if (ImGui.CollapsingHeader("Test"))
            {
                
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
