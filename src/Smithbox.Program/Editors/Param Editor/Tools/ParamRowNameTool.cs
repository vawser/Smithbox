using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Application;
using StudioCore.Editors.GparamEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamRowNameTool
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public bool IsSpecificParamForExport = false;
    public string ExportFilePath = null;

    public bool IsSpecificParam = false;
    public string FilePath = null;

    public string ImportLanguage = "English";
    public string ImportFolder = "English";

    public ParamRowNameTool(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void DisplayDropdown()
    {
        DisplayImportMenu();
        DisplayExportMenu();
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Row Names"))
        {
            ImGui.BeginChild("RowNameSection", ImGuiChildFlags.Borders);

            ImGui.BeginTabBar("rowNameTabs");

            RowNameImportTab();
            RowNameExportTab();

            ImGui.EndTabBar();

            ImGui.EndChild();
        }
    }

    public void RowNameImportTab()
    {
        var paramData = Project.Handler.ParamData;
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        if (ImGui.BeginTabItem($"Import"))
        {
            UIHelper.WrappedText("Use this section to import the row names from the internal files supplied by Smithbox, or from an external. These will overwrite the existing row names within the current project.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Language", "The source language to draw the names from for the Standard import");

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##languageSelection", ImportLanguage))
            {
                foreach (var language in paramData.RowImportLanguages.Options)
                {
                    if (ImGui.Selectable($"{language.Name}", ImportLanguage == language.Name))
                    {
                        ImportLanguage = language.Name;
                        ImportFolder = language.Folder;
                    }
                }

                ImGui.EndCombo();
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Standard", "The row names that are supplied by Smithbox");

            UIHelper.MultiButtonInput("standardImportActions",
                "standardImport_SelectedParam", "Import for Selected Param", "", StandardImportSelectedParam,
                "standardImport_AllParams", "Import for All Params", "", StandardImportAllParams);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("JSON", "Import row names from a JSON file.");

            UIHelper.MultiButtonInput("jsonImportActions",
                "jsonImport_SelectedParam", "Import for Selected Param", "", JsonImportSelectedParam,
                "jsonImport_AllParams", "Import for All Params", "", JsonImportAllParams);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("CSV", "Import row names from a CSV file.");

            UIHelper.MultiButtonInput("csvImportActions",
                "csvImport_SelectedParam", "Import for Selected Param", "", CsvImportSelectedParam);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Legacy Text", "Import row names from a legacy text file.");

            UIHelper.MultiButtonInput("legacyImportActions",
                "legacyImport_SelectedParam", "Import for Selected Param", "", LegacyImportSelectedParam,
                "legacyImport_AllParams", "Import for All Params", "", LegacyImportAllParams);

            ImGui.EndTabItem();
        }
    }

    public void DisplayImportMenu()
    {
        var paramData = Project.Handler.ParamData;
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        if (ImGui.BeginMenu("Import"))
        {
            foreach (var language in paramData.RowImportLanguages.Options)
            {
                if (ImGui.BeginMenu($"{language.Name} Names##{language.Folder}import"))
                {
                    if (ImGui.MenuItem($"Selected Param"))
                    {
                        StandardImportSelectedParam();
                    }
                    if (ImGui.MenuItem($"All"))
                    {
                        StandardImportAllParams();
                    }

                    ImGui.EndMenu();
                }
            }

            if (ImGui.BeginMenu("From JSON File"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    JsonImportSelectedParam();
                }
                UIHelper.Tooltip("Import the row names from the selected folder for the currently selected param.");

                if (ImGui.MenuItem($"All"))
                {
                    JsonImportAllParams();
                }
                UIHelper.Tooltip("Import the row names from the selected folder for all params.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("From CSV File"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    CsvImportSelectedParam();
                }
                UIHelper.Tooltip("This will import the external names from a CSV file, matching via row ID.");


                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("From Legacy Name Folder"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    LegacyImportSelectedParam();
                }
                UIHelper.Tooltip("This will import the external names from a legacy row name file (Stripped Row Name folder), matching via row index.");

                if (ImGui.MenuItem($"All"))
                {
                    LegacyImportAllParams();
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

    public void StandardImportSelectedParam()
    {
        var paramData = Project.Handler.ParamData;
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        RowNameHelper.ImportRowNamesForParam(
            Project,
            Project.Handler.ParamData.PrimaryBank,
            ImportFolder,
            activeView.Selection.GetActiveParam());
    }

    public void StandardImportAllParams()
    {
        var paramData = Project.Handler.ParamData;
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        RowNameHelper.ImportRowNames(
            Project,
            Project.Handler.ParamData.PrimaryBank,
            ImportFolder);
    }

    public void JsonImportSelectedParam()
    {
        IsSpecificParam = true;
        ImportFromJson();
    }

    public void JsonImportAllParams()
    {
        IsSpecificParam = false;
        ImportFromJson();
    }

    public void CsvImportSelectedParam()
    {
        IsSpecificParam = true;
        ImportFromCsv();
    }

    public void LegacyImportSelectedParam()
    {
        IsSpecificParam = true;
        ImportFromLegacyText();
    }
    public void LegacyImportAllParams()
    {
        IsSpecificParam = false;
        ImportFromLegacyText();
    }

    public void RowNameExportTab()
    {
        var paramData = Project.Handler.ParamData;
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        if (ImGui.BeginTabItem($"Export"))
        {
            UIHelper.WrappedText("Use this section to export the row names for the current project into an external file.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("JSON", "Export row names to a JSON file.");

            UIHelper.MultiButtonInput("jsonExportActions",
                "jsonExport_SelectedParam", "Export for Selected Param", "", JsonExportSelectedParam,
                "jsonExport_AllParams", "Export for All Params", "", JsonExportAllParams);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Text", "Export row names to a text file.");

            UIHelper.MultiButtonInput("textExportActions",
                "textExport_SelectedParam", "Export for Selected Param", "", LegacyExportSelectedParam,
                "textExport_AllParams", "Export for All Params", "", LegacyExportAllParams);

            ImGui.EndTabItem();
        }
    }

    public void JsonExportSelectedParam()
    {
        IsSpecificParamForExport = true;
        ExportAsJson();
    }
    public void JsonExportAllParams()
    {
        IsSpecificParamForExport = false;
        ExportAsJson();
    }

    public void LegacyExportSelectedParam()
    {
        IsSpecificParamForExport = true;
        ExportAsText();
    }

    public void LegacyExportAllParams()
    {
        IsSpecificParamForExport = true;
        ExportAsText();
    }

    public void DisplayExportMenu()
    {
        var paramData = Project.Handler.ParamData;
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;


        if (ImGui.BeginMenu("Export"))
        {
            if (ImGui.BeginMenu("JSON"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    IsSpecificParamForExport = true;
                    ExportAsJson();
                }
                UIHelper.Tooltip("Export the row names for your project to the selected folder.");

                if (ImGui.MenuItem($"All"))
                {
                    IsSpecificParamForExport = false;
                    ExportAsJson();
                }
                UIHelper.Tooltip("Export the row names for the currently selected param to the selected folder.");



                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export file will use the JSON storage format.");

            if (ImGui.BeginMenu("Text"))
            {
                if (ImGui.MenuItem($"Selected Param"))
                {
                    IsSpecificParamForExport = true;
                    ExportAsText();
                }
                UIHelper.Tooltip("Export the row names for your project to the selected folder.");

                if (ImGui.MenuItem($"All"))
                {
                    IsSpecificParamForExport = false;
                    ExportAsText();
                }
                UIHelper.Tooltip("Export the row names for the currently selected param to the selected folder.");

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export file will use the Text storage format. This format cannot be imported back in.");

            ImGui.EndMenu();
        }
    }

    public void ImportFromJson()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

        if (dialog)
        {
            FilePath = path;

            if (Path.Exists(FilePath))
            {
                if (IsSpecificParam)
                {
                    RowNameHelper.ImportRowNamesForParam(
                        Project,
                        Project.Handler.ParamData.PrimaryBank,
                        "External",
                        activeView.Selection.GetActiveParam(),
                        FilePath);
                }
                else
                {
                    RowNameHelper.ImportRowNames(
                        Project,
                        Project.Handler.ParamData.PrimaryBank,
                        "External",
                        FilePath);
                }
            }
        }
    }

    public void ImportFromCsv()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

        if (dialog)
        {
            FilePath = path;

            if (Path.Exists(FilePath))
            {
                if (IsSpecificParam)
                {
                    RowNameHelper.ImportRowNamesForParam_CSV(
                        Project,
                        FilePath,
                        activeView.Selection.GetActiveParam());
                }
                else
                {

                }
            }
        }
    }

    public void ImportFromLegacyText()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

        if (dialog)
        {
            FilePath = path;

            if (Path.Exists(FilePath))
            {
                if (IsSpecificParam)
                {
                    RowNameHelper.ImportRowNamesForParam_Legacy(
                        Project,
                        activeView.Selection.GetActiveParam(),
                        FilePath);
                }
                else
                {
                    RowNameHelper.ImportRowNamesForParam_Legacy(
                        Project,
                        FilePath);
                }
            }
        }
    }

    public void ExportAsJson()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

        if (dialog)
        {
            ExportFilePath = path;

            if (Path.Exists(ExportFilePath))
            {
                if (!IsSpecificParamForExport)
                {
                    RowNameHelper.ExportRowNames(
                        Project,
                        ParamRowNameExportType.JSON,
                        ExportFilePath);
                }
                else
                {
                    RowNameHelper.ExportRowNames(
                        Project,
                        ParamRowNameExportType.JSON,
                        ExportFilePath,
                        activeView.Selection.GetActiveParam());
                }
            }
        }
    }

    public void ExportAsText()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

        if (dialog)
        {
            ExportFilePath = path;

            if (Path.Exists(ExportFilePath))
            {
                if (!IsSpecificParamForExport)
                {
                    RowNameHelper.ExportRowNames(
                        Project,
                        ParamRowNameExportType.Text,
                        ExportFilePath);
                }
                else
                {
                    RowNameHelper.ExportRowNames(
                        Project,
                        ParamRowNameExportType.Text,
                        ExportFilePath,
                        activeView.Selection.GetActiveParam());
                }
            }
        }
    }
}
