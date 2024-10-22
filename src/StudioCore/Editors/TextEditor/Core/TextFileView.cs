using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Linq;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the file selection, viewing and editing.
/// </summary>
public class TextFileView
{
    public TextEditorScreen Screen;
    public TextPropertyDecorator Decorator;
    public TextSelectionManager Selection;
    public TextFilters Filters;
    public TextContextMenu ContextMenu;

    public TextFileView(TextEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// The main UI for the file view
    /// </summary>
    public void Display()
    {
        if (ImGui.Begin("Files##FmgContainerFileList"))
        {
            if (TextBank.PrimaryBankLoaded)
            {
                Filters.DisplayFileFilterSearch();

                int index = 0;

                ImGui.BeginChild("CategoryList");

                // Categories
                foreach (TextContainerCategory category in Enum.GetValues(typeof(TextContainerCategory)))
                {
                    ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;

                    if (category == CFG.Current.TextEditor_PrimaryCategory)
                    {
                        flags = ImGuiTreeNodeFlags.DefaultOpen;
                    }

                    // Only display if the category contains something
                    if (TextBank.FmgBank.Any(e => e.Value.ContainerDisplayCategory == category))
                    {
                        if (AllowedCategory(category))
                        {
                            DisplaySubCategories(category, flags, index);
                        }
                    }
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }
    }

    /// <summary>
    /// Display the sub-categories if applicable (DS2 only)
    /// </summary>
    private void DisplaySubCategories(TextContainerCategory category, ImGuiTreeNodeFlags flags, int index)
    {
        // DS2 
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            // Category Header
            if (ImGui.CollapsingHeader($"{category.GetDisplayName()}", flags))
            {
                // Common Sub-Header
                if (ImGui.CollapsingHeader($"Common", flags))
                {
                    foreach (var (path, info) in TextBank.FmgBank)
                    {
                        var fmgWrapper = info.FmgWrappers.First();
                        var id = fmgWrapper.ID;
                        var fmgName = fmgWrapper.Name;
                        var displayGroup = TextUtils.GetFmgGrouping(info, id, fmgName);

                        if (displayGroup == "Common")
                        {
                            if (info.ContainerDisplayCategory == category)
                            {
                                DisplayFileEntry(info, index);
                            }
                            index++;
                        }
                    }
                }

                // Blood Message Sub-Header
                if (ImGui.CollapsingHeader($"Blood Message", flags))
                {
                    foreach (var (path, info) in TextBank.FmgBank)
                    {
                        var fmgWrapper = info.FmgWrappers.First();
                        var id = fmgWrapper.ID;
                        var fmgName = fmgWrapper.Name;
                        var displayGroup = TextUtils.GetFmgGrouping(info, id, fmgName);

                        if (displayGroup == "Blood Message")
                        {
                            if (info.ContainerDisplayCategory == category)
                            {
                                DisplayFileEntry(info, index);
                            }
                            index++;
                        }
                    }
                }

                // Talk Sub-Header
                if (ImGui.CollapsingHeader($"Talk", flags))
                {
                    foreach (var (path, info) in TextBank.FmgBank)
                    {
                        var fmgWrapper = info.FmgWrappers.First();
                        var id = fmgWrapper.ID;
                        var fmgName = fmgWrapper.Name;
                        var displayGroup = TextUtils.GetFmgGrouping(info, id, fmgName);

                        if (displayGroup == "Talk")
                        {
                            if (info.ContainerDisplayCategory == category)
                            {
                                DisplayFileEntry(info, index);
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
                foreach (var (path, info) in TextBank.FmgBank)
                {
                    if (info.ContainerDisplayCategory == category)
                    {
                        DisplayFileEntry(info, index);
                    }
                    index++;
                }
            }
        }
    }

    /// <summary>
    /// Each file entry within a category
    /// </summary>
    private void DisplayFileEntry(TextContainerWrapper wrapper, int index)
    {
        var displayName = wrapper.Filename;

        // Display community name instead of raw container filename
        if(CFG.Current.TextEditor_DisplayCommunityContainerName)
        {
            // To get nice DS2 names, apply the FMG display name stuff on the container level
            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                displayName = TextUtils.GetFmgDisplayName(wrapper, -1, wrapper.Filename);
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

        if (Filters.IsFileFilterMatch(displayName, ""))
        {
            // Script row
            if (ImGui.Selectable($"{displayName}##{wrapper.Filename}{index}", index == Selection.SelectedContainerKey))
            {
                Selection.SelectFileContainer(wrapper, index);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && Selection.SelectNextFileContainer)
            {
                Selection.SelectNextFileContainer = false;
                Selection.SelectFileContainer(wrapper, index);
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
            {
                Selection.SelectNextFileContainer = true;
            }

            // Only apply to selection
            if (Selection.SelectedContainerKey != -1)
            {
                if (Selection.SelectedContainerKey == index)
                {
                    ContextMenu.FileContextMenu(wrapper);
                }

                if (Selection.FocusFileSelection && Selection.SelectedContainerKey == index)
                {
                    Selection.FocusFileSelection = false;
                    ImGui.SetScrollHereY();
                }
            }

            if (CFG.Current.TextEditor_DisplaySourcePath)
            {
                UIHelper.ShowHoverTooltip($"Source File: {wrapper.ReadPath}");
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
