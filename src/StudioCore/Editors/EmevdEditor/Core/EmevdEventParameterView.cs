﻿using Hexa.NET.ImGui;
using StudioCore.Editors.EmevdEditor.Enums;
using StudioCore.Editors.EmevdEditor.Framework;
using StudioCore.EmevdEditor;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the EMEVD event parameter viewing and editing.
/// </summary>
public class EmevdEventParameterView
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;
    private EmevdParameterManager ParameterManager;

    public EmevdEventParameterView(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        ParameterManager = screen.ParameterManager;
    }

    /// <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the event parameter view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Event Properties##EventParameterView");
        Selection.SwitchWindowContext(EmevdEditorContext.EventProperties);

        if (Selection.SelectedEvent != null)
        {
            var evt = Selection.SelectedEvent;

            ImGui.Text($"{evt.RestBehavior}");

            ImGui.Separator();

            foreach (var para in Selection.SelectedEvent.Parameters)
            {
                ImGui.Text($"InstructionIndex: {para.InstructionIndex}");
                ImGui.Text($"TargetStartByte: {para.TargetStartByte}");
                ImGui.Text($"SourceStartByte: {para.SourceStartByte}");
                ImGui.Text($"ByteCount: {para.ByteCount}");
                ImGui.Text($"UnkID: {para.UnkID}");

                ImGui.Separator();
            }
        }

        ImGui.End();
    }
}
