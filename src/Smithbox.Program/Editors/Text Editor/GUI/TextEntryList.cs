using Hexa.NET.ImGui;
using Hexa.NET.ImGuizmo;
using Org.BouncyCastle.Asn1.X509;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg entry selection and viewing
/// </summary>
public class TextEntryList
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public string EntryListFilter = "";
    public bool ExactEntryListFilter = false;

    public bool FocusSelection = false;

    public TextEntryList(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the fmg entry view
    /// </summary>
    public void Display()
    {
        DisplayTitle();

        if(Parent.Selection.IsFmgSelected())
        {
            DisplayHeader();
            DisplayEntryTable();
        }
        else
        {
            ImGui.Text(LOC.Get("TEXT_EntryList_Select_FMG_First"));
        }
    }


    public void DisplayTitle()
    {
        GUI.TitleHeader(
            LOC.Get("TEXT_EntryList_Header_Entries"),
            LOC.Get("TEXT_EntryList_Header_Entries_TT"));
    }

    public void DisplayHeader()
    {
        ImGui.BeginChild($"textEditor_EntryList_Section_Header", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        if (InputManager.IsPressed(KeybindID.TextEditor_Focus_Searchbar))
        {
            ImGui.SetKeyboardFocusHere();
        }

        EditorFilters.DisplaySearchbar("textEditor_EntryList",
            ref EntryListFilter, ref ExactEntryListFilter);

        // The 'modified' / 'unique' filter commands weren't mentioned anywhere in the UI
        GUI.Tooltip(LOC.Get("TEXT_EntryList_Filter_Commands_TT"));

        // Focus after clearing
        if (ImGui.IsItemEdited())
        {
            if (EntryListFilter == "")
            {
                FocusSelection = true;
            }
        }
        // Focus after clicking off
        if (ImGui.IsItemDeactivated())
        {
            FocusSelection = true;
        }

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.LocationArrow}##fmgFocusSelection", DPI.IconButtonSize))
        {
            FocusSelection = true;
        }
        GUI.Tooltip(
            LOC.Get("TEXT_EntryList_Focus_Selection_TT", 
            InputManager.GetHint(KeybindID.Jump),
            InputManager.GetHint(KeybindID.Find)));

        // Toggle: Null Entries
        GUI.DisplayToggleButton("nullEntryToggle", Icons.Tag,
            ref CFG.Current.TextEditor_Text_Entry_List_Display_Null_Entries,
            "TEXT_EntryList_NullEntries_Hidden",
            "TEXT_EntryList_NullEntries_Visible",
            "TEXT_EntryList_NullEntries_Hint");

        ImGui.SameLine();

        // Action: Select All
        if (ImGui.Button($"{Icons.Bars}##selectAllButton", DPI.IconButtonSize))
        {
            Parent.Editor.Shortcuts.HandleSelectAll();
        }
        GUI.Tooltip(LOC.Get("TEXT_EntryList_Select_All_TT"));

        ImGui.EndChild();
    }

    private void ListHeader()
    {
        var colFlags = ImGuiTableColumnFlags.WidthStretch;

        ImGui.TableSetupColumn("ID", colFlags, 0.2f);
        ImGui.TableSetupColumn("Name", colFlags);
        
        var columnCount = 2;
        ImGui.TableSetupScrollFreeze(columnCount, 1);

        // ID
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text(LOC.Get("TEXT_EntryList_Column_Header_ID"));

        // Name
        ImGui.TableSetColumnIndex(1);
        ImGui.Text(LOC.Get("TEXT_EntryList_Column_Header_Name"));
    }

    public void DisplayEntryTable()
    {
        ImGui.BeginChild("FmgEntriesList", ImGuiChildFlags.Borders);

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV;

        var columnCount = 2;

        if (ImGui.BeginTable($"fullEntryList", columnCount, tblFlags))
        {
            ListHeader();

            if (Parent.Selection.SelectedFmgWrapper != null && Parent.Selection.SelectedFmgWrapper.File != null)
            {
                DisplayEntryList();
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }

    public void DisplayEntryList()
    {
        // Keep index-keyed selections pointing at the same entry objects after
        // adds, deletes, reorders, undo or a save re-sorting the list.
        Parent.Selection.RevalidateEntrySelection();

        var fmg = Parent.Selection.SelectedFmgWrapper.File;

        if (IsDeletedMode())
        {
            DisplayDeletedRows();
            return;
        }

        RefreshVisibleRows(fmg);

        // With truncated names every row is one line high, so only the rows in view are drawn.
        // Untruncated multi-line names make rows uneven, which the clipper can't handle.
        if (!CFG.Current.TextEditor_Text_Entry_List_Truncate_Name)
        {
            foreach (var (index, entry) in _visibleRows)
            {
                DisplayEntryRow(entry, entry.ID, entry.Text, index);
            }

            return;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(_visibleRows.Count);

        // Make sure the row being scrolled to is drawn, so its SetScrollHereY runs
        if (FocusSelection)
        {
            var target = _visibleRows.FindIndex(r => Parent.Selection.IsFmgEntrySelected(r.Index));

            if (target != -1)
                clipper.IncludeItemByIndex(target);
            else
                FocusSelection = false; // Not listed (filtered out): nothing to scroll to
        }

        while (clipper.Step())
        {
            for (int row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                var (index, entry) = _visibleRows[row];

                DisplayEntryRow(entry, entry.ID, entry.Text, index);
            }
        }

        clipper.End();
    }

    /// <summary>
    /// The listed rows (index in the FMG, entry), in list order. Filtering ran IsEntryVisible
    /// (text matching) on every entry every frame; it now reruns only when something it depends on changes.
    /// </summary>
    private List<(int Index, FMG.Entry Entry)> _visibleRows = new();

    private FMG _rowsFmg;
    private int _rowsEntryCount = -1;
    private string _rowsFilter;
    private bool _rowsExact;
    private bool _rowsShowNull;
    private int _rowsDataVersion = -1;
    private int _rowsDiffVersion = -1;

    private void RefreshVisibleRows(FMG fmg)
    {
        var dataVersion = TextFinder.CurrentDataVersion;
        var diffVersion = Parent.DifferenceManager.Version;
        var showNull = CFG.Current.TextEditor_Text_Entry_List_Display_Null_Entries;

        var stale = _rowsFmg != fmg ||
            _rowsEntryCount != fmg.Entries.Count ||
            _rowsFilter != EntryListFilter ||
            _rowsExact != ExactEntryListFilter ||
            _rowsShowNull != showNull ||
            _rowsDataVersion != dataVersion ||
            _rowsDiffVersion != diffVersion;

        // Reorders that don't go through an action (the save sorts each FMG in place, Sort FMG)
        // change no version: catch them by checking each cached row is still where it was.
        // Reference compares only, far cheaper than the filter itself.
        if (!stale)
        {
            foreach (var (index, entry) in _visibleRows)
            {
                if (index >= fmg.Entries.Count || fmg.Entries[index] != entry)
                {
                    stale = true;
                    break;
                }
            }
        }

        if (!stale)
            return;

        _visibleRows.Clear();

        for (int i = 0; i < fmg.Entries.Count; i++)
        {
            var entry = fmg.Entries[i];

            if (IsEntryVisible(entry))
            {
                _visibleRows.Add((i, entry));
            }
        }

        _rowsFmg = fmg;
        _rowsEntryCount = fmg.Entries.Count;
        _rowsFilter = EntryListFilter;
        _rowsExact = ExactEntryListFilter;
        _rowsShowNull = showNull;
        _rowsDataVersion = dataVersion;
        _rowsDiffVersion = diffVersion;
    }

    /// <summary>
    /// Whether the entry is shown in the entry list: passes the search filter (or the
    /// 'modified'/'unique' commands) and isn't a hidden null entry.
    /// Every action that works on "the listed entries" (select all, range select,
    /// auto-select, copy) uses this, so none of them reach rows the user can't see.
    /// </summary>
    public bool IsEntryVisible(FMG.Entry entry)
    {
        if (!CFG.Current.TextEditor_Text_Entry_List_Display_Null_Entries && entry.Text == null)
            return false;

        // Ignore normal match if a special conditional commands has been used
        if (UsedMatchCommands(EntryListFilter))
        {
            return HandleMatchCommands(EntryListFilter, entry);
        }

        return EditorFilters.IsMatch(
            EntryListFilter, entry.ID.ToString(), ExactEntryListFilter, entry.Text);
    }

    public void DisplayEntryRow(FMG.Entry entry, int id, string contents, int index)
    {
        // ID
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        DisplayEntryRowID(entry, id, contents, index);

        // Name
        ImGui.TableSetColumnIndex(1);
        DisplayEntryRowName(entry, id, contents, index);

    }

    public void DisplayEntryRowID(FMG.Entry entry, int id, string contents, int index)
    {
        var isSelected = Parent.Selection.IsFmgEntrySelected(index);

        if(FocusSelection && isSelected)
        {
            ImGui.SetScrollHereY();

            FocusSelection = false;
        }

        // Selectable
        if (ImGui.Selectable($"{id}##fmgEntryID_{id}{index}", isSelected))
        {
            Parent.Selection.SelectFmgEntry(index, entry);
            Parent.TextEntryCreator.UpdateParameters(entry);
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && Parent.Selection.SelectNextFmgEntry)
        {
            Parent.Selection.SelectNextFmgEntry = false;
            Parent.Selection.SelectFmgEntry(index, entry);
            Parent.TextEntryCreator.UpdateParameters(entry);
        }
        if (ImGui.IsItemFocused())
        {
            if (InputManager.HasArrowSelection() || HasPageNavigation())
            {
                Parent.Selection.SelectNextFmgEntry = true;
            }
        }

        // No drag-to-reorder: FMG files are always written sorted by ID (FMG.Write sorts
        // the entry list), so a dragged order was lost on save and never reached the game.

        // Context Menu / Shortcuts
        if (Parent.Selection.IsFmgEntrySelected(index))
        {
            ContextMenu("id", index, Parent.Selection.SelectedFmgWrapper, entry, isSelected);
        }
    }

    public void DisplayEntryRowName(FMG.Entry entry, int id, string contents, int index)
    {
        var displayedText = FormatDisplayName(contents);
        var isSelected = Parent.Selection.IsFmgEntrySelected(index);

        // Unique (added) and modified rows take the shared vanilla colours
        var coloured = VanillaMarkers.PushColour(Parent.DifferenceManager.EntryStatus(entry));

        // Script row
        var clicked = ImGui.Selectable($"{displayedText}##fmgEntryName_{id}{index}", isSelected);

        VanillaMarkers.PopColour(coloured);

        if (clicked)
        {
            Parent.Selection.SelectFmgEntry(index, entry);
            Parent.TextEntryCreator.UpdateParameters(entry);
        }

        // Modified rows: show what the text was in vanilla
        if (ImGui.IsItemHovered() &&
            Parent.DifferenceManager.IsDifferentToVanilla(entry) &&
            !Parent.DifferenceManager.IsUniqueToProject(entry) &&
            Parent.DifferenceManager.TryGetVanillaText(entry, out var vanillaText))
        {
            GUI.Tooltip(LOC.Get("TEXT_Vanilla_Tooltip", vanillaText ?? "<null>"));
        }

        // Arrow Selection
        if (ImGui.IsItemHovered() && Parent.Selection.SelectNextFmgEntry)
        {
            Parent.Selection.SelectNextFmgEntry = false;
            Parent.Selection.SelectFmgEntry(index, entry);
            Parent.TextEntryCreator.UpdateParameters(entry);
        }
        if (ImGui.IsItemFocused())
        {
            if (InputManager.HasArrowSelection() || HasPageNavigation())
            {
                Parent.Selection.SelectNextFmgEntry = true;
            }
        }

        // Context Menu / Shortcuts
        if (Parent.Selection.IsFmgEntrySelected(index))
        {
            ContextMenu("name", index, Parent.Selection.SelectedFmgWrapper, entry, isSelected);
        }
    }

    /// <summary>
    /// Page Up/Down and Home/End: ImGui's keyboard navigation already moves focus for these;
    /// flagging them like the arrow keys makes the newly focused row become the selection.
    /// </summary>
    private static bool HasPageNavigation()
    {
        return ImGui.IsKeyPressed(ImGuiKey.PageUp) || ImGui.IsKeyPressed(ImGuiKey.PageDown) ||
               ImGui.IsKeyPressed(ImGuiKey.Home) || ImGui.IsKeyPressed(ImGuiKey.End);
    }

    public string FormatDisplayName(string contents)
    {
        var displayedText = contents;

        if (contents != null)
        {
            if (CFG.Current.TextEditor_Text_Entry_List_Truncate_Name)
            {
                if (contents.Contains("\n"))
                {
                    displayedText = $"{displayedText.Split("\n")[0]} <...>";
                }
            }
        }
        else
        {
            displayedText = "";

            if (CFG.Current.TextEditor_Text_Entry_List_Display_Null_Text)
            {
                displayedText = $"<null>";
            }
        }

        return displayedText;
    }

    public bool UsedMatchCommands(string rawInput)
    {
        bool isValid = true;

        if (rawInput == null)
            return isValid;

        var input = rawInput.Trim().ToLower();

        if (input == "modified")
        {
            return true;
        }
        else if (input == "unique")
        {
            return true;
        }
        else if (input == "deleted")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// The 'deleted' command lists vanilla entries the mod no longer has, instead of the FMG's own rows
    /// </summary>
    public bool IsDeletedMode()
    {
        return EntryListFilter != null && EntryListFilter.Trim().ToLower() == "deleted";
    }

    /// <summary>
    /// Rows for the 'deleted' command: greyed, not selectable, restored from the context menu
    /// </summary>
    private void DisplayDeletedRows()
    {
        var deleted = Parent.DifferenceManager.DeletedEntries;

        if (deleted.Count == 0)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(1);
            ImGui.TextDisabled(LOC.Get(CFG.Current.TextEditor_Include_Vanilla_Cache
                ? "TEXT_Vanilla_No_Deleted_Entries"
                : "TEXT_Vanilla_Cache_Disabled"));
            return;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(deleted.Count);

        while (clipper.Step())
        {
            for (int row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                var vanillaEntry = deleted[row];

                ImGui.TableNextRow();

                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);

                ImGui.TableSetColumnIndex(0);
                ImGui.Selectable($"{vanillaEntry.ID}##deletedEntryID_{row}", false, ImGuiSelectableFlags.SpanAllColumns);

                ImGui.PopStyleColor();

                GUI.Tooltip(LOC.Get("TEXT_Vanilla_Deleted_Row_TT"));

                if (ImGui.BeginPopupContextItem($"##deletedEntryContext_{row}"))
                {
                    if (ImGui.Selectable($"{LOC.Get("TEXT_Vanilla_Restore")}##restoreDeleted"))
                    {
                        Parent.ActionHandler.RestoreDeletedEntries(new[] { vanillaEntry });
                    }

                    if (ImGui.Selectable($"{LOC.Get("TEXT_Vanilla_Restore_All", deleted.Count)}##restoreAllDeleted"))
                    {
                        Parent.ActionHandler.RestoreDeletedEntries(deleted.ToList());
                    }

                    ImGui.EndPopup();
                }

                // TextUnformatted: TextDisabled treats its text as a format string ('%' in game text)
                ImGui.TableSetColumnIndex(1);
                ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
                ImGui.TextUnformatted(FormatDisplayName(vanillaEntry.Text));
                ImGui.PopStyleColor();
            }
        }

        clipper.End();
    }

    public bool HandleMatchCommands(string rawInput, FMG.Entry curEntry)
    {
        bool isValid = true;

        if (rawInput == null)
            return isValid;

        var input = rawInput.Trim().ToLower();

        if (input == "modified")
        {
            if (Parent.DifferenceManager.IsDifferentToVanilla(curEntry))
            {
                return true;
            }

            return false;
        }
        else if (input == "unique")
        {
            if (Parent.DifferenceManager.IsUniqueToProject(curEntry))
            {
                return true;
            }

            return false;
        }

        // 'deleted' lists vanilla rows instead (DisplayDeletedRows); none of the FMG's own rows
        return false;
    }

    public void ContextMenu(string title, int index, TextFmgWrapper fmgInfo, FMG.Entry entry, bool isMultiselecting)
    {
        if (ImGui.BeginPopupContextItem($"##FmgEntryContext{title}_{index}"))
        {
            // Create
            if (ImGui.Selectable($"{LOC.Get("TEXT_ContainerList_Context_Action_Create")}##createAction"))
            {
                Parent.TextEntryCreator.ShowModal = true;
            }

            // Duplicate
            if (ImGui.Selectable($"{LOC.Get("TEXT_ContainerList_Context_Action_Duplicate")}##duplicateAction"))
            {
                Parent.ActionHandler.DuplicateEntries();
            }

            // Delete
            if (ImGui.Selectable($"{LOC.Get("TEXT_ContainerList_Context_Action_Delete")}##deleteAction"))
            {
                Parent.ActionHandler.DeleteEntries();
            }

            // Copy Text
            if (ImGui.Selectable($"{LOC.Get("TEXT_ContainerList_Context_Action_Copy_Text")}##copyTextAction"))
            {
                Parent.Editor.Shortcuts.HandleCopyEntryText();
            }

            ImGui.Separator();

            // Copy / Paste Entries
            if (ImGui.MenuItem($"{LOC.Get("TEXT_EntryClipboard_Copy")}##copyEntriesAction", InputManager.GetHint(KeybindID.Copy)))
            {
                Parent.ActionHandler.CopyEntries();
            }
            GUI.Tooltip(LOC.Get("TEXT_EntryClipboard_Copy_TT"));

            if (ImGui.MenuItem($"{LOC.Get("TEXT_EntryClipboard_Paste")}##pasteEntriesAction", InputManager.GetHint(KeybindID.Paste), false, TextEntryClipboard.HasEntries))
            {
                Parent.ActionHandler.PasteEntries(asNew: false);
            }
            GUI.Tooltip(LOC.Get("TEXT_EntryClipboard_Paste_TT"));

            if (ImGui.MenuItem($"{LOC.Get("TEXT_EntryClipboard_Paste_As_New")}##pasteNewEntriesAction", InputManager.GetHint(KeybindID.TextEditor_Paste_As_New_Entries), false, TextEntryClipboard.HasEntries))
            {
                Parent.ActionHandler.PasteEntries(asNew: true);
            }
            GUI.Tooltip(LOC.Get("TEXT_EntryClipboard_Paste_As_New_TT"));

            ImGui.Separator();

            // Edit Selection
            if (ImGui.Selectable($"{LOC.Get("TEXT_BulkEdit_Action")}##bulkEditAction"))
            {
                Parent.TextBulkEditPopup.OpenRequested = true;
            }
            GUI.Tooltip(LOC.Get("TEXT_BulkEdit_Action_TT"));

            // Revert to Vanilla
            if (ImGui.Selectable($"{LOC.Get("TEXT_Vanilla_Revert")}##revertToVanillaAction"))
            {
                Parent.ActionHandler.RevertToVanilla();
            }
            GUI.Tooltip(LOC.Get("TEXT_Vanilla_Revert_TT"));

            ImGui.Separator();

            Parent.FmgImporter.TextEntryDropdownOptions();
            Parent.FmgExporter.TextEntryDropdownOptions();

            ImGui.EndPopup();
        }
    }
}
