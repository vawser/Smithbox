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

    public string FmgListFilter = "";
    public bool ExactFmgListFilter = false;

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
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_FileList_Header_Files"),
            LOC.Get("TEXT_FileList_Header_Files_TT"));

        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"textEditor_FileList_Header", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("textEditor_FmgList",
            ref FmgListFilter, ref ExactFmgListFilter);

        // Toggle Display Mode
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##toggleDisplayMode"))
        {
            CFG.Current.TextEditor_Text_File_List_Grouped_Display = !CFG.Current.TextEditor_Text_File_List_Grouped_Display;
        }

        var displayMode = LOC.Get("TEXT_FileList_Toggle_Grouped_Display");
        if (!CFG.Current.TextEditor_Text_File_List_Grouped_Display)
            displayMode = LOC.Get("TEXT_FileList_Toggle_Individual_Display");

        UIHelper.Tooltip(
            LOC.Get("TEXT_FileList_Toggle_Display_Type_TT", displayMode));

        ImGui.EndChild();

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
                DisplayFmgListEntries();
            }
        }
    }

    public void DisplayFmgListEntries()
    {
        // If not in Simple mode, display all FMG files fully
        if (!CFG.Current.TextEditor_Text_File_List_Grouped_Display)
        {
            var info = Parent.Selection.SelectedContainerWrapper;

            // Common
            if (TextUtils.HasGroupEntries(Project, info, "Common"))
            {
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_General")}##generalSection", ImGuiTreeNodeFlags.DefaultOpen))
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
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_Menu")}##menuSection", ImGuiTreeNodeFlags.DefaultOpen))
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
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_Title")}##titleSection", ImGuiTreeNodeFlags.DefaultOpen))
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
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_Summary")}##summarySection", ImGuiTreeNodeFlags.DefaultOpen))
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
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_Description")}##descriptionSection", ImGuiTreeNodeFlags.DefaultOpen))
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
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_Effect")}##effectSection", ImGuiTreeNodeFlags.DefaultOpen))
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
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_Unsorted")}##unsortedSection", ImGuiTreeNodeFlags.DefaultOpen))
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
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_Base")}##baseSection", ImGuiTreeNodeFlags.DefaultOpen))
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
                                UIHelper.Tooltip(LOC.Get("TEXT_FileList_Base_Priority_TT"));
                            }
                        }
                    }
                }
            }
            // DLC 1
            if (TextUtils.HasDLCEntries(Project, info, "DLC 1"))
            {
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_DLC1")}##dlc1Section", ImGuiTreeNodeFlags.DefaultOpen))
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
                                UIHelper.Tooltip(LOC.Get("TEXT_FileList_DLC1_Priority_TT"));
                            }
                        }
                    }
                }
            }
            // DLC 2
            if (TextUtils.HasDLCEntries(Project, info, "DLC 2"))
            {
                if (ImGui.CollapsingHeader(
                    $"{LOC.Get("TEXT_FileList_ListHeader_DLC2")}##dlc2section", ImGuiTreeNodeFlags.DefaultOpen))
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
                                UIHelper.Tooltip(LOC.Get("TEXT_FileList_DLC2_Priority_TT"));
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

        var isMatch = EditorFilters.IsMatch(
            FmgListFilter, fmgName, ExactFmgListFilter, displayName, false, false, id.ToString());

        if (isMatch)
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
                    ContextMenu(info);
                }

                if (Parent.Selection.FocusFmgSelection && Parent.Selection.SelectedFmgKey == id)
                {
                    Parent.Selection.FocusFmgSelection = false;
                    ImGui.SetScrollHereY();
                }
            }
        }
    }

    public void ContextMenu(TextFmgWrapper fmgInfo)
    {
        if (ImGui.BeginPopupContextItem($"##FmgContext{fmgInfo.ID}"))
        {
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_FileList_Context_Header_Information")}##infoMenuHEader"))
            {
                if (ImGui.Selectable($"{LOC.Get("TEXT_ContainerList_Context_ID", fmgInfo.ID)}##copyID"))
                {
                    ImGui.SetClipboardText(fmgInfo.ID.ToString());
                }

                if (ImGui.Selectable($"{LOC.Get("TEXT_ContainerList_Context_Name", fmgInfo.ID)}##copyName"))
                {
                    ImGui.SetClipboardText(fmgInfo.Name);
                }

                ImGui.EndMenu();
            }

            // TODO: with grouped FMGs, this will only sync the header FMG, not the associated sub-FMGs, should be fixed.
            Parent.Editor.ToolView.LanguageSyncTool.DisplaySyncOptions(Parent.Selection.SelectedFmgKey);

            Parent.FmgImporter.TextFileDropdownOptions();
            Parent.FmgExporter.TextFileDropdownOptions();

            ImGui.EndPopup();
        }
    }
}
