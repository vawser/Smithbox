using HKLib.hk2018.hkaiCollisionAvoidance;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    if (TextBank.FmgBank.Any(e => e.Value.Category == category) && AllowedCategory(category))
                    {
                        if (ImGui.CollapsingHeader($"{category.GetDisplayName()}", flags))
                        {
                            // Get relevant containers for each category
                            foreach (var (path, info) in TextBank.FmgBank)
                            {
                                if (info.Category == category)
                                {
                                    DisplayCategory(info, index);
                                }
                                index++;
                            }
                        }
                    }
                }

                ImGui.EndChild();
            }

            ImGui.End();
        }
    }

    /// <summary>
    /// Each discrete category: English, German, etc
    /// </summary>
    private void DisplayCategory(TextContainerInfo info, int index)
    {
        var displayName = info.Name;

        if(CFG.Current.TextEditor_DisplayPrettyContainerName)
        {
            displayName = TextUtils.GetPrettyContainerName(info.Name);
        }

        if (Filters.IsFileFilterMatch(displayName, ""))
        {
            // Script row
            if (ImGui.Selectable($"{displayName}##{info.Name}{index}", index == Selection.SelectedContainerKey))
            {
                Selection.SelectFileContainer(info, index);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && Selection.SelectNextFileContainer)
            {
                Selection.SelectNextFileContainer = false;
                Selection.SelectFileContainer(info, index);
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
                    ContextMenu.FileContextMenu(info);
                }

                if (Selection.FocusFileSelection && Selection.SelectedContainerKey == index)
                {
                    Selection.FocusFileSelection = false;
                    ImGui.SetScrollHereY();
                }
            }

            if (CFG.Current.TextEditor_DisplaySourcePath)
            {
                UIHelper.ShowHoverTooltip($"Source File: {info.AbsolutePath}");
            }
        }
    }

    private bool AllowedCategory(TextContainerCategory category)
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
