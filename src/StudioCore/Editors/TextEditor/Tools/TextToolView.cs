using ImGuiNET;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextToolView
{
    private TextEditorScreen Screen;
    private TextActionHandler ActionHandler;

    public TextToolView(TextEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    public void Display()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextEditor"))
        {
            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}