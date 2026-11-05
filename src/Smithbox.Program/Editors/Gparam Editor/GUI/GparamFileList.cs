using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
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

    // The file the delete confirmation is asking about
    private FileDictionaryEntry PendingDeleteEntry = null;
    private bool OpenDeleteConfirm = false;

    private const string DeleteConfirmId = "###gparamFileDeleteConfirm";

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

        DisplayDeleteConfirm();
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
        // A snapshot: Copy As and Delete change the bank's entries from inside the loop
        var entries = View.Project.Handler.GparamData.PrimaryBank.Entries.ToList();

        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

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

    private void DisplayFileSelectable(FileDictionaryEntry fileEntry, GparamWrapper wrapper, int index)
    {
        var curGparam = wrapper.Gparam;

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

        // Mark files with unsaved changes
        if (wrapper.IsDirty)
        {
            filename = $"{filename}*";
        }

        // File row
        // The ## suffix keeps the ImGui ID stable when the dirty marker toggles
        if (ImGui.Selectable($@" {filename}##gparamFile_{fileEntry.Filename}.{fileEntry.Extension}", View.Selection.IsSelectedFile(fileEntry)))
        {
            View.Selection.SetFileSelection(fileEntry);
        }

        // Scroll to a file jumped to from a tool
        if (View.Selection.FocusFile && View.Selection.IsSelectedFile(fileEntry))
        {
            View.Selection.FocusFile = false;
            ImGui.SetScrollHereY(0.5f);
        }

        // Right-click selects the row too, so its context menu opens on it
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && !View.Selection.IsSelectedFile(fileEntry))
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
        if (!View.Selection.IsSelectedFile(fileEntry))
            return;

        if (ImGui.BeginPopupContextItem($"##Gparam_File_Context"))
        {
            // Copy as
            if(ImGui.BeginMenu($"{LOC.Get("GPARAM_FileList_Context_Copy_As_Header")}##copyAsHeader"))
            {
                CopyAsMenu(fileEntry);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Copy_As_Action_TT"));

            // Delete
            if (IsDeletableGparamFile(fileEntry))
            {
                if (ImGui.Selectable($"{LOC.Get("GPARAM_FileList_Context_Delete_Action")}##deleteAction"))
                {
                    // Asked in a modal, opened outside this popup once it has closed
                    PendingDeleteEntry = fileEntry;
                    OpenDeleteConfirm = true;
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
            if (ImGui.BeginMenu($"{LOC.Get("GPARAM_FileList_Context_Target_Header")}##targetHeader"))
            {
                // Target in Global Search: a local search has the selected file only
                if (ImGui.Selectable($"{LOC.Get("GPARAM_FileList_Context_Target_In_Global_Search")}##globalSearchTarget"))
                {
                    View.ToolView.GlobalSearchTool.TargetFile(fileEntry.Filename);
                }
                GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_Target_In_Global_Search_TT"));

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    #region Copy As
    private string CopyAsFileName = "";

    public void CopyAsMenu(FileDictionaryEntry source)
    {
        ImGui.InputTextWithHint("##copyAsFileNameInput",
            LOC.Get("GPARAM_FileList_Context_CopyAs_Filename_Hint"),
            ref CopyAsFileName, 255);

        GUI.Tooltip(LOC.Get("GPARAM_FileList_Context_CopyAs_Filename_TT"));

        // Submit
        if(ImGui.Selectable($"{LOC.Get("GPARAM_FileList_Context_CopyAs_Submit")}##copyAsSubmit"))
        {
            CopyAs(source, CopyAsFileName.Trim());
        }
    }

    /// <summary>
    /// Write a copy of the file under a new name into the project, and add it to the list.
    /// The copy holds the GPARAM as it is in the editor, unsaved changes included, or the file as it is on disk if it
    /// hasn't been opened. It is read through the VFS, so a file only in the game archives can be copied too.
    /// </summary>
    private void CopyAs(FileDictionaryEntry source, string newName)
    {
        var bank = Project.Handler.GparamData.PrimaryBank;
        var oldName = source.Filename;

        if (newName == "")
        {
            Smithbox.LogError<GparamFileList>(LOC.Get("GPARAM_FileList_CopyAs_Filename_Empty"));
            return;
        }

        if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            Smithbox.LogError<GparamFileList>(LOC.Get("GPARAM_FileList_CopyAs_Filename_Invalid", newName));
            return;
        }

        // The new path differs from the old in the file name only: the leading name part is swapped,
        // keeping the folder and the extensions (e.g. .gparam.dcx)
        var slash = source.Path.LastIndexOf('/');
        var folder = slash >= 0 ? source.Path[..(slash + 1)] : "";
        var oldFileName = slash >= 0 ? source.Path[(slash + 1)..] : source.Path;

        if (!oldFileName.StartsWith(oldName, StringComparison.OrdinalIgnoreCase))
        {
            Smithbox.LogError<GparamFileList>(LOC.Get("GPARAM_FileList_CopyAs_Unexpected_Path", source.Path));
            return;
        }

        var newPath = folder + newName + oldFileName[oldName.Length..];

        var nameTaken = bank.Entries.Keys.Any(e =>
            string.Equals(e.Filename, newName, StringComparison.OrdinalIgnoreCase) && e.Extension == source.Extension);

        if (nameTaken || Project.VFS.FS.FileExists(newPath))
        {
            Smithbox.LogError<GparamFileList>(LOC.Get("GPARAM_FileList_CopyAs_Exists", newPath));
            return;
        }

        try
        {
            var data = BuildCopyData(source, oldName, newName);

            if (data == null)
            {
                Smithbox.LogError<GparamFileList>(LOC.Get("GPARAM_FileList_CopyAs_Missing_Source_Path", source.Path));
                return;
            }

            // Creates the folder in the project if it isn't there yet
            Project.VFS.ProjectFS.WriteFile(newPath, data);
        }
        catch (Exception e)
        {
            Smithbox.LogError<GparamFileList>(LOC.Get("GPARAM_FileList_CopyAs_Failed", newPath), e);
            return;
        }

        // Only now the file exists: add it so it is immediately editable
        var newFileEntry = source.Clone();
        newFileEntry.Path = newPath;
        newFileEntry.Filename = newName;

        Project.Locator.GparamFiles.Entries.Add(newFileEntry);
        bank.Entries.Add(newFileEntry, new GparamWrapper(newFileEntry));

        View.Selection.SetFileSelection(newFileEntry);

        Smithbox.Log<GparamFileList>(LOC.Get("GPARAM_FileList_CopyAs_Done", oldName, newPath));
    }

    /// <summary>
    /// The bytes of the copy, or null if the source can't be read
    /// </summary>
    private byte[] BuildCopyData(FileDictionaryEntry source, string oldName, string newName)
    {
        var bank = Project.Handler.GparamData.PrimaryBank;
        bank.Entries.TryGetValue(source, out var wrapper);

        var loaded = wrapper?.Gparam;

        // BB container: the GPARAM inside is found by its name, so it is renamed too, or the copy would never load
        if (source.Extension == "gparambnd")
        {
            var bndData = Project.VFS.FS.ReadFile(source.Path);

            if (bndData == null)
                return null;

            var binder = BND4.Read(bndData.Value);

            var inner = binder.Files.FirstOrDefault(e => Path.GetFileNameWithoutExtension(e.Name) == oldName);

            if (inner == null)
                return null;

            if (loaded != null)
            {
                inner.Bytes = loaded.Write();
            }

            var innerFileName = Path.GetFileName(inner.Name);
            var innerFolder = inner.Name[..^innerFileName.Length];

            inner.Name = innerFolder + newName + innerFileName[oldName.Length..];

            return binder.Write();
        }

        // Loose file: the editor's GPARAM keeps the compression it was read with
        if (loaded != null)
        {
            return loaded.Write();
        }

        return Project.VFS.FS.ReadFile(source.Path)?.ToArray();
    }
    #endregion

    #region Delete
    /// <summary>
    /// Only files the project holds can be deleted
    /// </summary>
    private bool IsDeletableGparamFile(FileDictionaryEntry entry)
    {
        return Project.VFS.ProjectFS.FileExists(entry.Path);
    }

    private void DisplayDeleteConfirm()
    {
        if (OpenDeleteConfirm)
        {
            OpenDeleteConfirm = false;
            ImGui.OpenPopup($"{LOC.Get("GPARAM_FileList_Delete_Confirm_Title")}{DeleteConfirmId}");
        }

        var open = true;
        var confirmed = false;

        if (ImGui.BeginPopupModal($"{LOC.Get("GPARAM_FileList_Delete_Confirm_Title")}{DeleteConfirmId}",
            ref open, ImGuiWindowFlags.AlwaysAutoResize))
        {
            var entry = PendingDeleteEntry;
            var hasVanilla = entry != null && Project.VFS.VanillaFS.FileExists(entry.Path);

            GUI.WrappedText(LOC.Get(hasVanilla
                ? "GPARAM_FileList_Delete_Confirm_Text_Revert"
                : "GPARAM_FileList_Delete_Confirm_Text_Remove",
                entry?.Path ?? ""));

            var cancelled = false;

            GUI.MultiButtonInput("gparamFileDeleteConfirmActions",
                "gparamFileDeleteConfirm",
                LOC.Get("GPARAM_FileList_Delete_Confirm"),
                LOC.Get("GPARAM_FileList_Delete_Confirm_TT"),
                () => confirmed = true,

                "gparamFileDeleteCancel",
                LOC.Get("GPARAM_FileList_Delete_Cancel"),
                LOC.Get("GPARAM_FileList_Delete_Cancel_TT"),
                () => cancelled = true);

            if (confirmed || cancelled)
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        if (confirmed && PendingDeleteEntry != null)
        {
            DeleteGparamFile(PendingDeleteEntry);
        }

        if (confirmed || !open)
        {
            PendingDeleteEntry = null;
        }
    }

    /// <summary>
    /// Delete the project's copy of the file. If the game data has the file too, the entry stays and shows the
    /// vanilla file from then on; if the file was the project's own (e.g. made with Copy As), the entry goes.
    /// </summary>
    private void DeleteGparamFile(FileDictionaryEntry entry)
    {
        var bank = Project.Handler.GparamData.PrimaryBank;

        try
        {
            Project.VFS.ProjectFS.Delete(entry.Path);
        }
        catch (Exception e)
        {
            Smithbox.LogError<GparamFileList>(LOC.Get("GPARAM_FileList_Delete_Failed", entry.Path), e);
            return;
        }

        var hasVanilla = Project.VFS.VanillaFS.FileExists(entry.Path);

        if (hasVanilla)
        {
            // Read again on next use, now from the vanilla file
            if (bank.Entries.TryGetValue(entry, out var wrapper))
            {
                wrapper.Gparam = null;
                wrapper.IsDirty = false;
            }
        }
        else
        {
            Project.Locator.GparamFiles.Entries.Remove(entry);
            bank.Entries.Remove(entry);
        }

        // The undo steps of every view may refer to the deleted GPARAM, which nothing displays or saves any more
        foreach (var view in View.Editor.ViewHandler.TexViews)
        {
            view.ActionManager.Clear();
        }

        // The Map, Model and Time Act Editors keep their own copies of the GPARAMs they read
        Project.Handler.MapData?.FogVolumeBank?.ClearSettingsCache();
        Project.Handler.MapData?.GparamLightingBank?.ClearCache();
        Project.Handler.ExistingViewportEnvironment?.LightingBank.ClearCache();

        // Every view's selection is checked against the bank each frame, so the others clear themselves.
        // This one shows the vanilla file in its place, if there is one.
        if (View.Selection.IsSelectedFile(entry))
        {
            View.Selection.ResetSelection();
            View.Selection.SelectedFileEntry = null;

            if (hasVanilla)
            {
                View.Selection.SetFileSelection(entry);
            }
        }

        Smithbox.Log<GparamFileList>(LOC.Get(hasVanilla
            ? "GPARAM_FileList_Delete_Done_Revert"
            : "GPARAM_FileList_Delete_Done_Remove", entry.Path));
    }
    #endregion
}
