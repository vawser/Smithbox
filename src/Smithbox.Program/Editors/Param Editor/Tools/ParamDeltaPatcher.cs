using Andre.Formats;
using Google.Protobuf.WellKnownTypes;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Keybinds;
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
using System.Threading;
using System.Threading.Tasks;
using Enum = System.Enum;

namespace StudioCore.Editors.ParamEditor;

public class ParamDeltaPatcher
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    private DeltaBuildProgress LoadProgress;
    private Action<DeltaBuildProgress> ReportProgress;
    private readonly object _progressLock = new();

    private string ExportName = "";

    private bool DisplaySelectiveImportModal = false;
    private bool DisplayExportProgressModal = false;
    private bool DisplayImportProgressModal = false;
    private bool InitialLayout = false;

    private List<DeltaImportEntry> ImportList = new();
    private DeltaImportEntry SelectedImport = null;
    private bool SelectImportEntry = false;

    private DeltaExportMode CurrentExportMode = DeltaExportMode.Modified;

    public ParamDeltaPatcher(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        ReportProgress = SetProgress;

        RefreshImportList();
    }

    #region Display
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
        UIHelper.SimpleHeader("Options", "Options to set for the delta import.");
        ImGui.Checkbox("Display All Entries", ref CFG.Current.ParamEditor_DeltaPatcher_Import_Display_All_Entries);
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            RefreshImportList();
        }
        UIHelper.Tooltip("If enabled, delta entries for all project types are displayed.");

        ImGui.Separator();

        ImGui.Checkbox("Include Modified Rows", ref CFG.Current.ParamEditor_DeltaPatcher_Import_Modified_Rows);
        UIHelper.Tooltip("If enabled, rows considered 'modified' within the delta will be applied. This means the import will modify rows within the primary bank with the same row ID and index as those in the delta.");

        ImGui.Checkbox("Include Added Rows", ref CFG.Current.ParamEditor_DeltaPatcher_Import_Added_Rows);
        UIHelper.Tooltip("If enabled, rows considered 'added' within the delta will be applied. This means the import will add these rows to the primary bank.");

        ImGui.Checkbox("Include Deleted Rows", ref CFG.Current.ParamEditor_DeltaPatcher_Import_Deleted_Rows);
        UIHelper.Tooltip("If enabled, rows considered 'delete' within the delta will be applied. This means the import will delete these rows from the primary bank.");

        ImGui.Separator();

        ImGui.Checkbox("Restrict Row Modification", ref CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Modify);
        UIHelper.Tooltip("If enabled, row modifications will only occur if the row hasn't already been modified.");

        ImGui.Checkbox("Restrict Row Addition", ref CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Add);
        UIHelper.Tooltip("If enabled, row additions will only occur if the row ID doesn't already exist in the primary bank.");

        UIHelper.SimpleHeader("Actions", "");
        if (ImGui.Button("Import", DPI.StandardButtonSize))
        {
            if (SelectedImport != null)
            {
                ImportDelta(SelectedImport);
                SelectedImport = null; // Deselect once done.
            }
            else
            {
                TaskLogs.AddError("No param delta has been selected.");
            }
        }
        UIHelper.Tooltip("Import the selected delta into the project's regulation.");

        //ImGui.SameLine();

        //if (ImGui.Button("Selective Import", DPI.StandardButtonSize))
        //{
        //    if (SelectedImport != null)
        //    {
        //        DisplaySelectiveImportModal = true;
        //    }
        //    else
        //    {
        //        TaskLogs.AddError("No param delta has been selected.");
        //    }
        //}
        //UIHelper.Tooltip("Import the selected delta into the project's regulation, via the Selective Import menu.");

        UIHelper.SimpleHeader("Entries", "");
        ImGui.BeginChild("importEntryList");
        foreach(var entry in ImportList)
        {
            var selected = entry == SelectedImport;

            var version = ParamUtils.ParseRegulationVersion(entry.Delta.ParamVersion);

            var displayName = $"{entry.Filename} [{version}]";

            if (ImGui.Selectable($"{displayName}##curEntry_{entry.Filename.GetHashCode()}", selected))
            {
                SelectedImport = entry;
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && SelectImportEntry)
            {
                SelectImportEntry = false;
                SelectedImport = entry;
            }

            if (ImGui.IsItemFocused())
            {
                if (InputManager.HasArrowSelection())
                {
                    SelectImportEntry = true;
                }
            }

            if (selected)
            {
                if (ImGui.BeginPopupContextItem($"##curEntryContext_{entry.Filename.GetHashCode()}"))
                {
                    if(ImGui.BeginMenu("Affected Params"))
                    {
                        var affectedParams = entry.Delta.Params;

                        foreach(var param in affectedParams)
                        {
                            var modCount = param.Rows.Where(e => e.State is RowDeltaState.Modified).Count();
                            var addCount = param.Rows.Where(e => e.State is RowDeltaState.Added).Count();
                            var deleteCount = param.Rows.Where(e => e.State is RowDeltaState.Deleted).Count();

                            // Technically inaccurate when not not applied directly to a fresh vanilla bank
                            if (modCount > 0)
                            {
                                ImGui.Text($"{param.Name}: will modify {modCount} rows.");
                            }
                            if (addCount > 0)
                            {
                                ImGui.Text($"{param.Name}: will add {addCount} rows.");
                            }
                            if (deleteCount > 0)
                            {
                                ImGui.Text($"{param.Name}: will delete {deleteCount} rows.");
                            }
                        }

                        ImGui.EndMenu();
                    }

                    if (ImGui.Selectable("Delete"))
                    {
                        DeleteDeltaPatch(entry.Filename);
                        RefreshImportList();

                        ImGui.CloseCurrentPopup();
                    }
                    UIHelper.Tooltip("Delete this delta file.");

                    ImGui.EndPopup();
                }
            }
        }

        ImGui.EndChild();
    }

    private void RefreshImportList()
    {
        ImportList.Clear();

        var sourceDir = ProjectUtils.GetParamDeltaFolder();

        if (Directory.Exists(sourceDir))
        {
            foreach (var file in Directory.EnumerateFiles(sourceDir))
            {
                var filename = Path.GetFileNameWithoutExtension(file);

                var entry = new DeltaImportEntry();
                entry.Filename = filename;
                entry.Delta = LoadDeltaPatch(filename);

                if (CFG.Current.ParamEditor_DeltaPatcher_Import_Display_All_Entries)
                {
                    ImportList.Add(entry);
                }
                else
                {
                    if (Project.Descriptor.ProjectType == entry.Delta.ProjectType)
                    {
                        ImportList.Add(entry);
                    }
                }
            }
        }
        else
        {
            Directory.CreateDirectory(sourceDir);
        }
    }

    public void DisplayExportTab()
    {
        UIHelper.SimpleHeader("Filename", "The name of the delta file.");
        DPI.ApplyInputWidth();
        ImGui.InputText("##inputFileName", ref ExportName, 255);

        UIHelper.SimpleHeader("Options", "Options to set for the delta builder.");

        ImGui.Text("Export Mode");
        DPI.ApplyInputWidth();
        if (ImGui.BeginCombo("##inputValue", CurrentExportMode.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(DeltaExportMode)))
            {
                var type = (DeltaExportMode)entry;

                if (ImGui.Selectable(type.GetDisplayName()))
                {
                    CurrentExportMode = (DeltaExportMode)entry;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.Checkbox("Ignore Indexed Params", ref CFG.Current.ParamEditor_DeltaPatcher_Export_Ignore_Indexed_Rows);
        UIHelper.Tooltip("If enabled, indexed params where the rows depending on row index as well as ID will be ignored when producing the delta.");

        UIHelper.SimpleHeader("Actions", "");
        if (ImGui.Button("Generate", DPI.StandardButtonSize))
        {
            GenerateDeltaPatch();
        }
        UIHelper.Tooltip("Generate a delta file that represents the changes made within this regulation compared to vanilla.");

        ImGui.SameLine();

        if (ImGui.Button("View Deltas", DPI.StandardButtonSize))
        {
            var storageDir = ProjectUtils.GetParamDeltaFolder();

            Process.Start("explorer.exe", storageDir);
        }
    }

    #endregion

    #region Delta Import
    public void ImportDelta(DeltaImportEntry entry)
    {
        _ = ImportDeltaAsync(entry);
    }

    private async Task ImportDeltaAsync(DeltaImportEntry entry)
    {
        DisplayImportProgressModal = true;
        InitialLayout = false;

        await Task.Yield();

        try
        {
            var success = HandleParamImport(entry.Delta);

            TaskLogs.AddInfo($"Finished '{entry.Filename}' param delta import.");

            Project.Handler.ParamData.PrimaryBank.RefreshPrimaryDiffCaches(true);
        }
        catch (Exception ex)
        {
            TaskLogs.AddError($"'{entry.Filename}' param delta import failed.", ex);
        }
        finally
        {
            DisplayImportProgressModal = false;
        }

        return;
    }

    public bool HandleParamImport(ParamDeltaPatch patch)
    {
        var primaryBank = Project.Handler.ParamData.PrimaryBank;
        var vanillaBank = Project.Handler.ParamData.VanillaBank;

        int total = patch.Params.Count;
        int processed = 0;

        foreach (var curParam in primaryBank.Params)
        {
            processed++;

            ReportProgress?.Invoke(new()
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

        HashSet<int> vanillaDiffCache = Editor.Project.Handler.ParamData.PrimaryBank.GetVanillaDiffRows(paramName);
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
        else if(rowDelta.State is RowDeltaState.Deleted or RowDeltaState.Modified)
        {
            var curRowID = 0;
            int internalIndex = 0;

            Param.Row rowToDelete = null;

            foreach(var row in srcParam.Rows)
            {
                // Handle the indexing of rows with the same ID
                if(row.ID == curRowID)
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
                        if(restrictRowMod && diffVanilla)
                        {
                            proceed = false;
                        }

                        if (proceed)
                        {
                            HandleFieldImport(row, rowDelta);
                        }
                    }
                    else if(delRows && 
                        rowDelta.State is RowDeltaState.Deleted)
                    {
                        rowToDelete = row;
                    }
                }

                curRowID = row.ID;
            }

            if(rowToDelete != null)
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
                    if(sbyte.TryParse(field.Value, out var sbyteVal))
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

    public void DrawSelectiveImportModal()
    {
        if (!DisplaySelectiveImportModal)
            return;

        ImGui.OpenPopup("Selective Import##selectiveImportMenu");

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(
            "Selective Import##selectiveImportMenu",
            ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
        {
            UIHelper.SimpleHeader("Selective Imports", "");
            ImGui.Text("TODO");

            UIHelper.SimpleHeader("Actions", "");
            if (ImGui.Button("Commit", DPI.StandardButtonSize))
            {
                ImportDelta(SelectedImport);
                SelectedImport = null; // Deselect once done.
            }

            ImGui.SameLine();

            if (ImGui.Button("Close", DPI.StandardButtonSize))
            {
                DisplaySelectiveImportModal = false;
            }

            ImGui.EndPopup();
        }
    }

    #endregion

    #region Delta Export
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

        DisplayExportProgressModal = true;
        InitialLayout = false;

        try
        {
            var patch = await Task.Run(BuildDeltaPatch);

            if (patch.Params.Count > 0)
            {
                WriteDeltaPatch(patch, ExportName);
                TaskLogs.AddLog($"Saved param delta: {ExportName}.json");

                RefreshImportList();
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
            DisplayExportProgressModal = false;
        }
    }

    public ParamDeltaPatch BuildDeltaPatch()
    {
        DisplayExportProgressModal = true;

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

            // Skip indexed params if the option is enabled
            if(CFG.Current.ParamEditor_DeltaPatcher_Export_Ignore_Indexed_Rows)
            {
                if (Project.Handler.ParamData.TableParamList.Params.Contains(primaryParam.Key))
                {
                    continue;
                }
            }

            if(CurrentExportMode is DeltaExportMode.Selected)
            {
                if(primaryParam.Key != Project.Handler.ParamEditor.ViewHandler.ActiveView.Selection.GetActiveParam())
                {
                    continue;
                }
            }

            var paramDelta = new ParamDelta();
            paramDelta.Name = primaryParam.Key;

            var vanillaParam = vanillaBank.Params[primaryParam.Key];

            List<RowDelta> rowDeltas = new List<RowDelta>();

            if (CurrentExportMode is DeltaExportMode.Modified)
            {
                rowDeltas = HandleRows(primaryParam.Value, vanillaParam);
            }

            if (CurrentExportMode is DeltaExportMode.Selected)
            {
                rowDeltas = HandleSelectedRows(primaryParam.Value, vanillaParam);
            }

            foreach (var entry in rowDeltas)
            {
                paramDelta.Rows.Add(entry);
            }

            if(paramDelta.Rows.Count > 0)
            {
                patch.Params.Add(paramDelta);
            }
        }

        DisplayExportProgressModal = false;

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
                    if(primaryCol.Def.InternalType == "dummy8")
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

        // TODO


        return rowDeltas;
    }
    #endregion

    #region IO
    public void DeleteDeltaPatch(string name)
    {
        var storageDir = ProjectUtils.GetParamDeltaFolder();

        var readPath = Path.Combine(storageDir, $"{name}.json");

        if (File.Exists(readPath))
        {
            File.Delete(readPath);
        }
    }

    public ParamDeltaPatch LoadDeltaPatch(string name)
    {
        var storageDir = ProjectUtils.GetParamDeltaFolder();

        var readPath = Path.Combine(storageDir, $"{name}.json");

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

    public void DrawExportProgressModal()
    {
        if (!DisplayExportProgressModal)
            return;

        ImGui.OpenPopup("Delta Export##deltaExport");

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(
            "Delta Export##deltaExport",
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

    public void DrawImportProgressModal()
    {
        if (!DisplayImportProgressModal)
            return;

        ImGui.OpenPopup("Delta Import##deltaImport");

        if (!InitialLayout)
        {
            UIHelper.SetupPopupWindow();
            InitialLayout = true;
        }

        if (ImGui.BeginPopupModal(
            "Delta Import##deltaImport",
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

public class DeltaImportEntry
{
    public string Filename { get; set; }
    public ParamDeltaPatch Delta { get; set; }
}

public enum DeltaExportMode
{
    Modified,
    Selected
}