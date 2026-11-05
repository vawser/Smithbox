using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.HavokEditor;

public class HavokBinderView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public string BinderFilter = "";
    public bool ExactBinderFilter = false;

    public HavokBinderView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Draw()
    {
        GUI.TitleHeader(
            LOC.Get("HAVOK_BinderView_Header"),
            LOC.Get("HAVOK_BinderView_Header_TT"));

        var data = Project.Handler.HavokData;

        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            DisplayBinderList(Project.Locator.HavokAnimationFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            DisplayBinderList(Project.Locator.HavokBehaviorFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            DisplayBinderList(Project.Locator.HavokCharacterFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            DisplayBinderList(Project.Locator.HavokCollisionFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            DisplayBinderList(Project.Locator.HavokAssetFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            DisplayBinderList(Project.Locator.HavokNavmeshFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            DisplayBinderList(Project.Locator.HavokCutsceneFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            DisplayBinderList(Project.Locator.HavokPartFiles.Entries);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            DisplayBinderList(Project.Locator.HavokRumbleFiles.Entries);
        }
        else
        {
            ImGui.BeginChild("havokBinderSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_BinderView_No_Source_File_Selected"));

            ImGui.EndChild();
        }
    }

    // The filtered binder list, rebuilt only when something it depends on changes.
    // Asset collision alone has thousands of binders, so filtering every frame stalls the editor.
    private List<FileDictionaryEntry> FilteredEntries = new();
    private HashSet<FileDictionaryEntry> FilteredSource;
    private int FilteredSourceCount = -1;
    private string FilteredFilter;
    private bool FilteredExact;
    private bool FilteredFullPath;
    private int FilteredCollisionType = -1;
    private int FilteredAliasVersion = -1;

    private void UpdateFilteredEntries(HashSet<FileDictionaryEntry> entries)
    {
        var fullPath = CFG.Current.HavokEditor_BinderList_Display_Full_Path;
        var collisionType = View.Selection.CategoryMode is HavokCategoryMode.Map_Collision
            ? CFG.Current.HavokEditor_FileList_Collision_Type_Filter
            : 0;

        if (FilteredSource == entries
            && FilteredSourceCount == entries.Count
            && FilteredFilter == BinderFilter
            && FilteredExact == ExactBinderFilter
            && FilteredFullPath == fullPath
            && FilteredCollisionType == collisionType
            && FilteredAliasVersion == View.Selection.BinderAliasVersion)
            return;

        FilteredSource = entries;
        FilteredSourceCount = entries.Count;
        FilteredFilter = BinderFilter;
        FilteredExact = ExactBinderFilter;
        FilteredFullPath = fullPath;
        FilteredCollisionType = collisionType;
        FilteredAliasVersion = View.Selection.BinderAliasVersion;

        FilteredEntries.Clear();

        foreach (var entry in entries)
        {
            var displayName = fullPath ? entry.Path : entry.Filename;

            View.Selection.BinderAliasCache.TryGetValue(entry.Filename, out var binderAlias);

            if (!EditorFilters.IsMatch(BinderFilter, displayName, ExactBinderFilter, binderAlias ?? ""))
                continue;

            if (collisionType == 1 && !entry.Filename.StartsWith("l"))
                continue;

            if (collisionType == 2 && !entry.Filename.StartsWith("h"))
                continue;

            if (collisionType == 3 && !entry.Filename.StartsWith("f"))
                continue;

            FilteredEntries.Add(entry);
        }
    }

    public void DisplayBinderList(HashSet<FileDictionaryEntry> entries)
    {
        DisplayHeader();

        ImGui.BeginChild("havokBinderSection", ImGuiChildFlags.Borders);

        UpdateFilteredEntries(entries);

        var bankDict = View.HavokBank.GetBank(View.Selection.CategoryMode);

        // Only the visible rows are drawn (rows are one line each, which the clipper relies on)
        var clipper = new ImGuiListClipper();
        clipper.Begin(FilteredEntries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var entry = FilteredEntries[i];
                var selected = View.Selection.BinderFileEntry == entry;
                var displayName = FilteredFullPath ? entry.Path : entry.Filename;

                // Mark binders that contain files with unsaved changes
                if (bankDict != null && View.HavokBank.IsBinderDirty(bankDict, entry))
                {
                    displayName = $"{displayName}*";
                }

                if (ImGui.Selectable($"{displayName}##binderEntry_{entry.Filename}", selected))
                {
                    View.Selection.ClearFileSelection();
                    View.Selection.BinderFileEntry = entry;

                    View.Selection.ClearFileAliasCache();
                    PopulateFileList(true);
                }

                if (CFG.Current.HavokEditor_BinderList_Display_Aliases
                    && View.Selection.BinderAliasCache.TryGetValue(entry.Filename, out var alias))
                {
                    // Not wrapped: a wrapped alias would make the row taller than the clipper expects
                    GUI.DisplayAlias(alias, false);
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        ImGui.BeginChild($"framedList_HavokBinderList", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokBinderSearch", ref BinderFilter, ref ExactBinderFilter);

        // Toggle: Full Paths
        GUI.DisplayToggleButton("fullPathToggle", Icons.FileText,
            ref CFG.Current.HavokEditor_BinderList_Display_Full_Path,
            "HAVOK_BinderView_BinderPath_Display_Short",
            "HAVOK_BinderView_BinderPath_Display_Full",
            "HAVOK_BinderView_BinderPath_Display_TT");

        // Toggle: Aliases
        GUI.DisplayToggleButton("aliasToggle", Icons.Book,
            ref CFG.Current.HavokEditor_BinderList_Display_Aliases,
            "HAVOK_BinderView_BinderPath_Display_Alias_Hide",
            "HAVOK_BinderView_BinderPath_Display_Alias_Show",
            "HAVOK_BinderView_BinderPath_Display_Alias_TT");

        if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            // Toggle: Collision Type
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Database}##collisionTypeToggle", DPI.IconButtonSize))
            {
                if (CFG.Current.HavokEditor_FileList_Collision_Type_Filter == 0)
                {
                    CFG.Current.HavokEditor_FileList_Collision_Type_Filter = 1;
                }
                else if (CFG.Current.HavokEditor_FileList_Collision_Type_Filter == 1)
                {
                    CFG.Current.HavokEditor_FileList_Collision_Type_Filter = 2;
                }
                else if (CFG.Current.HavokEditor_FileList_Collision_Type_Filter == 2)
                {
                    CFG.Current.HavokEditor_FileList_Collision_Type_Filter = 3;
                }
                else if (CFG.Current.HavokEditor_FileList_Collision_Type_Filter == 3)
                {
                    CFG.Current.HavokEditor_FileList_Collision_Type_Filter = 0;
                }
            }

            var toggleMode = "";
            if (CFG.Current.HavokEditor_FileList_Collision_Type_Filter == 0)
            {
                toggleMode = LOC.Get("HAVOK_FileView_FilePath_Collision_Type_Filter_All");
            }
            else if (CFG.Current.HavokEditor_FileList_Collision_Type_Filter == 1)
            {
                toggleMode = LOC.Get("HAVOK_FileView_FilePath_Collision_Type_Filter_Low");
            }
            else if (CFG.Current.HavokEditor_FileList_Collision_Type_Filter == 2)
            {
                toggleMode = LOC.Get("HAVOK_FileView_FilePath_Collision_Type_Filter_High");
            }
            else if (CFG.Current.HavokEditor_FileList_Collision_Type_Filter == 3)
            {
                toggleMode = LOC.Get("HAVOK_FileView_FilePath_Collision_Type_Filter_Fall");
            }

            GUI.Tooltip(LOC.Get("HAVOK_FileView_FilePath_Collision_Type_Filter_TT", toggleMode));
        }

        ImGui.EndChild();
    }

    public void Shortcuts()
    {

    }

    public void PopulateFileList(bool clearCache = false)
    {
        if (View.Selection.CategoryMode is HavokCategoryMode.Animation)
        {
            View.HavokBank.PopulateAnimationBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Behavior)
        {
            View.HavokBank.PopulateBehaviorBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Character)
        {
            View.HavokBank.PopulateCharacterBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision)
        {
            View.HavokBank.PopulateMapCollisionBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Asset_Collision)
        {
            View.HavokBank.PopulateAssetCollisionBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Navmesh)
        {
            View.HavokBank.PopulateNavmeshBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Cutscene)
        {
            View.HavokBank.PopulateCutsceneBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Part_Collidable)
        {
            View.HavokBank.PopulatePartBank(View.Selection.BinderFileEntry, clearCache);
        }
        else if (View.Selection.CategoryMode is HavokCategoryMode.Rumble)
        {
            View.HavokBank.PopulateRumbleBank(View.Selection.BinderFileEntry, clearCache);
        }

        RebuildFileAliasCache();
    }

    /// <summary>
    /// Rebuild this view's file aliases from the files in the selected binder
    /// </summary>
    public void RebuildFileAliasCache()
    {
        View.Selection.ClearFileAliasCache();

        // Map collision files have no aliases
        if (View.HavokBank.IsCombinedCategory(View.Selection.CategoryMode))
            return;

        var bankDict = View.HavokBank.GetBank(View.Selection.CategoryMode);

        if (bankDict == null || View.Selection.BinderFileEntry == null)
            return;

        if (!bankDict.TryGetValue(View.Selection.BinderFileEntry, out var files))
            return;

        foreach (var filename in files.Keys)
        {
            View.Selection.AddToFileAliasCache(filename);
        }
    }
}