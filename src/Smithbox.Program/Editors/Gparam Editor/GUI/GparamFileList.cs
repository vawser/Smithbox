using Hexa.NET.ImGui;
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

        for(int i = 0; i < Parent.Project.Handler.GparamData.PrimaryBank.Entries.Count; i++)
        {
            var entry = Parent.Project.Handler.GparamData.PrimaryBank.Entries.ElementAt(i);

            var alias = AliasHelper.GetGparamAliasName(Parent.Project, entry.Key.Filename);

            if (Parent.Filters.IsFileFilterMatch(entry.Key.Filename, alias))
            {
                ImGui.BeginGroup();

                // File row
                if (ImGui.Selectable($@" {entry.Key.Filename}", entry.Key.Filename == Parent.Selection._selectedGparamKey))
                {
                    Parent.Selection.SetFileSelection(entry.Key);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Parent.Selection.SelectGparamFile)
                {
                    Parent.Selection.SelectGparamFile = false;

                    Parent.Selection.SetFileSelection(entry.Key);
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
            }

            ContextMenu(entry.Key);
        }

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

        ImGui.EndChild();
    }

    public void ContextMenu(FileDictionaryEntry entry)
    {
        if (entry.Filename == Parent.Selection._selectedGparamKey)
        {
            if (ImGui.BeginPopupContextItem($"Options##Gparam_File_Context"))
            {
                if(ImGui.BeginMenu("Copy As"))
                {
                    CopyAsMenu();

                    ImGui.EndMenu();
                }

                if (IsDeletableGparamFile(entry))
                {
                    if (ImGui.Selectable("Delete"))
                    {
                        DeleteGparamFile(entry);
                    }
                    UIHelper.Tooltip("Will delete this GPARAM.");
                }

                ImGui.Separator();

                if (ImGui.BeginMenu("Quick Edit"))
                {
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
