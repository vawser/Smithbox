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
    public ParamEditorView View;
    public ProjectEntry Project;

    public bool IsSpecificParamForExport = false;
    public string ExportFilePath = null;

    public bool IsSpecificParam = false;
    public string FilePath = null;

    public string ImportLanguage = "English";
    public string ImportFolder = "English";

    public ParamRowNameTool(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void DisplayDropdown()
    {
        DisplayImportMenu();
        DisplayExportMenu();
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_RowNames_Header")}##rowNamesHeader"))
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

        // Import
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_RowNames_Tab_Import")}##importTab"))
        {
            GUI.WrappedText(LOC.Get("PARAM_RowNames_Import_Hint"));

            // Language
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNames_Header_Import_Language"),
                LOC.Get("PARAM_RowNames_Header_Import_Language_TT"));

            var curLanguage = paramData.RowImportLanguages.Options.FirstOrDefault(e => e.Name == ImportLanguage);

            var previewName = LOC.Get(curLanguage.Key);

            GUI.SetInputWidth();
            if (ImGui.BeginCombo("##languageSelection", previewName))
            {
                foreach (var language in paramData.RowImportLanguages.Options)
                {
                    var displayName = LOC.Get(language.Key);

                    if (ImGui.Selectable(displayName, ImportLanguage == language.Name))
                    {
                        ImportLanguage = language.Name;
                        ImportFolder = language.Folder;
                    }
                }

                ImGui.EndCombo();
            }

            // Standard
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNames_Header_Standard"),
                LOC.Get("PARAM_RowNames_Header_Standard_TT"));

            GUI.MultiButtonInput("standardImportActions",
                "standardImport_SelectedParam", 
                LOC.Get("PARAM_RowNames_Action_Import_Selected_Param"),
                LOC.Get("PARAM_RowNames_Action_Import_Selected_Param_TT"),
                StandardImportSelectedParam,

                "standardImport_AllParams", 
                LOC.Get("PARAM_RowNames_Action_Import_All_Params"),
                LOC.Get("PARAM_RowNames_Action_Import_All_Params_TT"),
                StandardImportAllParams);

            // JSON
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNames_Header_JSON"),
                LOC.Get("PARAM_RowNames_Header_JSON_TT"));

            GUI.MultiButtonInput("jsonImportActions",
                "jsonImport_SelectedParam",
                LOC.Get("PARAM_RowNames_Action_Import_Selected_Param"),
                LOC.Get("PARAM_RowNames_Action_Import_Selected_Param_TT"), 
                JsonImportSelectedParam,

                "jsonImport_AllParams",
                LOC.Get("PARAM_RowNames_Action_Import_All_Params"),
                LOC.Get("PARAM_RowNames_Action_Import_All_Params_TT"), 
                JsonImportAllParams);

            // CSV
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNames_Header_CSV"),
                LOC.Get("PARAM_RowNames_Header_CSV_TT"));

            GUI.MultiButtonInput("csvImportActions",
                "csvImport_SelectedParam",
                LOC.Get("PARAM_RowNames_Action_Import_Selected_Param"),
                LOC.Get("PARAM_RowNames_Action_Import_Selected_Param_TT"), 
                CsvImportSelectedParam);

            // Legacy Text
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNames_Header_Legacy"),
                LOC.Get("PARAM_RowNames_Header_Legacy_TT"));

            GUI.MultiButtonInput("legacyImportActions",
                "legacyImport_SelectedParam",
                LOC.Get("PARAM_RowNames_Action_Import_Selected_Param"),
                LOC.Get("PARAM_RowNames_Action_Import_Selected_Param_TT"), 
                LegacyImportSelectedParam,

                "legacyImport_AllParams",
                LOC.Get("PARAM_RowNames_Action_Import_All_Params"),
                LOC.Get("PARAM_RowNames_Action_Import_All_Params_TT"), 
                LegacyImportAllParams);

            ImGui.EndTabItem();
        }
    }

    public void DisplayImportMenu()
    {
        var paramData = Project.Handler.ParamData;
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        // Import
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowNames_Menu_Header_Import")}##importMenuHeader"))
        {
            foreach (var language in paramData.RowImportLanguages.Options)
            {
                // <language> Names
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowNames_Language_Names", language.Name)}##{language.Folder}import"))
                {
                    // Selected Params
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Language_Selected_Param")}##selectedParamsAction"))
                    {
                        StandardImportSelectedParam();
                    }

                    // All Params
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Language_All_Params")}##allParamsAction"))
                    {
                        StandardImportAllParams();
                    }

                    ImGui.EndMenu();
                }
            }

            // From JSON File
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowNames_Header_From_JSON_File")}##fromJsonFileMenuHeader"))
            {
                // Selected Param
                if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Action_Selected_Param")}##selectedParamAction"))
                {
                    JsonImportSelectedParam();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNames_JSON_Selected_Param_TT"));

                // All Params
                if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Action_All_Params")}##allParamsAction"))
                {
                    JsonImportAllParams();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNames_JSON_All_Params_TT"));

                ImGui.EndMenu();
            }

            // From CSV File
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowNames_Header_From_CSV_File")}##fromCsvFileMenuHeader"))
            {
                // Selected Param
                if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Action_Selected_Param")}##selectedParamAction"))
                {
                    CsvImportSelectedParam();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNames_CSV_Selected_Param_TT"));


                ImGui.EndMenu();
            }

            // From Legacy Name Folder
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowNames_Header_From_Legacy_Folder")}##fromLegacyFolderMenuHeader"))
            {
                // Selected Param
                if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Action_Selected_Param")}##selectedParamAction"))
                {
                    LegacyImportSelectedParam();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNames_Legacy_Selected_Param_TT"));

                // All Params
                if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Action_All_Params")}##allParamsAction"))
                {
                    LegacyImportAllParams();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNames_Legacy_All_Params_TT"));

                ImGui.EndMenu();
            }
            ImGui.Separator();

            // Toggle: Replace Empty Names Only
            ImGui.Checkbox($"{LOC.Get("PARAM_RowNames_Checkbox_Replace_Empty_Names_Only")}##toggleEmptyNameImport", 
                ref CFG.Current.Param_RowNameImport_ReplaceEmptyNamesOnly);
            GUI.Tooltip(LOC.Get("PARAM_RowNames_Checkbox_Replace_Empty_Names_Only_TT"));

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

        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_RowNames_Export_Tab")}##exportTab"))
        {
            GUI.WrappedText(LOC.Get("PARAM_RowNames_Export_Tab_Hint"));

            // JSON
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNames_Header_Export_JSON"),
                LOC.Get("PARAM_RowNames_Header_Export_JSON_TT"));

            GUI.MultiButtonInput("jsonExportActions",
                "jsonExport_SelectedParam", 
                LOC.Get("PARAM_RowNames_Action_Export_Selected_Param"),
                LOC.Get("PARAM_RowNames_Action_Export_Selected_Param_TT"),
                JsonExportSelectedParam,

                "jsonExport_AllParams",
                LOC.Get("PARAM_RowNames_Action_Export_All_Params"),
                LOC.Get("PARAM_RowNames_Action_Export_All_Params_TT"), 
                JsonExportAllParams);

            // Text
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNames_Header_Export_Text"),
                LOC.Get("PARAM_RowNames_Header_Export_Text_TT"));

            GUI.MultiButtonInput("textExportActions",
                "textExport_SelectedParam",
                LOC.Get("PARAM_RowNames_Action_Export_Selected_Param"),
                LOC.Get("PARAM_RowNames_Action_Export_Selected_Param_TT"), 
                LegacyExportSelectedParam,

                "textExport_AllParams",
                LOC.Get("PARAM_RowNames_Action_Export_All_Params"),
                LOC.Get("PARAM_RowNames_Action_Export_All_Params_TT"), 
                LegacyExportAllParams);

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

        // Export
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowNames_Header_Export")}##exportMenuHeader"))
        {
            // JSON
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowNames_Header_Export_JSON")}##exportJsonMenuHeader"))
            {
                // Selected Param
                if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Action_Selected_Param")}##exportSelectedParam"))
                {
                    IsSpecificParamForExport = true;
                    ExportAsJson();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNames_Action_Export_Selected_Param_TT"));

                // All Params
                if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Action_All_Params")}##exportAllParams"))
                {
                    IsSpecificParamForExport = false;
                    ExportAsJson();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNames_Action_Export_All_Params_TT"));

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("PARAM_RowNames_JSON_Export_TT"));

            // Text
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowNames_Header_Export_Text")}##textMenuHeader"))
            {
                // Selected Param
                if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Action_Selected_Param")}##exportSelectedParam"))
                {
                    IsSpecificParamForExport = true;
                    ExportAsText();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNames_Action_Export_Selected_Param_TT"));

                // All
                if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNames_Action_All_Params")}##exportAllParams"))
                {
                    IsSpecificParamForExport = false;
                    ExportAsText();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNames_Action_Export_All_Params_TT"));

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("PARAM_RowNames_Text_Export_TT"));

            ImGui.EndMenu();
        }
    }

    public void ImportFromJson()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        var dialog = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("PARAM_RowNames_Dialog_Select_Folder"), out var path);

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

        var dialog = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("PARAM_RowNames_Dialog_Select_Folder"), out var path);

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

        var dialog = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("PARAM_RowNames_Dialog_Select_Folder"), out var path);

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

        var dialog = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("PARAM_RowNames_Dialog_Select_Folder"), out var path);

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

        var dialog = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("PARAM_RowNames_Dialog_Select_Folder"), out var path);

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
