using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Andre.Formats;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Handles importing param deltas.
/// </summary>
public class ParamDeltaImporter
{
    private ParamDeltaPatcher Patcher;

    public ParamDeltaImporter(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }

    public void ImportDelta(string filename, ParamDeltaPatch delta)
    {
        _ = ImportDeltaAsync(filename, delta);
    }

    private async Task ImportDeltaAsync(string filename, ParamDeltaPatch delta)
    {
        Patcher.ImportProgressModal.DisplayModal = true;
        Patcher.ImportProgressModal.InitialLayout = false;

        await Task.Yield();

        try
        {
            var success = HandleParamImport(delta);

            Smithbox.Log(this, LOC.Get("PARAM_DeltaPatcher_Importer_Finished_Import", filename));

            // Re-diff everything on the next frame (the import may not go through actions for every change)
            Patcher.Project.Handler.ParamData.RequestFullDiffRefresh(false);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_DeltaPatcher_Importer_Failed_Import", filename), ex);
        }
        finally
        {
            Patcher.ImportProgressModal.DisplayModal = false;
        }

        return;
    }

    public bool HandleParamImport(ParamDeltaPatch patch)
    {
        var primaryBank = Patcher.Project.Handler.ParamData.PrimaryBank;
        var vanillaBank = Patcher.Project.Handler.ParamData.VanillaBank;

        int total = patch.Params.Count;
        int processed = 0;

        foreach (var curParam in primaryBank.Params)
        {
            var pDelta = patch.Params.FirstOrDefault(
                e => e.Name == curParam.Key);

            if (pDelta == null)
                continue;

            processed++;

            Patcher.ImportProgressModal.ReportProgress?.Invoke(new()
            {
                PhaseLabel = LOC.Get("PARAM_DeltaPatcher_Phase_Processing"),
                StepLabel = $"{curParam.Key}",
                Percent = processed / (float)total
            });

            var param = curParam.Value;

            if (param.Rows.Count == 0)
                continue;

            vanillaBank.Params.TryGetValue(curParam.Key, out var vanillaParam);

            // Work on a copy, so converting older rows to groups doesn't change the caller's patch
            var work = new ParamDelta
            {
                Name = pDelta.Name,
                Rows = pDelta.Rows.Select(ParamRowGroups.CloneRow).ToList(),
                Groups = pDelta.Groups.Select(ParamRowGroups.CloneGroup).ToList()
            };

            // Patches from before groups existed hold table params as rows, numbered in the project they came
            // from: apply them to the project's groups by position
            if (ParamRowGroups.IsTableParam(Patcher.Project, curParam.Key))
            {
                ParamRowGroups.ConvertPositionRows(work, param);
            }

            var changed = false;

            changed |= HandleGroupImport(curParam.Key, param, vanillaParam, work.Groups);
            changed |= HandleRowsImport(curParam.Key, param, work.Rows);

            if (changed)
            {
                ParamDirtyTracker.MarkDirty(param);
            }
        }

        return true;
    }

    #region Row Table Groups
    /// <summary>
    /// Each group replaces the project's group of the same ID in one step, honouring the Added / Modified /
    /// Deleted import options
    /// </summary>
    public bool HandleGroupImport(string paramName, Param param, Param vanillaParam, List<RowGroupDelta> groups)
    {
        if (groups.Count == 0)
            return false;

        var addRows = CFG.Current.ParamEditor_DeltaPatcher_Import_Added_Rows;
        var modRows = CFG.Current.ParamEditor_DeltaPatcher_Import_Modified_Rows;
        var delRows = CFG.Current.ParamEditor_DeltaPatcher_Import_Deleted_Rows;

        var restrictRowAdd = CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Add;
        var restrictRowMod = CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Modify;
        var allowOverwrite = CFG.Current.ParamEditor_DeltaPatcher_Import_Allow_Row_Overwrite;

        var fallbackTemplate = vanillaParam?.Rows.FirstOrDefault() ?? param.Rows.FirstOrDefault();
        var changed = false;

        foreach (var group in groups)
        {
            var oldRows = ParamRowGroups.GetGroup(param, group.ID);

            if (group.State == RowDeltaState.Deleted)
            {
                if (delRows && oldRows.Count > 0)
                {
                    ParamRowGroups.ReplaceGroup(param, group.ID, new List<Param.Row>());
                    changed = true;
                }

                continue;
            }

            var state = Patcher.ImportMode is DeltaImportMode.Simple ? RowDeltaState.Added : group.State;

            if (state == RowDeltaState.Added)
            {
                if (!addRows)
                    continue;

                // A group replaces the existing one rather than being added beside it (two copies of a
                // group would merge into one group of doubled rows)
                if (oldRows.Count > 0 && restrictRowAdd && !allowOverwrite)
                    continue;
            }
            else
            {
                if (!modRows)
                    continue;

                if (restrictRowMod && oldRows.Count > 0)
                {
                    var vanillaRows = ParamRowGroups.GetGroup(vanillaParam, group.ID);

                    if (!ParamRowGroups.GroupEquals(oldRows, vanillaRows))
                        continue;
                }
            }

            var templates = oldRows.Count > 0 ? oldRows : ParamRowGroups.GetGroup(vanillaParam, group.ID);
            var newRows = ParamRowGroups.CreateGroupRows(param, group, templates, fallbackTemplate);

            ParamRowGroups.ReplaceGroup(param, group.ID, newRows);
            changed = true;
        }

        return changed;
    }
    #endregion

    #region Rows
    /// <summary>
    /// Rows keyed by (ID, Index). Every key is matched to its row before anything changes, so a delete or insert
    /// can't move a later delta onto another row with the same ID.
    /// </summary>
    public bool HandleRowsImport(string paramName, Param srcParam, List<RowDelta> rowDeltas)
    {
        if (rowDeltas.Count == 0)
            return false;

        var addRows = CFG.Current.ParamEditor_DeltaPatcher_Import_Added_Rows;
        var modRows = CFG.Current.ParamEditor_DeltaPatcher_Import_Modified_Rows;
        var delRows = CFG.Current.ParamEditor_DeltaPatcher_Import_Deleted_Rows;

        var restrictRowAdd = CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Add;
        var restrictRowMod = CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Modify;
        var allowOverwrite = CFG.Current.ParamEditor_DeltaPatcher_Import_Allow_Row_Overwrite;

        // Keyed by row instance, so it tells apart rows that share an ID
        HashSet<Param.Row> vanillaDiffCache = Patcher.Project.Handler.ParamData.PrimaryBank.GetVanillaDiffRows(paramName);

        // Template for added rows
        var srcRow = srcParam.Rows.First();

        var positions = ParamRowGroups.BuildPositionMap(srcParam);
        var targets = rowDeltas
            .Select(rd => (Delta: rd, Target: positions.GetValueOrDefault((rd.ID, rd.Index))))
            .ToList();

        var toDelete = new List<Param.Row>();
        var changed = false;

        foreach (var (rowDelta, target) in targets)
        {
            var rowStateIsAdded = rowDelta.State is RowDeltaState.Added;

            if (Patcher.ImportMode is DeltaImportMode.Simple)
                rowStateIsAdded = true;

            if (rowStateIsAdded)
            {
                if (!addRows)
                    continue;

                var newRow = new Param.Row(target ?? srcRow);

                newRow.ID = rowDelta.ID;
                newRow.Name = rowDelta.Name;

                // Apply the fields from the delta to the new row
                HandleFieldImport(newRow, rowDelta);

                var idExists = target != null || srcParam.Rows.Any(e => e.ID == rowDelta.ID);

                if (allowOverwrite && target != null)
                {
                    var insertIndex = srcParam.IndexOfRow(target);

                    srcParam.InsertRow(insertIndex, newRow);
                    srcParam.RemoveRow(target);
                    changed = true;
                }
                else if (!idExists || !restrictRowAdd)
                {
                    ParamRowGroups.InsertRowAtPosition(srcParam, newRow, rowDelta.ID, rowDelta.Index);
                    changed = true;
                }
            }
            else if (rowDelta.State is RowDeltaState.Modified)
            {
                if (!modRows || target == null)
                    continue;

                // If row modification is restricted, and this row is already modified, then ignore.
                if (restrictRowMod && vanillaDiffCache.Contains(target))
                    continue;

                HandleFieldImport(target, rowDelta);
                changed = true;
            }
            else if (rowDelta.State is RowDeltaState.Deleted)
            {
                if (delRows && target != null)
                    toDelete.Add(target);
            }
        }

        foreach (var row in toDelete)
        {
            changed |= srcParam.RemoveRow(row);
        }

        return changed;
    }

    public void HandleFieldImport(Param.Row curRow, RowDelta rowDelta)
    {
        ParamRowGroups.ApplyFields(curRow, rowDelta.Fields);
    }
    #endregion
}
