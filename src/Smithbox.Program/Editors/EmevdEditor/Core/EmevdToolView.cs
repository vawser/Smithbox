using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.EventScriptEditorNS;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;

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

            Editor.EventInstanceFinder.Display();
            Editor.InstructionInstanceFinder.Display();
            Editor.ValueInstanceFinder.Display();

#if DEBUG
            Editor.UnknownInstructionFinder.Display();

            if (ImGui.CollapsingHeader("Template Reload"))
            {
                if (ImGui.Button("Reload"))
                {
                    Editor.Project.EmevdData.PrimaryBank.LoadEMEDF();
                }
            }
#endif

        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }


}
