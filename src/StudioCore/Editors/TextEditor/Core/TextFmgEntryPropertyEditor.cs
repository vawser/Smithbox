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
    private bool _idChanged;
    private bool _textChanged;
    private int _idCache;
    private string _textCache;

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

    public void DisplayGroupedTextInput(FMG.Entry entry, FmgEntryGroup fmgEntryGroup)
    {
        var textboxHeight = 32f;
        var textboxWidth = ImGui.GetWindowWidth() * 0.9f;
        var height = textboxHeight;

        var idChanged = false;
        var idCommit = false;

        var titleTextChanged = false;
        var titleTextCommit = false;

        var summaryTextChanged = false;
        var summaryTextCommit = false;

        var descriptionTextChanged = false;
        var descriptionTextCommit = false;

        var effectTextChanged = false;
        var effectTextCommit = false;

        // We assume Title always exists
        if (fmgEntryGroup.Title == null)
        {
            return;
        }

        var id = fmgEntryGroup.Title.ID;


        var titleText = "";
        var summaryText = "";
        var descriptionText = "";
        var effectText = "";

        if (ImGui.BeginTable($"fmgEditTableGrouped", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            // ID
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("ID");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(textboxWidth);
            if (ImGui.InputInt($"##fmgEntryIdInputGrouped", ref id))
            {
                idChanged = true;
            }

            idCommit = ImGui.IsItemDeactivatedAfterEdit();
            if (ImGui.IsItemActivated())
            {
                Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
            }

            // Title
            if (fmgEntryGroup.Title != null)
            {
                titleText = fmgEntryGroup.Title.Text;

                if (titleText == null)
                    titleText = "";

                height = 32 * DPI.GetUIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Title");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Title", ref titleText, 2000, new Vector2(-1, height)))
                {
                    titleTextChanged = true;
                }
                titleTextCommit = ImGui.IsItemDeactivatedAfterEdit();
                if (ImGui.IsItemActivated())
                {
                    Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
                }
            }
            // Summary
            if (fmgEntryGroup.Summary != null)
            {
                summaryText = fmgEntryGroup.Summary.Text;

                if (summaryText == null)
                    summaryText = "";

                height = (100 + ImGui.CalcTextSize(summaryText).Y) * DPI.GetUIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Summary");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Summary", ref summaryText, 2000, new Vector2(-1, height)))
                {
                    summaryTextChanged = true;
                }
                summaryTextCommit = ImGui.IsItemDeactivatedAfterEdit();
                if (ImGui.IsItemActivated())
                {
                    Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
                }
            }
            // Description
            if (fmgEntryGroup.Description != null)
            {
                descriptionText = fmgEntryGroup.Description.Text;

                if (descriptionText == null)
                    descriptionText = "";

                height = (100 + ImGui.CalcTextSize(descriptionText).Y) * DPI.GetUIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Description");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Description", ref descriptionText, 2000, new Vector2(-1, height)))
                {
                    descriptionTextChanged = true;
                }
                descriptionTextCommit = ImGui.IsItemDeactivatedAfterEdit();
                if (ImGui.IsItemActivated())
                {
                    Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
                }
            }
            // Effect
            if (fmgEntryGroup.Effect != null)
            {
                effectText = fmgEntryGroup.Effect.Text;

                if (effectText == null)
                    effectText = "";

                height = (100 + ImGui.CalcTextSize(effectText).Y) * DPI.GetUIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Effect");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Effect", ref effectText, 2000, new Vector2(-1, height)))
                {
                    effectTextChanged = true;
                }
                effectTextCommit = ImGui.IsItemDeactivatedAfterEdit();
                if (ImGui.IsItemActivated())
                {
                    Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
                }
            }

            ImGui.EndTable();
        }

        // Update the ID if it was changed and the id input was exited
        if (idChanged && idCommit)
        {
            var proceed = true;

            // If duplicate IDs are disallowed, then don't apply the ID action changes if there is a match
            if(!CFG.Current.TextEditor_Entry_AllowDuplicateIds)
            {
                var parentFmg = fmgEntryGroup.Title.Parent;
                foreach(var fmgEntry in parentFmg.Entries)
                {
                    if(fmgEntry.ID == id)
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
                    actions.Add(new ChangeFmgEntryID(Selection.SelectedContainer, fmgEntryGroup.Title, id));
                }
                if (fmgEntryGroup.Summary != null)
                {
                    actions.Add(new ChangeFmgEntryID(Selection.SelectedContainer, fmgEntryGroup.Summary, id));
                }
                if (fmgEntryGroup.Description != null)
                {
                    actions.Add(new ChangeFmgEntryID(Selection.SelectedContainer, fmgEntryGroup.Description, id));
                }
                if (fmgEntryGroup.Effect != null)
                {
                    actions.Add(new ChangeFmgEntryID(Selection.SelectedContainer, fmgEntryGroup.Effect, id));
                }

                var groupAction = new FmgGroupedAction(actions);
                Screen.EditorActionManager.ExecuteAction(groupAction);
            }
        }

        // Title Text
        if (fmgEntryGroup.Title != null && titleTextChanged && titleTextCommit)
        {
            var action = new ChangeFmgEntryText(Selection.SelectedContainer, fmgEntryGroup.Title, titleText);
            Screen.EditorActionManager.ExecuteAction(action);
        }
        // Summary Text
        if (fmgEntryGroup.Summary != null && summaryTextChanged && summaryTextCommit)
        {
            var action = new ChangeFmgEntryText(Selection.SelectedContainer, fmgEntryGroup.Summary, summaryText);
            Screen.EditorActionManager.ExecuteAction(action);
        }
        // Description Text
        if (fmgEntryGroup.Description != null && descriptionTextChanged && descriptionTextCommit)
        {
            var action = new ChangeFmgEntryText(Selection.SelectedContainer, fmgEntryGroup.Description, descriptionText);
            Screen.EditorActionManager.ExecuteAction(action);
        }
        // Effect Text
        if (fmgEntryGroup.Effect != null && effectTextChanged && effectTextCommit)
        {
            var action = new ChangeFmgEntryText(Selection.SelectedContainer, fmgEntryGroup.Effect, effectText);
            Screen.EditorActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    /// Editor view for single entry
    /// </summary>
    public void DisplayBasicTextInput(FMG.Entry entry)
    {
        var textboxHeight = 100;
        var textboxWidth = ImGui.GetWindowWidth() * 0.9f;

        var oldID = entry.ID;
        var oldContents = entry.Text;
        var id = entry.ID;
        var contents = entry.Text;

        // Correct contents if the entry.Text is null
        oldContents ??= "";
        contents ??= "";

        var height = (textboxHeight + ImGui.CalcTextSize(contents).Y) * DPI.GetUIScale();

        var idCommit = false;
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
                _idChanged = true;
                _idCache = id;
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
                _textChanged = true;
                _textCache = contents;
            }

            textCommit = ImGui.IsItemDeactivatedAfterEdit();
            if (ImGui.IsItemActivated())
            {
                Selection.CurrentSelectionContext = TextSelectionContext.FmgEntryContents;
            }

            ImGui.EndTable();
        }

        // Update the ID if it was changed and the id input was exited
        if(_idChanged && idCommit)
        {
            if (_idCache != oldID)
            {
                var action = new ChangeFmgEntryID(Selection.SelectedContainer, entry, _idCache);
                Screen.EditorActionManager.ExecuteAction(action);
            }

            _idChanged = false;
        }

        // Update the Text if it was changed and the text input was exited
        if (_textChanged && textCommit)
        {
            if (_textCache != oldContents)
            {
                var action = new ChangeFmgEntryText(Selection.SelectedContainer, entry, _textCache);
                Screen.EditorActionManager.ExecuteAction(action);
                _textCache = null;
            }

            _textChanged = false;
        }
    }
}

