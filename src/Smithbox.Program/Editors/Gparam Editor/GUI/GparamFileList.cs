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
    private GparamEditorView Parent;
    private ProjectEntry Project;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;

    public GparamFileList(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
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
        UIHelper.SimpleHeader("Files", "");

        // Search
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("GparamFileSearchSection", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("gparamEditor_FileList",
            ref FileListFilter, ref ExactFileListFilter);

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##aliasToggle"))
        {
            CFG.Current.GparamEditor_File_List_Display_Aliases = !CFG.Current.GparamEditor_File_List_Display_Aliases;
        }

        var aliasMode = "Displaying file aliases.";
        if (!CFG.Current.GparamEditor_File_List_Display_Aliases)
        {
            aliasMode = "Hiding file aliases.";
        }
        UIHelper.Tooltip($"Toggle the display of file aliases.\nCurrent Mode: {aliasMode}");

        // BND File Toggle
        if (Project.Descriptor.ProjectType is ProjectType.BB)
        {
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Book}##bbFileContainerToggle"))
            {
                CFG.Current.GparamEditor_File_List_Display_BB_BND_Files = !CFG.Current.GparamEditor_File_List_Display_BB_BND_Files;
            }

            var bndMode = "Displaying GPARAMBND files.";
            if (!CFG.Current.GparamEditor_File_List_Display_BB_BND_Files)
            {
                bndMode = "Displaying GPARAM files.";
            }
            UIHelper.Tooltip($"Toggle the display of GPARAMBND files.\nCurrent Mode: {bndMode}");
        }

        ImGui.EndChild();
    }

    private void DisplayFileList()
    {
        for(int i = 0; i < Parent.Project.Handler.GparamData.PrimaryBank.Entries.Count; i++)
        {
            var entry = Parent.Project.Handler.GparamData.PrimaryBank.Entries.ElementAt(i);

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
        var alias = AliasHelper.GetGparamAliasName(Parent.Project, fileEntry.Filename);

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
        if (ImGui.Selectable($@" {filename}", fileEntry.Filename == Parent.Selection._selectedGparamKey))
        {
            Parent.Selection.SetFileSelection(fileEntry);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && Parent.Selection.SelectGparamFile)
        {
            Parent.Selection.SelectGparamFile = false;

            Parent.Selection.SetFileSelection(fileEntry);
        }

        if (ImGui.IsItemFocused())
        {
            if (InputManager.HasArrowSelection())
            {
                Parent.Selection.SelectGparamFile = true;
            }
        }

        if (CFG.Current.GparamEditor_File_List_Display_Aliases)
        {
            UIHelper.DisplayAlias(alias);
        }

        ImGui.EndGroup();

        ContextMenu(fileEntry, curGparam);
    }

    private string OverrideFileName = "";

    public void ContextMenu(FileDictionaryEntry fileEntry, GPARAM curGparam)
    {
        var fileKey = Parent.Selection._selectedGparamKey;

        if (fileEntry.Filename != fileKey)
            return;

        if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
        {
            // Copy as
            if(ImGui.BeginMenu("Copy As"))
            {
                CopyAsMenu();

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Copy this GPARAM and rename the copied file.");

            // Delete
            if (IsDeletableGparamFile(fileEntry))
            {
                if (ImGui.Selectable("Delete"))
                {
                    DeleteGparamFile(fileEntry);
                }
                UIHelper.Tooltip("Will delete this GPARAM from the project.");
            }


            ImGui.Separator();

            if (ImGui.Selectable("Import"))
            {
                Parent.Editor.ToolView.DataTransferTool.ImportGPARAM(Project, Parent, fileEntry, curGparam);
            }
            UIHelper.Tooltip("Import a GPARAM json to overwrite this entry.");

            if (ImGui.BeginMenu("Export"))
            {
                ImGui.InputText("##overrideFilename", ref OverrideFileName, 255);
                UIHelper.Tooltip("Define the filename for the exported file.");

                if (ImGui.Selectable("Export File"))
                {
                    Parent.Editor.ToolView.DataTransferTool.ExportGparamFile(fileEntry, curGparam, OverrideFileName);
                }

                ImGui.EndMenu();
            }
            UIHelper.Tooltip("Export this currently selected GPARAM to JSON.");

            ImGui.Separator();

            // Quick Edit
            if (ImGui.BeginMenu("Quick Edit"))
            {
                // Target
                if (ImGui.Selectable("Target"))
                {
                    Parent.QuickEditHandler.UpdateFileFilter(fileEntry.Filename);
                }
                UIHelper.Tooltip("Add this file to the File Filter in the Quick Edit window.");

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    private string CopyAsFileName = "";

    public void CopyAsMenu()
    {
        ImGui.InputText("##copyAsFileNameInput", ref CopyAsFileName, 255);
        UIHelper.Tooltip("Enter the filename this file will be renamed to when copied.");

        if(ImGui.Selectable("Submit"))
        {
            if(CopyAsFileName == "")
            {
                Smithbox.LogError<GparamFileList>("Copy As filename cannot be empty.");
            }
            else
            {
                // Then actually copy the file
                var oldPath = Parent.Selection.SelectedFileEntry.Path;
                var srcPath = Path.Join(ProjectFileLocator.NormalizePath(Project.Descriptor.ProjectPath), oldPath);

                // Fallback to the vanilla version if there isn't an existing project-edited version
                if(!File.Exists(srcPath))
                {
                    srcPath = Path.Join(ProjectFileLocator.NormalizePath(Project.Descriptor.DataPath), oldPath);
                }

                if(!File.Exists(srcPath))
                {
                    Smithbox.LogError<GparamFileList>($"Failed to find the source file: {srcPath}.");
                }
                else
                {
                    // Add the new file to the internal structures so it is immediately editable
                    var oldName = Parent.Selection.SelectedFileEntry.Filename;
                    var newFileEntry = Parent.Selection.SelectedFileEntry.Clone();
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
