using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class NameExporterMenu
{
    public static bool IsSpecificParam = false;
    public static string FilePath = null;

    public static void Display(ProjectEntry curProject)
    {
        var paramData = curProject.Handler.ParamData;
        var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Export"))
        {
            if (ImGui.BeginMenu("JSON"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    IsSpecificParam = true;
                    DisplayJsonSelector(curProject);
                }
                UIHelper.Tooltip("Export the row names for your project to the selected folder.");

                if (ImGui.MenuItem($"All"))
                {
                    IsSpecificParam = false;
                    DisplayJsonSelector(curProject);
                }
                UIHelper.Tooltip("Export the row names for the currently selected param to the selected folder.");



                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export file will use the JSON storage format.");

            if (ImGui.BeginMenu("Text"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    IsSpecificParam = true;
                    DisplayTextSelector(curProject);
                }
                UIHelper.Tooltip("Export the row names for your project to the selected folder.");

                if (ImGui.MenuItem($"All"))
                {
                    IsSpecificParam = false;
                    DisplayTextSelector(curProject);
                }
                UIHelper.Tooltip("Export the row names for the currently selected param to the selected folder.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export file will use the Text storage format. This format cannot be imported back in.");

            ImGui.EndMenu();
        }
    }

    public static void DisplayJsonSelector(ProjectEntry project)
    {
        var activeView = project.Handler.ParamEditor.ViewHandler.ActiveView;

        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

        if (dialog)
        {
            FilePath = path;

            if (Path.Exists(FilePath))
            {
                if (IsSpecificParam)
                {
                    RowNameHelper.ExportRowNames(
                        project,
                        ParamRowNameExportType.JSON,
                        FilePath);
                }
                else
                {
                    RowNameHelper.ExportRowNames(
                        project,
                        ParamRowNameExportType.JSON,
                        FilePath,
                        activeView.Selection.GetActiveParam());
                }
            }
        }
    }

    public static void DisplayTextSelector(ProjectEntry project)
    {
        var activeView = project.Handler.ParamEditor.ViewHandler.ActiveView;

        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

        if (dialog)
        {
            FilePath = path;

            if (Path.Exists(FilePath))
            {
                if (IsSpecificParam)
                {
                    RowNameHelper.ExportRowNames(
                        project,
                        ParamRowNameExportType.JSON,
                        FilePath);
                }
                else
                {
                    RowNameHelper.ExportRowNames(
                        project,
                        ParamRowNameExportType.JSON,
                        FilePath,
                        activeView.Selection.GetActiveParam());
                }
            }
        }
    }
}