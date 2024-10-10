using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
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
    public TextEditorScreen Screen;
    public TextPropertyDecorator Decorator;
    public TextSelectionManager Selection;
    public TextFilters Filters;
    public TextContextMenu ContextMenu;

    private bool IsCurrentlyCopyingContents = false;

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
        if (ImGui.Begin("Text Entries##fmgEntryList"))
        {
            Filters.DisplayFmgEntryFilterSearch();

            ImGui.BeginChild("FmgEntriesList");

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

                        // Script row
                        if (ImGui.Selectable($"{id} {displayedText}##fmgEntry{id}{i}", Selection.IsFmgEntrySelected(i)))
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

                        // Context Menu / Shortcuts
                        if (Selection.IsFmgEntrySelected(i))
                        {
                            ContextMenu.FmgEntryContextMenu(i, entry, Selection.IsFmgEntrySelected(i));

                            // Copy Entry Contents
                            if (InputTracker.GetKey(KeyBindings.Current.TEXT_CopyEntryContents))
                            {
                                CopyEntryTextToClipboard(CFG.Current.TextEditor_TextCopy_IncludeID);
                            }
                            // Select All
                            if (InputTracker.GetKey(KeyBindings.Current.TEXT_SelectAll))
                            {
                                Selection.FmgEntryMultiselect.StoredIndices.Clear();
                                for (int j = 0; j < Selection.SelectedFmg.Entries.Count; j++)
                                {
                                    Selection.FmgEntryMultiselect.StoredIndices.Add(j);
                                }
                            }
                        }

                        // Focus Selection
                        if (Selection.FocusSelection && Selection.IsFmgEntrySelected(i))
                        {
                            Selection.FocusSelection = false;
                            ImGui.SetScrollHereY();
                        }

                    }
                }
            }

            ImGui.EndChild();

            ImGui.End();
        }
    }

    /// <summary>
    /// Copy the currently selected Text Entries to the clipboard
    /// </summary>
    private void CopyEntryTextToClipboard(bool includeID)
    {
        if (!IsCurrentlyCopyingContents)
        {
            IsCurrentlyCopyingContents = true;
            TaskManager.Run(
                new TaskManager.LiveTask($"Copy Text Entry Text to Clipboard", TaskManager.RequeueType.None, false,
            () =>
            {
                var AlterCopyTextAssignment = false;
                var copyText = "";

                for (int i = 0; i < Selection.SelectedFmg.Entries.Count; i++)
                {
                    var entry = Selection.SelectedFmg.Entries[i];

                    if (Filters.IsFmgEntryFilterMatch(entry.Text, entry.ID))
                    {
                        if (Selection.FmgEntryMultiselect.IsMultiselected(i))
                        {
                            var newText = $"{entry.Text}";

                            if (CFG.Current.TextEditor_TextCopy_EscapeNewLines)
                            {
                                newText = $"{entry.Text}".Replace("\n", "\\n");
                            }

                            if (AlterCopyTextAssignment)
                            {
                                if (includeID)
                                {
                                    copyText = $"{copyText}\n{entry.ID} {newText}";
                                }
                                else
                                {
                                    copyText = $"{copyText}\n{newText}";
                                }
                            }
                            else
                            {
                                AlterCopyTextAssignment = true;

                                if (includeID)
                                {
                                    copyText = $"{entry.ID} {newText}";
                                }
                                else
                                {
                                    copyText = $"{newText}";
                                }
                            }
                        }
                    }
                }

                PlatformUtils.Instance.SetClipboardText(copyText);
                PlatformUtils.Instance.MessageBox("Text Entry Contents copied to clipboard", "Clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
                IsCurrentlyCopyingContents = false;
            }));
        }
    }
}
