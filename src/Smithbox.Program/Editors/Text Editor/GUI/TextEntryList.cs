using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
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
        DisplayHeader();

        ImGui.BeginChild("FmgEntriesList", ImGuiChildFlags.Borders);

        if (Parent.Selection.SelectedFmgWrapper != null && Parent.Selection.SelectedFmgWrapper.File != null)
        {
            // Categories
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
                    if (ImGui.Selectable($"{id} {displayedText}##fmgEntry{id}{i}", Parent.Selection.IsFmgEntrySelected(i)))
                    {
                        Parent.Selection.SelectFmgEntry(i, entry);
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
                        Parent.Selection.SelectFmgEntry(i, entry);
                    }
                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            Parent.Selection.SelectNextFmgEntry = true;
                        }
                    }

                    // Context Menu / Shortcuts
                    if (Parent.Selection.IsFmgEntrySelected(i))
                    {
                        Parent.ContextMenu.FmgEntryContextMenu(i, Parent.Selection.SelectedFmgWrapper, entry, Parent.Selection.IsFmgEntrySelected(i));

                        Parent.Editor.Shortcuts.HandleSelectAll();
                        Parent.Editor.Shortcuts.HandleCopyEntryText();
                    }

                    // Focus Selection
                    if (Parent.Selection.FocusFmgEntrySelection && Parent.Selection.IsFmgEntrySelected(i))
                    {
                        Parent.Selection.FocusFmgEntrySelection = false;
                        ImGui.SetScrollHereY();
                    }

                }
            }
        }

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        UIHelper.SimpleHeader("Entries", "");

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
