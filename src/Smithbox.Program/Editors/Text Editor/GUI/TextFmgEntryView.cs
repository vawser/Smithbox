using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg entry selection and viewing
/// </summary>
public class TextFmgEntryView
{
    private TextEditorScreen Editor;
    private ProjectEntry Project;

    public TextFmgEntryView(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the fmg entry view
    /// </summary>
    public void Display()
    {
        if (ImGui.Begin("Text Entries##fmgEntryList"))
        {
            Editor.Selection.SwitchWindowContext(TextEditorContext.FmgEntry);

            Editor.Filters.DisplayFmgEntryFilterSearch();

            ImGui.SameLine();

            if(ImGui.Button($"{Icons.Eye}##fmgFocusSelection", DPI.IconButtonSize))
            {
                Editor.Selection.FocusFmgEntrySelection = true;
            }
            UIHelper.Tooltip($"Focus the currently selected entry.\nShortcut: {KeyBindings.Current.TEXT_FocusSelectedEntry.HintText}");

            ImGui.BeginChild("FmgEntriesList");
            Editor.Selection.SwitchWindowContext(TextEditorContext.FmgEntry);

            if (Editor.Selection.SelectedFmgWrapper != null && Editor.Selection.SelectedFmgWrapper.File != null)
            {
                // Categories
                for (int i = 0; i < Editor.Selection.SelectedFmgWrapper.File.Entries.Count; i++)
                {
                    var entry = Editor.Selection.SelectedFmgWrapper.File.Entries[i];
                    var id = entry.ID;
                    var contents = entry.Text;

                    if (Editor.Filters.IsFmgEntryFilterMatch(entry))
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
                        if (Editor.DifferenceManager.IsUniqueToProject(entry))
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_TextEditor_UniqueTextEntry_Text);
                        }
                        // Modified rows
                        else if (Editor.DifferenceManager.IsDifferentToVanilla(entry))
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_TextEditor_ModifiedTextEntry_Text);
                        }

                        // Script row
                        if (ImGui.Selectable($"{id} {displayedText}##fmgEntry{id}{i}", Editor.Selection.IsFmgEntrySelected(i)))
                        {
                            Editor.Selection.SelectFmgEntry(i, entry);
                        }

                        if (Editor.DifferenceManager.IsUniqueToProject(entry) ||
                            Editor.DifferenceManager.IsDifferentToVanilla(entry))
                        {
                            ImGui.PopStyleColor(1);
                        }

                        // Arrow Selection
                        if (ImGui.IsItemHovered() && Editor.Selection.SelectNextFmgEntry)
                        {
                            Editor.Selection.SelectNextFmgEntry = false;
                            Editor.Selection.SelectFmgEntry(i, entry);
                        }
                        if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                        {
                            Editor.Selection.SelectNextFmgEntry = true;
                        }

                        // Context Menu / Shortcuts
                        if (Editor.Selection.IsFmgEntrySelected(i))
                        {
                            Editor.ContextMenu.FmgEntryContextMenu(i, Editor.Selection.SelectedFmgWrapper, entry, Editor.Selection.IsFmgEntrySelected(i));

                            Editor.EditorShortcuts.HandleSelectAll();
                            Editor.EditorShortcuts.HandleCopyEntryText();
                        }

                        // Focus Selection
                        if (Editor.Selection.FocusFmgEntrySelection && Editor.Selection.IsFmgEntrySelected(i))
                        {
                            Editor.Selection.FocusFmgEntrySelection = false;
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
