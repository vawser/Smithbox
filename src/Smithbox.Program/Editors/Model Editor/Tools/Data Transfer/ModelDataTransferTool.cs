using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelDataTransferTool
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public string ImportPath = null;

    public string ExportPath = null;
    public string ExportString = "";
    public string ExportDirectory = "";
    public string ExportFilename = "";

    public ModelDataTransferTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void DisplayDropdown()
    {
        // Data Transfer
        if (ImGui.BeginMenu($"{LOC.Get("MODEL_DataTransfer_Menu_Title")}##dataTransferMenuHeader"))
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
        if (ImGui.BeginTabItem($"{LOC.Get("MODEL_DataTransfer_Tab_Import")}##importTab"))
        {
            GUI.WrappedText(LOC.Get("MODEL_DataTransfer_Tab_Import_Hint"));

            // TODO

            ImGui.EndTabItem();
        }
    }

    public void ImportMenu()
    {
        // Import
        if (ImGui.BeginMenu($"{LOC.Get("MODEL_DataTransfer_Tab_Import")}##importHeader"))
        {
            // TODO

            ImGui.EndMenu();
        }
    }
    #endregion

    #region Export
    public void ExportTab()
    {
        // Export
        if (ImGui.BeginTabItem($"{LOC.Get("MODEL_DataTransfer_Tab_Export")}##exportTab"))
        {
            GUI.WrappedText(LOC.Get("MODEL_DataTransfer_Tab_Export_Hint"));

            // TODO

            ImGui.EndTabItem();
        }
    }

    public void ExportMenu()
    {
        // Export
        if (ImGui.BeginMenu($"{LOC.Get("MODEL_DataTransfer_Tab_Export")}##exportHeader"))
        {
            // TODO

            ImGui.EndMenu();
        }
    }
    #endregion
}
