using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editors.EsdEditor.Enums;
using StudioCore.Editors.EsdEditor.Framework;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the tool view for this editor.
/// </summary>
public class EsdToolView
{
    private EsdEditorScreen Screen;
    private EsdSelectionManager Selection;
    private EsdActionHandler ActionHandler;

    public EsdToolView(EsdEditorScreen screen)
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
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.Scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_EsdEditor"))
        {
            Selection.SwitchWindowContext(EsdEditorContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }
}
