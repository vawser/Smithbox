using HKLib.hk2018.hkHashMapDetail;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Platform;
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
    public TextEditorScreen Editor;
    public TextPropertyDecorator Decorator;
    public TextSelectionManager Selection;
    public TextFilters Filters;
    public TextContextMenu ContextMenu;
    public TextDifferenceManager DifferenceManager;

    public TextFmgEntryView(TextEditorScreen screen)
    {
        Editor = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
        DifferenceManager = screen.DifferenceManager;
    }

    /// <summary>
    /// The main UI for the fmg entry view
    /// </summary>
    public void Display()
    {
        if (ImGui.Begin("Text Entries##fmgEntryList"))
        {
            Selection.SwitchWindowContext(TextEditorContext.FmgEntry);

            Filters.DisplayFmgEntryFilterSearch();

            ImGui.BeginChild("FmgEntriesList");
            Selection.SwitchWindowContext(TextEditorContext.FmgEntry);

            if (Selection.SelectedFmgWrapper != null && Selection.SelectedFmgWrapper.File != null)
            {
                // Categories
                for (int i = 0; i < Selection.SelectedFmgWrapper.File.Entries.Count; i++)
                {
                    var entry = Selection.SelectedFmgWrapper.File.Entries[i];
                    var id = entry.ID;
                    var contents = entry.Text;

                    if (Filters.IsFmgEntryFilterMatch(entry))
                    {
                        var displayedText = contents;

                        if (contents != null)
                        {
                            if (CFG.Current.TextEditor_TruncateTextDisplay)
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

                            if (CFG.Current.TextEditor_DisplayNullPlaceholder)
                            {
                                displayedText = $"<null>";
                            }
                        }

                        // Unique rows
                        if (DifferenceManager.IsUniqueToProject(entry))
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_TextEditor_UniqueTextEntry_Text);
                        }
                        // Modified rows
                        else if (DifferenceManager.IsDifferentToVanilla(entry))
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_TextEditor_ModifiedTextEntry_Text);
                        }

                        // Script row
                        if (ImGui.Selectable($"{id} {displayedText}##fmgEntry{id}{i}", Selection.IsFmgEntrySelected(i)))
                        {
                            Selection.SelectFmgEntry(i, entry);
                        }

                        if (DifferenceManager.IsUniqueToProject(entry) || 
                            DifferenceManager.IsDifferentToVanilla(entry))
                        {
                            ImGui.PopStyleColor(1);
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

                        // Context Menu / Shortcuts
                        if (Selection.IsFmgEntrySelected(i))
                        {
                            ContextMenu.FmgEntryContextMenu(i, Selection.SelectedFmgWrapper, entry, Selection.IsFmgEntrySelected(i));

                            Editor.EditorShortcuts.HandleSelectAll();
                            Editor.EditorShortcuts.HandleCopyEntryText();
                        }

                        // Focus Selection
                        if (Selection.FocusFmgEntrySelection && Selection.IsFmgEntrySelected(i))
                        {
                            Selection.FocusFmgEntrySelection = false;
                            ImGui.SetScrollHereY();
                        }

                    }
                }
            }

            ImGui.EndChild();

            ImGui.End();
        }
    }
}
