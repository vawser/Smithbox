using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Interface;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelToolWindow
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ModelToolWindow(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##modelEditorTools", ImGuiWindowFlags.MenuBar))
        {
            Editor.FocusManager.SwitchMapEditorContext(MapEditorContext.ToolWindow);

            var windowHeight = ImGui.GetWindowHeight();
            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            //if (CFG.Current.Interface_ModelEditor_Tool_Create)
            //{
            //    Editor.CreateAction.OnToolWindow();
            //}
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            //if (ImGui.MenuItem("Create"))
            //{
            //    CFG.Current.Interface_ModelEditor_Tool_Create = !CFG.Current.Interface_ModelEditor_Tool_Create;
            //}
            //UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Tool_Create);

            ImGui.EndMenu();
        }
    }

    public void OnMenubar()
    {

    }
}


