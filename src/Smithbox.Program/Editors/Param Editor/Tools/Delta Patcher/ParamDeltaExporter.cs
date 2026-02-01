using Andre.Formats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Handles exporting param deltas.
/// </summary>
public class ParamDeltaExporter
{
    private ParamDeltaPatcher Patcher;

    public ParamDeltaExporter(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }

    public void GenerateDeltaPatch()
    {
        if (string.IsNullOrWhiteSpace(Patcher.Selection.ExportName))
        {
            TaskLogs.AddError("Filename must not be empty.");
            return;
        }

        _ = GenerateDeltaPatchAsync();
    }

    private async Task GenerateDeltaPatchAsync()
    {
        var writeDir = ProjectUtils.GetParamDeltaFolder();
        var writePath = Path.Combine(writeDir, $"{Patcher.Selection.ExportName}.json");

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

        Patcher.ExportProgressModal.DisplayModal = true;
        Patcher.ExportProgressModal.InitialLayout = false;

        try
        {
            var patch = await Task.Run(BuildDeltaPatch);

            // Display the export preview modal, actual save only occurs within the modal
            Patcher.ExportPreviewModal.Show(patch);

            //if (patch.Params.Count > 0)
            //{
            //    Patcher.WriteDeltaPatch(patch, Patcher.Selection.ExportName);
            //    TaskLogs.AddLog($"Saved param delta: {Patcher.Selection.ExportName}.json");

            //    Patcher.Selection.RefreshImportList();
            //}
            //else
            //{
            //    TaskLogs.AddLog("Aborted param delta as no changes were detected.");
            //}
        }
        catch (Exception ex)
        {
            TaskLogs.AddError("Delta build failed", ex);
        }
        finally
        {
            Patcher.ExportProgressModal.DisplayModal = false;
        }
    }

    public ParamDeltaPatch BuildDeltaPatch()
    {
        var patch = new ParamDeltaPatch();

        var primaryBank = Patcher.Project.Handler.ParamData.PrimaryBank;
        var vanillaBank = Patcher.Project.Handler.ParamData.VanillaBank;

        patch.ProjectType = Patcher.Project.Descriptor.ProjectType;
        patch.ParamVersion = primaryBank.ParamVersion;
        patch.Tag = Patcher.Selection.ExportFileTag;

        int total = primaryBank.Params.Count;
        int processed = 0;

        foreach (var primaryParam in primaryBank.Params)
        {
            processed++;

            Patcher.ExportProgressModal.ReportProgress?.Invoke(new()
            {
                PhaseLabel = "Processing",
                StepLabel = $"{primaryParam.Key}",
                Percent = processed / (float)total
            });

            if (!vanillaBank.Params.ContainsKey(primaryParam.Key))
                continue;

            // Skip indexed params if the option is enabled
            if (CFG.Current.ParamEditor_DeltaPatcher_Export_Ignore_Indexed_Rows)
            {
                if (Patcher.Project.Handler.ParamData.TableParamList.Params.Contains(primaryParam.Key))
                {
                    continue;
                }
            }

            if (Patcher.Selection.CurrentExportMode is DeltaExportMode.Selected)
            {
                if (primaryParam.Key != Patcher.Project.Handler.ParamEditor.ViewHandler.ActiveView.Selection.GetActiveParam())
                {
                    continue;
                }
            }

            var paramDelta = new ParamDelta();
            paramDelta.Name = primaryParam.Key;

            var vanillaParam = vanillaBank.Params[primaryParam.Key];

            List<RowDelta> rowDeltas = new List<RowDelta>();

            if (Patcher.Selection.CurrentExportMode is DeltaExportMode.Modified)
            {
                rowDeltas = HandleRows(primaryParam.Value, vanillaParam);
            }

            if (Patcher.Selection.CurrentExportMode is DeltaExportMode.Selected)
            {
                rowDeltas = HandleSelectedRows(primaryParam.Value, vanillaParam);
            }

            foreach (var entry in rowDeltas)
            {
                paramDelta.Rows.Add(entry);
            }

            if (paramDelta.Rows.Count > 0)
            {
                patch.Params.Add(paramDelta);
            }
        }

        return patch;
    }

    #region Modified Export
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
        }

        // Determine deleted rows
        for (int i = 0; i < vanillaParam.Rows.Count; i++)
        {
            Param.Row row = vanillaParam.Rows[i];

            // Indexed Row
            if (row.ID == curRowID)
            {
                // TODO: currently we don't add indexed row deletions since it is a thorny thing to detect
            }
            // Individual Row
            else
            {
                var primaryRow = primaryParam.Rows.FirstOrDefault(e => e.ID == row.ID);
                if (primaryRow == null)
                {
                    var rowDelta = new RowDelta();
                    rowDelta.ID = row.ID;
                    rowDelta.Name = row.Name;
                    rowDelta.State = RowDeltaState.Deleted;
                }
            }

            curRowID = row.ID;
        }

        return rowDeltas;
    }

    public (bool, RowDelta) HandleRowComparison(Param primaryParam, Param vanillaParam, Param.Row row, ref int curRowID, ref int internalIndex)
    {
        var rowDelta = new RowDelta();
        rowDelta.ID = row.ID;
        rowDelta.Name = row.Name;
        rowDelta.State = RowDeltaState.Modified;

        // Indexed Row
        if (row.ID == curRowID)
        {
            var vInternalIndex = 0;

            foreach (var vRow in vanillaParam.Rows)
            {
                if (vRow.ID == row.ID)
                {
                    if (!vRow.DataEquals(row))
                    {
                        if (internalIndex == vInternalIndex)
                        {
                            var fieldDeltas = HandleFields(row, vRow);
                            foreach (var entry in fieldDeltas)
                            {
                                rowDelta.Fields.Add(entry);
                            }
                        }
                    }

                    vInternalIndex++;
                }
            }

            internalIndex++;
        }
        // Individual Row
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

                    // For the dummy8 values with more than 1 byte, do this
                    if (primaryCol.Def.InternalType == "dummy8")
                    {
                        if (primaryCol.Def.ArrayLength > 1)
                        {
                            fieldDelta.Value = ParamUtils.Dummy8Write((byte[])curValue);
                        }
                    }

                    rowDelta.Fields.Add(fieldDelta);
                }
            }
        }

        rowDelta.Index = internalIndex;

        curRowID = row.ID;

        if (rowDelta.Fields.Count > 0)
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

        if (primaryCol.Def.InternalType == "dummy8")
        {
            if (primaryCol.Def.ArrayLength > 1)
            {
                fieldDelta.Value = ParamUtils.Dummy8Write((byte[])curValue);
            }
        }

        var vanillaValue = vanillaCol.GetValue(vanillaRow);
        if (vanillaValue.ToString() != curValue.ToString())
        {
            return (true, fieldDelta);
        }

        return (false, null);
    }
    #endregion

    #region Selected Export
    public List<RowDelta> HandleSelectedRows(Param primaryParam, Param vanillaParam)
    {
        var rowDeltas = new List<RowDelta>();

        var curRowID = 0;
        var internalIndex = 0;

        var selectedRows = Patcher.Editor.ViewHandler.ActiveView.Selection.GetSelectedRows();

        for (int i = 0; i < primaryParam.Rows.Count; i++)
        {
            Param.Row row = primaryParam.Rows[i];

            if (selectedRows.Contains(row))
            {
                var rowDelta = HandleSelectedRow(primaryParam, vanillaParam, row, ref curRowID, ref internalIndex);
                rowDeltas.Add(rowDelta);
            }
        }

        return rowDeltas;
    }

    public RowDelta HandleSelectedRow(Param primaryParam, Param vanillaParam, Param.Row row, ref int curRowID, ref int internalIndex)
    {
        var rowDelta = new RowDelta();
        rowDelta.ID = row.ID;
        rowDelta.Name = row.Name;
        rowDelta.State = RowDeltaState.Modified;
        rowDelta.Fields = new List<FieldDelta>();

        foreach (var primaryCol in row.Columns)
        {
            var fieldDelta = new FieldDelta();

            var curField = primaryCol.Def.InternalName;
            var curValue = primaryCol.GetValue(row);

            fieldDelta.Field = curField;
            fieldDelta.Value = curValue.ToString();

            if (primaryCol.Def.InternalType == "dummy8")
            {
                if (primaryCol.Def.ArrayLength > 1)
                {
                    fieldDelta.Value = ParamUtils.Dummy8Write((byte[])curValue);
                }
            }

            rowDelta.Fields.Add(fieldDelta);
        }

        return rowDelta;
    }

    #endregion
}
