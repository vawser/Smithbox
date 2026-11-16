using Hexa.NET.ImGui;
using StudioCore.Keybinds;

namespace StudioCore.Editors.TextEditor;

public class TextFindMenu
{
    public TextEditorView View;
    public ProjectEntry Project;

    private int FindEntryID = -1;

    public TextFindMenu(TextEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Open()
    {
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
        if (ImGui.BeginPopup("findPopup"))
        {
            if (ImGui.Button($"{Icons.LocationArrow}###jumpToAction"))
            {
                Jump(fromSelection: true);
            }

            // The click that leaves the ID box may be on this button: let the button jump then,
            // rather than both jumping (which skipped a duplicate)
            var jumpButtonUsed = ImGui.IsItemHovered() || ImGui.IsItemActive();

            ImGui.SameLine();

            ImGui.InputInt("##findId", ref FindEntryID);

            // Jump when the box is left after an edit (Enter or Tab), not only from the button
            if (GUI.InputCommitted() && !jumpButtonUsed)
            {
                Jump(fromSelection: false);
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Select the entry with the find ID. From the button, the search starts after the current
    /// selection and wraps, so pressing it again steps through entries that share the ID (it
    /// used to select every match in turn, the last one winning). From the box, it starts at
    /// the top, so typing an ID goes to its first entry.
    /// </summary>
    private void Jump(bool fromSelection)
    {
        var entries = View.Selection.SelectedFmgWrapper?.File?.Entries;

        if (entries == null || entries.Count == 0)
            return;

        var start = fromSelection ? View.Selection._selectedFmgEntryIndex + 1 : 0;

        for (int n = 0; n < entries.Count; n++)
        {
            var i = (start + n) % entries.Count;
            var entry = entries[i];

            if (entry.ID == FindEntryID)
            {
                View.Selection.RevealEntry(i, entry);
                return;
            }
        }
    }
}
