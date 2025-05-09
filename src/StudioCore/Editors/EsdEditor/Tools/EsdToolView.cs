﻿using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.EsdEditor.Enums;
using StudioCore.Editors.EsdEditor.Framework;
using StudioCore.Interface;
using StudioCore.TalkEditor;
using System.Numerics;

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
        if (Screen.Project.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

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
