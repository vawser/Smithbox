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

    public void ImportDelta(DeltaImportEntry entry)
    {
        _ = ImportDeltaAsync(entry);
    }

    private async Task ImportDeltaAsync(DeltaImportEntry entry)
    {
        Patcher.ImportProgressModal.DisplayModal = true;
        Patcher.ImportProgressModal.InitialLayout = false;

        await Task.Yield();

        try
        {
            var success = HandleParamImport(entry.Delta);

            TaskLogs.AddInfo($"Finished '{entry.Filename}' param delta import.");

            Patcher.Project.Handler.ParamData.PrimaryBank.RefreshPrimaryDiffCaches(true);
        }
        catch (Exception ex)
        {
            TaskLogs.AddError($"'{entry.Filename}' param delta import failed.", ex);
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
            processed++;

            Patcher.ImportProgressModal.ReportProgress?.Invoke(new()
            {
                PhaseLabel = "Processing",
                StepLabel = $"{curParam.Key}",
                Percent = processed / (float)total
            });

            var pDelta = patch.Params.FirstOrDefault(
                e => e.Name == curParam.Key);

            if (pDelta == null)
                continue;

            var param = curParam.Value;

            // Store this to use in the Row Import section as a template for additions.
            var srcRow = param.Rows.First();

            var vanillaParam = vanillaBank.Params.FirstOrDefault(e => e.Key == curParam.Key);

            // Swap orientation to delta first now as the delta is the 'truth'
            foreach (var rDelta in pDelta.Rows)
            {
                HandleRowImport(curParam.Key, param, vanillaParam.Value, srcRow, rDelta);
            }

            //Thread.Sleep(1000);
        }

        return true;
    }

    public void HandleRowImport(string paramName, Param srcParam, Param vanillaParam, Param.Row srcRow, RowDelta rowDelta)
    {
        var addRows = CFG.Current.ParamEditor_DeltaPatcher_Import_Added_Rows;
        var modRows = CFG.Current.ParamEditor_DeltaPatcher_Import_Modified_Rows;
        var delRows = CFG.Current.ParamEditor_DeltaPatcher_Import_Deleted_Rows;

        var restrictRowAdd = CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Add;
        var restrictRowMod = CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Modify;

        HashSet<int> vanillaDiffCache = Patcher.Project.Handler.ParamData.PrimaryBank.GetVanillaDiffRows(paramName);
        var diffVanilla = vanillaDiffCache.Contains(rowDelta.ID);

        if (addRows &&
            rowDelta.State is RowDeltaState.Added)
        {
            var newRow = new Param.Row(srcRow);

            newRow.ID = rowDelta.ID;

            // Apply the fields from the delta to the new row
            HandleFieldImport(newRow, rowDelta);

            var insertRow = srcParam.Rows.FirstOrDefault(e => e.ID == rowDelta.ID);
            if (insertRow != null)
            {
                if (!restrictRowAdd)
                {
                    var insertIndex = srcParam.Rows.ToList().IndexOf(insertRow);
                    srcParam.InsertRow(insertIndex, newRow);
                }
            }
            else
            {
                srcParam.AddRow(newRow);
            }
        }
        else if (rowDelta.State is RowDeltaState.Deleted or RowDeltaState.Modified)
        {
            var curRowID = 0;
            int internalIndex = 0;

            Param.Row rowToDelete = null;

            foreach (var row in srcParam.Rows)
            {
                // Handle the indexing of rows with the same ID
                if (row.ID == curRowID)
                {
                    internalIndex++;
                }
                else
                {
                    internalIndex = 0;
                }

                if (rowDelta.ID == row.ID && rowDelta.Index == internalIndex)
                {
                    if (modRows &&
                        rowDelta.State is RowDeltaState.Modified)
                    {
                        var proceed = true;

                        // If row modification is restricted, and this row is already modified, then ignore.
                        if (restrictRowMod && diffVanilla)
                        {
                            proceed = false;
                        }

                        if (proceed)
                        {
                            HandleFieldImport(row, rowDelta);
                        }
                    }
                    else if (delRows &&
                        rowDelta.State is RowDeltaState.Deleted)
                    {
                        rowToDelete = row;
                    }
                }

                curRowID = row.ID;
            }

            if (rowToDelete != null)
            {
                srcParam.RemoveRow(rowToDelete);
            }
        }
    }

    public void HandleFieldImport(Param.Row curRow, RowDelta rowDelta)
    {
        foreach (var field in rowDelta.Fields)
        {
            var curFieldDef = curRow.Def.Fields.FirstOrDefault(e => e.InternalName == field.Field);

            if (curFieldDef == null)
                continue;

            var curCol = curRow.Columns.FirstOrDefault(e => e.Def == curFieldDef);

            if (curCol == null)
                continue;

            // We have to re-cast the string here to the correct type
            // since the SetValue method assumes an object, so passing a string fails
            switch (curFieldDef.DisplayType)
            {
                case PARAMDEF.DefType.s8:
                    if (sbyte.TryParse(field.Value, out var sbyteVal))
                    {
                        curCol.SetValue(curRow, sbyteVal);
                    }
                    break;
                case PARAMDEF.DefType.s16:
                    if (short.TryParse(field.Value, out var shortVal))
                    {
                        curCol.SetValue(curRow, shortVal);
                    }
                    break;
                case PARAMDEF.DefType.s32:
                case PARAMDEF.DefType.b32:
                    if (int.TryParse(field.Value, out var intVal))
                    {
                        curCol.SetValue(curRow, intVal);
                    }
                    break;
                case PARAMDEF.DefType.f32:
                case PARAMDEF.DefType.angle32:
                    if (float.TryParse(field.Value, out var floatVal))
                    {
                        curCol.SetValue(curRow, floatVal);
                    }
                    break;
                case PARAMDEF.DefType.f64:
                    if (double.TryParse(field.Value, out var doubleVal))
                    {
                        curCol.SetValue(curRow, doubleVal);
                    }
                    break;
                case PARAMDEF.DefType.u8:
                case PARAMDEF.DefType.dummy8:
                    if (curCol.Def.ArrayLength > 1)
                    {
                        byte[] val = ParamUtils.Dummy8Read(field.Value, curCol.Def.ArrayLength);
                        curCol.SetValue(curRow, val);
                    }
                    else
                    {
                        if (byte.TryParse(field.Value, out var byteVal))
                        {
                            curCol.SetValue(curRow, byteVal);
                        }
                    }
                    break;
                case PARAMDEF.DefType.u16:
                    if (ushort.TryParse(field.Value, out var ushortVal))
                    {
                        curCol.SetValue(curRow, ushortVal);
                    }
                    break;
                case PARAMDEF.DefType.u32:
                    if (uint.TryParse(field.Value, out var uintVal))
                    {
                        curCol.SetValue(curRow, uintVal);
                    }
                    break;
                case PARAMDEF.DefType.fixstr:
                case PARAMDEF.DefType.fixstrW:
                    curCol.SetValue(curRow, field.Value);
                    break;
            }
        }
    }
}
