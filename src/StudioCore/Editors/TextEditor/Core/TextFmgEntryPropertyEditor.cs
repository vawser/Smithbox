using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.TextEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg entry editing
/// </summary>
public class TextFmgEntryPropertyEditor
{
    public TextEditorScreen Screen;
    public TextPropertyDecorator Decorator;
    public TextSelectionManager Selection;
    public TextFilters Filters;
    public TextContextMenu ContextMenu;
    public TextEntryGroupManager EntryGroupManager;

    public TextFmgEntryPropertyEditor(TextEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
        Filters = screen.Filters;
        ContextMenu = screen.ContextMenu;
        EntryGroupManager = screen.EntryGroupManager;
    }

    /// <summary>
    /// The main UI for the fmg entry view
    /// </summary>
    public void Display()
    {
        if (ImGui.Begin("Contents##fmgEntryContents"))
        {
            ImGui.BeginChild("FmgEntryContents");

            if (Selection._selectedFmgEntry != null)
            {
                DisplayEditor();
            }

            ImGui.EndChild();

            ImGui.End();
        }
    }

    /// <summary>
    /// Group Editor: ID and Text edit inputs for all associated entries + the main one
    /// </summary>
    public void DisplayEditor()
    {
        var fmgEntryGroup = EntryGroupManager.GetEntryGroup(Selection._selectedFmgEntry);

        // Display normally if entry has no groups, or it has been disabled
        if(!fmgEntryGroup.SupportsGrouping || !CFG.Current.TextEditor_Entry_DisplayGroupedEntries)
        {
            DisplayBasicTextInput(Selection._selectedFmgEntry);
        }
        else
        {
            DisplayGroupedTextInput(Selection._selectedFmgEntry, fmgEntryGroup);
        }
    }

    private int _idCache = -1;
    private string _textCache = "";

    public void DisplayGroupedTextInput(FMG.Entry entry, FmgEntryGroup fmgEntryGroup)
    {
        var textboxHeight = 32f;
        var textboxWidth = ImGui.GetWindowWidth() * 0.9f;
        var height = textboxHeight;

        // We assume Title always exists
        if (fmgEntryGroup.Title == null)
        {
            return;
        }

        if (ImGui.BeginTable($"fmgEditTableGrouped", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            // ID
            if (fmgEntryGroup.Title != null)
            {
                var curId = fmgEntryGroup.Title.ID;

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text("ID");

                ImGui.TableSetColumnIndex(1);

                ImGui.SetNextItemWidth(textboxWidth);
                if (ImGui.InputInt($"##fmgEntryIdInputGrouped", ref curId))
                {
                    _idCache = curId;
                }

                var idCommit = ImGui.IsItemDeactivatedAfterEdit();
                if (ImGui.IsItemActivated())
                {
                    Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
                }

                // Update the ID if it was changed and the id input was exited
                if (idCommit)
                {
                    var proceed = true;

                    // If duplicate IDs are disallowed, then don't apply the ID action changes if there is a match
                    if (!CFG.Current.TextEditor_Entry_AllowDuplicateIds)
                    {
                        var parentFmg = fmgEntryGroup.Title.Parent;
                        foreach (var fmgEntry in parentFmg.Entries)
                        {
                            if (fmgEntry.ID == _idCache)
                            {
                                proceed = false;
                            }
                        }
                    }

                    if (proceed)
                    {
                        List<EditorAction> actions = new List<EditorAction>();

                        if (fmgEntryGroup.Title != null)
                        {
                            actions.Add(new ChangeFmgEntryID(Selection.SelectedContainer, fmgEntryGroup.Title, _idCache));
                        }
                        if (fmgEntryGroup.Summary != null)
                        {
                            actions.Add(new ChangeFmgEntryID(Selection.SelectedContainer, fmgEntryGroup.Summary, _idCache));
                        }
                        if (fmgEntryGroup.Description != null)
                        {
                            actions.Add(new ChangeFmgEntryID(Selection.SelectedContainer, fmgEntryGroup.Description, _idCache));
                        }
                        if (fmgEntryGroup.Effect != null)
                        {
                            actions.Add(new ChangeFmgEntryID(Selection.SelectedContainer, fmgEntryGroup.Effect, _idCache));
                        }

                        var groupAction = new FmgGroupedAction(actions);
                        Screen.EditorActionManager.ExecuteAction(groupAction);
                    }
                }
            }

            // Title
            if (fmgEntryGroup.Title != null)
            {
                var curText = fmgEntryGroup.Title.Text;

                if (curText == null)
                    curText = "";

                height = 32 * DPI.GetUIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Title");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Title", ref curText, 2000, new Vector2(-1, height)))
                {
                    _textCache = curText;
                }

                var titleTextCommit = ImGui.IsItemDeactivatedAfterEdit();

                if (ImGui.IsItemActivated())
                {
                    Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
                }

                // Title Text
                if (fmgEntryGroup.Title != null && titleTextCommit)
                {
                    var action = new ChangeFmgEntryText(Selection.SelectedContainer, fmgEntryGroup.Title, _textCache);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
            }
            // Summary
            if (fmgEntryGroup.Summary != null)
            {
                var curText = fmgEntryGroup.Summary.Text;

                if (curText == null)
                    curText = "";

                height = (100 + ImGui.CalcTextSize(curText).Y) * DPI.GetUIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Summary");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Summary", ref curText, 2000, new Vector2(-1, height)))
                {
                    _textCache = curText;
                }

                var summaryTextCommit = ImGui.IsItemDeactivatedAfterEdit();

                if (ImGui.IsItemActivated())
                {
                    Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
                }

                if (fmgEntryGroup.Summary != null && summaryTextCommit)
                {
                    var action = new ChangeFmgEntryText(Selection.SelectedContainer, fmgEntryGroup.Summary, _textCache);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
            }
            // Description
            if (fmgEntryGroup.Description != null)
            {
                var curText = fmgEntryGroup.Description.Text;

                if (curText == null)
                    curText = "";

                height = (100 + ImGui.CalcTextSize(curText).Y) * DPI.GetUIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Description");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Description", ref curText, 2000, new Vector2(-1, height)))
                {
                    _textCache = curText;
                }

                var descriptionTextCommit = ImGui.IsItemDeactivatedAfterEdit();

                if (ImGui.IsItemActivated())
                {
                    Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
                }

                if (fmgEntryGroup.Description != null && descriptionTextCommit)
                {
                    var action = new ChangeFmgEntryText(Selection.SelectedContainer, fmgEntryGroup.Description, _textCache);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
            }
            // Effect
            if (fmgEntryGroup.Effect != null)
            {
                var curText = fmgEntryGroup.Effect.Text;

                if (curText == null)
                    curText = "";

                height = (100 + ImGui.CalcTextSize(curText).Y) * DPI.GetUIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Effect");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Effect", ref curText, 2000, new Vector2(-1, height)))
                {
                    _textCache = curText;
                }

                var effectTextCommit = ImGui.IsItemDeactivatedAfterEdit();

                if (ImGui.IsItemActivated())
                {
                    Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
                }

                if (fmgEntryGroup.Effect != null && effectTextCommit)
                {
                    var action = new ChangeFmgEntryText(Selection.SelectedContainer, fmgEntryGroup.Effect, _textCache);
                    Screen.EditorActionManager.ExecuteAction(action);
                }
            }

            ImGui.EndTable();
        }
    }

    /// <summary>
    /// Editor view for single entry
    /// </summary>
    public void DisplayBasicTextInput(FMG.Entry entry)
    {
        var textboxHeight = 100;
        var textboxWidth = ImGui.GetWindowWidth() * 0.9f;

        var id = entry.ID;
        var contents = entry.Text;

        // Correct contents if the entry.Text is null
        if (contents == null)
            contents = "";

        var height = (textboxHeight + ImGui.CalcTextSize(contents).Y) * DPI.GetUIScale();

        var idChanged = false;
        var idCommit = false;

        var textChanged = false;
        var textCommit = false;

        if (ImGui.BeginTable($"fmgEditTableBasic", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("ID");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(textboxWidth);
            if(ImGui.InputInt($"##fmgEntryIdInputBasic", ref id))
            {
                idChanged = true;
            }

            idCommit = ImGui.IsItemDeactivatedAfterEdit();
            if (ImGui.IsItemActivated())
            {
                Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
            }

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Text");

            ImGui.TableSetColumnIndex(1);

            if (ImGui.InputTextMultiline($"##fmgTextInputBasic", ref contents, 2000, new Vector2(-1, height)))
            {
                textChanged = true;
            }
            textCommit = ImGui.IsItemDeactivatedAfterEdit();
            if (ImGui.IsItemActivated())
            {
                Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
            }

            ImGui.EndTable();
        }

        // Update the ID if it was changed and the id input was exited
        if(idChanged && idCommit)
        {
            var action = new ChangeFmgEntryID(Selection.SelectedContainer, entry, id);
            Screen.EditorActionManager.ExecuteAction(action);
        }

        // Update the Text if it was changed and the text input was exited
        if (textChanged && textCommit)
        {
            var action = new ChangeFmgEntryText(Selection.SelectedContainer, entry, contents);
            Screen.EditorActionManager.ExecuteAction(action);
        }
    }
}

