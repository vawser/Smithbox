using Hexa.NET.ImGui;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Text.Json;
using Enum = System.Enum;

namespace StudioCore.Editors.ParamEditor;

public class ParamDeltaPatcher
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public ParamDeltaSelection Selection;

    public ParamDeltaImporter Importer;
    public ParamDeltaExporter Exporter;

    public ParamDeltaProgressModal ImportProgressModal;
    public ParamDeltaProgressModal ExportProgressModal;

    public ParamImportPreviewModal ImportPreviewModal;
    public ParamExportPreviewModal ExportPreviewModal;

    public DeltaImportMode ImportMode = DeltaImportMode.Complex;

    public ParamDeltaPatcher(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Selection = new(this);

        Importer = new(this);
        Exporter = new(this);

        ImportProgressModal = new("PARAM_DeltaPatcher_Import_Modal_Name", this);
        ExportProgressModal = new("PARAM_DeltaPatcher_Export_Modal_Name", this);

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

        // Param Delta Patcher
        if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_DeltaPatcher_Header_Patcher")}##deltaPatcherHeader"))
        {
            ImGui.BeginChild("ParamDeltaPatcherToolSection", ImGuiChildFlags.Borders);

            if (ImGui.BeginTabBar("deltaTabs"))
            {
                // Import
                if(ImGui.BeginTabItem($"{LOC.Get("PARAM_DeltaPatcher_Tab_Import")}##importTab"))
                {
                    DisplayImportTab();

                    ImGui.EndTabItem();
                }

                // Export
                if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DeltaPatcher_Tab_Export")}##exportTab"))
                {
                    DisplayExportTab();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.EndChild();
        }
    }

    public void DisplayImportTab()
    {
        GUI.WrappedText(LOC.Get("PARAM_DeltaPatcher_Import_Hint"));

        // Import Mode
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DeltaPatcher_Header_Import_Mode"),
            LOC.Get("PARAM_DeltaPatcher_Header_Import_Mode_TT"));

        var previewName = LOC.Get(ImportMode.GetDisplayName());

        // Current Import Mode
        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##importMode", previewName))
        {
            foreach (var entry in Enum.GetValues(typeof(DeltaImportMode)))
            {
                var curType = (DeltaImportMode)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == ImportMode))
                {
                    ImportMode = curType;
                }
            }

            ImGui.EndCombo();
        }

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DeltaPatcher_Header_Options"),
            LOC.Get("PARAM_DeltaPatcher_Header_Options_TT"));

        // Toggle: Display All
        ImGui.Checkbox($"{LOC.Get("PARAM_DeltaPatcher_Checkbox_Display_All")}##displayAllToggle",
            ref CFG.Current.ParamEditor_DeltaPatcher_Import_Display_All_Entries);

        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            Selection.RefreshImportList();
        }
        GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_Checkbox_Display_All_TT"));

        // Toggle: Include Modified Rows
        ImGui.Checkbox($"{LOC.Get("PARAM_DeltaPatcher_Checkbox_Include_Modified")}##includeModifiedToggle",
            ref CFG.Current.ParamEditor_DeltaPatcher_Import_Modified_Rows);
        GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_Checkbox_Include_Modified_TT"));

        // Toggle: Include Added Rows
        ImGui.Checkbox($"{LOC.Get("PARAM_DeltaPatcher_Checkbx_Include_Added")}##includeAddedToggle",
            ref CFG.Current.ParamEditor_DeltaPatcher_Import_Added_Rows);
        GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_Checkbx_Include_Added_TT"));

        // Toggle: Include Deleted Rows
        ImGui.Checkbox($"{LOC.Get("PARAM_DeltaPatcher_Checkbox_Include_Deleted")}##includeDeletedToggle",
            ref CFG.Current.ParamEditor_DeltaPatcher_Import_Deleted_Rows);
        GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_Checkbox_Include_Deleted_TT"));

        // Toggle: Restrict Row Modification
        ImGui.Checkbox($"{LOC.Get("PARAM_DeltaPatcher_Checkbox_Restrict_Modify")}##restrictModifyToggle",
            ref CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Modify);
        GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_Checkbox_Restrict_Modify_TT"));

        // Toggle: Restrict Row Addition
        ImGui.Checkbox($"{LOC.Get("PARAM_DeltaPatcher_Checkbox_Restrict_Addition")}##restrictAddToggle",
            ref CFG.Current.ParamEditor_DeltaPatcher_Import_Restrict_Row_Add);
        GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_Checkbox_Restrict_Addition_TT"));

        // Toggle: Allow Row Overwrite
        ImGui.Checkbox($"{LOC.Get("PARAM_DeltaPatcher_Checkbox_Allow_Overwrite")}##allowOverwriteToggle",
            ref CFG.Current.ParamEditor_DeltaPatcher_Import_Allow_Row_Overwrite);
        GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_Checkbox_Allow_Overwrite_TT"));

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DeltaPatcher_Header_Actions"),
            LOC.Get("PARAM_DeltaPatcher_Header_Actions_TT"));

        GUI.MultiButtonInput("deltaImportActions",
            "importDelta", 
            LOC.Get("PARAM_DeltaPatcher_Action_Import_Delta"),
            LOC.Get("PARAM_DeltaPatcher_Action_Import_Delta_TT"),
            ImportAction,

            "refreshDeltaList",
            LOC.Get("PARAM_DeltaPatcher_Action_Refresh_List"),
            LOC.Get("PARAM_DeltaPatcher_Action_Refresh_List_TT"),
            RefreshImportListAction);

        if (Selection.ImportList.Count > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_DeltaPatcher_Header_Entries"),
                LOC.Get("PARAM_DeltaPatcher_Header_Entries_TT"));

            ImGui.BeginChild("importEntryList");

            DisplayEntryList();

            ImGui.Text("");

            ImGui.EndChild();
        }

        if(Selection.QueueImportListRefresh)
        {
            Selection.QueueImportListRefresh = false;
            Selection.RefreshImportList();
        }
    }

    public void ImportAction()
    {
        if (Selection.SelectedImport != null)
        {
            ImportPreviewModal.Show(Selection.SelectedImport.Filename, Selection.SelectedImport.Delta);
        }
    }

    public void RefreshImportListAction()
    {

        Selection.RefreshImportList();
    }

    public void DisplayEntryList()
    {
        // Build the folder dictionary
        Dictionary<string, List<DeltaImportEntry>> _folders = new();

        if (Selection.ImportList.Count > 0)
        {
            foreach (var entry in Selection.ImportList)
            {
                if (entry.Delta.Tag != "")
                {
                    if (_folders.ContainsKey(entry.Delta.Tag))
                    {
                        _folders[entry.Delta.Tag].Add(entry);
                    }
                    else
                    {
                        _folders.Add(entry.Delta.Tag, new List<DeltaImportEntry>() { entry });
                    }
                }
            }
        }

        // General
        if (Selection.ImportList.Any(e => e.Delta.Tag == ""))
        {
            if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_DeltaPatcher_General_Folder")}##importFolder_General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var entry in Selection.ImportList)
                {
                    if (entry.Delta.Tag == "")
                    {
                        DisplayImportEntry(entry);
                    }
                }
            }
        }

        // Folders
        foreach (var folder in _folders)
        {
            var name = folder.Key;
            var entries = folder.Value;

            if (ImGui.CollapsingHeader($"{name}##importFolder_{name}", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var entry in entries)
                {
                    DisplayImportEntry(entry);
                }
            }
        }
    }

    public void DisplayImportEntry(DeltaImportEntry entry)
    {
        var selected = entry == Selection.SelectedImport;

        var version = ParamUtils.ParseRegulationVersion(entry.Delta.ParamVersion);

        var displayName = $"{entry.Filename} [{version}]";

        // Import Entry
        if (ImGui.Selectable($"{displayName}##curEntry_{entry.Filename.GetHashCode()}", selected))
        {
            Selection.SelectedImport = entry;
            Selection.Edit_OriginalName = entry.Filename;
            Selection.Edit_FileTag = entry.Delta.Tag;
            Selection.Edit_Name = entry.Filename;
        }

        if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            if (selected)
            {
                ImportPreviewModal.Show(Selection.SelectedImport.Filename, Selection.SelectedImport.Delta);
            }
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && Selection.SelectImportEntry)
        {
            Selection.SelectImportEntry = false;

            Selection.SelectedImport = entry;
            Selection.Edit_OriginalName = entry.Filename;
            Selection.Edit_FileTag = entry.Delta.Tag;
            Selection.Edit_Name = entry.Filename;
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
                // Edit
                if(ImGui.BeginMenu($"{LOC.Get("PARAM_DeltaPatcher_ImportContext_Header_Edit")}##editMenuHEader"))
                {
                    // Name
                    ImGui.InputText($"{LOC.Get("PARAM_DeltaPatcher_ImportContext_Name")}##nameInput", 
                        ref Selection.Edit_Name, 255);

                    // Tag
                    ImGui.InputText($"{LOC.Get("PARAM_DeltaPatcher_ImportContext_Tag")}##tagInput", 
                        ref Selection.Edit_FileTag, 255);

                    // Update
                    if(ImGui.Button($"{LOC.Get("PARAM_DeltaPatcher_ImportContext_Action_Update")}##updateAction", DPI.SelectorButtonSize))
                    {
                        entry.Delta.Tag = Selection.Edit_FileTag;

                        WriteDeltaPatch(entry.Delta, Selection.Edit_Name);

                        // Delete old
                        var writeDir = ProjectUtils.GetParamDeltaFolder();
                        var writePath = Path.Combine(writeDir, $"{Selection.Edit_OriginalName}.json");

                        if(File.Exists(writePath))
                        {
                            File.Delete(writePath);
                        }

                        Selection.QueueImportListRefresh = true;
                    }

                    ImGui.EndMenu();
                }

                // Delete
                if (ImGui.Selectable($"{LOC.Get("PARAM_DeltaPatcher_ImportContext_Action_Delete")}##deleteAction"))
                {
                    DeleteDeltaPatch(entry.Filename);
                    Selection.QueueImportListRefresh = true;

                    ImGui.CloseCurrentPopup();
                }
                GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_ImportContext_Action_Delete_TT"));

                ImGui.EndPopup();
            }
        }
    }

    public void DisplayExportTab()
    {
        GUI.WrappedText(LOC.Get("PARAM_DeltaPatcher_Export_Hint"));

        // Filename
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DeltaPatcher_Header_Filename"),
            LOC.Get("PARAM_DeltaPatcher_Header_Filename_TT"));

        GUI.HintTextInput($"inputFileName", ref Selection.ExportName, LOC.Get("PARAM_DeltaPatcher_Filename_Input_Hint"));

        // Tags
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DeltaPatcher_Header_Tags"),
            LOC.Get("PARAM_DeltaPatcher_Header_Tags_TT"));

        GUI.HintTextInput($"inputFileTag", ref Selection.ExportFileTag, LOC.Get("PARAM_DeltaPatcher_Tags_Input_Hint"));

        // Param Type
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DeltaPatcher_Header_Param_Type"),
            LOC.Get("PARAM_DeltaPatcher_Header_Param_Type_TT"));

        var paramType_PreviewName = LOC.Get(Selection.CurrentParamMode.GetDisplayName());

        var width = ImGui.GetWindowWidth() * 0.5f;
        ImGui.PushItemWidth(width);
        if (ImGui.BeginCombo("##inputValue_ParamType", paramType_PreviewName))
        {
            foreach (var entry in Enum.GetValues(typeof(DeltaExportMode)))
            {
                var type = (DeltaExportMode)entry;

                var displayName = LOC.Get(type.GetDisplayName());

                if (ImGui.Selectable(displayName, Selection.CurrentParamMode == type))
                {
                    Selection.CurrentParamMode = (DeltaExportMode)entry;
                }
                GUI.Tooltip(type.GetDescription());
            }
            ImGui.EndCombo();
        }

        // Selection Type
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DeltaPatcher_Header_Selection_Type"),
            LOC.Get("PARAM_DeltaPatcher_Header_Selection_Type_TT"));

        var selectionType_PreviewName = LOC.Get(Selection.CurrentRowMode.GetDisplayName());

        ImGui.PushItemWidth(width);
        if (ImGui.BeginCombo("##inputValue_SelectionType", selectionType_PreviewName))
        {
            foreach (var entry in Enum.GetValues(typeof(DeltaSelectionMode)))
            {
                var type = (DeltaSelectionMode)entry;

                var displayName = LOC.Get(type.GetDisplayName());

                if (ImGui.Selectable(displayName, Selection.CurrentRowMode == type))
                {
                    Selection.CurrentRowMode = (DeltaSelectionMode)entry;
                }
                GUI.Tooltip(type.GetDescription());
            }
            ImGui.EndCombo();
        }

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DeltaPatcher_Header_Options"),
            LOC.Get("PARAM_DeltaPatcher_Header_Options_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_DeltaPatcher_Checkbox_Ignore_Indexed")}##ignoreIndexedToggle", ref CFG.Current.ParamEditor_DeltaPatcher_Export_Ignore_Indexed_Rows);
        GUI.Tooltip(LOC.Get("PARAM_DeltaPatcher_Checkbox_Ignore_Indexed_TT"));

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_DeltaPatcher_Header_Actions"),
            LOC.Get("PARAM_DeltaPatcher_Header_Actions_TT"));

        GUI.MultiButtonInput("exportDeltaActions",
            "exportDelta", 
            LOC.Get("PARAM_DeltaPatcher_Action_Export_Delta"),
            LOC.Get("PARAM_DeltaPatcher_Action_Export_Delta_TT"),
            ExportAction,

            "openDeltaFolder",
            LOC.Get("PARAM_DeltaPatcher_Action_Open_Delta_Folder"),
            LOC.Get("PARAM_DeltaPatcher_Action_Open_Delta_Folder_TT"), 
            OpenDeltaFolder);

        ImGui.Text("");
    }

    #endregion

    public void ExportAction()
    {
        Exporter.GenerateDeltaPatch();
    }

    public void OpenDeltaFolder()
    {
        var storageDir = ProjectUtils.GetParamDeltaFolder();

        Process.Start("explorer.exe", storageDir);
    }

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
                    Smithbox.LogError(this, LOC.Get("PARAM_DeltaPatcher_Deserialize_Patch_FAIL", readPath), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_DeltaPatcher_Read_Patch_FAIL", readPath), e);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("PARAM_DeltaPatcher_Find_Patch_FAIL", readPath));
        }

        return deltaPatch;
    }

    public void WriteDeltaPatch(ParamDeltaPatch patch, string name)
    {
        if(name == "" || name == null)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_DeltaPatcher_Write_Missing_Filename", name));
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
            Smithbox.LogError(this, LOC.Get("PARAM_DeltaPatcher_Write_Patch_FAIL", writePath), ex);
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
    [Display(Name = "PARAM_ENUM_DeltaExportMode_All", Description = "PARAM_ENUM_DeltaExportMode_All_TT")]
    All,
    [Display(Name = "PARAM_ENUM_DeltaExportMode_Selected", Description = "PARAM_ENUM_DeltaExportMode_Selected_TT")]
    Selected
}

public enum DeltaSelectionMode
{
    [Display(Name = "PARAM_ENUM_DeltaSelectionMode_Modified", Description = "PARAM_ENUM_DeltaSelectionMode_Modified_TT")]
    Modified,
    [Display(Name = "PARAM_ENUM_DeltaSelectionMode_Selected", Description = "PARAM_ENUM_DeltaSelectionMode_Selected_TT")]
    Selected,
    [Display(Name = "PARAM_ENUM_DeltaSelectionMode_All", Description = "PARAM_ENUM_DeltaSelectionMode_All_TT")]
    All
}

public enum DeltaImportMode
{
    [Display(Name = "PARAM_ENUM_DeltaImportMode_Insert_Overwrite")]
    Simple,
    [Display(Name = "PARAM_ENUM_DeltaImportMode_Conditional")]
    Complex
}