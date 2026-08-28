using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

public class GparamFileList
{
    private GparamEditorView View;
    private ProjectEntry Project;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;

    public GparamFileList(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for this view
    /// </summary>
    public void Display()
    {
        DisplayHeader();

        // Files
        ImGui.BeginChild("GparamFileSection", ImGuiChildFlags.Borders);

        DisplayFileList();

        ImGui.EndChild();
    }
    public void DisplayHeader()
    {
        GUI.TitleHeader(
            LOC.Get("GPARAM_FileList_Header"),
            LOC.Get("GPARAM_FileList_Header_TT"));

        // Search
        ImGui.BeginChild("GparamFileSearchSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("gparamEditor_FileList",
            ref FileListFilter, ref ExactFileListFilter);

        // Toggle: Aliases
        GUI.DisplayToggleButton("aliasToggle", Icons.StickyNote,
            ref CFG.Current.GparamEditor_File_List_Display_Aliases,
            "GPARAM_FileList_Alias_Toggle_Hide",
            "GPARAM_FileList_Alias_Toggle_Show",
            "GPARAM_FileList_Alias_Toggle_TT");

        // BND File Toggle
        if (Project.Descriptor.ProjectType is ProjectType.BB)
        {
            // Toggle: GPARAMBND
            GUI.DisplayToggleButton("gparambndToggle", Icons.Database,
                ref CFG.Current.GparamEditor_File_List_Display_BB_BND_Files,
                "GPARAM_FileList_TargetBnd_Toggle_GPARAM",
                "GPARAM_FileList_TargetBnd_Toggle_GPARAMBND",
                "GPARAM_FileList_TargetBnd_Toggle_TT");
        }

        ImGui.EndChild();
    }

    private void DisplayFileList()
    {
        for(int i = 0; i < View.Project.Handler.GparamData.PrimaryBank.Entries.Count; i++)
        {
            var entry = View.Project.Handler.GparamData.PrimaryBank.Entries.ElementAt(i);

            // For BB, toggle which gparam files are displayed
            if (Project.Descriptor.ProjectType is ProjectType.BB)
            {
                if (CFG.Current.GparamEditor_File_List_Display_BB_BND_Files)
                {
                    if (entry.Key.Extension == "gparam")
                        continue;
                }
                else
                {
                    if (entry.Key.Extension == "gparambnd")
                        continue;
                }
            }

            DisplayFileSelectable(entry.Key, entry.Value, i);
        }
    }

    private void DisplayFileSelectable(FileDictionaryEntry fileEntry, GPARAM curGparam, int index)
    {
        var alias = AliasHelper.GetGparamAliasName(View.Project, fileEntry.Filename);

        var isMatch = EditorFilters.IsMatch(
            FileListFilter, fileEntry.Filename, ExactFileListFilter, alias, false, true);

        if (!isMatch)
            return;

        ImGui.BeginGroup();

        var filename = fileEntry.Filename;

        if (Project.Descriptor.ProjectType is ProjectType.BB)
        {
            if (CFG.Current.GparamEditor_File_List_Display_BB_BND_Files)
            {
                filename = $"{filename} [BND]";
            }
        }

        // File row
        if (ImGui.Selectable($@" {filename}", fileEntry.Filename == View.Selection._selectedGparamKey))
        {
            View.Selection.SetFileSelection(fileEntry);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && View.Selection.SelectGparamFile)
        {
            View.Selection.SelectGparamFile = false;

            View.Selection.SetFileSelection(fileEntry);
        }

        if (ImGui.IsItemFocused())
        {
            if (InputManager.HasArrowSelection())
            {
                View.Selection.SelectGparamFile = true;
            }
        }

        if (CFG.Current.GparamEditor_File_List_Display_Aliases)
        {
            GUI.DisplayAlias(alias);
        }

        ImGui.EndGroup();

        ContextMenu(fileEntry, curGparam);
    }

    private string OverrideFileName = "";

    public void ContextMenu(FileDictionaryEntry fileEntry, GPARAM curGparam)
    {
        var fileKey = View.Selection._selectedGparamKey;

        if (fileEntry.Filename != fileKey)
            return;

        if (ImGui.BeginPopupContextItem($"##Gparam_File_Context"))
        {
            // Copy as
            if(ImGui.BeginMenu($"{LOC.Get("GPARAM_FileList_Context_Copy_As_Header")}##copyAsHeader"))
            {
                CopyAsMenu();

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Copy_As_Action_TT"));

            // Delete
            if (IsDeletableGparamFile(fileEntry))
            {
                if (ImGui.Selectable($"{LOC.Get("GPARAM_FileList_Context_Delete_Action")}##deleteAction"))
                {
                    DeleteGparamFile(fileEntry);
                }
                GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Delete_Action_TT"));
            }

            ImGui.Separator();

            // Import
            if (ImGui.Selectable($"{LOC.Get("GPARAM_FileList_Context_Import_Action")}##importAction"))
            {
                View.ToolView.DataTransferTool.ImportGPARAM(Project, View, fileEntry, curGparam);
            }
            GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Import_Action_TT"));

            // Export
            if (ImGui.BeginMenu($"{LOC.Get("GPARAM_FileList_Context_Export_Header")}##exportHeader"))
            {
                ImGui.InputTextWithHint("##overrideFilename", 
                    LOC.Get("GPARAM_FileList_Context_Export_Filename_Hint"), ref OverrideFileName, 255);
                GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Export_Filename_TT"));

                // Export File
                if (ImGui.Selectable($"{LOC.Get("GPARAM_FileList_Context_Export_Action")}##exportAction"))
                {
                    View.ToolView.DataTransferTool.ExportGparamFile(fileEntry, curGparam, OverrideFileName);
                }
                GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Export_Action_TT"));

                ImGui.EndMenu();
            }

            ImGui.Separator();

            // Copy Name
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_FileList_Context_Copy_Name_Action")}##copyName"))
            {
                ImGui.SetClipboardText(fileEntry.Filename);
            }
            GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Copy_Name_Action_TT"));

            // Copy Path
            if (ImGui.MenuItem($"{LOC.Get("GPARAM_FileList_Context_Copy_Path_Action")}##copyPath"))
            {
                ImGui.SetClipboardText(fileEntry.Path);
            }
            GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Copy_Path_Action_TT"));

            ImGui.Separator();

            // Quick Target
            if (ImGui.BeginMenu($"{LOC.Get("GPARAM_FileList_Context_QuickEdit_Header")}##quickEditHeader"))
            {
                // Target in Quick Edit
                if (ImGui.Selectable($"{LOC.Get("GPARAM_FileList_Context_Target_In_Quick_Edit")}##quickEditTarget"))
                {
                    View.QuickEditHandler.UpdateFileFilter(fileEntry.Filename);
                }
                GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Target_In_Quick_Edit_TT"));

                // Target in Data Finder
                if (ImGui.Selectable($"{LOC.Get("GPARAM_FileList_Context_Target_In_Data_Finder")}##dataFinderTarget"))
                {
                    View.ToolView.DataFinder.UpdateFileFilter(fileEntry.Filename);
                }
                GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Target_In_Data_Finder_TT"));

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    private string CopyAsFileName = "";

    public void CopyAsMenu()
    {
        ImGui.InputTextWithHint("##copyAsFileNameInput", 
            LOC.Get("GPARAM_FileList_Context_CopyAs_Filename_Hint"),
            ref CopyAsFileName, 255);

        GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_CopyAs_Filename_TT"));

        // Submit
        if(ImGui.Selectable($"{LOC.Get("GPARAM_FileList_Context_CopyAs_Submit")}##copyAsSubmit"))
        {
            if(CopyAsFileName == "")
            {
                Smithbox.LogError<GparamFileList>(
                    LOC.Get("GPARAM_FileList_CopyAs_Filename_Empty"));
            }
            else
            {
                // Then actually copy the file
                var oldPath = View.Selection.SelectedFileEntry.Path;
                var srcPath = Path.Join(ProjectFileLocator.NormalizePath(Project.Descriptor.ProjectPath), oldPath);

                // Fallback to the vanilla version if there isn't an existing project-edited version
                if(!File.Exists(srcPath))
                {
                    srcPath = Path.Join(ProjectFileLocator.NormalizePath(Project.Descriptor.DataPath), oldPath);
                }

                if(!File.Exists(srcPath))
                {
                    Smithbox.LogError<GparamFileList>(
                        LOC.Get("GPARAM_FileList_CopyAs_Missing_Source_Path", srcPath));
                }
                else
                {
                    // Add the new file to the internal structures so it is immediately editable
                    var oldName = View.Selection.SelectedFileEntry.Filename;
                    var newFileEntry = View.Selection.SelectedFileEntry.Clone();
                    newFileEntry.Path = newFileEntry.Path.Replace(oldName, CopyAsFileName);
                    newFileEntry.Filename = newFileEntry.Filename.Replace(oldName, CopyAsFileName);

                    Project.Locator.GparamFiles.Entries.Add(newFileEntry);
                    Project.Handler.GparamData.PrimaryBank.Entries.Add(newFileEntry, null);

                    var copyPath = Path.Join(ProjectFileLocator.NormalizePath(Project.Descriptor.ProjectPath), newFileEntry.Path);

                    File.Copy(srcPath, copyPath);
                }
            }
        }
    }

    // Only allow files in the project directory to be deleted.
    private bool IsDeletableGparamFile(FileDictionaryEntry entry)
    {
        var srcPath = Path.Join(ProjectFileLocator.NormalizePath(Project.Descriptor.ProjectPath), entry.Path);

        if(File.Exists(srcPath))
        {
            return true;
        }

        return false;
    }

    private void DeleteGparamFile(FileDictionaryEntry entry)
    {
        var srcPath = Path.Join(ProjectFileLocator.NormalizePath(Project.Descriptor.ProjectPath), entry.Path);

        if (File.Exists(srcPath))
        {
            File.Delete(srcPath);
        }

        Project.Locator.GparamFiles.Entries.Remove(entry);
        Project.Handler.GparamData.PrimaryBank.Entries.Remove(entry);
    }
}
