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

        // Export CSV
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_CSV")}##importCsvMenuHeader",
            activeParamExists))
        {
            ExportMenu();

            ImGui.EndMenu();
        }

        // Import CSV
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Import_CSV")}##exportCsvMenuHeader",
            activeParamExists))
        {
            ImportMenu();

            ImGui.EndMenu();
        }

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

        // Import
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataTransfer_Tab_Import")}##importTab"))
        {
            UIHelper.WrappedText(LOC.Get("PARAM_DataTransfer_ImportTab_Hint"));

            // CSV Input
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_CSV_Input"),
                LOC.Get("PARAM_DataTransfer_Header_CSV_Input_TT"));

            UIHelper.MultilineTextInput("csvImportText", ref activeView.MassEdit.State.MassEditInput_CSV);

            // Import Type
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Import_Type"),
                LOC.Get("PARAM_DataTransfer_Header_Import_Type_TT"));

            var previewName = LOC.Get(ImportType.GetDisplayName());

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##csvImportType", previewName))
            {
                foreach (var entry in Enum.GetValues(typeof(CsvImportType)))
                {
                    var handlingType = (CsvImportType)entry;

                    var displayName = LOC.Get(handlingType.GetDisplayName());

                    if (ImGui.Selectable(displayName, handlingType == ImportType))
                    {
                        ImportType = handlingType;
                    }
                }

                ImGui.EndCombo();
            }

            // Specific Field for Impot
            if (ImportType is CsvImportType.SpecificField)
            {
                UIHelper.Spacer();
                UIHelper.SimpleHeader(
                    LOC.Get("PARAM_DataTransfer_Header_Specific_Field"),
                    LOC.Get("PARAM_DataTransfer_Header_Specific_Field_TT"));

                UIHelper.SinglelineTextInputWithHint("SpecificFieldInput", ref SpecificFieldName,
                    LOC.Get("PARAM_DataTransfer_Specific_Field_Hint"));
            }

            // Delimiter
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Delimiter"),
                LOC.Get("PARAM_DataTransfer_Header_Delimiter_TT"));

            MassEditUtils.DelimiterInputText();

            // Options
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Options"),
                LOC.Get("PARAM_DataTransfer_Header_Options_TT"));

            // Append New Rows
            ImGui.Checkbox($"{LOC.Get("PARAM_DataTransfer_Checkbox_Append_Mode")}##toggleAppendMode", 
                ref CFG.Current.Param_CSV_Append_Only);

            UIHelper.Tooltip(LOC.Get("PARAM_DataTransfer_Checkbox_Append_Mode_TT"));

            if (CFG.Current.Param_CSV_Append_Only)
            {
                // Replace Existing Rows
                ImGui.Checkbox($"{LOC.Get("PARAM_DataTransfer_Checkbox_Replace_Existing")}##toggleReplaceExisting", 
                    ref CFG.Current.Param_CSV_Replace_Row);

                UIHelper.Tooltip(LOC.Get("PARAM_DataTransfer_Checkbox_Replace_Existing_TT"));
            }

            // ACtions
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Actions"),
                LOC.Get("PARAM_DataTransfer_Header_Actions_TT"));

            UIHelper.MultiButtonInput("csvImportActions",
                "importCsv", 
                LOC.Get("PARAM_DataTransfer_Action_Import"),
                LOC.Get("PARAM_DataTransfer_Action_Import_TT"),
                ImportCsv,

                "importCsvFromFile",
                LOC.Get("PARAM_DataTransfer_Action_File_Import"),
                LOC.Get("PARAM_DataTransfer_Action_File_Import_TT"), 
                ImportCsvFromFile);

            // Result
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Result"),
                LOC.Get("PARAM_DataTransfer_Header_Result_TT"));

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

    public void ImportMenu()
    {
        var primaryBank = View.Editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        // All Fields
        if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Import_All_Fields")}##importAllFieldsAction"))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditCSVImport");
        }

        // Row NAme
        if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Import_Row_Name")}##importRowNameAction"))
        {
            EditorCommandQueue.AddCommand(@"param/menu/massEditSingleCSVImport/Name");
        }

        // Specific Field
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Import_Specific_Field")}##importSpecificFieldMenuHeader"))
        {
            foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
            {
                // <field>
                if (ImGui.MenuItem(field.InternalName))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVImport/{field.InternalName}");
                }
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_From_File")}##fromFileMenuHeader", View.Selection.ActiveParamExists()))
        {
            // All Fields
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Import_All_Fields")}##importAllFieldsAction_file"))
            {
                var dialog = PlatformUtils.Instance.OpenFileDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_File"), out var path);

                if (dialog)
                {
                    ImportPath = path;
                    ImportAllFields(ImportSourceType.File, ImportPath);
                }
            }

            // Row Name
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Import_Row_Name")}##importRowNameAction_file"))
            {
                var dialog = PlatformUtils.Instance.OpenFileDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_File"), out var path);

                if (dialog)
                {
                    ImportPath = path;
                    ImportSpecificField(ImportSourceType.File, ImportPath, "Name");
                }
            }

            // Specific Field
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Import_Specific_Field")}##importSpecificFieldMenuHeader_file"))
            {
                foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    // <field>
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        SpecificFieldName = field.InternalName;

                        var dialog = PlatformUtils.Instance.OpenFileDialog(
                            LOC.Get("PARAM_DataTransfer_Dialog_Select_File"), out var path);

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
            Smithbox.LogError<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Log_Failed_CSV_Import", result));
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
            Smithbox.LogError<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Log_Failed_CSV_Import", result));
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

        // Export
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataTransfer_Tab_Export")}##exportTab"))
        {
            UIHelper.WrappedText(LOC.Get("PARAM_DataTransfer_ExportTab_Hint"));

            // Export Type
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Export_Type"),
                LOC.Get("PARAM_DataTransfer_Header_Export_Type_TT"));

            var previewName = LOC.Get(CsvExportType.GetDisplayName());

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##csvExportType", previewName))
            {
                foreach (var entry in Enum.GetValues(typeof(CsvExportType)))
                {
                    var curExportType = (CsvExportType)entry;

                    var displayName = LOC.Get(curExportType.GetDisplayName());

                    if (ImGui.Selectable(displayName, curExportType == CsvExportType))
                    {
                        CsvExportType = curExportType;
                    }
                }

                ImGui.EndCombo();
            }

            // Export Directory
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Export_Directory"),
                LOC.Get("PARAM_DataTransfer_Header_Export_Directory_TT"));

            UIHelper.SinglelineTextInputWithHint("csvExportDir", ref ExportDirectory, 
                LOC.Get("PARAM_DataTransfer_Export_Dir_Hint"));

            UIHelper.MultiButtonInput("csvExportDir",
                "setDirectory", 
                LOC.Get("PARAM_DataTransfer_Action_Set_Export_Directory"),
                LOC.Get("PARAM_DataTransfer_Action_Set_Export_Directory_TT"),
                SetExportDirectory,

                "openDirectory",
                LOC.Get("PARAM_DataTransfer_Action_Open_Export_Directory"),
                LOC.Get("PARAM_DataTransfer_Action_Open_Export_Directory_TT"), 
                OpenExportDirectory);

            // Export Filename
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Export_Filename"),
                LOC.Get("PARAM_DataTransfer_Header_Export_Filename_TT"));

            UIHelper.SinglelineTextInputWithHint("csvExportFilename", ref ExportFilename, LOC.Get("PARAM_DataTransfer_Export_Filename_Hint"));

            // Export Output
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Export_Output"),
                LOC.Get("PARAM_DataTransfer_Header_Export_Output_TT"));

            // Has to use TextUnformatted as the CSV output string can be massive,
            // and it exceeds the internal buffers used by InputTextMultiline
            ImGui.BeginChild("OutputTextSection", new Vector2(0, 250), ImGuiChildFlags.Borders);
            ImGui.TextUnformatted(ExportString);
            ImGui.EndChild();

            UIHelper.MultiButtonInput("csvOutputActions",
                "copyToClipboard", 
                LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard"),
                LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard_TT"),
                CopyOutputToClipboard);

            // Actions
            UIHelper.Spacer();
            UIHelper.SimpleHeader(
                LOC.Get("PARAM_DataTransfer_Header_Actions"),
                LOC.Get("PARAM_DataTransfer_Header_Actions_TT"));

            if(CsvExportType is CsvExportType.AllParams or CsvExportType.ModifiedParams)
            {
                UIHelper.MultiButtonInput("csvMultipleExportActions",
                    "exportCsvFile", 
                    LOC.Get("PARAM_DataTransfer_Action_Export_to_File"),
                    LOC.Get("PARAM_DataTransfer_Action_Export_to_File_TT"),
                    ExportMultipleToFile);
            }
            else
            {
                UIHelper.MultiButtonInput("csvSingleExportActions",
                    "exportCsvClipboard",
                    LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard"),
                    LOC.Get("PARAM_DataTransfer_Action_Copy_to_Clipboard_TT"),
                    ExportSingleToClipboard,

                    "exportCsvFile",
                    LOC.Get("PARAM_DataTransfer_Action_Export_to_File"),
                    LOC.Get("PARAM_DataTransfer_Action_Export_to_File_TT"),
                    ExportSingleToFile);
            }


            ImGui.EndTabItem();
        }
    }

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

                TryWriteFile(writePath, csvString);

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

                TryWriteFile(writePath, csvString);

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

        if (CsvExportType is CsvExportType.SelectedParam)
        {
            var csvString = ParamIO.GenerateCSV(
                Project,
                targetParam.Rows,
                targetParam,
                delimiter[0]);

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
                delimiter[0]);

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
                delimiter[0]);

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

        if(!Directory.Exists(ExportDirectory))
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

            TryWriteFile(writePath, csvString);

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

            TryWriteFile(writePath, csvString);

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

            TryWriteFile(writePath, csvString);

            ExportString = csvString;
            Smithbox.Log<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Saved_Selected_Rows_CSV_to_File", writePath));
        }
    }

    public void ExportMenu()
    {
        var primaryBank = View.Project.Handler.ParamData.PrimaryBank;

        // All Rows
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_All_Rows")}##allRowsMenuHeader"))
        {
            CsvExportDisplay(ParamUpgradeRowGetType.AllRows);
            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Quick Actions
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_Quick_Action")}##quickActionMenuHeader"))
        {
            // Export Selected Names to Window
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Action_Export_Selected_Names_Wnd")}##exportSelectedNamesWnd"))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/2");
            }

            // Export Selected Param to Window
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Action_Export_Selected_Param_Wnd")}##exportSelectedParamWnd"))
            {
                EditorCommandQueue.AddCommand(@"param/menu/massEditCSVExport/0");
            }

            // Export Selected Param to File
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Action_Export_Selected_Param_File")}##exportSelectedParamFile"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                if (dialog)
                {
                    ExportPath = path;
                    ExportEntireParam();
                }
            }

            ImGui.EndMenu();
        }

        // Modified Rows
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_Modified_Rows")}##modifiedRowsMenuHeader", primaryBank.GetVanillaDiffRows(View.Selection.GetActiveParam()).Any()))
        {
            CsvExportDisplay(ParamUpgradeRowGetType.ModifiedRows);
            ImGui.EndMenu();
        }

        // Selected Rows
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_Selected_Rows")}##selectedRowsMenuHeader", View.Selection.RowSelectionExists()))
        {
            CsvExportDisplay(ParamUpgradeRowGetType.SelectedRows);
            ImGui.EndMenu();
        }

        // All Params
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Export_Header_All_Params")}##allParamsMenuHeader"))
        {
            // Export All Params to File
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_All_Params_to_File")}##exportAllParamsToFile"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                if (dialog)
                {
                    ExportPath = path;
                    ExportAllParams();
                }
            }

            // Export All Modified Params to File
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_All_Modified_Params_to_File")}##exportAllModifiedParamsToFile"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

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

        // Export to Window
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_to_Window")}##exportToWindowMenuHeader"))
        {
            // Export All Fields
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_All_Fields")}##exportAllFieldsAction"))
            {
                EditorCommandQueue.AddCommand($@"param/menu/massEditCSVExport/{rowType}");
            }

            // Export Specific Field
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_Specific_Field")}##exportSpecificFieldHeader"))
            {
                // Row Name
                if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_Row_Name")}##exportRowNameAction"))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/Name/{rowType}");
                }

                foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    // <field>
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVExport/{field.InternalName}/{rowType}");
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        // Export to File
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_to_File")}##exportToFileMenuHeader"))
        {
            // Export All Fields
            if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_All_Fields")}##exportAllFieldsAction"))
            {
                RowType = rowType;

                var dialog = PlatformUtils.Instance.OpenFolderDialog(
                    LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                if (dialog)
                {
                    ExportPath = path;
                    ExportAllFields(RowType);
                }
            }

            // Export Specific Field
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_DataTransfer_Header_Export_Specific_Field")}##exportSpecificFieldHeader"))
            {
                // Row Name
                if (ImGui.MenuItem($"{LOC.Get("PARAM_DataTransfer_Export_Row_Name")}##exportRowNameAction"))
                {
                    RowType = rowType;

                    var dialog = PlatformUtils.Instance.OpenFolderDialog(
                        LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

                    if (dialog)
                    {
                        ExportPath = path;
                        ExportNameField(RowType);
                    }
                }

                foreach (PARAMDEF.Field field in primaryBank.Params[View.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    // <field>
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        SpecificFieldName = field.InternalName;
                        RowType = rowType;

                        var dialog = PlatformUtils.Instance.OpenFolderDialog(
                            LOC.Get("PARAM_DataTransfer_Dialog_Select_Folder"), out var path);

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
        // Toggle: Append New Rows
        ImGui.Checkbox($"{LOC.Get("PARAM_DataTransfer_Checkbox_Append_Mode")}##toggleAppendMode", 
            ref CFG.Current.Param_CSV_Append_Only);
        UIHelper.Tooltip(LOC.Get("PARAM_DataTransfer_Checkbox_Append_Mode_TT"));

        // Toggle: Replace Existing Rows
        ImGui.Checkbox($"{LOC.Get("PARAM_DataTransfer_Checkbox_Replace_Existing")}##toggleReplaceExisitng", 
            ref CFG.Current.Param_CSV_Replace_Row);
        UIHelper.Tooltip(LOC.Get("PARAM_DataTransfer_Checkbox_Replace_Existing_TT"));

        var displayDelimiter = CFG.Current.Param_Export_Delimiter;
        if (displayDelimiter == "\t")
        {
            displayDelimiter = "\\t";
        }

        // Delimiter
        UIHelper.SimpleHeader(
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

    #region Utils
    private void TryWriteFile(string path, string text)
    {
        try
        {
            File.WriteAllText(path, text);
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Write_File_FAIL", path), e);
        }

        Smithbox.Log<ParamDataTransferTool>(
            LOC.Get("PARAM_DataTransfer_Write_file_PASS", path));
    }

    private string TryReadFile(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Smithbox.LogError<ParamDataTransferTool>(
                LOC.Get("PARAM_DataTransfer_Write_File_FAIL", path), e);

            return null;
        }
    }
    #endregion
}

public enum CsvExportType
{
    [Display(Name = "PARAM_ENUM_CsvExportType_All_Params")]
    AllParams,

    [Display(Name = "PARAM_ENUM_CsvExportType_Modified_Params")]
    ModifiedParams,

    [Display(Name = "PARAM_ENUM_CsvExportType_Selected_Param")]
    SelectedParam,

    [Display(Name = "PARAM_ENUM_CsvExportType_Modified_Rows")]
    ModifiedRows,

    [Display(Name = "PARAM_ENUM_CsvExportType_Selected_Rows")]
    SelectedRows
}

public enum CsvImportType
{
    [Display(Name = "PARAM_ENUM_CsvImportType_All_Fields")]
    AllFields,
    [Display(Name = "PARAM_ENUM_CsvImportType_Row_Name")]
    RowName,
    [Display(Name = "PARAM_ENUM_CsvImportType_Specific_Field")]
    SpecificField
}

public enum ImportSourceType
{
    UserInput,
    File
}