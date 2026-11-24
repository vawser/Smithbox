using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The Behavior Editor's file list: the files of every behbnd in one list, so a behavior is opened with one click.
/// The behavior graphs (Behaviors/) are listed; the character and project files can be shown too.
/// A file's context menu and shortcuts act on its binder, as in the Havok Editor's file list
/// (multi-select is within one binder).
/// </summary>
public partial class HavokFileView
{
    /// <summary>Scroll the list to the selected file on the next draw (after a selection made elsewhere)</summary>
    public bool ScrollToSelection = false;

    private enum BehaviorFileKind { Behavior, Character, Project }

    private record BehaviorRow(FileDictionaryEntry Binder, string FilePath, string Label, string Alias);

    // The filtered rows, rebuilt only when something they depend on changes
    private readonly List<BehaviorRow> BehaviorRows = new();
    private int RowsIndexCompleted = -1;
    private int RowsBankVersion = -1;
    private string RowsFilter;
    private bool RowsExact;
    private bool RowsFullPath;
    private bool RowsAliases;
    private bool RowsSupportFiles;

    private readonly Dictionary<FileDictionaryEntry, string> BinderAliases = new();

    private void DisplayBehaviorFileList(Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict)
    {
        var index = View.Editor.BehaviorIndex;

        if (index == null)
            return;

        index.EnsureStarted();

        DisplayBehaviorHeader();

        ImGui.BeginChild("havokFileSection", ImGuiChildFlags.Borders);

        if (IsFileActionQueued)
        {
            ProcessFileAction();
            IsFileActionQueued = false;

            ImGui.EndChild();
            return;
        }

        if (!index.IsReady)
        {
            ImGui.TextDisabled(LOC.Get("BHV_Havok_Indexing", index.Completed, index.Total));
        }
        else if (index.Total == 0)
        {
            GUI.WrappedText(LOC.Get("BHV_Havok_No_Behbnds"));
        }

        UpdateBehaviorRows(bankDict, index);

        // Only the visible rows are drawn (rows are one line each, which the clipper relies on)
        var clipper = new ImGuiListClipper();
        clipper.Begin(BehaviorRows.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                DisplayBehaviorRow(bankDict, BehaviorRows[i]);
            }
        }

        clipper.End();

        // The selected row may be outside the clipped range, so it's scrolled to by its position in the list
        if (ScrollToSelection)
        {
            ScrollToSelection = false;

            var selectedIndex = BehaviorRows.FindIndex(r => Equals(r.Binder, View.Selection.BinderFileEntry)
                && r.FilePath == View.Selection.FilePath);

            if (selectedIndex >= 0)
            {
                var rowHeight = ImGui.GetTextLineHeightWithSpacing();
                ImGui.SetScrollY(Math.Max(0, selectedIndex * rowHeight - ImGui.GetWindowHeight() / 2));
            }
        }

        // Right-clicking empty space offers Paste into the selected file's binder
        if (View.Selection.BinderFileEntry != null
            && ImGui.BeginPopupContextWindow("havokBehaviorFileListBackground",
                ImGuiPopupFlags.MouseButtonRight | ImGuiPopupFlags.NoOpenOverItems))
        {
            if (ImGui.Selectable($"{LOC.Get("HAVOK_FileView_ContextAction_Paste")}##pasteBackgroundAction",
                false, ClipboardFileList.Count > 0 ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
            {
                QueuePaste(bankDict);
            }
            GUI.Tooltip(LOC.Get("HAVOK_FileView_ContextAction_Paste_TT", InputManager.GetHint(KeybindID.Paste)));

            ImGui.EndPopup();
        }

        ImGui.EndChild();
    }

    private void DisplayBehaviorRow(Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict, BehaviorRow row)
    {
        var inSelectedBinder = Equals(View.Selection.BinderFileEntry, row.Binder);
        var selected = inSelectedBinder && SoftSelectEntries.Contains(row.FilePath);

        var label = row.Label;

        // Mark files with unsaved changes
        if (bankDict.TryGetValue(row.Binder, out var files) && files.TryGetValue(row.FilePath, out var wrapper) && wrapper.IsDirty)
        {
            label = $"{label}*";
        }

        // The saved file against the game's entry of the same name in the game's binder
        var vanillaStatus = View.HavokBank.EntryStatus(row.Binder, false, row.FilePath);
        var coloured = VanillaMarkers.PushColour(vanillaStatus);

        var clicked = ImGui.Selectable($"{label}##behaviorFileEntry_{row.Binder.Path}_{row.FilePath}", selected);

        VanillaMarkers.PopColour(coloured);
        if (coloured)
            VanillaMarkers.Tooltip(vanillaStatus);

        if (clicked)
        {
            // 'Soft' select for the binder actions: only within the selected file's binder
            if (InputManager.HasCtrlDown() && inSelectedBinder)
            {
                if (!SoftSelectEntries.Remove(row.FilePath))
                {
                    SoftSelectEntries.Add(row.FilePath);
                }
            }
            else
            {
                SelectBehaviorFile(row.Binder, row.FilePath);
            }
        }

        // Right-clicking a file that isn't selected selects it, so its context menu opens
        if (!selected && ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            SelectBehaviorFile(row.Binder, row.FilePath);
            selected = true;
        }

        if (selected)
        {
            DisplayContextMenu(bankDict, row.Binder, row.FilePath, row.Label);
        }

        if (RowsAliases && !string.IsNullOrEmpty(row.Alias))
        {
            // Not wrapped: a wrapped alias would make the row taller than the clipper expects
            GUI.DisplayAlias(row.Alias, false);
        }
    }

    /// <summary>
    /// Show a behbnd's file: its binder is read into the bank (as picking it in the Havok Editor's binder list),
    /// then the file is loaded
    /// </summary>
    public void SelectBehaviorFile(FileDictionaryEntry binder, string filepath)
    {
        if (!Equals(View.Selection.BinderFileEntry, binder))
        {
            View.Selection.ClearFileSelection();
            View.Selection.BinderFileEntry = binder;

            View.Selection.ClearFileAliasCache();
            View.BinderView.PopulateFileList(true);
        }

        // The binder may have failed to read
        if (View.HavokBank.BehaviorBank.TryGetValue(binder, out var files) && files.ContainsKey(filepath))
        {
            SelectFile(filepath);
        }
    }

    private void UpdateBehaviorRows(Dictionary<FileDictionaryEntry, Dictionary<string, HavokFileWrapper>> bankDict,
        HavokBehaviorFileIndex index)
    {
        var fullPath = CFG.Current.BehaviorEditor.BehaviorEditor_FileList_Display_Full_Path;
        var aliases = CFG.Current.BehaviorEditor.BehaviorEditor_FileList_Display_Aliases;
        var supportFiles = CFG.Current.BehaviorEditor.BehaviorEditor_FileList_Show_Support_Files;

        if (RowsIndexCompleted == index.Completed
            && RowsBankVersion == View.HavokBank.FileListVersion
            && RowsFilter == FileFilter
            && RowsExact == ExactFileFilter
            && RowsFullPath == fullPath
            && RowsAliases == aliases
            && RowsSupportFiles == supportFiles
            && !EditorFilters.HasStatusTerm(FileFilter)) // a vanilla-state search follows the background compares
            return;

        RowsIndexCompleted = index.Completed;
        RowsBankVersion = View.HavokBank.FileListVersion;
        RowsFilter = FileFilter;
        RowsExact = ExactFileFilter;
        RowsFullPath = fullPath;
        RowsAliases = aliases;
        RowsSupportFiles = supportFiles;

        BehaviorRows.Clear();

        foreach (var binder in index.Binders)
        {
            // The bank's copy once the binder has been opened (it has the staged file actions), else the index's
            IEnumerable<string> names;

            if (bankDict.TryGetValue(binder, out var files))
            {
                names = files.Keys;

                // Binders with changes stay in the bank, so the index only takes over once they're saved
                index.Update(binder, files.Keys);
            }
            else if (index.TryGetFiles(binder, out var indexed))
                names = indexed;
            else
                continue;

            var alias = GetBinderAlias(binder);

            foreach (var filepath in names)
            {
                if (!IsHavokFile(filepath))
                    continue;

                var kind = GetBehaviorFileKind(filepath);

                if (kind is not BehaviorFileKind.Behavior && !supportFiles)
                    continue;

                var label = GetBehaviorRowLabel(binder, filepath, kind, fullPath);

                if (!EditorFilters.IsMatch(FileFilter, label, ExactFileFilter,
                        () => View.HavokBank.EntryStatus(binder, false, filepath), alias ?? ""))
                    continue;

                BehaviorRows.Add(new BehaviorRow(binder, filepath, label, alias));
            }
        }
    }

    /// <summary>
    /// Behavior graphs are in a Behaviors folder, the character data in a Characters folder;
    /// the project file is at the binder's root
    /// </summary>
    private static BehaviorFileKind GetBehaviorFileKind(string filepath)
    {
        var folder = Path.GetDirectoryName(filepath.Replace('\\', '/')) ?? "";

        if (folder.Contains("Behavior", StringComparison.OrdinalIgnoreCase))
            return BehaviorFileKind.Behavior;

        if (folder.Contains("Character", StringComparison.OrdinalIgnoreCase))
            return BehaviorFileKind.Character;

        return BehaviorFileKind.Project;
    }

    /// <summary>
    /// The binder and file name (c2010: c9997), or the binder alone where the file is named after it (c0000);
    /// the full paths in full path mode
    /// </summary>
    private static string GetBehaviorRowLabel(FileDictionaryEntry binder, string filepath, BehaviorFileKind kind, bool fullPath)
    {
        if (fullPath)
            return $"{binder.Path} : {filepath}";

        var stem = Path.GetFileName(filepath.Replace('\\', '/')).Split('.')[0];

        var label = stem.Equals(binder.Filename, StringComparison.OrdinalIgnoreCase)
            ? binder.Filename
            : $"{binder.Filename}: {stem}";

        return kind switch
        {
            BehaviorFileKind.Character => $"{label} ({LOC.Get("BHV_Havok_Kind_Character")})",
            BehaviorFileKind.Project => $"{label} ({LOC.Get("BHV_Havok_Kind_Project")})",
            _ => label
        };
    }

    private string GetBinderAlias(FileDictionaryEntry binder)
    {
        if (!BinderAliases.TryGetValue(binder, out var alias))
        {
            alias = AliasHelper.GetCharacterAlias(Project, binder.Filename);
            BinderAliases[binder] = alias;
        }

        return alias;
    }

    private void DisplayBehaviorHeader()
    {
        ImGui.BeginChild($"framedList_HavokBehaviorFileList", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("havokBehaviorFileSearch", ref FileFilter, ref ExactFileFilter);

        // Toggle: Full Paths
        GUI.DisplayToggleButton("fullPathToggle", Icons.FileText,
            ref CFG.Current.BehaviorEditor.BehaviorEditor_FileList_Display_Full_Path,
            "HAVOK_FileView_FilePath_Display_Short",
            "HAVOK_FileView_FilePath_Display_Full",
            "HAVOK_FileView_FilePath_Display_TT");

        // Toggle: Aliases
        GUI.DisplayToggleButton("aliasToggle", Icons.Book,
            ref CFG.Current.BehaviorEditor.BehaviorEditor_FileList_Display_Aliases,
            "HAVOK_FileView_FilePath_Display_Alias_Hide",
            "HAVOK_FileView_FilePath_Display_Alias_Show",
            "HAVOK_FileView_FilePath_Display_Alias_TT");

        // Toggle: Character and project files
        GUI.DisplayToggleButton("supportFileToggle", Icons.FilesO,
            ref CFG.Current.BehaviorEditor.BehaviorEditor_FileList_Show_Support_Files,
            "BHV_Havok_Support_Files_Hide",
            "BHV_Havok_Support_Files_Show",
            "BHV_Havok_Support_Files_TT");

        // How many files the context actions (Delete, Export, Compress...) will act on
        if (SoftSelectEntries.Count > 1)
        {
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled(LOC.Get("HAVOK_FileView_Selection_Count", SoftSelectEntries.Count));
            GUI.Tooltip(LOC.Get("HAVOK_FileView_Selection_Count_TT"));
        }

        ImGui.EndChild();
    }
}
