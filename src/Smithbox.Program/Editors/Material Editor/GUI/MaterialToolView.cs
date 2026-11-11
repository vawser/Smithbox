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
    }

    public void Draw()
    {
        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        if (CFG.Current.Interface_MaterialEditor_Tool_Search)
        {
            // Material Search and Mass Edit
            SearchTool.OnToolWindow();
        }

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

        if (CFG.Current.Interface_MaterialEditor_Tool_UsedBy)
        {
            // Used By
            UsedByTool.OnToolWindow();
        }

        // Data
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data"));

        if (CFG.Current.Interface_MaterialEditor_Tool_ShaderInfo)
        {
            // Shader Information
            ShaderTool.OnToolWindow();
        }

        // Validation
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

        if (CFG.Current.Interface_MaterialEditor_Tool_MaterialCheck)
        {
            // Material Check
            CheckTool.OnToolWindow();
        }

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

    public void ViewMenu()
    {
        // View
        if (ImGui.BeginMenu($"{LOC.Get("MAT_Tools_Header_View")}##viewMenuHeader"))
        {
            // Data Transfer
            if (ImGui.MenuItem($"{LOC.Get("MAT_Tools_Data_Transfer")}##dataTransferToggle"))
            {
                CFG.Current.MaterialEditor_Tool_Data_Transfer = !CFG.Current.MaterialEditor_Tool_Data_Transfer;
            }
            GUI.ShowActiveStatus(CFG.Current.MaterialEditor_Tool_Data_Transfer);

            // Material Search
            if (ImGui.MenuItem($"{LOC.Get("MAT_ToolWindow_View_Toggle_Material_Search")}##materialSearchToolToggle"))
            {
                CFG.Current.Interface_MaterialEditor_Tool_Search = !CFG.Current.Interface_MaterialEditor_Tool_Search;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_Tool_Search);

            // Used By
            if (ImGui.MenuItem($"{LOC.Get("MAT_ToolWindow_View_Toggle_Used_By")}##usedByToolToggle"))
            {
                CFG.Current.Interface_MaterialEditor_Tool_UsedBy = !CFG.Current.Interface_MaterialEditor_Tool_UsedBy;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_Tool_UsedBy);

            // Shader Information
            if (ImGui.MenuItem($"{LOC.Get("MAT_ToolWindow_View_Toggle_Shader_Info")}##shaderInfoToolToggle"))
            {
                CFG.Current.Interface_MaterialEditor_Tool_ShaderInfo = !CFG.Current.Interface_MaterialEditor_Tool_ShaderInfo;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_Tool_ShaderInfo);

            // Material Check
            if (ImGui.MenuItem($"{LOC.Get("MAT_ToolWindow_View_Toggle_Material_Check")}##materialCheckToolToggle"))
            {
                CFG.Current.Interface_MaterialEditor_Tool_MaterialCheck = !CFG.Current.Interface_MaterialEditor_Tool_MaterialCheck;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MaterialEditor_Tool_MaterialCheck);

            ImGui.EndMenu();
        }
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


