using Hexa.NET.ImGui;
using StudioCore.Keybinds;
using System;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Ctrl+F popup: jump to an entry of the selected FMG by ID, or by text it contains
/// </summary>
public class TextFindMenu
{
    public TextEditorView View;
    public ProjectEntry Project;

    private int FindEntryID = -1;
    private string FindText = "";
    private bool FindByText = false;

    /// <summary>
    /// Shown under the input after a search that found nothing; cleared by the next search
    /// </summary>
    private bool LastSearchFailed = false;

    public TextFindMenu(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Open()
    {
        LastSearchFailed = false;
        ImGui.OpenPopup("findPopup");
    }

    public void Shortcut()
    {
        // Not while typing (Ctrl+F belongs to the text box then), and only for the active view:
        // every view runs this, so all of them opened the popup at once.
        if (ImGui.GetIO().WantTextInput)
            return;

        if (View.Editor.ViewHandler.ActiveView != View)
            return;

        // Find
        if (InputManager.IsPressed(KeybindID.Find))
        {
            Open();
        }
    }

    public void Display()
    {
        if (!ImGui.BeginPopup("findPopup"))
            return;

        // Mode: ID or Text
        if (ImGui.RadioButton($"{LOC.Get("TEXT_Find_Mode_ID")}##findModeId", !FindByText))
        {
            FindByText = false;
            LastSearchFailed = false;
        }

        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("TEXT_Find_Mode_Text")}##findModeText", FindByText))
        {
            FindByText = true;
            LastSearchFailed = false;
        }

        if (ImGui.Button($"{Icons.LocationArrow}###jumpToAction"))
        {
            Jump(fromSelection: true);
        }
        GUI.Tooltip(LOC.Get("TEXT_Find_Jump_TT"));

        // The click that leaves the input may be on this button: let the button jump then,
        // rather than both jumping (which skipped a match)
        var jumpButtonUsed = ImGui.IsItemHovered() || ImGui.IsItemActive();

        ImGui.SameLine();

        if (FindByText)
        {
            DPI.ApplyInputWidth(250f);
            ImGui.InputText("##findText", ref FindText, 512);
        }
        else
        {
            DPI.ApplyInputWidth(250f);
            ImGui.InputInt("##findId", ref FindEntryID);
        }

        // Jump when the input is left after an edit (Enter or Tab), not only from the button
        if (GUI.InputCommitted() && !jumpButtonUsed)
        {
            Jump(fromSelection: false);
        }

        if (LastSearchFailed)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TEXT_Find_Not_Found"));
        }

        ImGui.EndPopup();
    }

    /// <summary>
    /// Select the next matching entry. From the button, the search starts after the current
    /// selection and wraps, so pressing it again steps through every match. From the input,
    /// it starts at the top, so a new search goes to the first match.
    /// Text matches ignore case. A match hidden by the entry list filter clears the filter (RevealEntry).
    /// </summary>
    private void Jump(bool fromSelection)
    {
        var entries = View.Selection.SelectedFmgWrapper?.File?.Entries;

        LastSearchFailed = true;

        if (entries == null || entries.Count == 0)
            return;

        if (FindByText && string.IsNullOrEmpty(FindText))
            return;

        var start = fromSelection ? View.Selection._selectedFmgEntryIndex + 1 : 0;

        for (int n = 0; n < entries.Count; n++)
        {
            var i = (start + n) % entries.Count;
            var entry = entries[i];

            var isMatch = FindByText
                ? entry.Text != null && entry.Text.Contains(FindText, StringComparison.OrdinalIgnoreCase)
                : entry.ID == FindEntryID;

            if (isMatch)
            {
                LastSearchFailed = false;
                View.Selection.RevealEntry(i, entry);
                return;
            }
        }
    }
}
