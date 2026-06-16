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

    public MatDataTransferTool DataTransferTool;

    public MaterialToolWindow(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        DataTransferTool = new(editor, project);
    }

    public void Draw()
    {
        if (!CFG.Current.Interface_MaterialEditor_ToolWindow)
            return;

        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_MaterialEditor);
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

            // Data Transfer
            //if(CFG.Current.MaterialEditor_Tool_Data_Transfer)
            //{
            //    if (ImGui.CollapsingHeader("Data Transfer"))
            //    {
            //        DataTransferTool.Display();
            //    }
            //}
        }

        ImGui.End();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Data Transfer"))
            {
                CFG.Current.MaterialEditor_Tool_Data_Transfer = !CFG.Current.MaterialEditor_Tool_Data_Transfer;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MaterialEditor_Tool_Data_Transfer);

            ImGui.EndMenu();
        }
    }

    public void ToolMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            DataTransferTool.DisplayDropdown();

            ImGui.EndMenu();
        }
    }
}


