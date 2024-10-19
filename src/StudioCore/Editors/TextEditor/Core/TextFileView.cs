using HKLib.hk2018.hkaiCollisionAvoidance;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core.Project;
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
                        // DS2 
                        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                        {
                            if (ImGui.CollapsingHeader($"{category.GetDisplayName()}", flags))
                            {
                                if (ImGui.CollapsingHeader($"Common", flags))
                                {
                                    foreach (var (path, info) in TextBank.FmgBank)
                                    {
                                        var fmgInfo = info.FmgInfos.First();
                                        var id = fmgInfo.ID;
                                        var fmgName = fmgInfo.Name;
                                        var displayGroup = TextUtils.GetFmgGrouping(info, id, fmgName);

                                        if (displayGroup == "Common")
                                        {
                                            if (info.Category == category)
                                            {
                                                DisplayCategory(info, index);
                                            }
                                            index++;
                                        }
                                    }
                                }

                                if (ImGui.CollapsingHeader($"Blood Message", flags))
                                {
                                    foreach (var (path, info) in TextBank.FmgBank)
                                    {
                                        var fmgInfo = info.FmgInfos.First();
                                        var id = fmgInfo.ID;
                                        var fmgName = fmgInfo.Name;
                                        var displayGroup = TextUtils.GetFmgGrouping(info, id, fmgName);

                                        if (displayGroup == "Blood Message")
                                        {
                                            if (info.Category == category)
                                            {
                                                DisplayCategory(info, index);
                                            }
                                            index++;
                                        }
                                    }
                                }

                                if (ImGui.CollapsingHeader($"Talk", flags))
                                {
                                    foreach (var (path, info) in TextBank.FmgBank)
                                    {
                                        var fmgInfo = info.FmgInfos.First();
                                        var id = fmgInfo.ID;
                                        var fmgName = fmgInfo.Name;
                                        var displayGroup = TextUtils.GetFmgGrouping(info, id, fmgName);

                                        if (displayGroup == "Talk")
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
                        }
                        // Normal
                        else
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
            // To get nice DS2 names, apply the FMG display name stuff on the container level
            if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                displayName = TextUtils.GetFmgDisplayName(info, -1, info.Name);
            }
            else
            {
                displayName = TextUtils.GetPrettyContainerName(info.Name);
            }
        }

        // Only show unused containers in Advanced mode
        if(!CFG.Current.TextEditor_AdvancedPresentationMode)
        {
            // Hide Base and DLC1 containers as they are not used
            if(Smithbox.ProjectType is ProjectType.ER)
            {
                if(info.Name == "item.msgbnd.dcx" || info.Name == "menu.msgbnd.dcx" ||
                   info.Name == "item_dlc01.msgbnd.dcx" || info.Name == "menu_dlc01.msgbnd.dcx")
                {
                    return;
                }
            }
            // Hide Base and DLC1 containers as they are not used
            if (Smithbox.ProjectType is ProjectType.DS3)
            {
                if (info.Name == "item_dlc1.msgbnd.dcx" || info.Name == "menu_dlc1.msgbnd.dcx")
                {
                    return;
                }
            }
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
