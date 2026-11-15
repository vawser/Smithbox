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
    public MaterialEditorView View;
    public ProjectEntry Project;

    public MatDataTransferTool DataTransferTool;
    public MatCheckTool CheckTool;
    public MatSearchTool SearchTool;
    public MatUsedByTool UsedByTool;
    public MatShaderTool ShaderTool;
    public MatInGameTool InGameTool;

    private ToolSections Sections = new();

    public MaterialToolWindow(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        DataTransferTool = new(view, project);
        CheckTool = new(view, project);
        SearchTool = new(view, project);
        UsedByTool = new(view, project);
        ShaderTool = new(view, project);
        InGameTool = new(view, project);
    }

    public void Draw()
    {
        Sections.Begin();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        // Material Search and Mass Edit
        SearchTool.OnToolWindow();

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

        // Used By
        UsedByTool.OnToolWindow();

        // Data
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data"));

        // Shader Information
        ShaderTool.OnToolWindow();

        // Validation
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

        // Material Check
        CheckTool.OnToolWindow();

        // Game Memory
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        // In-Game
        InGameTool.OnToolWindow();

        Sections.End();

        // Data Transfer
        //if(CFG.Current.MaterialEditor_Tool_Data_Transfer)
        //{
        //    if (ImGui.CollapsingHeader("Data Transfer"))
        //    {
        //        DataTransferTool.Display();
        //    }
        //}
    }

    public void ToolMenu()
    {
        // Tools
        if (ImGui.BeginMenu($"{LOC.Get("MAT_Tools_Header_Tools")}##toolsMenuHeader"))
        {
            DataTransferTool.DisplayDropdown();

            ImGui.EndMenu();
        }
    }
}


