using Andre.Formats;
using Google.Protobuf.WellKnownTypes;
using Hexa.NET.ImGui;
using HKLib.hk2018.hkSerialize.Note;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

    public ParamDeltaSelection Selection;

    private ParamDeltaImporter Importer;
    private ParamDeltaExporter Exporter;

    public ParamDeltaProgressModal ImportProgressModal;
    public ParamDeltaProgressModal ExportProgressModal;

    public ParamSelectiveImportModal SelectiveImportModal;
    public ParamImportPreviewModal ImportPreviewModal;
    public ParamExportPreviewModal ExportPreviewModal;

    public ParamDeltaPatcher(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        Selection = new(this);

        Importer = new(this);
        Exporter = new(this);

        ImportProgressModal = new("Delta Import", this);
        ExportProgressModal = new("Delta Export", this);

        SelectiveImportModal = new(this);
        ImportPreviewModal = new(this);
        ExportPreviewModal = new(this);

        Selection.RefreshImportList();
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
            Selection.RefreshImportList();
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
            if (Selection.SelectedImport != null)
            {
                Importer.ImportDelta(Selection.SelectedImport);
                Selection.SelectedImport = null; // Deselect once done.
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
        foreach(var entry in Selection.ImportList)
        {
            var selected = entry == Selection.SelectedImport;

            var version = ParamUtils.ParseRegulationVersion(entry.Delta.ParamVersion);

            var displayName = $"{entry.Filename} [{version}]";

            if (ImGui.Selectable($"{displayName}##curEntry_{entry.Filename.GetHashCode()}", selected))
            {
                Selection.SelectedImport = entry;
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && Selection.SelectImportEntry)
            {
                Selection.SelectImportEntry = false;
                Selection.SelectedImport = entry;
            }

            if (ImGui.IsItemFocused())
            {
                if (InputManager.HasArrowSelection())
                {
                    Selection.SelectImportEntry = true;
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
                        Selection.QueueImportListRefresh = true;

                        ImGui.CloseCurrentPopup();
                    }
                    UIHelper.Tooltip("Delete this delta file.");

                    ImGui.EndPopup();
                }
            }
        }

        ImGui.EndChild();

        if(Selection.QueueImportListRefresh)
        {
            Selection.QueueImportListRefresh = false;
            Selection.RefreshImportList();
        }
    }

    public void DisplayExportTab()
    {
        UIHelper.SimpleHeader("Filename", "The name of the delta file.");
        DPI.ApplyInputWidth();
        ImGui.InputText("##inputFileName", ref Selection.ExportName, 255);

        UIHelper.SimpleHeader("Options", "Options to set for the delta builder.");

        ImGui.Text("Export Mode");
        DPI.ApplyInputWidth();
        if (ImGui.BeginCombo("##inputValue", Selection.CurrentExportMode.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(DeltaExportMode)))
            {
                var type = (DeltaExportMode)entry;

                if (ImGui.Selectable(type.GetDisplayName()))
                {
                    Selection.CurrentExportMode = (DeltaExportMode)entry;
                }
            }
            ImGui.EndCombo();
        }

        ImGui.Checkbox("Ignore Indexed Params", ref CFG.Current.ParamEditor_DeltaPatcher_Export_Ignore_Indexed_Rows);
        UIHelper.Tooltip("If enabled, indexed params where the rows depending on row index as well as ID will be ignored when producing the delta.");

        UIHelper.SimpleHeader("Actions", "");
        if (ImGui.Button("Export", DPI.StandardButtonSize))
        {
            Exporter.GenerateDeltaPatch();
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
    [Display(Name = "Modified")]
    Modified,
    [Display(Name = "Selected")]
    Selected
}