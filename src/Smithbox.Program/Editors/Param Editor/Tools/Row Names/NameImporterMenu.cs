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


public static class NameImporterMenu
{
    public static bool IsSpecificParam = false;
    public static string FilePath = null;

    public static void Display(ProjectEntry curProject)
    {
        var paramData = curProject.Handler.ParamData;
        var activeView = curProject.Handler.ParamEditor.ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Import"))
        {
            if (ImGui.BeginMenu("Community Names"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    RowNameHelper.ImportRowNamesForParam(
                        curProject,
                        curProject.Handler.ParamData.PrimaryBank,
                        ParamRowNameImportType.Community,
                        activeView.Selection.GetActiveParam());
                }
                if (ImGui.MenuItem($"All"))
                {
                    RowNameHelper.ImportRowNames(
                        curProject,
                        curProject.Handler.ParamData.PrimaryBank,
                        ParamRowNameImportType.Community);
                }

                ImGui.EndMenu();
            }

            if (ParamUtils.HasDeveloperRowNames(curProject))
            {
                if (ImGui.BeginMenu("Developer Names"))
                {
                    if (ImGui.MenuItem($"Selected Param"))
                    {
                        RowNameHelper.ImportRowNamesForParam(
                            curProject,
                            curProject.Handler.ParamData.PrimaryBank,
                            ParamRowNameImportType.Developer,
                            activeView.Selection.GetActiveParam());
                    }
                    if (ImGui.MenuItem($"All"))
                    {
                        RowNameHelper.ImportRowNames(
                            curProject,
                            curProject.Handler.ParamData.PrimaryBank,
                            ParamRowNameImportType.Developer);
                    }
                    ImGui.EndMenu();
                }
            }

            if (ImGui.BeginMenu("From JSON File"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    IsSpecificParam = true;
                    DisplayJsonSelector(curProject);
                }
                UIHelper.Tooltip("Import the row names from the selected folder for the currently selected param.");

                if (ImGui.MenuItem($"All"))
                {
                    IsSpecificParam = false;
                    DisplayJsonSelector(curProject);
                }
                UIHelper.Tooltip("Import the row names from the selected folder for all params.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("From CSV File"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    IsSpecificParam = true;
                    DisplayCsvSelector(curProject);
                }
                UIHelper.Tooltip("This will import the external names from a CSV file, matching via row ID.");


                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("From Legacy Name Folder"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    IsSpecificParam = true;
                    DisplayLegacySelector(curProject);
                }
                UIHelper.Tooltip("This will import the external names from a legacy row name file (Stripped Row Name folder), matching via row index.");

                if (ImGui.MenuItem($"All"))
                {
                    IsSpecificParam = false;
                    DisplayLegacySelector(curProject);
                }
                UIHelper.Tooltip("This will import the external names from a legacy row name file (older Stripped Row Name folder), matching via row index.");

                ImGui.EndMenu();
            }
            ImGui.Separator();

            ImGui.Checkbox("Replace Empty Names Only", ref CFG.Current.Param_RowNameImport_ReplaceEmptyNamesOnly);

            UIHelper.Tooltip("If enabled, only rows with empty names will have their row names replaced with the import name.");

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
                    RowNameHelper.ImportRowNamesForParam(
                        project,
                        project.Handler.ParamData.PrimaryBank,
                        ParamRowNameImportType.External,
                        activeView.Selection.GetActiveParam(),
                        FilePath);
                }
                else
                {
                    RowNameHelper.ImportRowNames(
                        project,
                        project.Handler.ParamData.PrimaryBank,
                        ParamRowNameImportType.External,
                        FilePath);
                }
            }
        }
    }

    public static void DisplayCsvSelector(ProjectEntry project)
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
                    RowNameHelper.ImportRowNamesForParam_CSV(
                        project,
                        FilePath,
                        activeView.Selection.GetActiveParam());
                }
                else
                {

                }
            }
        }
    }
    public static void DisplayLegacySelector(ProjectEntry project)
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
                    RowNameHelper.ImportRowNamesForParam_Legacy(
                        project,
                        activeView.Selection.GetActiveParam(),
                        FilePath);
                }
                else
                {
                    RowNameHelper.ImportRowNamesForParam_Legacy(
                        project,
                        FilePath);
                }
            }
        }
    }

}