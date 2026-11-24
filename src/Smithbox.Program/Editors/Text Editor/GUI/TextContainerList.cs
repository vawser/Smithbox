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
        GUI.TitleHeader(
            LOC.Get("TEXT_ContainerList_Header_Containers"),
            LOC.Get("TEXT_ContainerList_Header_Containers_TT"));

        ImGui.BeginChild($"textEditor_ContainerList_Header", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("textEditor_ContainerList",
            ref ContainerListFilter, ref ExactContainerListFilter);

        // Toggle Primary Display Only
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##displayPrimaryCategoryOnlyToggle"))
        {
            CFG.Current.TextEditor.TextEditor_Container_List_Display_Primary_Category_Only = !CFG.Current.TextEditor.TextEditor_Container_List_Display_Primary_Category_Only;
        }

        var categoryMode = LOC.Get("TEXT_ContainerList_Category_Mode_Primary_Only");
        if (!CFG.Current.TextEditor.TextEditor_Container_List_Display_Primary_Category_Only)
            categoryMode = LOC.Get("TEXT_ContainerList_Category_Mode_All");

        GUI.Tooltip(LOC.Get("TEXT_ContainerList_Category_Mode_TT", categoryMode));

        ImGui.EndChild();

        ImGui.BeginChild("CategoryList", new Vector2(width, height), ImGuiChildFlags.Borders);

        RefreshContainerCache();

        // Categories
        foreach (TextContainerCategory category in Enum.GetValues(typeof(TextContainerCategory)))
        {
            ImGuiTreeNodeFlags flags = ImGuiTreeNodeFlags.None;

            if (category == CFG.Current.TextEditor.TextEditor_Primary_Category)
            {
                flags = ImGuiTreeNodeFlags.DefaultOpen;
            }

            // Only display if the category contains something
            if (_presentCategories.Contains(category))
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
    /// The bank's containers sorted by file entry, and the categories that have any. The sort
    /// ran once per category every frame (plus a scan per category for the presence check).
    /// The bank only gains containers while loading, so it's rebuilt when the bank or its count changes.
    /// </summary>
    private List<KeyValuePair<FileDictionaryEntry, TextContainerWrapper>> _orderedContainers = new();
    private HashSet<TextContainerCategory> _presentCategories = new();
    private TextBank _cachedBank;
    private int _cachedContainerCount = -1;

    private void RefreshContainerCache()
    {
        var bank = Project.Handler.TextData.PrimaryBank;

        if (bank == _cachedBank && bank.Containers.Count == _cachedContainerCount)
            return;

        _orderedContainers = bank.Containers.OrderBy(e => e.Key).ToList();
        _presentCategories = _orderedContainers.Select(e => e.Value.ContainerDisplayCategory).ToHashSet();

        _cachedBank = bank;
        _cachedContainerCount = bank.Containers.Count;
    }

    /// <summary>
    /// Display the sub-categories if applicable (DS2 only)
    /// </summary>
    private void DisplaySubCategories(TextContainerCategory category, ImGuiTreeNodeFlags flags, int index)
    {
        // DS2 
        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            var orderedList = _orderedContainers;
            // GetDisplayName is the loc key (this showed e.g. "TEXT_Language_English")
            var name = LOC.Get(category.GetDisplayName());

            GUI.SimpleHeader(name, "");

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
            if (ImGui.CollapsingHeader($"{LOC.Get("TEXT_ContainerList_DS2_Talk")}##{name}_talk", flags))
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
            var orderedList = _orderedContainers;

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
        if(CFG.Current.TextEditor.TextEditor_Container_List_Display_Community_Names)
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
        if (CFG.Current.TextEditor.TextEditor_Container_List_Hide_Unused_Containers)
        {
            if(wrapper.IsContainerUnused())
            {
                return;
            }
        }

        var isMatch = EditorFilters.IsMatch(ContainerListFilter, displayName, ExactContainerListFilter,
            () => CFG.Current.TextEditor.TextEditor_Include_Vanilla_Cache ? Project.VFS.FileStatus(wrapper.FileEntry.Path) : VanillaStatus.None);

        if (isMatch)
        {
            // Mark containers that contain files with unsaved changes
            if (wrapper.IsDirty())
            {
                displayName = $"{displayName}*";
            }

            // Script row
            // Selected and identified by the file entry itself: the display index depended on
            // which DS2 headers were expanded, so the highlight moved when one collapsed.
            // The saved container against the game's copy (Added / Modified; identical copies stay uncoloured)
            var status = CFG.Current.TextEditor.TextEditor_Include_Vanilla_Cache
                ? Project.VFS.FileStatus(wrapper.FileEntry.Path)
                : VanillaStatus.None;
            var coloured = VanillaMarkers.PushColour(status);

            var clicked = ImGui.Selectable($"{displayName}##{wrapper.FileEntry.Path}", View.Selection.SelectedFileDictionaryEntry == entry);

            VanillaMarkers.PopColour(coloured);
            if (coloured)
                VanillaMarkers.Tooltip(status);

            if (clicked)
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
            if (View.Selection.SelectedFileDictionaryEntry != null)
            {
                if (View.Selection.SelectedFileDictionaryEntry == entry)
                {
                    ContextMenu(wrapper);
                }

                if (View.Selection.FocusFileSelection && View.Selection.SelectedFileDictionaryEntry == entry)
                {
                    View.Selection.FocusFileSelection = false;
                    ImGui.SetScrollHereY();
                }
            }

            // Display hint if normal File List is displayed to user knows about the game's usage of the containers
            if (!CFG.Current.TextEditor.TextEditor_Container_List_Hide_Unused_Containers)
            {
                if (Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.ER)
                {
                    if (wrapper.FileEntry.Filename.Contains("item") || wrapper.FileEntry.Filename.Contains("menu"))
                    {
                        if (wrapper.FileEntry.Filename.Contains("dlc2") || wrapper.FileEntry.Filename.Contains("dlc02"))
                        {
                            GUI.Tooltip(LOC.Get("TEXT_ContainerList_Is_Primary_Container_TT"));
                        }
                        else if (wrapper.FileEntry.Filename.Contains("dlc1") || wrapper.FileEntry.Filename.Contains("dlc01"))
                        {
                            GUI.Tooltip(LOC.Get("TEXT_ContainerList_Obsolete_Container_TT"));
                        }
                        else
                        {
                            GUI.Tooltip(LOC.Get("TEXT_ContainerList_Obsolete_Container_TT"));
                        }
                    }
                }
            }
            if (CFG.Current.TextEditor.TextEditor_Container_List_Display_Source_Path)
            {
                GUI.Tooltip(LOC.Get("TEXT_ContainerList_Source_Path_TT", wrapper.FileEntry.Path));
            }
        }
    }

    public bool AllowedCategory(TextContainerCategory category)
    {
        if (CFG.Current.TextEditor.TextEditor_Container_List_Display_Primary_Category_Only)
        {
            if (category == CFG.Current.TextEditor.TextEditor_Primary_Category)
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
                    ImGui.SetClipboardText(info.FileEntry.Path);
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
