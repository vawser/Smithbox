using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

public class ParamDataTransferTool
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public string ImportPath = null;
    public string ExportPath = null;

    public ParamUpgradeRowGetType RowType = ParamUpgradeRowGetType.AllRows;
    public string SpecificFieldName = "";

    public CsvImportMode ImportMode = CsvImportMode.SelectedParam;
    public CsvImportType ImportType = CsvImportType.AllFields;

    public CsvExportType CsvExportType = CsvExportType.SelectedParam;

    public CsvExportMode CsvExportMode = CsvExportMode.Window;

    public string ExportString = "";
    public string ExportDirectory = "";
    public string ExportFilename = "";
    public string ImportDirectory = "";

    public ParamDataImportTab ImportTab;
    public ParamDataExportTab ExportTab;

    public ParamDataImportMenu ImportMenu;
    public ParamDataExportMenu ExportMenu;

    public ParamDataTransferTool(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        ImportTab = new(view, project, this);
        ExportTab = new(view, project, this);

        ImportMenu = new(view, project, this);
        ExportMenu = new(view, project, this);
    }

    public void DisplayDropdown()
    {
        var activeParamExists = View.Selection.ActiveParamExists();

        ExportMenu.Display();
        ImportMenu.Display();

        // CSV Settings
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_CSV_Settings")}##csvSettingsMenuHeader",
            activeParamExists))
        {
            SettingMenu();

            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_DataTransfer_Header_Data_Transfer")}##dataTransferHeader"))
        {
            ImGui.BeginChild("DataTransferSection", ImGuiChildFlags.Borders);

            ImGui.BeginTabBar("dataTransferTabs");

            ImportTab.Display();
            ExportTab.Display();

            ImGui.EndTabBar();

            ImGui.EndChild();
        }
    }

    #region Import

    public void ImportCsv()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;
        var primaryBank = activeView.Editor.Project.Handler.ParamData.PrimaryBank;
        var inputState = activeView.MassEdit.State;
        var activeParam = activeView.Selection.GetActiveParam();

        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        if (activeParam == null)
        {
            Smithbox.Log<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_No_Param_Selected"));
            return;
        }

        if (ImportType == CsvImportType.AllFields)
        {
            ImportAllFields(ImportSourceType.UserInput, ImportPath);
        }
        else if (ImportType == CsvImportType.RowName)
        {
            ImportSpecificField(ImportSourceType.UserInput, ImportPath, "Name");
        }
        else if (ImportType == CsvImportType.SpecificField)
        {
            ImportSpecificField(ImportSourceType.UserInput, ImportPath, SpecificFieldName);
        }

    }

    public void ImportCsvFromFile()
    {
        var dialog = PlatformUtils.Instance.OpenFileDialog(
            LOC.Get("PARAM_DataTransfer_Dialog_Select_File"), out var path);

        if (dialog)
        {
            ImportPath = path;

            if (ImportType == CsvImportType.AllFields)
            {
                ImportAllFields(ImportSourceType.File, ImportPath);
            }
            else if (ImportType == CsvImportType.RowName)
            {
                ImportSpecificField(ImportSourceType.File, ImportPath, "Name");
            }
            else if (ImportType == CsvImportType.SpecificField)
            {
                ImportSpecificField(ImportSourceType.File, ImportPath, SpecificFieldName);
            }
        }
    }
    public void ImportCsvFromSourceFolder()
    {
        if (Directory.Exists(ImportDirectory))
        {
            foreach (var filepath in Directory.EnumerateFiles(ImportDirectory))
            {
                if (filepath.EndsWith(".csv"))
                {
                    var filename = Path.GetFileNameWithoutExtension(filepath);

                    if (ImportType == CsvImportType.AllFields)
                    {
                        ImportAllFields(ImportSourceType.File, filepath, filename);
                    }
                    else if (ImportType == CsvImportType.RowName)
                    {
                        ImportSpecificField(ImportSourceType.File, filepath, "Name", filename);
                    }
                    else if (ImportType == CsvImportType.SpecificField)
                    {
                        ImportSpecificField(ImportSourceType.File, filepath, SpecificFieldName, filename);
                    }
                }
            }
        }
        else
        {
            Smithbox.Log<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Invalid_Import_Directory"));
        }
    }
    public void SetImportDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("PARAM_DataTransfer_Dialog_Select_Import_Destination"), out path);

        if (result)
        {
            ImportDirectory = path;
        }
    }

    public void OpenImportDirectory()
    {
        Process.Start("explorer.exe", ImportDirectory);
    }

    public void ImportAllFields(ImportSourceType importType, string csvPath, string targetParam = "")
    {
        var primaryBank = View.Editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        var csvString = "";

        if (targetParam == "")
            targetParam = View.Selection.GetActiveParam();

        if (targetParam == null)
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_Invalid_Param_Target"));
            return;
        }

        if (importType is ImportSourceType.File)
        {
            csvString = ParamDataTransferUtils.TryReadFile(csvPath);
        }
        else if (importType is ImportSourceType.UserInput)
        {
            csvString = View.MassEdit.State.MassEditInput_CSV;
        }

        (var result, CompoundAction action) = ParamIO.ApplyCSV(
                Project,
                primaryBank,
                csvString,
                targetParam,
                CFG.Current.Param_CSV_Append_Only,
                CFG.Current.Param_CSV_Replace_Row,
                delimiter[0]);

        if (action != null)
        {
            if (action.HasActions)
            {
                View.Editor.ActionManager.ExecuteAction(action);
            }

            View.Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
        }
        else
        {
            Smithbox.LogError<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Log_Failed_CSV_Import", result));
        }

        if (importType is ImportSourceType.UserInput)
        {
            View.MassEdit.State.MassEditResult_CSV = result;
        }
    }

    public void ImportSpecificField(ImportSourceType importType, string csvPath, string internalName, string targetParam = "")
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        var csvString = "";

        if (targetParam == "")
            targetParam = View.Selection.GetActiveParam();

        if (importType is ImportSourceType.File)
        {
            csvString = ParamDataTransferUtils.TryReadFile(csvPath);
        }
        else if (importType is ImportSourceType.UserInput)
        {
            csvString = View.MassEdit.State.MassEditInput_CSV;
        }

        (var result, CompoundAction action) = ParamIO.ApplySingleCSV(
            Project,
            primaryBank,
            csvString,
            targetParam,
            internalName,
            delimiter[0],
            false);

        if (action != null)
        {
            View.Editor.ActionManager.ExecuteAction(action);

            View.Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
        }
        else
        {
            Smithbox.LogError<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Log_Failed_CSV_Import", result));
        }

        if (importType is ImportSourceType.UserInput)
        {
            View.MassEdit.State.MassEditResult_CSV = result;
        }
    }
    public void PasteFromClipboard()
    {
        View.MassEdit.State.MassEditInput_CSV = PlatformUtils.Instance.GetClipboardText();
    }
    #endregion

    #region Export
    public void SetExportDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog(
            LOC.Get("PARAM_DataTransfer_Dialog_Select_Export_Destination"), out path);

        if (result)
        {
            ExportDirectory = path;
        }
    }

    public void OpenExportDirectory()
    {
        Process.Start("explorer.exe", ExportDirectory);
    }

    public void CopyOutputToClipboard()
    {
        PlatformUtils.Instance.SetClipboardText(ExportString);
    }

    public void ExportMultipleToFile()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        if (ExportDirectory == "")
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_No_Export_Directory"));
            return;
        }

        if (!Directory.Exists(ExportDirectory))
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_Invalid_Export_Directory"));
            return;
        }

        if (CsvExportType is CsvExportType.AllParams)
        {
            foreach (KeyValuePair<string, Param> param in primaryBank.Params)
            {
                IReadOnlyList<Param.Row> rows = param.Value.Rows;

                var writePath = Path.Combine(ExportDirectory, $"{param.Key}.csv");

                var csvString = ParamIO.GenerateCSV(
                    Project,
                    rows,
                    param.Value,
                    delimiter[0]);

                ParamDataTransferUtils.TryWriteFile(writePath, csvString);

                ExportString = LOC.Get("PARAM_DataTransfer_Export_String");
                Smithbox.Log<ParamDataTransferTool>(
                    LOC.Get("PARAM_DataTransfer_Save_Param_CSV_to_File", param.Key));
            }
        }
        else if (CsvExportType is CsvExportType.ModifiedParams)
        {
            foreach (KeyValuePair<string, Param> param in primaryBank.Params)
            {
                IReadOnlyList<Param.Row> rows = param.Value.Rows;

                var writePath = Path.Combine(ExportDirectory, $"{param.Key}.csv");

                var csvString = ParamIO.GenerateCSV(
                    Project,
                    rows,
                    param.Value,
                    delimiter[0]);

                ParamDataTransferUtils.TryWriteFile(writePath, csvString);

                ExportString = LOC.Get("PARAM_DataTransfer_Export_String");
                Smithbox.Log<ParamDataTransferTool>(
                    LOC.Get("PARAM_DataTransfer_Save_Modified_Param_CSV_to_File", param.Key));
            }
        }
    }

    public void ExportSingleToClipboard()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        var activeParam = View.Selection.GetActiveParam();

        if (activeParam == null)
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_No_Selected_Param"));
            return;
        }

        if (!primaryBank.Params.ContainsKey(activeParam))
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_Missing_Param_Data"));
            return;
        }

        var targetParam = primaryBank.Params.GetValueOrDefault(activeParam);

        if (targetParam == null)
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_Missing_Param_Data"));
            return;
        }

        var includedName = ExportTab.IncludeName;
        var includedFields = ExportTab.FieldInclusionDict;

        if (CsvExportType is CsvExportType.SelectedParam)
        {
            var csvString = ParamIO.GenerateCSV(
                Project,
                targetParam.Rows,
                targetParam,
                delimiter[0],
                includedName,
                includedFields);

            PlatformUtils.Instance.SetClipboardText(csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Saved_Selected_Param_CSV_to_Clipboard"));
        }
        else if (CsvExportType is CsvExportType.ModifiedRows)
        {
            IReadOnlyList<Param.Row> rows = CsvExportGetRows(ParamUpgradeRowGetType.ModifiedRows);

            var csvString = ParamIO.GenerateCSV(
                Project,
                rows,
                targetParam,
                delimiter[0],
                includedName,
                includedFields);

            PlatformUtils.Instance.SetClipboardText(csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Saved_Modified_Rows_CSV_to_Clipboard"));
        }
        else if (CsvExportType is CsvExportType.SelectedRows)
        {
            IReadOnlyList<Param.Row> rows = CsvExportGetRows(ParamUpgradeRowGetType.SelectedRows);

            var csvString = ParamIO.GenerateCSV(
                Project,
                rows,
                targetParam,
                delimiter[0],
                includedName,
                includedFields);

            PlatformUtils.Instance.SetClipboardText(csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Saved_Selected_Rows_CSV_to_Clipboard"));
        }
    }

    public void ExportSingleToFile()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        var activeParam = View.Selection.GetActiveParam();

        if (activeParam == null)
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_No_Selected_Param"));
            return;
        }

        if (ExportDirectory == "")
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_No_Export_Directory"));
            return;
        }

        if (!Directory.Exists(ExportDirectory))
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_Invalid_Export_Directory"));
            return;
        }

        if (!primaryBank.Params.ContainsKey(activeParam))
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_Missing_Param_Data"));
            return;
        }

        var targetParam = primaryBank.Params.GetValueOrDefault(activeParam);

        if (targetParam == null)
        {
            Smithbox.LogError<ParamDataTransferTool>(LOC.Get("PARAM_DataTransfer_Log_Missing_Param_Data"));
            return;
        }

        if (CsvExportType is CsvExportType.SelectedParam)
        {
            var writePath = Path.Combine(ExportDirectory, $"{activeParam}.csv");

            var csvString = ParamIO.GenerateCSV(
                Project,
                targetParam.Rows,
                targetParam,
                delimiter[0]);

            ParamDataTransferUtils.TryWriteFile(writePath, csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Saved_Selected_Param_CSV_to_File", writePath));
        }
        else if (CsvExportType is CsvExportType.ModifiedRows)
        {
            IReadOnlyList<Param.Row> rows = CsvExportGetRows(ParamUpgradeRowGetType.ModifiedRows);

            var writePath = Path.Combine(ExportDirectory, $"{activeParam}.csv");

            var csvString = ParamIO.GenerateCSV(
                Project,
                rows,
                targetParam,
                delimiter[0]);

            ParamDataTransferUtils.TryWriteFile(writePath, csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Saved_Modified_Rows_CSV_to_File", writePath));
        }
        else if (CsvExportType is CsvExportType.SelectedRows)
        {
            IReadOnlyList<Param.Row> rows = CsvExportGetRows(ParamUpgradeRowGetType.SelectedRows);

            var writePath = Path.Combine(ExportDirectory, $"{activeParam}.csv");

            var csvString = ParamIO.GenerateCSV(
                Project,
                rows,
                targetParam,
                delimiter[0]);

            ParamDataTransferUtils.TryWriteFile(writePath, csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Saved_Selected_Rows_CSV_to_File", writePath));
        }
    }

    public void ExportEntireParam()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = primaryBank.Params[View.Selection.GetActiveParam()].Rows;

        var writePath = Path.Combine(ExportPath, $"{View.Selection.GetActiveParam()}.csv");

        var csvString = ParamIO.GenerateCSV(
            Project,
            rows,
            primaryBank.Params[View.Selection.GetActiveParam()],
            delimiter[0]);

        ParamDataTransferUtils.TryWriteFile(writePath, csvString);
    }

    public void ExportAllParams()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        foreach (KeyValuePair<string, Param> param in primaryBank.Params)
        {
            IReadOnlyList<Param.Row> rows = param.Value.Rows;

            var writePath = Path.Combine(ExportPath, $"{param.Key}.csv");

            var csvString = ParamIO.GenerateCSV(
                Project,
                rows,
                param.Value,
                delimiter[0]);

            ParamDataTransferUtils.TryWriteFile(writePath, csvString);
        }
    }

    public void ExportAllModifiedParams()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        foreach (KeyValuePair<string, Param> param in primaryBank.Params)
        {
            var result = primaryBank.GetVanillaDiffRows(param.Key);

            if (result.Count > 0)
            {
                IReadOnlyList<Param.Row> rows = param.Value.Rows;

                var writePath = Path.Combine(ExportPath, $"{param.Key}.csv");

                var csvString = ParamIO.GenerateCSV(
                    Project,
                    rows,
                    param.Value,
                    delimiter[0]);

                ParamDataTransferUtils.TryWriteFile(writePath, csvString);
            }
        }
    }
    public void ExportAllFields(ParamUpgradeRowGetType rowType)
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);

        var outputString = ParamIO.GenerateCSV(Project, rows, primaryBank.Params[View.Selection.GetActiveParam()], delimiter[0]);

        var writePath = Path.Combine(ExportPath, $"{View.Selection.GetActiveParam()}.csv");

        ParamDataTransferUtils.TryWriteFile(writePath, outputString);
    }

    public void ExportNameField(ParamUpgradeRowGetType rowType)
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);

        var outputString = ParamIO.GenerateSingleCSV(rows, primaryBank.Params[View.Selection.GetActiveParam()], "Name", delimiter[0]);

        var writePath = Path.Combine(ExportPath, $"{View.Selection.GetActiveParam()}.csv");

        ParamDataTransferUtils.TryWriteFile(writePath, outputString);
    }

    public void ExportSpecificField(ParamUpgradeRowGetType rowType)
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);

        var outputString = ParamIO.GenerateSingleCSV(rows, primaryBank.Params[View.Selection.GetActiveParam()], SpecificFieldName, delimiter[0]);

        var writePath = Path.Combine(ExportPath, $"{View.Selection.GetActiveParam()}.csv");

        ParamDataTransferUtils.TryWriteFile(writePath, outputString);
    }

    public IReadOnlyList<Param.Row> CsvExportGetRows(ParamUpgradeRowGetType rowType)
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;

        IReadOnlyList<Param.Row> rows;

        var activeParam = View.Selection.GetActiveParam();

        if (rowType == ParamUpgradeRowGetType.AllRows)
        {
            // All rows
            rows = primaryBank.Params[activeParam].Rows;
        }
        else if (rowType == ParamUpgradeRowGetType.ModifiedRows)
        {
            // Modified rows
            // p is a row belonging to primaryBank.Params[activeParam], i.e. the same bank
            // the cache came from, so a reference lookup is valid and correctly separates
            // rows that share an ID.
            HashSet<Param.Row> vanillaDiffCache = primaryBank.GetVanillaDiffRows(activeParam);

            rows = primaryBank.Params[activeParam].Rows
                .Where(p => vanillaDiffCache.Contains(p))
                .ToList();
        }
        else if (rowType == ParamUpgradeRowGetType.SelectedRows)
        {
            // Selected rows
            rows = View.Selection.GetSelectedRows();
        }
        else
        {
            throw new NotSupportedException();
        }

        return rows;
    }

    #endregion

    #region Settings
    public static void SettingMenu()
    {
        // Toggle: Append New Rows
        ImGui.Checkbox($"{LOC.Get("PARAM_DataTransfer_Checkbox_Append_Mode")}##toggleAppendMode",
            ref CFG.Current.Param_CSV_Append_Only);
        GUI.Tooltip(LOC.Get("PARAM_DataTransfer_Checkbox_Append_Mode_TT"));

        // Toggle: Replace Existing Rows
        ImGui.Checkbox($"{LOC.Get("PARAM_DataTransfer_Checkbox_Replace_Existing")}##toggleReplaceExisitng",
            ref CFG.Current.Param_CSV_Replace_Row);
        GUI.Tooltip(LOC.Get("PARAM_DataTransfer_Checkbox_Replace_Existing_TT"));

        var displayDelimiter = CFG.Current.Param_Export_Delimiter;
        if (displayDelimiter == "\t")
        {
            displayDelimiter = "\\t";
        }

        // Delimiter
        GUI.SimpleHeader(
            LOC.Get("PARAM_DataTransfer_Header_Delimiter"),
            LOC.Get("PARAM_DataTransfer_Header_Delimiter_TT"));

        if (ImGui.InputText("##delimiter", ref displayDelimiter, 2))
        {
            if (displayDelimiter == "\\t")
                displayDelimiter = "\t";

            CFG.Current.Param_Export_Delimiter = displayDelimiter;
        }
    }

    #endregion
}