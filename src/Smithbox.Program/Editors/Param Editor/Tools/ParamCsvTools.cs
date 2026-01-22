using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid.MetalBindings;

namespace StudioCore.Editors.ParamEditor;


public static class ParamCsvTools
{
    private static string ExportPath = null;
    private static string ImportPath = null;

    private static ParamUpgradeRowGetType RowType = ParamUpgradeRowGetType.AllRows;
    private static string SpecificFieldName = "";

    public static void ExportMenu(ParamView curView)
    {
        var primaryBank = curView.Project.Handler.ParamData.PrimaryBank;

        DelimiterInputText();

        if (ImGui.BeginMenu("All rows"))
        {
            CsvExportDisplay(curView.Editor, ParamUpgradeRowGetType.AllRows);
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

                if(dialog)
                {
                    ExportPath = path;
                    ExportEntireParam(curView);
                }
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Modified rows", primaryBank.GetVanillaDiffRows(curView.Selection.GetActiveParam()).Any()))
        {
            CsvExportDisplay(curView.Editor, ParamUpgradeRowGetType.ModifiedRows);
            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("Selected rows", curView.Selection.RowSelectionExists()))
        {
            CsvExportDisplay(curView.Editor, ParamUpgradeRowGetType.SelectedRows);
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
                    ExportAllParams(curView);
                }
            }

            if (ImGui.MenuItem("Export all modified params to file"))
            {
                var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                if (dialog)
                {
                    ExportPath = path;
                    ExportAllModifiedParams(curView);
                }
            }

            ImGui.EndMenu();
        }
    }

    private static void ExportEntireParam(ParamView curView)
    {
        var primaryBank = curView.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = primaryBank.Params[curView.Selection.GetActiveParam()].Rows;

        var writePath = Path.Combine(ExportPath, $"{curView.Selection.GetActiveParam()}.csv");

        var csvString = ParamIO.GenerateCSV(
            curView.Project,
            rows,
            primaryBank.Params[curView.Selection.GetActiveParam()],
            delimiter[0]);

        TryWriteFile(writePath, csvString);
    }

    private static void ExportAllParams(ParamView curView)
    {
        var primaryBank = curView.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        foreach (KeyValuePair<string, Param> param in primaryBank.Params)
        {
            IReadOnlyList<Param.Row> rows = param.Value.Rows;

            var writePath = Path.Combine(ExportPath, $"{param.Key}.csv");

            var csvString = ParamIO.GenerateCSV(
                curView.Project,
                rows,
                param.Value,
                delimiter[0]);

            TryWriteFile(writePath, csvString);
        }
    }

    private static void ExportAllModifiedParams(ParamView curView)
    {
        var primaryBank = curView.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        foreach (KeyValuePair<string, Param> param in primaryBank.Params)
        {
            var result = primaryBank.GetVanillaDiffRows(param.Key);

            if (result.Count > 0)
            {
                IReadOnlyList<Param.Row> rows = param.Value.Rows;

                var writePath = Path.Combine(ExportPath, $"{param.Key}.csv");

                var csvString = ParamIO.GenerateCSV(
                    curView.Project,
                    rows,
                    param.Value,
                    delimiter[0]);

                TryWriteFile(writePath, csvString);
            }
        }
    }

    public static void CsvExportDisplay(ParamEditorScreen editor, ParamUpgradeRowGetType rowType)
    {
        var activeView = editor.ViewHandler.ActiveView;
        var primaryBank = editor.Project.Handler.ParamData.PrimaryBank;
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

                foreach (PARAMDEF.Field field in primaryBank.Params[activeView.Selection.GetActiveParam()].AppliedParamdef.Fields)
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
                    ExportAllFields(editor, RowType);
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
                        ExportNameField(editor, RowType);
                    }
                }

                foreach (PARAMDEF.Field field in primaryBank.Params[activeView.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        SpecificFieldName = field.InternalName;
                        RowType = rowType;

                        var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                        if (dialog)
                        {
                            ExportPath = path;
                            ExportSpecificField(editor, RowType);
                        }
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    private static void ExportAllFields(ParamEditorScreen editor, ParamUpgradeRowGetType rowType)
    {
        var activeView = editor.ViewHandler.ActiveView;
        var primaryBank = editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = CsvExportGetRows(editor, rowType);

        var outputString = ParamIO.GenerateCSV(editor.Project, rows, primaryBank.Params[activeView.Selection.GetActiveParam()], delimiter[0]);

        var writePath = Path.Combine(ExportPath, $"{activeView.Selection.GetActiveParam()}.csv");

        TryWriteFile(writePath, outputString);
    }

    private static void ExportNameField(ParamEditorScreen editor, ParamUpgradeRowGetType rowType)
    {
        var activeView = editor.ViewHandler.ActiveView;
        var primaryBank = editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = CsvExportGetRows(editor, rowType);

        var outputString = ParamIO.GenerateSingleCSV(rows, primaryBank.Params[activeView.Selection.GetActiveParam()], "Name", delimiter[0]);

        var writePath = Path.Combine(ExportPath, $"{activeView.Selection.GetActiveParam()}.csv");

        TryWriteFile(writePath, outputString);
    }

    private static void ExportSpecificField(ParamEditorScreen editor, ParamUpgradeRowGetType rowType)
    {
        var activeView = editor.ViewHandler.ActiveView;
        var primaryBank = editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        IReadOnlyList<Param.Row> rows = CsvExportGetRows(editor, rowType);

        var outputString = ParamIO.GenerateSingleCSV(rows, primaryBank.Params[activeView.Selection.GetActiveParam()], SpecificFieldName, delimiter[0]);

        var writePath = Path.Combine(ExportPath, $"{activeView.Selection.GetActiveParam()}.csv");

        TryWriteFile(writePath, outputString);
    }

    public static IReadOnlyList<Param.Row> CsvExportGetRows(ParamEditorScreen editor, ParamUpgradeRowGetType rowType)
    {
        var activeView = editor.ViewHandler.ActiveView;
        var primaryBank = editor.Project.Handler.ParamData.PrimaryBank;

        IReadOnlyList<Param.Row> rows;

        var activeParam = activeView.Selection.GetActiveParam();

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
            rows = activeView.Selection.GetSelectedRows();
        }
        else
        {
            throw new NotSupportedException();
        }

        return rows;
    }

    public static void ImportMenu(ParamView curView)
    {
        var primaryBank = curView.Editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        DelimiterInputText();

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
            foreach (PARAMDEF.Field field in primaryBank.Params[curView.Selection.GetActiveParam()].AppliedParamdef.Fields)
            {
                if (ImGui.MenuItem(field.InternalName))
                {
                    EditorCommandQueue.AddCommand($@"param/menu/massEditSingleCSVImport/{field.InternalName}");
                }
            }

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu("From file...", curView.Selection.ActiveParamExists()))
        {
            if (ImGui.MenuItem("All fields"))
            {
                var dialog = PlatformUtils.Instance.OpenFileDialog("Select File", out var path);

                if (dialog)
                {
                    ImportPath = path;
                    ImportAllFields(curView, ImportPath);
                }
            }
            if (ImGui.MenuItem("Row Name"))
            {
                var dialog = PlatformUtils.Instance.OpenFileDialog("Select File", out var path);

                if (dialog)
                {
                    ImportPath = path;
                    ImportSpecificField(curView, ImportPath, "Name");
                }
            }

            if (ImGui.BeginMenu("Specific Field"))
            {
                foreach (PARAMDEF.Field field in primaryBank.Params[curView.Selection.GetActiveParam()].AppliedParamdef.Fields)
                {
                    if (ImGui.MenuItem(field.InternalName))
                    {
                        SpecificFieldName = field.InternalName;

                        var dialog = PlatformUtils.Instance.OpenFileDialog("Select File", out var path);

                        if (dialog)
                        {
                            ImportPath = path;
                            ImportSpecificField(curView, ImportPath, SpecificFieldName);
                        }
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    private static void ImportAllFields(ParamView curView, string csvPath)
    {
        var primaryBank = curView.Editor.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        var csvString = TryReadFile(csvPath);

        (var result, CompoundAction action) = ParamIO.ApplyCSV(
            curView.Project,
            primaryBank,
            csvString,
            curView.Selection.GetActiveParam(),
            false,
            false,
            delimiter[0]);

        if (action != null)
        {
            if (action.HasActions)
            {
                curView.Editor.ActionManager.ExecuteAction(action);
            }

            curView.Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
        }
        else
        {
            TaskLogs.AddError($"Failed to import CSV: {result}");
        }
    }

    private static void ImportSpecificField(ParamView curView, string csvPath, string internalName)
    {
        var primaryBank = curView.Project.Handler.ParamData.PrimaryBank;
        var delimiter = CFG.Current.Param_Export_Delimiter;

        var csvString = TryReadFile(csvPath);

        (var result, CompoundAction action) = ParamIO.ApplySingleCSV(
            curView.Project,
            primaryBank,
            csvString,
            curView.Selection.GetActiveParam(),
            internalName,
            delimiter[0],
            false);

        if (action != null)
        {
            curView.Editor.ActionManager.ExecuteAction(action);

            curView.Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();
        }
        else
        {
            TaskLogs.AddError($"Failed to import CSV: {result}");
        }
    }

    private static void DelimiterInputText()
    {
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
    }

    private static void TryWriteFile(string path, string text)
    {
        if (Path.Exists(path))
        {
            TaskLogs.AddError("");
        }

        try
        {
            File.WriteAllText(path, text);
        }
        catch (Exception e)
        {
            TaskLogs.AddError($"Failed to write file for CSV: {path}.", e);
        }
    }

    private static string TryReadFile(string path)
    {
        try
        {
            return File.ReadAllText(path);
        }
        catch (Exception e)
        {
            TaskLogs.AddError($"Failed to read file for CSV: {path}.", e);

            return null;
        }
    }
}

public enum CsvHandlingType
{
    AllFields,
    RowName,
    SpecificField
}

public enum CsvFileHandlingType
{
    Entire,
    All,
    AllModified
}