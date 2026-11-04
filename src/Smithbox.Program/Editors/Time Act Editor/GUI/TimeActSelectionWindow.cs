using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSelectionWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private string TimeActListFilter = "";
    private bool ExactTimeActFilter = false;

    private bool _arrowKeyPressed = false;

    // Rebuilt only when the filter or the time acts change, as the aliases are matched as well
    private List<TimeActEntry> CachedFilteredTimeActs;
    private List<TimeActEntry> CachedTimeActs;
    private int CachedTimeActsVersion;
    private string CachedFilter = "";
    private bool CachedExactFilter = false;

    // The time act whose rename menu is open, and the name being typed for it
    private TimeActEntry RenameTarget;
    private string RenameBuffer = "";

    // The time act to bring into view the next time the list is drawn, such as one jumped to from a search
    private TimeActEntry ScrollTarget;

    public TimeActSelectionWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("TAE_SelectionWindow_List_Header"),
            LOC.Get("TAE_SelectionWindow_List_Header_TT"));

        EditorFilters.DisplayFramedListFilter("timeActEditor_SelectionList_Filter",
            ref TimeActListFilter, ref ExactTimeActFilter);

        ImGui.BeginChild($"SelectionListSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplaySelectionList();

        ImGui.EndChild();
    }

    public void DisplaySelectionList()
    {
        var source = View.Selection.SelectedTimeActContainerWrapper;

        if (source == null)
            return;

        var filteredTimeActs = GetFilteredTimeActs(source);

        if (InputManager.HasArrowSelection())
        {
            _arrowKeyPressed = true;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredTimeActs.Count);

        var scrollIndex = ScrollTarget != null ? filteredTimeActs.IndexOf(ScrollTarget) : -1;

        // A hidden time act can't be brought into view
        if (scrollIndex < 0)
        {
            ScrollTarget = null;
        }
        else
        {
            clipper.IncludeItemByIndex(scrollIndex);
        }

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var entry = filteredTimeActs[i];

                var selected = View.Selection.SelectedTimeAct == entry;

                var alias = GetTimeActAlias(source, entry);

                var labelName = entry.IsDirty ? $"{entry.Name}*" : entry.Name;

                if (ImGui.Selectable($"{labelName}##timeActEntry{entry.BinderID}", selected))
                {
                    SelectTimeAct(entry);
                }

                if (entry == ScrollTarget)
                {
                    ImGui.SetScrollHereY();
                    ScrollTarget = null;
                }

                // The animation count isn't known until the time act has been read
                if (entry.TimeAct != null)
                {
                    GUI.Tooltip(LOC.Get("TAE_SelectionWindow_Entry_Details", entry.Name, entry.TimeAct.Animations.Count));
                }

                if (_arrowKeyPressed && ImGui.IsItemFocused() && !selected)
                {
                    SelectTimeAct(entry);

                    _arrowKeyPressed = false;
                }

                ContextMenu(source, entry);

                if (alias != "")
                {
                    GUI.DisplayAlias(alias, CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
                }
            }
        }

        clipper.End();
    }

    private List<TimeActEntry> GetFilteredTimeActs(TimeActContainerWrapper source)
    {
        if (CachedFilteredTimeActs != null &&
            CachedTimeActs == source.TimeActs &&
            CachedTimeActsVersion == source.TimeActsVersion &&
            CachedFilter == TimeActListFilter &&
            CachedExactFilter == ExactTimeActFilter)
        {
            return CachedFilteredTimeActs;
        }

        CachedTimeActs = source.TimeActs;
        CachedTimeActsVersion = source.TimeActsVersion;
        CachedFilter = TimeActListFilter;
        CachedExactFilter = ExactTimeActFilter;

        CachedFilteredTimeActs = source.TimeActs
            .Where(e => EditorFilters.IsMatch(TimeActListFilter, e.Name, ExactTimeActFilter,
                GetTimeActAlias(source, e), true, true))
            .ToList();

        return CachedFilteredTimeActs;
    }

    /// <summary>
    /// Time act aliases are stored per character: a000 of c0000 is stored as c0000_000.
    /// </summary>
    private string GetTimeActAlias(TimeActContainerWrapper source, TimeActEntry entry)
    {
        return Project.GetAliasName(ProjectAliasType.TimeActs, $"{source.Name}_{entry.Name.Substring(1)}");
    }

    private void ContextMenu(TimeActContainerWrapper source, TimeActEntry entry)
    {
        if (!ImGui.BeginPopupContextItem($"##timeActEntryContext{entry.BinderID}"))
            return;

        var canEdit = Project.Handler.TimeActData.CanSaveTimeActs;

        // Duplicate
        if (ImGui.MenuItem($"{LOC.Get("TAE_SelectionWindow_Context_Duplicate")}##duplicateTimeAct", "", false, canEdit))
        {
            DuplicateTimeAct(source, entry);
        }
        GUI.Tooltip(LOC.Get("TAE_SelectionWindow_Context_Duplicate_TT"));

        // Delete
        if (ImGui.MenuItem($"{LOC.Get("TAE_SelectionWindow_Context_Delete")}##deleteTimeAct", "", false, canEdit))
        {
            View.ViewportActionManager.ExecuteAction(new TimeActAddEntryAction(View, source, entry, true));
        }
        GUI.Tooltip(LOC.Get("TAE_SelectionWindow_Context_Delete_TT"));

        // Rename
        if (ImGui.BeginMenu($"{LOC.Get("TAE_SelectionWindow_Context_Rename")}##renameTimeAct", canEdit))
        {
            RenameMenu(source, entry);

            ImGui.EndMenu();
        }
        else if (RenameTarget == entry)
        {
            RenameTarget = null;
        }

        ImGui.Separator();

        // Import
        if (ImGui.MenuItem($"{LOC.Get("TAE_SelectionWindow_Context_Import")}##importTimeAct", "", false, canEdit))
        {
            ImportTimeAct(source, entry);
        }
        GUI.Tooltip(LOC.Get("TAE_SelectionWindow_Context_Import_TT"));

        // Export
        if (ImGui.MenuItem($"{LOC.Get("TAE_SelectionWindow_Context_Export")}##exportTimeAct"))
        {
            ExportTimeAct(source, entry);
        }
        GUI.Tooltip(LOC.Get("TAE_SelectionWindow_Context_Export_TT", GetExportFolderLabel()));

        // Set Export Folder
        if (ImGui.MenuItem($"{LOC.Get("TAE_SelectionWindow_Context_Set_Export_Folder")}##setTimeActExportFolder"))
        {
            SelectExportFolder();
        }
        GUI.Tooltip(LOC.Get("TAE_SelectionWindow_Context_Set_Export_Folder_TT", GetExportFolderLabel()));

        // Open Export Folder
        if (ImGui.MenuItem($"{LOC.Get("TAE_SelectionWindow_Context_Open_Export_Folder")}##openTimeActExportFolder", "",
            false, Directory.Exists(CFG.Current.TimeActEditor_Export_Directory)))
        {
            Process.Start("explorer.exe", CFG.Current.TimeActEditor_Export_Directory);
        }

        ImGui.EndPopup();
    }

    /// <summary>
    /// The name is checked as it is typed, so it can only be applied once it fits the mask and is free
    /// </summary>
    private void RenameMenu(TimeActContainerWrapper source, TimeActEntry entry)
    {
        // Filled in with the current name each time the menu is opened
        if (RenameTarget != entry)
        {
            RenameTarget = entry;
            RenameBuffer = entry.Name;
        }

        // The buffer is updated as it is typed, so the name is checked live and applied once the field is left
        ImGui.InputText("##timeActRenameInput", ref RenameBuffer, 32);
        var confirmed = ImGui.IsItemDeactivatedAfterEdit();
        GUI.Tooltip(LOC.Get("TAE_SelectionWindow_Context_Rename_TT", $"a{new string('X', source.NameDigits)}"));

        var unchanged = RenameBuffer == entry.Name;
        var error = unchanged ? null : source.ValidateTimeActName(RenameBuffer, entry);

        if (error != null)
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, error);
        }

        var canApply = !unchanged && error == null;

        if (confirmed && canApply && source.TryParseTimeActName(RenameBuffer, out var bindIndex))
        {
            var oldName = entry.Name;

            View.ViewportActionManager.ExecuteAction(new TimeActRenameEntryAction(View, source, entry, RenameBuffer, bindIndex));

            Smithbox.Log(this, LOC.Get("TAE_SelectionWindow_Renamed", oldName, entry.Name));

            RenameTarget = null;

            ImGui.CloseCurrentPopup();
        }
    }

    /// <summary>
    /// Adds a .tae file to the source. It takes the name of the file if that fits the mask,
    /// and can replace the time act of that name. Otherwise it goes in the next free category.
    /// </summary>
    private void ImportTimeAct(TimeActContainerWrapper source, TimeActEntry entry)
    {
        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("TAE_SelectionWindow_Dialog_Select_Import_File"),
            ["tae"], out var path))
            return;

        byte[] bytes;
        TAE imported;

        try
        {
            bytes = File.ReadAllBytes(path);
            imported = TAE.Read(bytes);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_SelectionWindow_Import_Failed", path), e);
            return;
        }

        // A time act of another game wouldn't be read by this one
        entry.Load();

        if (entry.TimeAct != null &&
            (entry.TimeAct.Format != imported.Format || entry.TimeAct.BigEndian != imported.BigEndian))
        {
            Smithbox.LogError(this, LOC.Get("TAE_SelectionWindow_Import_Wrong_Format", path,
                imported.Format, entry.TimeAct.Format));
            return;
        }

        var name = Path.GetFileNameWithoutExtension(path).ToLower();

        if (source.TryParseTimeActName(name, out var bindIndex))
        {
            var existing = source.TimeActs.FirstOrDefault(e => e.BindIndex == bindIndex);

            if (existing != null)
            {
                var result = PlatformUtils.Instance.MessageBox(
                    LOC.Get("TAE_SelectionWindow_Dialog_Import_Replace", existing.Name),
                    LOC.Get("SYS_Warning_Header"),
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (result == DialogResult.Cancel)
                    return;

                if (result == DialogResult.Yes)
                {
                    var replacement = existing.CreateSibling(existing.Name, existing.BindIndex, bytes);

                    View.ViewportActionManager.ExecuteAction(new TimeActReplaceEntryAction(View, source, existing, replacement));

                    Smithbox.Log(this, LOC.Get("TAE_SelectionWindow_Imported", path, replacement.Name));
                    return;
                }

                // Added alongside it instead
                bindIndex = source.GetFreeBindIndex(existing);
            }
        }
        else
        {
            bindIndex = source.GetFreeBindIndex(entry);
        }

        var added = entry.CreateSibling(source.FormatTimeActName(bindIndex), bindIndex, bytes);

        View.ViewportActionManager.ExecuteAction(new TimeActAddEntryAction(View, source, added));

        Smithbox.Log(this, LOC.Get("TAE_SelectionWindow_Imported", path, added.Name));
    }

    private void DuplicateTimeAct(TimeActContainerWrapper source, TimeActEntry entry)
    {
        var bindIndex = source.GetFreeBindIndex(entry);

        var copy = entry.Duplicate(source.FormatTimeActName(bindIndex), bindIndex);

        View.ViewportActionManager.ExecuteAction(new TimeActAddEntryAction(View, source, copy));

        Smithbox.Log(this, LOC.Get("TAE_SelectionWindow_Duplicated", entry.Name, copy.Name));
    }

    public static string GetExportFolderLabel()
    {
        var folder = CFG.Current.TimeActEditor_Export_Directory;

        return string.IsNullOrEmpty(folder) ? LOC.Get("TAE_SelectionWindow_Export_Folder_Unset") : folder;
    }

    public static bool SelectExportFolder()
    {
        if (!PlatformUtils.Instance.OpenFolderDialog(LOC.Get("TAE_SelectionWindow_Dialog_Select_Export_Folder"), out var folder))
            return false;

        CFG.Current.TimeActEditor_Export_Directory = folder;

        return true;
    }

    /// <summary>
    /// Writes the time act as a .tae file, under a folder named after its source, as every character has an a00.
    /// Unsaved changes are included.
    /// </summary>
    private void ExportTimeAct(TimeActContainerWrapper source, TimeActEntry entry)
    {
        if (!Directory.Exists(CFG.Current.TimeActEditor_Export_Directory) && !SelectExportFolder())
            return;

        var exportPath = Path.Join(CFG.Current.TimeActEditor_Export_Directory, source.Name, $"{entry.Name}.tae");

        if (File.Exists(exportPath))
        {
            var result = PlatformUtils.Instance.MessageBox(
                LOC.Get("TAE_SelectionWindow_Dialog_Overwrite_File", exportPath),
                LOC.Get("SYS_Warning_Header"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.No)
                return;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(exportPath));

            File.WriteAllBytes(exportPath, entry.GetBytes());

            Smithbox.Log(this, LOC.Get("TAE_SelectionWindow_Exported", exportPath));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_SelectionWindow_Export_Failed", exportPath), e);
        }
    }

    public void ClearSelection()
    {
        View.Selection.SelectedTimeAct = null;
        View.Selection.SelectedAnimation = null;
        View.Selection.SelectedEvent = null;

        View.Playback.OnAnimationChanged();
    }

    /// <summary>
    /// Brings a time act into view in the list, once it is next drawn
    /// </summary>
    public void ScrollTo(TimeActEntry entry)
    {
        ScrollTarget = entry;
    }

    public void SelectTimeAct(TimeActEntry entry)
    {
        entry.Load();

        View.Selection.SelectedTimeAct = entry;
        View.Selection.SelectedAnimation = null;
        View.Selection.SelectedEvent = null;

        var firstAnimation = entry.TimeAct?.Animations.FirstOrDefault();

        if (firstAnimation != null)
        {
            View.AnimationSelectionWindow.SelectAnimation(firstAnimation);
        }
        else
        {
            View.Playback.OnAnimationChanged();
        }
    }
}
