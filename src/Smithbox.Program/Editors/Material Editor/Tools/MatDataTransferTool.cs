using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

public class MatDataTransferTool
{
    public MaterialEditorView View;
    public ProjectEntry Project;

    public MatDataTransferTool(MaterialEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void DisplayDropdown()
    {
        // Data Transfer
        if (ImGui.BeginMenu($"{LOC.Get("MAT_Tools_DataTransfer_Header")}##dataTransferMenuHeader"))
        {
            ImportMenu();
            ExportMenu();

            ImGui.EndMenu();
        }
    }
    public void Display()
    {
        ImGui.BeginChild("DataTransferSection", ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("dataTransferTabs");

        ImportTab();
        ExportTab();

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    #region Import
    public void ImportTab()
    {
        // Import
        if (ImGui.BeginTabItem($"{LOC.Get("MAT_Tools_DataTransfer_Tab_Import")}##importTab"))
        {
            GUI.WrappedText(LOC.Get("MAT_Tools_DataTransfer_Import_Hint"));

            // Export and import are on each material, in the File List (MatDataTransfer)
            ImGui.TextDisabled(LOC.Get("MAT_Transfer_Menu_Hint"));

            ImGui.EndTabItem();
        }
    }

    public void ImportMenu()
    {
        // Import
        if (ImGui.BeginMenu($"{LOC.Get("MAT_Tools_DataTransfer_Header_Import")}##importHeader"))
        {
            // Export and import are on each material, in the File List (MatDataTransfer)
            ImGui.TextDisabled(LOC.Get("MAT_Transfer_Menu_Hint"));

            ImGui.EndMenu();
        }
    }
    #endregion

    #region Export
    public void ExportTab()
    {
        // Export
        if (ImGui.BeginTabItem($"{LOC.Get("MAT_Tools_DataTransfer_Tab_Export")}##exportTab"))
        {
            GUI.WrappedText(LOC.Get("MAT_Tools_DataTransfer_Export_Hint"));

            // Export and import are on each material, in the File List (MatDataTransfer)
            ImGui.TextDisabled(LOC.Get("MAT_Transfer_Menu_Hint"));

            ImGui.EndTabItem();
        }
    }

    public void ExportMenu()
    {
        // Export
        if (ImGui.BeginMenu($"{LOC.Get("MAT_Tools_DataTransfer_Header_Export")}##exportHeader"))
        {
            // Export and import are on each material, in the File List (MatDataTransfer)
            ImGui.TextDisabled(LOC.Get("MAT_Transfer_Menu_Hint"));

            ImGui.EndMenu();
        }
    }
    #endregion
}
