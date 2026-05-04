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

        Parent.Filters.DisplayFileFilterSearch();

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

            DisplayFileSelectable(entry.Key, i);
        }
    }

    private void DisplayFileSelectable(FileDictionaryEntry fileEntry, int index)
    {
        var alias = AliasHelper.GetGparamAliasName(Parent.Project, fileEntry.Filename);

        if (!Parent.Filters.IsFileFilterMatch(fileEntry.Filename, alias))
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

        ContextMenu(fileEntry);
    }

    public void ContextMenu(FileDictionaryEntry entry)
    {
        var fileKey = Parent.Selection._selectedGparamKey;

        if (entry.Filename != fileKey)
            return;

        if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
        {
            // Copy as
            if(ImGui.BeginMenu("Copy As"))
            {
                CopyAsMenu();

                ImGui.EndMenu();
            }

            // Delete
            if (IsDeletableGparamFile(entry))
            {
                if (ImGui.Selectable("Delete"))
                {
                    DeleteGparamFile(entry);
                }
                UIHelper.Tooltip("Will delete this GPARAM.");
            }

            ImGui.Separator();

            // Quick Edit
            if (ImGui.BeginMenu("Quick Edit"))
            {
                // Target
                if (ImGui.Selectable("Target"))
                {
                    Parent.QuickEditHandler.UpdateFileFilter(entry.Filename);
                }
                UIHelper.Tooltip("Add this file to the File Filter in the Quick Edit window.");

                ImGui.EndMenu();
            }

            ImGui.EndPopup();
        }
    }

    private string copyAsFileName = "";

    public void CopyAsMenu()
    {
        ImGui.InputText("##copyAsFileNameInput", ref copyAsFileName, 255);
        UIHelper.Tooltip("Enter the filename this file will be renamed to when copied.");

        if(ImGui.Selectable("Submit"))
        {
            if(copyAsFileName == "")
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
                    newFileEntry.Path = newFileEntry.Path.Replace(oldName, copyAsFileName);
                    newFileEntry.Filename = newFileEntry.Filename.Replace(oldName, copyAsFileName);

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
