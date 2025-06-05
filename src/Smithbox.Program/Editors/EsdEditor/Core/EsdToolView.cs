using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using System.Numerics;

namespace StudioCore.EzStateEditorNS;

/// <summary>
/// Handles the tool view for this editor.
/// </summary>
public class EsdToolView
{
    public EsdEditorScreen Editor;
    public ProjectEntry Project;

    public EsdToolView(EsdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnProjectChanged()
    {

    }


    public void Display()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.GetUIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_EsdEditor", ImGuiWindowFlags.MenuBar))
        {
            Editor.Selection.SwitchWindowContext(EsdEditorContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();
            var defaultButtonSize = new Vector2(windowWidth, 32);

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            //if (ImGui.MenuItem("Power Edit"))
            //{
            //    CFG.Current.Interface_BehaviorEditor_Tool_PowerEdit = !CFG.Current.Interface_BehaviorEditor_Tool_PowerEdit;
            //}
            //UIHelper.ShowActiveStatus(CFG.Current.Interface_BehaviorEditor_Tool_PowerEdit);

            ImGui.EndMenu();
        }
    }
}
