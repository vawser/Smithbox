using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg entry editing
/// </summary>
public class TextFmgEntryPropertyEditor
{
    private TextEditorScreen Editor;
    private ProjectEntry Project;

    public TextFmgEntryPropertyEditor(TextEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the fmg entry view
    /// </summary>
    public void Display()
    {
        if (ImGui.Begin("Contents##fmgEntryContents"))
        {
            Editor.Selection.SwitchWindowContext(TextEditorContext.FmgEntryContents);

            ImGui.BeginChild("FmgEntryContents");
            Editor.Selection.SwitchWindowContext(TextEditorContext.FmgEntryContents);

            if (Editor.Selection._selectedFmgEntry != null)
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
        var fmgEntryGroup = Editor.EntryGroupManager.GetEntryGroup(Editor.Selection._selectedFmgEntry);

        // Display normally if entry has no groups, or it has been disabled
        if(!fmgEntryGroup.SupportsGrouping || !CFG.Current.TextEditor_Entry_DisplayGroupedEntries)
        {
            DisplayBasicTextInput(Editor.Selection._selectedFmgEntry);
        }
        else
        {
            DisplayGroupedTextInput(Editor.Selection._selectedFmgEntry, fmgEntryGroup);
        }
    }

    private int _idCache = -1;
    private string _textCache = "";

    public void DisplayGroupedTextInput(FMG.Entry entry, FmgEntryGroup fmgEntryGroup)
    {
        var windowWidth = ImGui.GetWindowWidth();

        var textboxHeight = 32f;
        var textboxWidth = ImGui.GetWindowWidth() * 0.92f;
        var height = textboxHeight;

        var selectedFmgWrapper = Editor.Selection.SelectedFmgWrapper;
        var selectedEntry = Editor.Selection._selectedFmgEntry;

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

                DPI.ApplyInputWidth(textboxWidth);
                if (ImGui.InputInt($"##fmgEntryIdInputGrouped", ref curId))
                {
                    _idCache = curId;
                    Editor.Selection.CurrentWindowContext = TextEditorContext.FmgEntryContents;
                }

                var idCommit = ImGui.IsItemDeactivatedAfterEdit();

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
                            actions.Add(new ChangeFmgEntryID(Editor, Editor.Selection.SelectedContainerWrapper, fmgEntryGroup.Title, _idCache));
                        }
                        if (fmgEntryGroup.Summary != null)
                        {
                            actions.Add(new ChangeFmgEntryID(Editor, Editor.Selection.SelectedContainerWrapper, fmgEntryGroup.Summary, _idCache));
                        }
                        if (fmgEntryGroup.Description != null)
                        {
                            actions.Add(new ChangeFmgEntryID(Editor, Editor.Selection.SelectedContainerWrapper, fmgEntryGroup.Description, _idCache));
                        }
                        if (fmgEntryGroup.Effect != null)
                        {
                            actions.Add(new ChangeFmgEntryID(Editor, Editor.Selection.SelectedContainerWrapper, fmgEntryGroup.Effect, _idCache));
                        }

                        var groupAction = new FmgGroupedAction(actions);
                        Editor.EditorActionManager.ExecuteAction(groupAction);
                    }
                }
            }

            // Title
            if (fmgEntryGroup.Title != null)
            {
                var curText = fmgEntryGroup.Title.Text;

                if (curText == null)
                    curText = "";

                height = 32 * DPI.UIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Title");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Title", ref curText, 2000, new Vector2(-1, height)))
                {
                    _textCache = curText;
                    Editor.Selection.CurrentWindowContext = TextEditorContext.FmgEntryContents;
                }

                var titleTextCommit = ImGui.IsItemDeactivatedAfterEdit();

                // Title Text
                if (fmgEntryGroup.Title != null && titleTextCommit)
                {
                    var action = new ChangeFmgEntryText(Editor, Editor.Selection.SelectedContainerWrapper, fmgEntryGroup.Title, _textCache);
                    Editor.EditorActionManager.ExecuteAction(action);
                }
            }
            else if (fmgEntryGroup.Title == null && fmgEntryGroup.SupportsTitle)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Title");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.Button("Add Title Entry", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Editor.ActionHandler.AddTitleEntry(selectedFmgWrapper, selectedEntry);
                }
            }

            // Summary
            if (fmgEntryGroup.Summary != null)
            {
                var curText = fmgEntryGroup.Summary.Text;

                if (curText == null)
                    curText = "";

                height = (100 + ImGui.CalcTextSize(curText).Y) * DPI.UIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Summary");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Summary", ref curText, 2000, new Vector2(-1, height)))
                {
                    _textCache = curText;
                    Editor.Selection.CurrentWindowContext = TextEditorContext.FmgEntryContents;
                }

                var summaryTextCommit = ImGui.IsItemDeactivatedAfterEdit();

                if (fmgEntryGroup.Summary != null && summaryTextCommit)
                {
                    var action = new ChangeFmgEntryText(Editor, Editor.Selection.SelectedContainerWrapper, fmgEntryGroup.Summary, _textCache);
                    Editor.EditorActionManager.ExecuteAction(action);
                }
            }
            else if (fmgEntryGroup.Summary == null && fmgEntryGroup.SupportsSummary)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Summary");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.Button("Add Summary Entry", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Editor.ActionHandler.AddSummaryEntry(selectedFmgWrapper, selectedEntry);
                }
            }

            // Description
            if (fmgEntryGroup.Description != null)
            {
                var curText = fmgEntryGroup.Description.Text;

                if (curText == null)
                    curText = "";

                height = (100 + ImGui.CalcTextSize(curText).Y) * DPI.UIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Description");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Description", ref curText, 2000, new Vector2(-1, height)))
                {
                    _textCache = curText;
                    Editor.Selection.CurrentWindowContext = TextEditorContext.FmgEntryContents;
                }

                var descriptionTextCommit = ImGui.IsItemDeactivatedAfterEdit();

                if (fmgEntryGroup.Description != null && descriptionTextCommit)
                {
                    var action = new ChangeFmgEntryText(Editor, Editor.Selection.SelectedContainerWrapper, fmgEntryGroup.Description, _textCache);
                    Editor.EditorActionManager.ExecuteAction(action);
                }
            }
            else if (fmgEntryGroup.Description == null && fmgEntryGroup.SupportsDescription)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Description");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.Button("Add Description Entry", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Editor.ActionHandler.AddDescriptionEntry(selectedFmgWrapper, selectedEntry);
                }
            }

            // Effect
            if (fmgEntryGroup.Effect != null)
            {
                var curText = fmgEntryGroup.Effect.Text;

                if (curText == null)
                    curText = "";

                height = (100 + ImGui.CalcTextSize(curText).Y) * DPI.UIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Effect");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Effect", ref curText, 2000, new Vector2(-1, height)))
                {
                    _textCache = curText;
                    Editor.Selection.CurrentWindowContext = TextEditorContext.FmgEntryContents;
                }

                var effectTextCommit = ImGui.IsItemDeactivatedAfterEdit();

                if (fmgEntryGroup.Effect != null && effectTextCommit)
                {
                    var action = new ChangeFmgEntryText(Editor, Editor.Selection.SelectedContainerWrapper, fmgEntryGroup.Effect, _textCache);
                    Editor.EditorActionManager.ExecuteAction(action);
                }
            }
            else if (fmgEntryGroup.Effect == null && fmgEntryGroup.SupportsEffect)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text("Effect");

                ImGui.TableSetColumnIndex(1);

                if (ImGui.Button("Add Effect Entry", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Editor.ActionHandler.AddEffectEntry(selectedFmgWrapper, selectedEntry);
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
        var textboxWidth = ImGui.GetWindowWidth() * 0.92f;

        if (ImGui.BeginTable($"fmgEditTableBasic", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            // ID
            var curId = entry.ID;

            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.Text("ID");

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth(textboxWidth);
            if (ImGui.InputInt($"##fmgEntryIdInputBasic", ref curId))
            {
                _idCache = curId;
                Editor.Selection.CurrentWindowContext = TextEditorContext.FmgEntryContents;
            }

            var idCommit = ImGui.IsItemDeactivatedAfterEdit();

            if (idCommit)
            {
                var action = new ChangeFmgEntryID(Editor, Editor.Selection.SelectedContainerWrapper, entry, _idCache);
                Editor.EditorActionManager.ExecuteAction(action);
            }

            // Text
            var curText = entry.Text;

            // Correct contents if the entry.Text is null
            if (curText == null)
                curText = "";

            var height = (textboxHeight + ImGui.CalcTextSize(curText).Y) * DPI.UIScale();

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);

            ImGui.Text("Text");

            ImGui.TableSetColumnIndex(1);

            if (ImGui.InputTextMultiline($"##fmgTextInputBasic", ref curText, 2000, new Vector2(-1, height)))
            {
                _textCache = curText;
                Editor.Selection.CurrentWindowContext = TextEditorContext.FmgEntryContents;
            }

            var textCommit = ImGui.IsItemDeactivatedAfterEdit();
            if (textCommit)
            {
                var action = new ChangeFmgEntryText(Editor, Editor.Selection.SelectedContainerWrapper, entry, _textCache);
                Editor.EditorActionManager.ExecuteAction(action);
            }

            ImGui.EndTable();
        }
    }
}

