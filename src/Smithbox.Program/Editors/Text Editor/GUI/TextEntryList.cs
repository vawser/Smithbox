using Hexa.NET.ImGui;
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
            ImGui.Text("Select a FMG to see entries.");
        }
    }


    public void DisplayTitle()
    {
        var title = "Entries";

        UIHelper.SimpleHeader($"{title}", "");
    }

    public void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"textEditor_EntryList_Section_Header", searchHeight, ImGuiChildFlags.Borders);

        if (InputManager.IsPressed(KeybindID.TextEditor_Focus_Searchbar))
        {
            ImGui.SetKeyboardFocusHere();
        }

        EditorFilters.DisplayListFilter("textEditor_EntryList",
            ref EntryListFilter, ref ExactEntryListFilter);

        // Focus after clearing
        if (ImGui.IsItemEdited())
        {
            if (EntryListFilter == "")
            {
                Parent.Selection.FocusFmgEntrySelection = true;
            }
        }
        // Focus after clicking off
        if (ImGui.IsItemDeactivated())
        {
            Parent.Selection.FocusFmgEntrySelection = true;
        }

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Eye}##fmgFocusSelection", DPI.IconButtonSize))
        {
            Parent.Selection.FocusFmgEntrySelection = true;
        }
        UIHelper.Tooltip($"Focus the currently selected entry.\nShortcut: {InputManager.GetHint(KeybindID.Jump)}");

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
        ImGui.Text("ID");

        // Name
        ImGui.TableSetColumnIndex(1);
        ImGui.Text("Name");
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
        for (int i = 0; i < Parent.Selection.SelectedFmgWrapper.File.Entries.Count; i++)
        {
            var entry = Parent.Selection.SelectedFmgWrapper.File.Entries[i];
            var id = entry.ID;
            var contents = entry.Text;

            var isMatch = EditorFilters.IsMatch(
                EntryListFilter, entry.ID.ToString(), ExactEntryListFilter, entry.Text);

            // Ignore normal match if a special conditional commands has been used
            if (UsedMatchCommands(EntryListFilter))
            {
                isMatch = HandleMatchCommands(EntryListFilter, entry);
            }

            if (isMatch)
            {
                DisplayEntryRow(entry, id, contents, i);
            }
        }
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


    private FMG.Entry DragSourceEntry;

    public void DisplayEntryRowID(FMG.Entry entry, int id, string contents, int index)
    {
        var isSelected = Parent.Selection.IsFmgEntrySelected(index);

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
            if (InputManager.HasArrowSelection())
            {
                Parent.Selection.SelectNextFmgEntry = true;
            }
        }

        // Drag source
        if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
        {
            DragSourceEntry = entry;
            unsafe
            {
                byte dummy = 0;
                ImGui.SetDragDropPayload("FMG_ENTRY", &dummy, 1);
            }
            ImGui.Text($"Entry {entry.ID}");
            ImGui.EndDragDropSource();
        }

        // Drop target
        if (ImGui.BeginDragDropTarget())
        {
            unsafe
            {
                var fmg = Parent.Selection.SelectedFmgWrapper.File;
                var payload = ImGui.AcceptDragDropPayload("FMG_ENTRY");
                if (!payload.IsNull && DragSourceEntry != null && DragSourceEntry != entry)
                {
                    int dropTargetIndex = fmg.Entries.IndexOf(entry);

                    if (dropTargetIndex >= 0)
                    {
                        List<FMG.Entry> entriesToMove;
                        var selectedEntries = Parent.Selection.FmgEntryMultiselect.StoredEntries;

                        entriesToMove = selectedEntries.Values.ToList();

                        // If the user is viewing in grouped mode,
                        // we need to handle each of the possible associated entries
                        // so the assumed behavior of moving the title moves the other entries too.
                        if (CFG.Current.TextEditor_Text_Entry_Enable_Grouped_Entries)
                        {
                            FmgEntryUtils.ReorderFmgEntryGroup(Parent, Parent.Selection.SelectedFmgWrapper, fmg, entriesToMove, dropTargetIndex);
                        }
                        else
                        {
                            var action = new ReorderEntryAction(fmg, entriesToMove, dropTargetIndex);
                            Parent.ActionManager.ExecuteAction(action);
                        }
                    }

                    DragSourceEntry = null;
                }
            }

            ImGui.EndDragDropTarget();
        }

        // Context Menu / Shortcuts
        if (Parent.Selection.IsFmgEntrySelected(index))
        {
            Parent.ContextMenu.FmgEntryContextMenu("id", index, Parent.Selection.SelectedFmgWrapper, entry, isSelected);

            Parent.Editor.Shortcuts.HandleSelectAll();
            Parent.Editor.Shortcuts.HandleCopyEntryText();
        }

        // Focus Selection
        if (Parent.Selection.FocusFmgEntrySelection && isSelected)
        {
            Parent.Selection.FocusFmgEntrySelection = false;
            ImGui.SetScrollHereY();
        }
    }

    public void DisplayEntryRowName(FMG.Entry entry, int id, string contents, int index)
    {
        var displayedText = FormatDisplayName(contents);
        var isSelected = Parent.Selection.IsFmgEntrySelected(index);

        // Unique rows
        if (Parent.DifferenceManager.IsUniqueToProject(entry))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_TextEditor_UniqueTextEntry_Text);
        }
        // Modified rows
        else if (Parent.DifferenceManager.IsDifferentToVanilla(entry))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_TextEditor_ModifiedTextEntry_Text);
        }

        // Script row
        if (ImGui.Selectable($"{displayedText}##fmgEntryName_{id}{index}", isSelected))
        {
            Parent.Selection.SelectFmgEntry(index, entry);
            Parent.TextEntryCreator.UpdateParameters(entry);
        }

        if (Parent.DifferenceManager.IsUniqueToProject(entry) ||
            Parent.DifferenceManager.IsDifferentToVanilla(entry))
        {
            ImGui.PopStyleColor(1);
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
            if (InputManager.HasArrowSelection())
            {
                Parent.Selection.SelectNextFmgEntry = true;
            }
        }

        // Context Menu / Shortcuts
        if (Parent.Selection.IsFmgEntrySelected(index))
        {
            Parent.ContextMenu.FmgEntryContextMenu("name", index, Parent.Selection.SelectedFmgWrapper, entry, isSelected);

            Parent.Editor.Shortcuts.HandleSelectAll();
            Parent.Editor.Shortcuts.HandleCopyEntryText();
        }

        // Focus Selection
        if (Parent.Selection.FocusFmgEntrySelection && isSelected)
        {
            Parent.Selection.FocusFmgEntrySelection = false;
            ImGui.SetScrollHereY();
        }
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

        return false;
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

        return false;
    }
}
