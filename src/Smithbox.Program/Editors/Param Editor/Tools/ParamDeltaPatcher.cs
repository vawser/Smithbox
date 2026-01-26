using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamDeltaPatcher
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    private DeltaBuildProgress LoadProgress;
    private Action<DeltaBuildProgress> ReportProgress;
    private readonly object _progressLock = new();

    private string ExportName = "";

    private bool DisplayProgressModal = false;
    private bool InitialLayout = false;

    public ParamDeltaPatcher(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        ReportProgress = SetProgress;
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2((windowWidth * 0.725f), 32);

        var paramData = Project.Handler.ParamData;

        if (ImGui.CollapsingHeader("Param Delta Patcher"))
        {
            if(ImGui.BeginTabBar("deltaTabs"))
            {
                if(ImGui.BeginTabItem("Import"))
                {
                    DisplayImportTab();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Export"))
                {
                    DisplayExportTab();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
    }

    public void DisplayImportTab()
    {

    }

    public void DisplayExportTab()
    {
        ImGui.Text("Filename:");
        ImGui.InputText("##inputFileName", ref ExportName, 255);

        if(ImGui.Button("Generate", DPI.StandardButtonSize))
        {
            GenerateDeltaPatch();
        }

        ImGui.SameLine();

        if (ImGui.Button("View Deltas", DPI.StandardButtonSize))
        {
            var storageDir = ProjectUtils.GetParamDeltaFolder();

            Process.Start("explorer.exe", storageDir);
        }
    }

    #region Delta Builder
    public void GenerateDeltaPatch()
    {
        if (string.IsNullOrWhiteSpace(ExportName))
        {
            TaskLogs.AddError("Filename must not be empty.");
            return;
        }

        _ = GenerateDeltaPatchAsync();
    }

    private async Task GenerateDeltaPatchAsync()
    {
        var writeDir = ProjectUtils.GetParamDeltaFolder();
        var writePath = Path.Combine(writeDir, $"{ExportName}.json");

        if (File.Exists(writePath))
        {
            var dialog = PlatformUtils.Instance.MessageBox(
                $"Do you want to overwrite this delta patch file: {writePath}",
                "Delta Patcher",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dialog is DialogResult.No)
                return;
        }

        DisplayProgressModal = true;
        InitialLayout = false;

        try
        {
            var patch = await Task.Run(BuildDeltaPatch);

            if (patch.Params.Count > 0)
            {
                WriteDeltaPatch(patch, ExportName);
                TaskLogs.AddLog($"Saved param delta: {ExportName}.json");
            }
            else
            {
                TaskLogs.AddLog("Aborted param delta as no changes were detected.");
            }
        }
        catch (Exception ex)
        {
            TaskLogs.AddError("Delta build failed", ex);
        }
        finally
        {
            DisplayProgressModal = false;
        }
    }

    public ParamDeltaPatch BuildDeltaPatch()
    {
        DisplayProgressModal = true;

        var patch = new ParamDeltaPatch();

        var primaryBank = Project.Handler.ParamData.PrimaryBank;
        var vanillaBank = Project.Handler.ParamData.VanillaBank;

        patch.ProjectType = Project.Descriptor.ProjectType;
        patch.ParamVersion = primaryBank.ParamVersion;

        int total = primaryBank.Params.Count;
        int processed = 0;

        foreach (var primaryParam in primaryBank.Params)
        {
            processed++;

            ReportProgress?.Invoke(new()
            {
                PhaseLabel = "Processing",
                StepLabel = $"{primaryParam.Key}",
                Percent = processed / (float)total
            });

            if (!vanillaBank.Params.ContainsKey(primaryParam.Key))
                continue;

            var paramDelta = new ParamDelta();
            paramDelta.Name = primaryParam.Key;

            var vanillaParam = vanillaBank.Params[primaryParam.Key];

            var rowDeltas = HandleRows(primaryParam.Value, vanillaParam);
            foreach (var entry in rowDeltas)
            {
                paramDelta.Rows.Add(entry);
            }

            if(paramDelta.Rows.Count > 0)
            {
                patch.Params.Add(paramDelta);
            }
        }

        DisplayProgressModal = false;

        return patch;
    }

    public List<RowDelta> HandleRows(Param primaryParam, Param vanillaParam)
    {
        var rowDeltas = new List<RowDelta>();

        var curRowID = 0;
        var internalIndex = 0;

        for (int i = 0; i < primaryParam.Rows.Count; i++)
        {
            Param.Row row = primaryParam.Rows[i];

            var (add, rowDelta) = HandleRowComparison(primaryParam, vanillaParam, row, ref curRowID, ref internalIndex);
            if (add)
            {
                rowDeltas.Add(rowDelta);
            }

            curRowID = row.ID;
        }

        // Determine deleted rows
        for (int i = 0; i < vanillaParam.Rows.Count; i++)
        {
            Param.Row row = vanillaParam.Rows[i];

            var primaryRow = primaryParam.Rows.FirstOrDefault(e => e.ID == row.ID);
            if(primaryRow == null)
            {
                var rowDelta = new RowDelta();
                rowDelta.ID = row.ID;
                rowDelta.Name = row.Name;
                rowDelta.State = RowDeltaState.Deleted;
            }

            // TODO: handle indexed rows
        }

        return rowDeltas;
    }

    public (bool, RowDelta) HandleRowComparison(Param primaryParam, Param vanillaParam, Param.Row row, ref int curRowID, ref int internalIndex)
    {
        var rowDelta = new RowDelta();
        rowDelta.ID = row.ID;
        rowDelta.Name = row.Name;
        rowDelta.State = RowDeltaState.Modified;

        // TODO: Handle indexed rows
        if (row.ID == curRowID && false)
        {
            var vInternalIndex = 0;

            foreach (var vRow in vanillaParam.Rows)
            {
                if (vRow.ID == row.ID)
                {
                    if (internalIndex == vInternalIndex)
                    {

                        var fieldDeltas = HandleFields(row, vRow);
                        foreach(var entry in fieldDeltas)
                        {
                            rowDelta.Fields.Add(entry);
                        }
                    }

                    vInternalIndex++;
                }
            }

            internalIndex++;
        }
        else
        {
            internalIndex = 0;

            var vanillaRow = vanillaParam.Rows.FirstOrDefault(e => e.ID == row.ID);
            if (vanillaRow != null)
            {
                var fieldDeltas = HandleFields(row, vanillaRow);
                foreach (var entry in fieldDeltas)
                {
                    rowDelta.Fields.Add(entry);
                }
            }
            else
            {
                rowDelta.State = RowDeltaState.Added;

                // If the row is unique to the project, add all fields to the delta for the row entry
                foreach (var primaryCol in row.Columns)
                {
                    var fieldDelta = new FieldDelta();

                    var curField = primaryCol.Def.InternalName;
                    var curValue = primaryCol.GetValue(row);

                    fieldDelta.Field = curField;
                    fieldDelta.Value = curValue.ToString();

                    rowDelta.Fields.Add(fieldDelta);
                }
            }
        }

        rowDelta.Index = internalIndex;

        curRowID = row.ID;

        if(rowDelta.Fields.Count > 0)
        {
            return (true, rowDelta);
        }
        else
        {
            return (false, null);
        }
    }

    public List<FieldDelta> HandleFields(Param.Row primaryRow, Param.Row vanillaRow)
    {
        var fieldDeltas = new List<FieldDelta>();

        if (primaryRow.DataEquals(vanillaRow))
        {
            return fieldDeltas;
        }

        foreach (var primaryCol in primaryRow.Columns)
        {
            var vanillaCol = vanillaRow.Columns.FirstOrDefault(e => e.Def.InternalName == primaryCol.Def.InternalName);

            if (vanillaCol == null)
                continue;

            var (add, fieldDelta) = HandleFieldComparison(primaryRow, vanillaRow, primaryCol, vanillaCol);
            if (add)
            {
                fieldDeltas.Add(fieldDelta);
            }
        }

        return fieldDeltas;
    }

    public (bool, FieldDelta) HandleFieldComparison(Param.Row primaryRow, Param.Row vanillaRow, Param.Column primaryCol, Param.Column vanillaCol)
    {
        var fieldDelta = new FieldDelta();

        var curField = primaryCol.Def.InternalName;
        var curValue = primaryCol.GetValue(primaryRow);

        fieldDelta.Field = curField;
        fieldDelta.Value = curValue.ToString();

        var vanillaValue = vanillaCol.GetValue(vanillaRow);
        if (vanillaValue.ToString() != curValue.ToString())
        {
            return (true, fieldDelta);
        }

        return (false, null);
    }

    #endregion

    #region IO
    public ParamDeltaPatch LoadDeltaPatch(string name)
    {
        var storageDir = ProjectUtils.GetParamDeltaFolder();

        var readPath = Path.Combine(storageDir, $"{ExportName}.json");

        var deltaPatch = new ParamDeltaPatch();

        if (File.Exists(readPath))
        {
            try
            {
                var filestring = File.ReadAllText(readPath);

                try
                {
                    deltaPatch = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.ParamDeltaPatch);
                }
                catch (Exception e)
                {
                    TaskLogs.AddError("Failed to deserialize delta patch", e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError("Failed to read delta patch", e);
            }
        }
        else
        {
            TaskLogs.AddError("Failed to find delta patch");
        }

        return deltaPatch;
    }

    public void WriteDeltaPatch(ParamDeltaPatch patch, string name)
    {
        if(name == "" || name == null)
        {
            TaskLogs.AddError("Failed to write delta patch as filename is empty.");
            return;
        }

        var writeDir = ProjectUtils.GetParamDeltaFolder();

        var writePath = Path.Combine(writeDir, $"{name}.json");

        if (!Directory.Exists(writeDir))
        {
            Directory.CreateDirectory(writeDir);
        }

        try
        {
            string jsonString = JsonSerializer.Serialize(patch, ParamEditorJsonSerializerContext.Default.ParamDeltaPatch);

            var fs = new FileStream(writePath, FileMode.Create);
            var data = Encoding.ASCII.GetBytes(jsonString);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Dispose();
        }
        catch (Exception ex)
        {
            TaskLogs.AddError("Failed to write delta patch", ex);
        }
    }
    #endregion

    #region Progress Modal
    public void SetProgress(DeltaBuildProgress progress)
    {
        lock (_progressLock)
        {
            LoadProgress = progress;
        }
    }

    public void DrawProgressModal()
    {
        if (!DisplayProgressModal)
            return;

        ImGui.OpenPopup("Delta Builder##deltaBuilder");

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(
            "Delta Builder##deltaBuilder",
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            DeltaBuildProgress progress;
            lock (_progressLock)
                progress = LoadProgress;

            if (!string.IsNullOrEmpty(progress.PhaseLabel))
            {
                ImGui.Text(progress.PhaseLabel);
                ImGui.Spacing();
            }

            ImGui.ProgressBar(
                Math.Clamp(progress.Percent, 0f, 1f),
                new System.Numerics.Vector2(400, 0),
                $"{(int)(progress.Percent * 100)}%"
            );

            if (!string.IsNullOrEmpty(progress.StepLabel))
            {
                ImGui.Spacing();
                ImGui.TextDisabled(progress.StepLabel);
            }

            ImGui.EndPopup();
        }
    }
    #endregion
}

public struct DeltaBuildProgress
{
    public string PhaseLabel;
    public string StepLabel;
    public float Percent;
}