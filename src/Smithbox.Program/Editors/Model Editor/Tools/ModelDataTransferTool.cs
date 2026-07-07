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

    public ModelDataTransferTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void DisplayDropdown()
    {
        if (ImGui.BeginMenu("Data Transfer"))
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
        if (ImGui.BeginTabItem($"Import"))
        {
            GUI.WrappedText("Use this section to import JSON data, applying the data to your current project.");

            // TODO

            ImGui.EndTabItem();
        }
    }

    public void ImportMenu()
    {
        if (ImGui.BeginMenu("Import"))
        {
            // TODO

            ImGui.EndMenu();
        }
    }
    #endregion

    #region Export
    public void ExportTab()
    {
        if (ImGui.BeginTabItem($"Export"))
        {
            GUI.WrappedText("Use this section to export JSON data from your current project.");

            // TODO

            ImGui.EndTabItem();
        }
    }

    public void ExportMenu()
    {
        if (ImGui.BeginMenu("Export"))
        {
            // TODO

            ImGui.EndMenu();
        }
    }
    #endregion
}
