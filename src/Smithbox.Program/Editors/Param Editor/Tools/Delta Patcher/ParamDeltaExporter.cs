using Andre.Formats;
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
            Smithbox.LogError(this, LOC.Get("PARAM_DeltaPatcher_Exporter_Filename_Empty"));
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
                LOC.Get("PARAM_DeltaPatcher_Exporter_Overwrite_Patch", writePath),
                LOC.Get("SYS_Warning_Header"),
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
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_DeltaPatcher_Exporter_Delta_Failed"), ex);
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

        var selectedRows = Patcher.View.Selection.GetSelectedRows();

        foreach (var primaryParam in primaryBank.Params)
        {
            processed++;

            Patcher.ExportProgressModal.ReportProgress?.Invoke(new()
            {
                PhaseLabel = LOC.Get("PARAM_DeltaPatcher_Phase_Processing"),
                StepLabel = $"{primaryParam.Key}",
                Percent = processed / (float)total
            });

            if (!vanillaBank.Params.ContainsKey(primaryParam.Key))
                continue;

            var isTableParam = ParamRowGroups.IsTableParam(Patcher.Project, primaryParam.Key);

            // Skip indexed params if the option is enabled
            if (CFG.Current.ParamEditor.ParamEditor_DeltaPatcher_Export_Ignore_Indexed_Rows && isTableParam)
                continue;

            if (Patcher.Selection.CurrentParamMode is DeltaExportMode.Selected)
            {
                if (primaryParam.Key != Patcher.Project.Handler.ParamEditor.ViewHandler.ActiveView.Selection.GetActiveParam())
                {
                    continue;
                }
            }

            var paramDelta = new ParamDelta();
            paramDelta.Name = primaryParam.Key;

            var vanillaParam = vanillaBank.Params[primaryParam.Key];

            if (isTableParam)
            {
                paramDelta.Groups = Patcher.Selection.CurrentRowMode switch
                {
                    DeltaSelectionMode.Modified => ParamRowGroups.DiffGroups(primaryParam.Value, vanillaParam),
                    DeltaSelectionMode.Selected => HandleGroups(primaryParam.Value, vanillaParam,
                        selectedRows.Where(r => r.GetParentParam() == primaryParam.Value).Select(r => r.ID).ToHashSet()),
                    DeltaSelectionMode.All => HandleGroups(primaryParam.Value, vanillaParam, null),
                    _ => new List<RowGroupDelta>()
                };
            }
            else
            {
                paramDelta.Rows = Patcher.Selection.CurrentRowMode switch
                {
                    DeltaSelectionMode.Modified => HandleRows(primaryParam.Value, vanillaParam),
                    DeltaSelectionMode.Selected => HandleSelectedRows(primaryParam.Value, selectedRows),
                    DeltaSelectionMode.All => HandleSelectedRows(primaryParam.Value, null),
                    _ => new List<RowDelta>()
                };
            }

            if (paramDelta.Rows.Count > 0 || paramDelta.Groups.Count > 0)
            {
                patch.Params.Add(paramDelta);
            }
        }

        return patch;
    }

    #region Row Table Groups
    /// <summary>
    /// Whole groups for the Selected and All modes: the groups of <paramref name="ids"/>, or every group if null
    /// </summary>
    public List<RowGroupDelta> HandleGroups(Param primaryParam, Param vanillaParam, HashSet<int> ids)
    {
        var result = new List<RowGroupDelta>();
        var vanillaIds = vanillaParam.Rows.Select(r => r.ID).ToHashSet();

        foreach (var (id, rows) in ParamRowGroups.Build(primaryParam))
        {
            if (ids != null && !ids.Contains(id))
                continue;

            var state = vanillaIds.Contains(id) ? RowDeltaState.Modified : RowDeltaState.Added;
            result.Add(ParamRowGroups.SerializeGroup(id, rows, state));
        }

        return result;
    }
    #endregion

    #region Modified Export
    /// <summary>
    /// Rows that differ from vanilla, keyed by (ID, Index). A row with no vanilla row at its key is Added, and a
    /// vanilla row with no project row at its key is Deleted.
    /// </summary>
    public List<RowDelta> HandleRows(Param primaryParam, Param vanillaParam)
    {
        var rowDeltas = new List<RowDelta>();

        var primaryRows = ParamRowGroups.BuildPositionMap(primaryParam);
        var vanillaRows = ParamRowGroups.BuildPositionMap(vanillaParam);

        // Built from param order, with no removals, so this enumerates in param order
        foreach (var (key, row) in primaryRows)
        {
            if (vanillaRows.TryGetValue(key, out var vanillaRow))
            {
                var fieldDeltas = HandleFields(row, vanillaRow);

                if (fieldDeltas.Count > 0)
                {
                    rowDeltas.Add(new RowDelta
                    {
                        ID = row.ID,
                        Index = key.Index,
                        Name = row.Name,
                        State = RowDeltaState.Modified,
                        Fields = fieldDeltas
                    });
                }
            }
            else
            {
                rowDeltas.Add(ParamRowGroups.SerializeRow(row, key.Index, RowDeltaState.Added));
            }
        }

        foreach (var (key, vanillaRow) in vanillaRows)
        {
            if (primaryRows.ContainsKey(key))
                continue;

            rowDeltas.Add(new RowDelta
            {
                ID = key.ID,
                Index = key.Index,
                Name = vanillaRow.Name,
                State = RowDeltaState.Deleted
            });
        }

        return rowDeltas;
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

            var value = ParamRowGroups.SerializeValue(primaryCol, primaryRow);

            if (value != ParamRowGroups.SerializeValue(vanillaCol, vanillaRow))
            {
                fieldDeltas.Add(new FieldDelta
                {
                    Field = primaryCol.Def.InternalName,
                    Value = value
                });
            }
        }

        return fieldDeltas;
    }
    #endregion

    #region Selected / All Export
    /// <summary>
    /// Full rows, as Added, for the rows in <paramref name="rows"/> (every row if null)
    /// </summary>
    public List<RowDelta> HandleSelectedRows(Param primaryParam, IEnumerable<Param.Row> rows)
    {
        var rowDeltas = new List<RowDelta>();
        var included = rows?.ToHashSet();

        foreach (var (key, row) in ParamRowGroups.BuildPositionMap(primaryParam))
        {
            if (included != null && !included.Contains(row))
                continue;

            rowDeltas.Add(ParamRowGroups.SerializeRow(row, key.Index, RowDeltaState.Added));
        }

        return rowDeltas;
    }
    #endregion
}
