using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg selection, viewing and editing.
/// </summary>
public class TextFileList
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    private List<EntryGroupAssociation> Groupings;

    public TextFileList(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the fmg view
    /// </summary>
    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader("Files", "");

        Parent.Filters.DisplayFmgFilterSearch();

        ImGui.BeginChild("FmgFileList", new Vector2(width, height), ImGuiChildFlags.Borders);

        DisplayFmgList();

        ImGui.EndChild();
    }

    public void DisplayFmgList()
    {
        if (Parent.Selection.SelectedContainerWrapper != null && Parent.Selection.SelectedContainerWrapper.FmgWrappers != null)
        {
            // Ignore the grouping stuff for DS2 as it happens on the FMG Container level
            if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                {
                    HandleFmgView(fmgInfo);
                }
            }
            else
            {
                // If not in Simple mode, display all FMG files fully
                if (!CFG.Current.TextEditor_Text_File_List_Grouped_Display)
                {
                    var info = Parent.Selection.SelectedContainerWrapper;

                    // Common
                    if (TextUtils.HasGroupEntries(Project, info, "Common"))
                    {
                        if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

                                if (displayGroup == "Common")
                                {
                                    HandleFmgView(fmgInfo);
                                }
                            }
                        }
                    }

                    // Menu
                    if (TextUtils.HasGroupEntries(Project, info, "Menu"))
                    {
                        if (ImGui.CollapsingHeader("Menu", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

                                if (displayGroup == "Menu")
                                {
                                    HandleFmgView(fmgInfo);
                                }
                            }
                        }
                    }

                    // Title
                    if (TextUtils.HasGroupEntries(Project, info, "Title"))
                    {
                        if (ImGui.CollapsingHeader("Titles", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

                                if (displayGroup == "Title")
                                {
                                    HandleFmgView(fmgInfo);
                                }
                            }
                        }
                    }

                    // Summary
                    if (TextUtils.HasGroupEntries(Project, info, "Summary"))
                    {
                        if (ImGui.CollapsingHeader("Summaries", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

                                if (displayGroup == "Summary")
                                {
                                    HandleFmgView(fmgInfo);
                                }
                            }
                        }
                    }

                    // Description
                    if (TextUtils.HasGroupEntries(Project, info, "Description"))
                    {
                        if (ImGui.CollapsingHeader("Descriptions", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

                                if (displayGroup == "Description")
                                {
                                    HandleFmgView(fmgInfo);
                                }
                            }
                        }
                    }

                    // Effect
                    if (TextUtils.HasGroupEntries(Project, info, "Effect"))
                    {
                        if (ImGui.CollapsingHeader("Effects", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

                                if (displayGroup == "Effect")
                                {
                                    HandleFmgView(fmgInfo);
                                }
                            }
                        }
                    }

                    // Unknown - Fallback group
                    if (TextUtils.HasGroupEntries(Project, info, "Unknown"))
                    {
                        if (ImGui.CollapsingHeader("Unknown", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

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
                    var info = Parent.Selection.SelectedContainerWrapper;

                    // Base
                    if (TextUtils.HasDLCEntries(Project, info, ""))
                    {
                        if (ImGui.CollapsingHeader("Base", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);
                                var dlcGroup = TextUtils.GetFmgDlcGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

                                if (TextUtils.IsSimpleFmg(displayGroup) && dlcGroup == "")
                                {
                                    HandleFmgView(fmgInfo);
                                    if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.ER)
                                    {
                                        UIHelper.Tooltip("This FMG has the highest priority for new entries, so it is recommended you always add new entries in this section.");
                                    }
                                }
                            }
                        }
                    }
                    // DLC 1
                    if (TextUtils.HasDLCEntries(Project, info, "DLC 1"))
                    {
                        if (ImGui.CollapsingHeader("DLC 1", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);
                                var dlcGroup = TextUtils.GetFmgDlcGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

                                if (TextUtils.IsSimpleFmg(displayGroup) && dlcGroup == "DLC 1")
                                {
                                    HandleFmgView(fmgInfo);
                                    if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.ER)
                                    {
                                        UIHelper.Tooltip("This FMG contains entries associated with DLC 1, edit them here.\n\nHowever, it is NOT recommended to add new entries in this FMG, as any entry with the same ID in the Base section FMG will take precedence.");
                                    }
                                }
                            }
                        }
                    }
                    // DLC 2
                    if (TextUtils.HasDLCEntries(Project, info, "DLC 2"))
                    {
                        if (ImGui.CollapsingHeader("DLC 2", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            foreach (var fmgInfo in Parent.Selection.SelectedContainerWrapper.FmgWrappers)
                            {
                                var id = fmgInfo.ID;
                                var fmgName = fmgInfo.Name;
                                var displayGroup = TextUtils.GetFmgGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);
                                var dlcGroup = TextUtils.GetFmgDlcGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

                                if (TextUtils.IsSimpleFmg(displayGroup) && dlcGroup == "DLC 2")
                                {
                                    HandleFmgView(fmgInfo);
                                    if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.ER)
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

    private void HandleFmgView(TextFmgWrapper info)
    {
        var id = info.ID;
        var fmgName = info.Name;
        var displayName = TextUtils.GetFmgDisplayName(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);
        var dlcGroupingName = TextUtils.GetFmgDlcGrouping(Project, Parent.Selection.SelectedContainerWrapper, id, fmgName);

        if (Parent.Filters.IsFmgFilterMatch(fmgName, displayName, id))
        {
            var selectableName = $"{displayName}";

            if (!CFG.Current.TextEditor_Text_File_List_Display_Community_Names)
            {
                selectableName = $"{fmgName}";
            }

            if (CFG.Current.TextEditor_Text_File_List_Display_ID)
            {
                selectableName = $"[{id}] {selectableName}";
            }

            // Only show DLC markers in non Simple mode
            if(!CFG.Current.TextEditor_Text_File_List_Grouped_Display && dlcGroupingName != "")
            {
                selectableName = $"{selectableName} [{dlcGroupingName}]";
            }

            // Script row
            if (ImGui.Selectable($"{selectableName}##{id}{selectableName}", id == Parent.Selection.SelectedFmgKey))
            {
                Parent.Selection.SelectFmg(info);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && Parent.Selection.SelectNextFmg)
            {
                Parent.Selection.SelectNextFmg = false;
                Parent.Selection.SelectFmg(info);
            }

            if (ImGui.IsItemFocused())
            {
                if (InputManager.HasArrowSelection())
                {
                    Parent.Selection.SelectNextFmg = true;
                }
            }

            // Only apply to selection
            if (Parent.Selection.SelectedFmgKey != -1)
            {
                if (Parent.Selection.SelectedFmgKey == id)
                {
                    Parent.ContextMenu.FmgContextMenu(info);
                }

                if (Parent.Selection.FocusFmgSelection && Parent.Selection.SelectedFmgKey == id)
                {
                    Parent.Selection.FocusFmgSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }
}
