using HKLib.hk2018.hkAsyncThreadPool;
using Hexa.NET.ImGui;
using Silk.NET.OpenGL;
using StudioCore.Configuration;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MSBS.Event;
using StudioCore.Core;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg selection, viewing and editing.
/// </summary>
public class TextFmgView
{
    public TextEditorScreen Editor;
    public TextPropertyDecorator Decorator;
    public TextSelectionManager Selection;
    public TextFilters Filters;
    public TextContextMenu ContextMenu;
    public TextDifferenceManager DifferenceManager;

    private List<EntryGroupAssociation> Groupings;

    public TextFmgView(TextEditorScreen screen)
    {
        Editor = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// The main UI for the fmg view
    /// </summary>
    public void Display()
    {
        if (ImGui.Begin("Text Files##fmgList"))
        {
            Selection.SwitchWindowContext(TextEditorContext.Fmg);

            Filters.DisplayFmgFilterSearch();

            ImGui.BeginChild("FmgFileList");
            Selection.SwitchWindowContext(TextEditorContext.Fmg);

            if (Selection.SelectedContainerWrapper != null && Selection.SelectedContainerWrapper.FmgWrappers != null)
            {
                // Ignore the grouping stuff for DS2 as it happens on the FMG Container level
                if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
                {
                    foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                    {
                        HandleFmgView(fmgInfo);
                    }
                }
                else
                {
                    // If not in Simple mode, display all FMG files fully
                    if (!CFG.Current.TextEditor_SimpleFmgList)
                    {
                        var info = Selection.SelectedContainerWrapper;

                        // Common
                        if (TextUtils.HasGroupEntries(Editor.Project, info, "Common"))
                        {
                            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (displayGroup == "Common")
                                    {
                                        HandleFmgView(fmgInfo);
                                    }
                                }
                            }
                        }

                        // Menu
                        if (TextUtils.HasGroupEntries(Editor.Project, info, "Menu"))
                        {
                            if (ImGui.CollapsingHeader("Menu", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (displayGroup == "Menu")
                                    {
                                        HandleFmgView(fmgInfo);
                                    }
                                }
                            }
                        }

                        // Title
                        if (TextUtils.HasGroupEntries(Editor.Project, info, "Title"))
                        {
                            if (ImGui.CollapsingHeader("Titles", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (displayGroup == "Title")
                                    {
                                        HandleFmgView(fmgInfo);
                                    }
                                }
                            }
                        }

                        // Summary
                        if (TextUtils.HasGroupEntries(Editor.Project, info, "Summary"))
                        {
                            if (ImGui.CollapsingHeader("Summaries", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (displayGroup == "Summary")
                                    {
                                        HandleFmgView(fmgInfo);
                                    }
                                }
                            }
                        }

                        // Description
                        if (TextUtils.HasGroupEntries(Editor.Project, info, "Description"))
                        {
                            if (ImGui.CollapsingHeader("Descriptions", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (displayGroup == "Description")
                                    {
                                        HandleFmgView(fmgInfo);
                                    }
                                }
                            }
                        }

                        // Effect
                        if (TextUtils.HasGroupEntries(Editor.Project, info, "Effect"))
                        {
                            if (ImGui.CollapsingHeader("Effects", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (displayGroup == "Effect")
                                    {
                                        HandleFmgView(fmgInfo);
                                    }
                                }
                            }
                        }

                        // Unknown - Fallback group
                        if (TextUtils.HasGroupEntries(Editor.Project, info, "Unknown"))
                        {
                            if (ImGui.CollapsingHeader("Unknown", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (displayGroup == "Unknown")
                                    {
                                        HandleFmgView(fmgInfo);
                                    }
                                }
                            }
                        }
                    }
                    // Otherwise, display only the Title FMG files and ungrouped FMG files, split by Base/DLC1/DLC2
                    else
                    {
                        var info = Selection.SelectedContainerWrapper;

                        // Base
                        if (TextUtils.HasDLCEntries(Editor.Project, info, ""))
                        {
                            if (ImGui.CollapsingHeader("Base", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);
                                    var dlcGroup = TextUtils.GetFmgDlcGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (TextUtils.IsSimpleFmg(displayGroup) && dlcGroup == "")
                                    {
                                        HandleFmgView(fmgInfo);
                                        if (CFG.Current.TextEditor_DisplayFmgPrecedenceHint)
                                        {
                                            if (Editor.Project.ProjectType is ProjectType.DS3 or ProjectType.ER)
                                            {
                                                UIHelper.Tooltip("This FMG has the highest priority for new entries, so it is recommended you always add new entries in this section.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // DLC 1
                        if (TextUtils.HasDLCEntries(Editor.Project, info, "DLC 1"))
                        {
                            if (ImGui.CollapsingHeader("DLC 1", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);
                                    var dlcGroup = TextUtils.GetFmgDlcGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (TextUtils.IsSimpleFmg(displayGroup) && dlcGroup == "DLC 1")
                                    {
                                        HandleFmgView(fmgInfo);
                                        if (CFG.Current.TextEditor_DisplayFmgPrecedenceHint)
                                        {
                                            if (Editor.Project.ProjectType is ProjectType.DS3 or ProjectType.ER)
                                            {
                                                UIHelper.Tooltip("This FMG contains entries associated with DLC 1, edit them here.\n\nHowever, it is NOT recommended to add new entries in this FMG, as any entry with the same ID in the Base section FMG will take precedence.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        // DLC 2
                        if (TextUtils.HasDLCEntries(Editor.Project, info, "DLC 2"))
                        {
                            if (ImGui.CollapsingHeader("DLC 2", ImGuiTreeNodeFlags.DefaultOpen))
                            {
                                foreach (var fmgInfo in Selection.SelectedContainerWrapper.FmgWrappers)
                                {
                                    var id = fmgInfo.ID;
                                    var fmgName = fmgInfo.Name;
                                    var displayGroup = TextUtils.GetFmgGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);
                                    var dlcGroup = TextUtils.GetFmgDlcGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

                                    if (TextUtils.IsSimpleFmg(displayGroup) && dlcGroup == "DLC 2")
                                    {
                                        HandleFmgView(fmgInfo);
                                        if (CFG.Current.TextEditor_DisplayFmgPrecedenceHint)
                                        {
                                            if (Editor.Project.ProjectType is ProjectType.DS3 or ProjectType.ER)
                                            {
                                                UIHelper.Tooltip("This FMG contains entries associated with DLC 2, edit them here.\n\nHowever, it is NOT recommended to add new entries in this FMG, as any entry with the same ID in the Base or DLC 1 section FMG will take precedence.");
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            ImGui.EndChild();
            ImGui.End();
        }
    }

    private void HandleFmgView(TextFmgWrapper info)
    {
        var id = info.ID;
        var fmgName = info.Name;
        var displayName = TextUtils.GetFmgDisplayName(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);
        var dlcGroupingName = TextUtils.GetFmgDlcGrouping(Editor.Project, Selection.SelectedContainerWrapper, id, fmgName);

        if (Filters.IsFmgFilterMatch(fmgName, displayName, id))
        {
            var selectableName = $"{displayName}";

            if (!CFG.Current.TextEditor_DisplayFmgPrettyName)
            {
                selectableName = $"{fmgName}";
            }

            if (CFG.Current.TextEditor_DisplayFmgID)
            {
                selectableName = $"[{id}] {selectableName}";
            }

            // Only show DLC markers in non Simple mode
            if(!CFG.Current.TextEditor_SimpleFmgList && dlcGroupingName != "")
            {
                selectableName = $"{selectableName} [{dlcGroupingName}]";
            }

            // Script row
            if (ImGui.Selectable($"{selectableName}##{id}{selectableName}", id == Selection.SelectedFmgKey))
            {
                Selection.SelectFmg(info);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && Selection.SelectNextFmg)
            {
                Selection.SelectNextFmg = false;
                Selection.SelectFmg(info);
            }
            if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
            {
                Selection.SelectNextFmg = true;
            }

            // Only apply to selection
            if (Selection.SelectedFmgKey != -1)
            {
                if (Selection.SelectedFmgKey == id)
                {
                    ContextMenu.FmgContextMenu(info);
                }

                if (Selection.FocusFmgSelection && Selection.SelectedFmgKey == id)
                {
                    Selection.FocusFmgSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }
}
