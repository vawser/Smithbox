using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the file selection, viewing and editing.
/// </summary>
public class TextFileView
{
    private TextEditorScreen Editor;
    private ProjectEntry Project;

    public TextFileView(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file view
    /// </summary>
    public void Display()
    {
        if (ImGui.Begin("Files##FmgContainerFileList"))
        {
            Editor.Selection.SwitchWindowContext(TextEditorContext.File);

            Editor.Filters.DisplayFileFilterSearch();

            int index = 0;

            ImGui.BeginChild("CategoryList");
            Editor.Selection.SwitchWindowContext(TextEditorContext.File);

            // Categories
            foreach (TextContainerCategory category in Enum.GetValues(typeof(TextContainerCategory)))
            {
                ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;

                if (category == CFG.Current.TextEditor_PrimaryCategory)
                {
                    flags = ImGuiTreeNodeFlags.DefaultOpen;
                }

                // Only display if the category contains something
                if (Editor.Project.TextData.PrimaryBank.Entries.Any(e => e.Value.ContainerDisplayCategory == category))
                {
                    if (AllowedCategory(category))
                    {
                        DisplaySubCategories(category, flags, index);
                    }
                }
            }

            ImGui.EndChild();

            ImGui.End();
        }
    }

    /// <summary>
    /// Display the sub-categories if applicable (DS2 only)
    /// </summary>
    private void DisplaySubCategories(TextContainerCategory category, ImGuiTreeNodeFlags flags, int index)
    {
        // DS2 
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            // Category Header
            if (ImGui.CollapsingHeader($"{category.GetDisplayName()}", flags))
            {
                // Common Sub-Header
                if (ImGui.CollapsingHeader($"Common", flags))
                {
                    foreach (var (fileEntry, info) in Editor.Project.TextData.PrimaryBank.Entries)
                    {
                        var fmgWrapper = info.FmgWrappers.First();
                        var id = fmgWrapper.ID;
                        var fmgName = fmgWrapper.Name;
                        var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, info, id, fmgName);

                        if (displayGroup == "Common")
                        {
                            if (info.ContainerDisplayCategory == category)
                            {
                                DisplayFileEntry(fileEntry, info, index);
                            }
                            index++;
                        }
                    }
                }

                // Blood Message Sub-Header
                if (ImGui.CollapsingHeader($"Blood Message", flags))
                {
                    foreach (var (fileEntry, info) in Editor.Project.TextData.PrimaryBank.Entries)
                    {
                        var fmgWrapper = info.FmgWrappers.First();
                        var id = fmgWrapper.ID;
                        var fmgName = fmgWrapper.Name;
                        var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, info, id, fmgName);

                        if (displayGroup == "Blood Message")
                        {
                            if (info.ContainerDisplayCategory == category)
                            {
                                DisplayFileEntry(fileEntry, info, index);
                            }
                            index++;
                        }
                    }
                }

                // Talk Sub-Header
                if (ImGui.CollapsingHeader($"Talk", flags))
                {
                    foreach (var (fileEntry, info) in Editor.Project.TextData.PrimaryBank.Entries)
                    {
                        var fmgWrapper = info.FmgWrappers.First();
                        var id = fmgWrapper.ID;
                        var fmgName = fmgWrapper.Name;
                        var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, info, id, fmgName);

                        if (displayGroup == "Talk")
                        {
                            if (info.ContainerDisplayCategory == category)
                            {
                                DisplayFileEntry(fileEntry, info, index);
                            }
                            index++;
                        }
                    }
                }
            }
        }
        // Normal
        else
        {
            // Category Header
            if (ImGui.CollapsingHeader($"{category.GetDisplayName()}", flags))
            {
                // Get relevant containers for each category
                foreach (var (fileEntry, info) in Editor.Project.TextData.PrimaryBank.Entries)
                {
                    if (info.ContainerDisplayCategory == category)
                    {
                        DisplayFileEntry(fileEntry, info, index);
                    }
                    index++;
                }
            }
        }
    }

    /// <summary>
    /// Each file entry within a category
    /// </summary>
    private void DisplayFileEntry(FileDictionaryEntry entry, TextContainerWrapper wrapper, int index)
    {
        var displayName = wrapper.FileEntry.Filename;

        // Display community name instead of raw container filename
        if(CFG.Current.TextEditor_DisplayCommunityContainerName)
        {
            // To get nice DS2 names, apply the FMG display name stuff on the container level
            if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                displayName = TextUtils.GetFmgDisplayName(Editor.Project, wrapper, -1, wrapper.FileEntry.Filename);
            }
            else
            {
                displayName = wrapper.GetContainerDisplayName();
            }
        }

        // If in Simple mode, hide unused containers
        if (CFG.Current.TextEditor_SimpleFileList)
        {
            if(wrapper.IsContainerUnused())
            {
                return;
            }
        }

        if (Editor.Filters.IsFileFilterMatch(displayName, "", wrapper))
        {
            // Script row
            if (ImGui.Selectable($"{displayName}##{wrapper.FileEntry.Filename}{index}", index == Editor.Selection.SelectedContainerKey))
            {
                Editor.Selection.SelectFileContainer(entry, wrapper, index);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && Editor.Selection.SelectNextFileContainer)
            {
                Editor.Selection.SelectNextFileContainer = false;
                Editor.Selection.SelectFileContainer(entry, wrapper, index);
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
            {
                Editor.Selection.SelectNextFileContainer = true;
            }

            // Only apply to selection
            if (Editor.Selection.SelectedContainerKey != -1)
            {
                if (Editor.Selection.SelectedContainerKey == index)
                {
                    Editor.ContextMenu.FileContextMenu(wrapper);
                }

                if (Editor.Selection.FocusFileSelection && Editor.Selection.SelectedContainerKey == index)
                {
                    Editor.Selection.FocusFileSelection = false;
                    ImGui.SetScrollHereY();
                }
            }

            // Display hint if normal File List is displayed to user knows about the game's usage of the containers
            if (CFG.Current.TextEditor_DisplayContainerPrecedenceHint && !CFG.Current.TextEditor_SimpleFileList)
            {
                if (Editor.Project.ProjectType is ProjectType.DS3 or ProjectType.ER)
                {
                    if (wrapper.FileEntry.Filename.Contains("item") || wrapper.FileEntry.Filename.Contains("menu"))
                    {
                        if (wrapper.FileEntry.Filename.Contains("dlc2") || wrapper.FileEntry.Filename.Contains("dlc02"))
                        {
                            UIHelper.Tooltip("This container is the only one used by the game.\nOnly use this one.");
                        }
                        else if (wrapper.FileEntry.Filename.Contains("dlc1") || wrapper.FileEntry.Filename.Contains("dlc01"))
                        {
                            UIHelper.Tooltip("This container is no longer used by the game.\nDo not use this one.");
                        }
                        else
                        {
                            UIHelper.Tooltip("This container is no longer used by the game.\nDo not use this one.");
                        }
                    }
                }
            }
            if (CFG.Current.TextEditor_DisplaySourcePath)
            {
                UIHelper.Tooltip($"Source File: {wrapper.FileEntry.Path}");
            }
        }
    }

    public bool AllowedCategory(TextContainerCategory category)
    {
        if (CFG.Current.TextEditor_DisplayPrimaryCategoryOnly)
        {
            if (category == CFG.Current.TextEditor_PrimaryCategory)
            {
                return true;
            }

            return false;
        }
        else
        {
            return true;
        }
    }
}
