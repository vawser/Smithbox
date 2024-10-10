using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg entry selection and viewing
/// </summary>
public class TextFmgEntryView
{
    public TextEditorScreen Screen;
    public TextPropertyDecorator Decorator;
    public TextSelectionManager Selection;
    public TextFilters Filters;
    public TextContextMenu ContextMenu;

    public TextFmgEntryView(TextEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
    }

    /// <summary>
    /// The main UI for the fmg entry view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Text Entries##fmgEntryList");

        Filters.DisplayFmgEntryFilterSearch();

        if (Selection.SelectedFmg != null)
        {
            // Categories
            for (int i = 0; i < Selection.SelectedFmg.Entries.Count; i++)
            {
                var entry = Selection.SelectedFmg.Entries[i];
                var id = entry.ID;
                var contents = entry.Text;

                if (Filters.IsFmgEntryFilterMatch(contents, id))
                {
                    // Script row
                    if (ImGui.Selectable($"{id} {contents}##fmgEntry{id}{i}", id == Selection._selectedFmgEntryIndex))
                    {
                        Selection.SelectFmgEntry(i, entry);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectNextFmgEntry)
                    {
                        Selection.SelectNextFmgEntry = false;
                        Selection.SelectFmgEntry(i, entry);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectNextFmgEntry = true;
                    }

                    // Only apply to selection
                    if (Selection._selectedFmgEntryIndex != -1)
                    {
                        if (Selection._selectedFmgEntryIndex == id)
                        {
                            ContextMenu.FmgEntryContextMenu(i, entry);
                        }
                    }
                }
            }
        }

        ImGui.End();
    }
}
