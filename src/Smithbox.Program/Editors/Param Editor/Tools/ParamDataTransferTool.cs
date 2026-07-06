using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.ComponentModel.DataAnnotations;
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

    public CsvImportType ImportType = CsvImportType.AllFields;

    public CsvExportType CsvExportType = CsvExportType.SelectedParam;

    public string ExportString = "";
    public string ExportDirectory = "";
    public string ExportFilename = "";

    public ParamDataTransferTool(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void DisplayDropdown()
    {
        var activeParamExists = View.Selection.ActiveParamExists();

        if (ImGui.BeginMenu("Export CSV", activeParamExists))
        {
            ExportMenu();

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Import CSV", activeParamExists))
        {
            ImportMenu();

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("CSV Settings", activeParamExists))
        {
            SettingMenu();

            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Data Transfer"))
        {
            ImGui.BeginChild("DataTransferSection", ImGuiChildFlags.Borders);

            ImGui.BeginTabBar("dataTransferTabs");

            ImportTab();
            ExportTab();

            ImGui.EndTabBar();

            ImGui.EndChild();
        }
    }

    #region Import

    public void ImportTab()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;

        if (ImGui.BeginTabItem($"Import"))
        {
            UIHelper.WrappedText("Use this section to import CSV data, applying the data to your current project.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("CSV Input", "The CSV to import as param data.");

            UIHelper.MultilineTextInput("csvImportText", ref activeView.MassEdit.State.MassEditInput_CSV);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Import Type", "The type of import to apply. Determines how the CSV data is interpreted.");

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##csvImportType", ImportType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(CsvImportType)))
                {
                    var handlingType = (CsvImportType)entry;

                    if (ImGui.Selectable($"{handlingType.GetDisplayName()}", handlingType == ImportType))
                    {
                        ImportType = handlingType;
                    }
                }

                ImGui.EndCombo();
            }

            if (ImportType is CsvImportType.SpecificField)
            {
                UIHelper.Spacer();
                UIHelper.SimpleHeader("Specific Field for Import", "The field to target from the import source.");

                UIHelper.SinglelineTextInput("SpecificFieldInput", ref SpecificFieldName);
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Delimiter", "");
            MassEditUtils.DelimiterInputText();

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Options", "");
            ImGui.Checkbox("Append Mode", ref CFG.Current.Param_CSV_Append_Only);
            UIHelper.Tooltip("Append new rows instead of ID based insertion (this will create out-of-order IDs)");

            if (CFG.Current.Param_CSV_Append_Only)
            {
                ImGui.Checkbox("Replace Existing Rows", ref CFG.Current.Param_CSV_Replace_Row);
                UIHelper.Tooltip("Replace existing rows instead of updating them (they will be moved to the end)");
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            UIHelper.MultiButtonInput("csvImportActions",
                "importCsv", "Import", "Import from the text input above.", ImportCsv,
                "importCsvFromFile", "Import from File", "Import from an external file.", ImportCsvFromFile);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Result", "");
            ImGui.Text(activeView.MassEdit.State.MassEditResult_CSV);

            ImGui.EndTabItem();
        }
    }

    public void ImportCsv()
    {
        var activeView = Project.Handler.ParamEditor.ViewHandler.ActiveView;
        var primaryBank = activeView.Editor.Project.Handler.ParamData.PrimaryBank;
        var inputState = activeView.MassEdit.State;
        var activeParam = activeView.Selection.GetActiveParam();

        var delimiter = CFG.Current.Param_Export_Delimiter[0];

        if (activeParam == null)
        {
            Smithbox.Log<ParamDataTransferTool>("No param has been selected.");
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
        var dialog = PlatformUtils.Instance.OpenFileDialog("Select File", out var path);

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

    public void ImportMenu()
    {
        var primaryBank = View.Editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        if (ImGui.MenuItem("All fields"))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
        }

        if (ImGui.MenuItem("Row Name"))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditSingleCSVImport/Name");
        }

        if (ImGui.BeginMenu("Specific Field"))
        {
            foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
            {
                if (ImGui.MenuItem(field.InternalName))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVImport/{field.InternalName}");
                }
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("From file...", View.Selection.ActiveParamExists()))
        {
            if (ImGui.MenuItem("All fields"))
            {
                var dialog = PlatformUtils.Instance.OpenFileDialog("Select File", out var path);

                if (dialog)
                {
                    ImportPath = path;
                    ImportAllFields(ImportSourceType.File, ImportPath);
                }
            }
            if (ImGui.MenuItem("Row Name"))
            {
                var dialog = PlatformUtils.Instance.OpenFileDialog("Select File", out var path);

                if (dialog)
                {
                    ImportPath = path;
                    ImportSpecificField(ImportSourceType.File, ImportPath, "Name");
                }
            }

            if (ImGui.BeginMenu("Specific Field"))
            {
                foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        SpecificFieldName = field.InternalName;

                        var dialog = PlatformUtils.Instance.OpenFileDialog("Select File", out var path);

                        if (dialog)
                        {
                            ImportPath = path;
                            ImportSpecificField(ImportSourceType.File, ImportPath, SpecificFieldName);
                        }
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    private void ImportAllFields(ImportSourceType importType, string csvPath)
    {
        var primaryBank = View.Editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        var csvString = ""; 

        if (importType is ImportSourceType.File)
        {
            csvString = TryReadFile(csvPath);
        }
        else if (importType is ImportSourceType.UserInput)
        {
            csvString = View.MassEdit.State.MassEditInput_CSV;
        }

        (var result, CompoundAction action) = ParamIO.ApplyCSV(
                Project,
                primaryBank,
                csvString,
                View.Selection.GetActiveParam(),
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
            Smithbox.LogError<ParamDataTransferTool>($"Failed to import CSV: {result}");
        }

        if (importType is ImportSourceType.UserInput)
        {
            View.MassEdit.State.MassEditResult_CSV = result;
        }
    }

    private void ImportSpecificField(ImportSourceType importType, string csvPath, string internalName)
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        var csvString = "";

        if (importType is ImportSourceType.File)
        {
            csvString = TryReadFile(csvPath);
        }
        else if (importType is ImportSourceType.UserInput)
        {
            csvString = View.MassEdit.State.MassEditInput_CSV;
        }

        (var result, CompoundAction action) = ParamIO.ApplySingleCSV(
            Project,
            primaryBank,
            csvString,
            View.Selection.GetActiveParam(),
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
            Smithbox.LogError<ParamDataTransferTool>($"Failed to import CSV: {result}");
        }

        if (importType is ImportSourceType.UserInput)
        {
            View.MassEdit.State.MassEditResult_CSV = result;
        }
    }
    #endregion

    #region Export
    public void ExportTab()
    {
        // Only supports a partial set of the dropdown export actions

        if (ImGui.BeginTabItem($"Export"))
        {
            UIHelper.WrappedText("Use this section to export CSV data from your current project.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Export Type", "");

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##csvExportType", CsvExportType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(CsvExportType)))
                {
                    var curExportType = (CsvExportType)entry;

                    if (ImGui.Selectable($"{curExportType.GetDisplayName()}", curExportType == CsvExportType))
                    {
                        CsvExportType = curExportType;
                    }
                }

                ImGui.EndCombo();
            }

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Export Directory", "The directory to export the CSV data to.");
            UIHelper.SinglelineTextInput("csvExportDir", ref ExportDirectory);

            UIHelper.MultiButtonInput("csvExportDir",
                "setDirectory", "Set Export Directory", "", SetExportDirectory,
                "openDirectory", "Open Export Directory", "", OpenExportDirectory);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Export Filename", "The filename to export the CSV data under (if blank the param name is used)");
            UIHelper.SinglelineTextInput("csvExportFilename", ref ExportFilename);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Export Output", "");

            // Has to use TextUnformatted as the CSV output string can be massive,
            // and it exceeds the internal buffers used by InputTextMultiline
            ImGui.BeginChild("OutputTextSection", new Vector2(0, 250), ImGuiChildFlags.Borders);
            ImGui.TextUnformatted(ExportString);
            ImGui.EndChild();

            UIHelper.MultiButtonInput("csvOutputActions",
                "copyToClipboard", "Copy to Clipboard", "Copy the output to the clibpaord", CopyOutputToClipboard);

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Actions", "");

            if(CsvExportType is CsvExportType.AllParams or CsvExportType.ModifiedParams)
            {
                UIHelper.MultiButtonInput("csvMultipleExportActions",
                    "exportCsvFile", "Export to File", "Export the param data to file", ExportMultipleToFile);
            }
            else
            {
                UIHelper.MultiButtonInput("csvSingleExportActions",
                    "exportCsvClipboard", "Export to Clipboard", "Export the param data as specified above to the clipboard.", ExportSingleToClipboard,
                    "exportCsvFile", "Export to File", "Export the param data as specified above to a file.", ExportSingleToFile);
            }


            ImGui.EndTabItem();
        }
    }

    public void SetExportDirectory()
    {
        string path;
        var result = PlatformUtils.Instance.OpenFolderDialog("Select Export Destination", out path);
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
            Smithbox.LogError<ParamDataTransferTool>("Export directory has not been set.");
            return;
        }

        if (!Directory.Exists(ExportDirectory))
        {
            Smithbox.LogError<ParamDataTransferTool>("Export directory is invalid.");
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

                TryWriteFile(writePath, csvString);

                ExportString = "Not generated when exporting multiple params.";
                Smithbox.Log<ParamDataTransferTool>($"Saved {param.Key} param CSV to file.");
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

                TryWriteFile(writePath, csvString);

                ExportString = "Not generated when exporting multiple params.";
                Smithbox.Log<ParamDataTransferTool>($"Saved modified {param.Key} param CSV to file.");
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
            Smithbox.LogError<ParamDataTransferTool>("No param has been selected.");
            return;
        }

        if (!primaryBank.Params.ContainsKey(activeParam))
        {
            Smithbox.LogError<ParamDataTransferTool>("Failed to find param data.");
            return;
        }

        var targetParam = primaryBank.Params.GetValueOrDefault(activeParam);

        if (targetParam == null)
        {
            Smithbox.LogError<ParamDataTransferTool>("Failed to find param data.");
            return;
        }

        if (CsvExportType is CsvExportType.SelectedParam)
        {
            var csvString = ParamIO.GenerateCSV(
                Project,
                targetParam.Rows,
                targetParam,
                delimiter[0]);

            PlatformUtils.Instance.SetClipboardText(csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>("Saved selected param CSV to clipboard.");
        }
        else if (CsvExportType is CsvExportType.ModifiedRows)
        {
            IReadOnlyList<Param.Row> rows = CsvExportGetRows(ParamUpgradeRowGetType.ModifiedRows);

            var csvString = ParamIO.GenerateCSV(
                Project,
                rows,
                targetParam,
                delimiter[0]);

            PlatformUtils.Instance.SetClipboardText(csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>("Saved modified rows CSV to clipboard.");
        }
        else if (CsvExportType is CsvExportType.SelectedRows)
        {
            IReadOnlyList<Param.Row> rows = CsvExportGetRows(ParamUpgradeRowGetType.SelectedRows);

            var csvString = ParamIO.GenerateCSV(
                Project,
                rows,
                targetParam,
                delimiter[0]);

            PlatformUtils.Instance.SetClipboardText(csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>("Saved selected rows CSV to clipboard.");
        }
    }

    public void ExportSingleToFile()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        var activeParam = View.Selection.GetActiveParam();

        if (activeParam == null)
        {
            Smithbox.LogError<ParamDataTransferTool>("No param has been selected.");
            return;
        }

        if (ExportDirectory == "")
        {
            Smithbox.LogError<ParamDataTransferTool>("Export directory has not been set.");
            return;
        }

        if(!Directory.Exists(ExportDirectory))
        {
            Smithbox.LogError<ParamDataTransferTool>("Export directory is invalid.");
            return;
        }

        if (!primaryBank.Params.ContainsKey(activeParam))
        {
            Smithbox.LogError<ParamDataTransferTool>("Failed to find param data.");
            return;
        }

        var targetParam = primaryBank.Params.GetValueOrDefault(activeParam);

        if (targetParam == null)
        {
            Smithbox.LogError<ParamDataTransferTool>("Failed to find param data.");
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

            TryWriteFile(writePath, csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>($"Saved selected param CSV to file: {writePath}");
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

            TryWriteFile(writePath, csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>($"Saved modified rows CSV to file: {writePath}");
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

            TryWriteFile(writePath, csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>($"Saved selected rows CSV to file: {writePath}");
        }
    }

    public void ExportMenu()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;

        if (ImGui.BeginMenu("All rows"))
        {
            CsvExportDisplay(ParamUpgradeRowGetType.AllRows);
            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.BeginMenu("Quick action"))
        {
            if (ImGui.MenuItem("Export selected Names to window"))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/2");
            }

            if (ImGui.MenuItem("Export entire param to window"))
            {
                EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
            }

            if (ImGui.MenuItem("Export entire param to file"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                if (dialog)
                {
                    ExportPath = path;
                    ExportEntireParam();
                }
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Modified rows", primaryBank.GetVanillaDiffRows(View.Selection.GetActiveParam()).Any()))
        {
            CsvExportDisplay(ParamUpgradeRowGetType.ModifiedRows);
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Selected rows", View.Selection.RowSelectionExists()))
        {
            CsvExportDisplay(ParamUpgradeRowGetType.SelectedRows);
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("All params"))
        {
            if (ImGui.MenuItem("Export all params to file"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                if (dialog)
                {
                    ExportPath = path;
                    ExportAllParams();
                }
            }

            if (ImGui.MenuItem("Export all modified params to file"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                if (dialog)
                {
                    ExportPath = path;
                    ExportAllModifiedParams();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void CsvExportDisplay(ParamUpgradeRowGetType rowType)
    {
        var primaryBank = View.Editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        if (ImGui.BeginMenu("Export to window..."))
        {
            if (ImGui.MenuItem("Export all fields"))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/{rowType}");
            }

            if (ImGui.BeginMenu("Export specific field"))
            {
                if (ImGui.MenuItem("Row name"))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/{rowType}");
                }

                foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/{field.InternalName}/{rowType}");
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Export to file..."))
        {
            if (ImGui.MenuItem("Export all fields"))
            {
                RowType = rowType;

                var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                if (dialog)
                {
                    ExportPath = path;
                    ExportAllFields(RowType);
                }
            }

            if (ImGui.BeginMenu("Export specific field"))
            {
                if (ImGui.MenuItem("Row name"))
                {
                    RowType = rowType;

                    var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                    if (dialog)
                    {
                        ExportPath = path;
                        ExportNameField(RowType);
                    }
                }

                foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        SpecificFieldName = field.InternalName;
                        RowType = rowType;

                        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                        if (dialog)
                        {
                            ExportPath = path;
                            ExportSpecificField(RowType);
                        }
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    private void ExportEntireParam()
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

        TryWriteFile(writePath, csvString);
    }

    private void ExportAllParams()
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

            TryWriteFile(writePath, csvString);
        }
    }

    private void ExportAllModifiedParams()
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

                TryWriteFile(writePath, csvString);
            }
        }
    }
    private void ExportAllFields(ParamUpgradeRowGetType rowType)
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);

        var outputString = ParamIO.GenerateCSV(Project, rows, primaryBank.Params[View.Selection.GetActiveParam()], delimiter[0]);

        var writePath = Path.Combine(ExportPath, $"{View.Selection.GetActiveParam()}.csv");

        TryWriteFile(writePath, outputString);
    }

    private void ExportNameField(ParamUpgradeRowGetType rowType)
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);

        var outputString = ParamIO.GenerateSingleCSV(rows, primaryBank.Params[View.Selection.GetActiveParam()], "Name", delimiter[0]);

        var writePath = Path.Combine(ExportPath, $"{View.Selection.GetActiveParam()}.csv");

        TryWriteFile(writePath, outputString);
    }

    private void ExportSpecificField(ParamUpgradeRowGetType rowType)
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = CsvExportGetRows(rowType);

        var outputString = ParamIO.GenerateSingleCSV(rows, primaryBank.Params[View.Selection.GetActiveParam()], SpecificFieldName, delimiter[0]);

        var writePath = Path.Combine(ExportPath, $"{View.Selection.GetActiveParam()}.csv");

        TryWriteFile(writePath, outputString);
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
            HashSet<int> vanillaDiffCache = primaryBank.GetVanillaDiffRows(activeParam);

            rows = primaryBank.Params[activeParam].Rows
                .Where(p => vanillaDiffCache.Contains(p.ID))
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
        UIHelper.SimpleHeader("Import", "");

        ImGui.Checkbox("Append Only", ref CFG.Current.Param_CSV_Append_Only);
        UIHelper.Tooltip("If enabled, rows may only be appended during an CSV import.");

        ImGui.Checkbox("Allow Overwrite", ref CFG.Current.Param_CSV_Replace_Row);
        UIHelper.Tooltip("If enabled, rows may be overwritten during an CSV import if the imported row ID matches an existing row ID.");

        UIHelper.SimpleHeader("Export", "");

        var displayDelimiter = CFG.Current.Param_Export_Delimiter;
        if (displayDelimiter == "\t")
        {
            displayDelimiter = "\\t";
        }

        if (ImGui.InputText("Delimiter", ref displayDelimiter, 2))
        {
            if (displayDelimiter == "\\t")
                displayDelimiter = "\t";

            CFG.Current.Param_Export_Delimiter = displayDelimiter;
        }
        UIHelper.Tooltip("The CSV delimiter to use during export.");
    }

    #endregion

    #region Utils
    private void TryWriteFile(string path, string text)
    {
        try
        {
            File.WriteAllText(path, text);
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamDataTransferTool>($"Failed to write file for CSV: {path}.", e);
        }

        Smithbox.Log<ParamDataTransferTool>($"Exported CSV file: {path}");
    }

    private string TryReadFile(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamDataTransferTool>($"Failed to read file for CSV: {path}.", e);

            return null;
        }
    }
    #endregion
}

public enum CsvExportType
{
    [Display(Name = "All Params")]
    AllParams,
    [Display(Name = "Modified Params")]
    ModifiedParams,
    [Display(Name = "Selected Param")]
    SelectedParam,
    [Display(Name = "Modified Rows")]
    ModifiedRows,
    [Display(Name = "Selected Rows")]
    SelectedRows
}

public enum CsvImportType
{
    [Display(Name ="All Fields")]
    AllFields,
    [Display(Name = "Row Name")]
    RowName,
    [Display(Name = "Specific Field")]
    SpecificField
}

public enum ImportSourceType
{
    UserInput,
    File
}