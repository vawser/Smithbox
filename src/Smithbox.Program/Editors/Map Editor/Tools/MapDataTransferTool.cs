using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.MaterialEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MapDataTransferTool
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public MapDataTransferTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
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
            UIHelper.WrappedText("Use this section to import JSON data, applying the data to your current project.");

            // TODO

            ImGui.EndTabItem();
        }
    }

    public void ImportMenu()
    {
        var curView = Editor.ViewHandler.ActiveView;

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
            UIHelper.WrappedText("Use this section to export JSON data from your current project.");

            // TODO

            ImGui.EndTabItem();
        }
    }

    public void ExportMenu()
    {
        var curView = Editor.ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Export"))
        {
            // TODO

            ImGui.EndMenu();
        }
    }
    #endregion
}
