using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the file selection, viewing and editing.
/// </summary>
public class TextContainerList
{
    private TextEditorView View;
    private ProjectEntry Project;

    private string ContainerListFilter = "";
    private bool ExactContainerListFilter = false;

    public TextContainerList(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file view
    /// </summary>
    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader(
            LOC.Get("TEXT_ContainerList_Header_Containers"),
            LOC.Get("TEXT_ContainerList_Header_Containers_TT"));

        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"textEditor_ContainerList_Header", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("textEditor_ContainerList",
            ref ContainerListFilter, ref ExactContainerListFilter);

        // Toggle Primary Display Only
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##displayPrimaryCategoryOnlyToggle"))
        {
            CFG.Current.TextEditor_Container_List_Display_Primary_Category_Only = !CFG.Current.TextEditor_Container_List_Display_Primary_Category_Only;
        }

        var categoryMode = LOC.Get("TEXT_ContainerList_Category_Mode_Primary_Only");
        if (!CFG.Current.TextEditor_Container_List_Display_Primary_Category_Only)
            categoryMode = LOC.Get("TEXT_ContainerList_Category_Mode_All");

        UIHelper.Tooltip(LOC.Get("TEXT_ContainerList_Category_Mode_TT", categoryMode));

        ImGui.EndChild();

        ImGui.BeginChild("CategoryList", new Vector2(width, height), ImGuiChildFlags.Borders);

        // Categories
        foreach (TextContainerCategory category in Enum.GetValues(typeof(TextContainerCategory)))
        {
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;

            if (category == CFG.Current.TextEditor_Primary_Category)
            {
                flags = ImGuiTreeNodeFlags.DefaultOpen;
            }

            // Only display if the category contains something
            if (Project.Handler.TextData.PrimaryBank.Containers.Any(e => e.Value.ContainerDisplayCategory == category))
            {
                if (AllowedCategory(category))
                {
                    DisplaySubCategories(category, flags, 0);
                }
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Display the sub-categories if applicable (DS2 only)
    /// </summary>
    private void DisplaySubCategories(TextContainerCategory category, ImGuiTreeNodeFlags flags, int index)
    {
        // DS2 
        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var orderedList = Project.Handler.TextData.PrimaryBank.Containers.OrderBy(e => e.Key);
            var name = $"{category.GetDisplayName()}";

            UIHelper.SimpleHeader(name, "");

            // Common Sub-Header
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_ContainerList_DS2_Common")}##{name}_common", flags))
            {
                foreach (var (fileEntry, info) in orderedList)
                {
                    var fmgWrapper = info.FmgWrappers.First();
                    var id = fmgWrapper.ID;
                    var fmgName = fmgWrapper.Name;
                    var displayGroup = TextUtils.GetFmgGrouping(Project, info, id, fmgName);

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
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_ContainerList_DS2_Blood_Message")}##{name}_bloodmsg", flags))
            {
                foreach (var (fileEntry, info) in orderedList)
                {
                    var fmgWrapper = info.FmgWrappers.First();
                    var id = fmgWrapper.ID;
                    var fmgName = fmgWrapper.Name;
                    var displayGroup = TextUtils.GetFmgGrouping(Project, info, id, fmgName);

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
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_ContainerList_DS2_Talk")}##{name}_common", flags))
            {
                foreach (var (fileEntry, info) in orderedList)
                {
                    var fmgWrapper = info.FmgWrappers.First();
                    var id = fmgWrapper.ID;
                    var fmgName = fmgWrapper.Name;
                    var displayGroup = TextUtils.GetFmgGrouping(Project, info, id, fmgName);

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
        // Normal
        else
        {
            var orderedList = Project.Handler.TextData.PrimaryBank.Containers.OrderBy(e => e.Key);

            var displayName = LOC.Get(category.GetDisplayName());

            // Category Header
            if (ImGui.CollapsingHeader($"{displayName}##{category.GetDisplayName()}", flags))
            {
                // Get relevant containers for each category
                foreach (var (fileEntry, info) in orderedList)
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
        if(CFG.Current.TextEditor_Container_List_Display_Community_Names)
        {
            // To get nice DS2 names, apply the FMG display name stuff on the container level
            if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
            {
                displayName = TextUtils.GetFmgDisplayName(Project, wrapper, -1, wrapper.FileEntry.Filename);
            }
            else
            {
                displayName = wrapper.GetContainerDisplayName();
            }
        }

        // If in Simple mode, hide unused containers
        if (CFG.Current.TextEditor_Container_List_Hide_Unused_Containers)
        {
            if(wrapper.IsContainerUnused())
            {
                return;
            }
        }

        var isMatch = EditorFilters.IsMatch(ContainerListFilter, displayName, ExactContainerListFilter);

        if (isMatch)
        {
            // Script row
            if (ImGui.Selectable($"{displayName}##{wrapper.FileEntry.Filename}{index}", index == View.Selection.SelectedContainerKey))
            {
                View.Selection.SelectFileContainer(entry, wrapper, index);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && View.Selection.SelectNextFileContainer)
            {
                View.Selection.SelectNextFileContainer = false;
                View.Selection.SelectFileContainer(entry, wrapper, index);
            }

            if(ImGui.IsItemFocused())
            {           
                if (InputManager.HasArrowSelection())
                {
                    View.Selection.SelectNextFileContainer = true;
                }
            }

            // Only apply to selection
            if (View.Selection.SelectedContainerKey != -1)
            {
                if (View.Selection.SelectedContainerKey == index)
                {
                    ContextMenu(wrapper);
                }

                if (View.Selection.FocusFileSelection && View.Selection.SelectedContainerKey == index)
                {
                    View.Selection.FocusFileSelection = false;
                    ImGui.SetScrollHereY();
                }
            }

            // Display hint if normal File List is displayed to user knows about the game's usage of the containers
            if (!CFG.Current.TextEditor_Container_List_Hide_Unused_Containers)
            {
                if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.ER)
                {
                    if (wrapper.FileEntry.Filename.Contains("item") || wrapper.FileEntry.Filename.Contains("menu"))
                    {
                        if (wrapper.FileEntry.Filename.Contains("dlc2") || wrapper.FileEntry.Filename.Contains("dlc02"))
                        {
                            UIHelper.Tooltip(LOC.Get("TEXT_ContainerList_Is_Primary_Container_TT"));
                        }
                        else if (wrapper.FileEntry.Filename.Contains("dlc1") || wrapper.FileEntry.Filename.Contains("dlc01"))
                        {
                            UIHelper.Tooltip(LOC.Get("TEXT_ContainerList_Obsolete_Container_TT"));
                        }
                        else
                        {
                            UIHelper.Tooltip(LOC.Get("TEXT_ContainerList_Obsolete_Container_TT"));
                        }
                    }
                }
            }
            if (CFG.Current.TextEditor_Container_List_Display_Source_Path)
            {
                UIHelper.Tooltip(LOC.Get("TEXT_ContainerList_Source_Path_TT", wrapper.FileEntry.Path));
            }
        }
    }

    public bool AllowedCategory(TextContainerCategory category)
    {
        if (CFG.Current.TextEditor_Container_List_Display_Primary_Category_Only)
        {
            if (category == CFG.Current.TextEditor_Primary_Category)
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

    public void ContextMenu(TextContainerWrapper info)
    {
        if (ImGui.BeginPopupContextItem($"##FileContext{info.FileEntry.Filename}"))
        {
            // Information
            if (ImGui.BeginMenu($"{LOC.Get("TEXT_ContainerList_Context_Header_Information")}##infoMenuHeader"))
            {
                // Filename
                if (ImGui.Selectable($"{LOC.Get("TEXT_ContainerList_Context_Filename", info.FileEntry.Filename)}##copyFilename"))
                {
                    ImGui.SetClipboardText(info.FileEntry.Filename);
                }

                // Path
                if (ImGui.Selectable($"{LOC.Get("TEXT_ContainerList_Context_Path", info.FileEntry.Path)}##copyPath"))
                {
                    ImGui.SetClipboardText(info.FileEntry.Filename);
                }

                ImGui.Separator();

                // Type
                ImGui.Text(LOC.Get("TEXT_ContainerList_Context_Type", 
                    LOC.Get(info.ContainerType.GetDisplayName())));

                // Display Category
                ImGui.Text(LOC.Get("TEXT_ContainerList_Context_Display_Category",
                    LOC.Get(info.ContainerDisplayCategory.GetDisplayName())));

                // Display Sub-Category
                ImGui.Text(LOC.Get("TEXT_ContainerList_Display_Sub_Category",
                    LOC.Get(info.ContainerDisplaySubCategory.GetDisplayName())));

                // Compression Type
                ImGui.Text(LOC.Get("TEXT_ContainerList_Context_Compression_Type", 
                    info.CompressionType.ToString()));

                ImGui.EndMenu();
            }

            View.ToolView.LanguageSyncTool.DisplaySyncOptions();

            View.FmgImporter.ContainerDropdownOptions();
            View.FmgExporter.ContainerDropdownOptions();

            ImGui.EndPopup();
        }
    }
}
