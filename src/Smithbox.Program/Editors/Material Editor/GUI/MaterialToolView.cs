using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;


public class MaterialToolWindow
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialToolWindow(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Draw()
    {
        if (!CFG.Current.Interface_MaterialEditor_ToolWindow)
            return;

        if (ImGui.Begin("Tools##ToolConfigureWindow_MaterialEditor", UIHelper.GetMainWindowFlags()))
        {
            FocusManager.SetFocus(EditorFocusContext.MaterialEditor_Tools);

            var windowHeight = ImGui.GetWindowHeight();
            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

        }

        ImGui.End();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {

            ImGui.EndMenu();
        }
    }

    public void ToolMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {

            ImGui.EndMenu();
        }
    }

    public void OnMenubar()
    {

    }
}


