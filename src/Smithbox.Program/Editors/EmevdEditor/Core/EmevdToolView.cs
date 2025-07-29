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
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_EmevdEditor", ImGuiWindowFlags.MenuBar))
        {
            Editor.Selection.SwitchWindowContext(EmevdEditorContext.ToolWindow);

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            if (CFG.Current.Interface_EmevdEditor_Tool_EventInstanceFinder)
            {
                Editor.EventInstanceFinder.Display();
            }

            if (CFG.Current.Interface_EmevdEditor_Tool_InstructionInstanceFinder)
            {
                Editor.InstructionInstanceFinder.Display();
            }

            if (CFG.Current.Interface_EmevdEditor_Tool_ValueInstanceFinder)
            {
                Editor.ValueInstanceFinder.Display();
            }

            if (CFG.Current.Interface_EmevdEditor_Tool_UnknownInstructionFinder)
            {
                Editor.UnknownInstructionFinder.Display();
            }

            if (CFG.Current.Interface_EmevdEditor_Tool_TemplateReloader)
            {
                if (ImGui.CollapsingHeader("Template Reloader"))
                {
                    if (ImGui.Button("Reload", DPI.StandardButtonSize))
                    {
                        Editor.Project.EmevdData.PrimaryBank.LoadEMEDF();
                    }
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }


    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Event Instance Finder"))
            {
                CFG.Current.Interface_EmevdEditor_Tool_EventInstanceFinder = !CFG.Current.Interface_EmevdEditor_Tool_EventInstanceFinder;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_EventInstanceFinder);

            if (ImGui.MenuItem("Instruction Instance Finder"))
            {
                CFG.Current.Interface_EmevdEditor_Tool_InstructionInstanceFinder = !CFG.Current.Interface_EmevdEditor_Tool_InstructionInstanceFinder;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_InstructionInstanceFinder);

            if (ImGui.MenuItem("Value Instance Finder"))
            {
                CFG.Current.Interface_EmevdEditor_Tool_ValueInstanceFinder = !CFG.Current.Interface_EmevdEditor_Tool_ValueInstanceFinder;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_ValueInstanceFinder);

            if (ImGui.MenuItem("Unknown Instruction Finder"))
            {
                CFG.Current.Interface_EmevdEditor_Tool_UnknownInstructionFinder = !CFG.Current.Interface_EmevdEditor_Tool_UnknownInstructionFinder;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_UnknownInstructionFinder);

            if (ImGui.MenuItem("Template Reloader"))
            {
                CFG.Current.Interface_EmevdEditor_Tool_TemplateReloader = !CFG.Current.Interface_EmevdEditor_Tool_TemplateReloader;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_EmevdEditor_Tool_TemplateReloader);

            ImGui.EndMenu();
        }
    }
}
