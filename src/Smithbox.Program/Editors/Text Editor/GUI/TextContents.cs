using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TextEditor;

/// <summary>
/// Handles the fmg entry editing
/// </summary>
public class TextContents
{
    private TextEditorView Parent;
    private ProjectEntry Project;

    public TextContents(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the fmg entry view
    /// </summary>
    public void Display()
    {
        GUI.TitleHeader(
            LOC.Get("TEXT_Contents_Header_Contents"),
            LOC.Get("TEXT_Contents_Header_Contents_TT"));

        ImGui.BeginChild("FmgEntryContents", ImGuiChildFlags.Borders);

        // Preview toggle
        var previewIcon = CFG.Current.TextEditor_Contents_Show_Text_Preview ? Icons.Eye : Icons.EyeSlash;
        if (ImGui.Button($"{previewIcon}##toggleTextPreview", DPI.IconButtonSize))
        {
            CFG.Current.TextEditor_Contents_Show_Text_Preview = !CFG.Current.TextEditor_Contents_Show_Text_Preview;
        }
        GUI.Tooltip(LOC.Get("TEXT_Contents_Preview_Toggle_TT",
            LOC.Get(CFG.Current.TextEditor_Contents_Show_Text_Preview ? "TEXT_Contents_Preview_Shown" : "TEXT_Contents_Preview_Hidden")));

        if (Parent.Selection._selectedFmgEntry != null)
        {
            DisplayEditor();
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Group Editor: ID and Text edit inputs for all associated entries + the main one
    /// </summary>
    // The entry group was rebuilt every frame (4 linear FMG scans). Reuse it until the
    // selected entry, the selected FMG, or the text data changes.
    private FMG.Entry _groupCacheEntry;
    private TextFmgWrapper _groupCacheFmg;
    private int _groupCacheVersion = -1;
    private FmgEntryGroup _groupCache;

    private FmgEntryGroup GetCachedEntryGroup(FMG.Entry entry)
    {
        var version = TextFinder.CurrentDataVersion;
        var fmg = Parent.Selection.SelectedFmgWrapper;

        if (_groupCache == null || _groupCacheEntry != entry || _groupCacheFmg != fmg || _groupCacheVersion != version)
        {
            _groupCache = Parent.EntryGroupManager.GetEntryGroup(entry);
            _groupCacheEntry = entry;
            _groupCacheFmg = fmg;
            _groupCacheVersion = version;
        }

        return _groupCache;
    }

    public void DisplayEditor()
    {
        var fmgEntryGroup = GetCachedEntryGroup(Parent.Selection._selectedFmgEntry);

        if (fmgEntryGroup != null)
        {
            // Display normally if entry has no groups, or it has been disabled
            if (!fmgEntryGroup.SupportsGrouping || !CFG.Current.TextEditor_Text_Entry_Enable_Grouped_Entries)
            {
                DisplayBasicTextInput(Parent.Selection._selectedFmgEntry);
            }
            else
            {
                DisplayGroupedTextInput(Parent.Selection._selectedFmgEntry, fmgEntryGroup);
            }
        }
    }

    private int _idCache = -1;
    private string _textCache = "";

    /// <summary>
    /// Buffer size for the text inputs, in UTF-8 bytes. A fixed 2000 truncated longer
    /// entries as soon as they were edited (about 660 CJK characters).
    /// </summary>
    // Last refused ID change, shown under the ID field while that entry stays selected
    private string _idError;
    private FMG.Entry _idErrorEntry;

    private void DisplayIdError()
    {
        if (_idError == null)
            return;

        // Selecting another entry dismisses it
        if (_idErrorEntry != Parent.Selection._selectedFmgEntry)
        {
            _idError = null;
            _idErrorEntry = null;
            return;
        }

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, _idError);
    }

    /// <summary>
    /// Hover readout of the text length; the byte count is what the input buffer limits.
    /// </summary>
    private static void ShowLengthTooltip(string text)
    {
        if (!ImGui.IsItemHovered())
            return;

        text ??= "";
        GUI.Tooltip(LOC.Get("TEXT_Contents_Text_Length_TT", text.Length, Encoding.UTF8.GetByteCount(text)));
    }

    /// <summary>
    /// Wrapped, markup-highlighted copy of the text under its box (the box itself can't wrap).
    /// </summary>
    private static void ShowPreview(string id, string text)
    {
        if (!CFG.Current.TextEditor_Contents_Show_Text_Preview)
            return;

        TextMarkupPreview.Display(id, text);
    }

    private static nuint GetEditBufferSize(string text)
    {
        var byteCount = Encoding.UTF8.GetByteCount(text ?? "");
        return (nuint)Math.Max(2000, byteCount + 4096);
    }

    /// <summary>
    /// Change the ID of the entry and, when grouped, its associated entries.
    /// Returns without changing anything (and logs why) if the ID collides
    /// and duplicate IDs aren't allowed.
    /// </summary>
    private void ApplyIdChange(List<FMG.Entry> targets, int newId)
    {
        targets = targets.Where(e => e != null).ToList();

        if (targets.Count == 0 || targets.All(e => e.ID == newId))
            return;

        if (!CFG.Current.TextEditor_Text_Entry_Allow_Duplicate_ID)
        {
            // Check every FMG the change writes to, ignoring the entries being changed
            foreach (var target in targets)
            {
                var others = Parent.ActionHandler.GetGroupFmgs(target);
                var collision = others
                    .SelectMany(f => f.Entries)
                    .FirstOrDefault(e => e.ID == newId && !targets.Contains(e));

                if (collision != null)
                {
                    var message = LOC.Get("TEXT_Contents_ID_Collision", newId, collision.Parent?.Name ?? "");
                    Smithbox.LogWarning(this, message);

                    // Also shown under the ID field, where the user is looking
                    _idError = message;
                    _idErrorEntry = Parent.Selection._selectedFmgEntry;
                    return;
                }
            }
        }

        _idError = null;
        _idErrorEntry = null;

        var actions = new List<EditorAction>();

        foreach (var target in targets)
        {
            actions.Add(new ChangeFmgEntryID(Parent, Parent.Selection.SelectedContainerWrapper, target, newId));
        }

        Parent.ActionManager.ExecuteAction(new FmgGroupedAction(actions));
    }

    public void DisplayGroupedTextInput(FMG.Entry entry, FmgEntryGroup fmgEntryGroup)
    {
        var windowWidth = ImGui.GetWindowWidth();

        var textboxHeight = 32f;
        var textboxWidth = ImGui.GetWindowWidth() * 0.92f;
        var height = textboxHeight;

        var selectedFmgWrapper = Parent.Selection.SelectedFmgWrapper;
        var selectedEntry = Parent.Selection._selectedFmgEntry;

        // The Title entry may not exist (e.g. a description with no name entry).
        // This used to return early and leave the panel blank, so the
        // "Add Title Entry" button below could never show.

        if (ImGui.BeginTable($"fmgEditTableGrouped", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);
            //ImGui.TableHeadersRow();

            // ID: shown from the title when it exists, otherwise from the selected entry
            {
                var idSource = fmgEntryGroup.Title ?? entry;
                var curId = idSource.ID;

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_ID"));

                ImGui.TableSetColumnIndex(1);

                DPI.ApplyInputWidth(textboxWidth);
                if (ImGui.InputInt($"##fmgEntryIdInputGrouped", ref curId))
                {
                    _idCache = curId;
                }

                var idCommit = ImGui.IsItemDeactivatedAfterEdit();

                // Update the ID of every entry in the group once the input is exited
                if (idCommit)
                {
                    ApplyIdChange(new List<FMG.Entry>
                    {
                        fmgEntryGroup.Title,
                        fmgEntryGroup.Summary,
                        fmgEntryGroup.Description,
                        fmgEntryGroup.Effect,
                        entry
                    }.Distinct().ToList(), _idCache);
                }

                DisplayIdError();
            }

            // Title
            if (fmgEntryGroup.Title != null)
            {
                var curText = fmgEntryGroup.Title.Text;

                if (curText == null)
                    curText = "";

                // Grows for multi-line titles (was a fixed 32px)
                height = Math.Max(32f, ImGui.CalcTextSize(curText).Y + 16f) * DPI.UIScale();

                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);

                ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Title"));

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Title", ref curText, GetEditBufferSize(curText), new Vector2(-1, height)))
                {
                    _textCache = curText;
                }

                var titleTextCommit = ImGui.IsItemDeactivatedAfterEdit();
                ShowLengthTooltip(curText);
                ShowPreview("title", curText);

                // Title Text
                if (fmgEntryGroup.Title != null && titleTextCommit)
                {
                    var action = new ChangeFmgEntryText(Parent, Parent.Selection.SelectedContainerWrapper, fmgEntryGroup.Title, _textCache);
                    Parent.ActionManager.ExecuteAction(action);
                }
            }
            else if (fmgEntryGroup.Title == null && fmgEntryGroup.SupportsTitle)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Title"));

                ImGui.TableSetColumnIndex(1);

                if (ImGui.Button($"{LOC.Get("TEXT_Contents_Add_Title_Entry")}##addTitleEntry", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Parent.ActionHandler.AddTitleEntry(selectedFmgWrapper, selectedEntry);
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

                ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Summary"));

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Summary", ref curText, GetEditBufferSize(curText), new Vector2(-1, height)))
                {
                    _textCache = curText;
                }

                var summaryTextCommit = ImGui.IsItemDeactivatedAfterEdit();
                ShowLengthTooltip(curText);
                ShowPreview("summary", curText);

                if (fmgEntryGroup.Summary != null && summaryTextCommit)
                {
                    var action = new ChangeFmgEntryText(Parent, Parent.Selection.SelectedContainerWrapper, fmgEntryGroup.Summary, _textCache);
                    Parent.ActionManager.ExecuteAction(action);
                }
            }
            else if (fmgEntryGroup.Summary == null && fmgEntryGroup.SupportsSummary)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Summary"));

                ImGui.TableSetColumnIndex(1);

                if (ImGui.Button($"{LOC.Get("TEXT_Contents_Add_Summary_Entry")}##addSummaryEntry", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Parent.ActionHandler.AddSummaryEntry(selectedFmgWrapper, selectedEntry);
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

                ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Description"));

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Description", ref curText, GetEditBufferSize(curText), new Vector2(-1, height)))
                {
                    _textCache = curText;
                }

                var descriptionTextCommit = ImGui.IsItemDeactivatedAfterEdit();
                ShowLengthTooltip(curText);
                ShowPreview("description", curText);

                if (fmgEntryGroup.Description != null && descriptionTextCommit)
                {
                    var action = new ChangeFmgEntryText(Parent, Parent.Selection.SelectedContainerWrapper, fmgEntryGroup.Description, _textCache);
                    Parent.ActionManager.ExecuteAction(action);
                }
            }
            else if (fmgEntryGroup.Description == null && fmgEntryGroup.SupportsDescription)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Description"));

                ImGui.TableSetColumnIndex(1);

                if (ImGui.Button($"{LOC.Get("TEXT_Contents_Add_Description_Entry")}##addDescriptionEntry", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Parent.ActionHandler.AddDescriptionEntry(selectedFmgWrapper, selectedEntry);
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

                ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Effect"));

                ImGui.TableSetColumnIndex(1);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Effect", ref curText, GetEditBufferSize(curText), new Vector2(-1, height)))
                {
                    _textCache = curText;
                }

                var effectTextCommit = ImGui.IsItemDeactivatedAfterEdit();
                ShowLengthTooltip(curText);
                ShowPreview("effect", curText);

                if (fmgEntryGroup.Effect != null && effectTextCommit)
                {
                    var action = new ChangeFmgEntryText(Parent, Parent.Selection.SelectedContainerWrapper, fmgEntryGroup.Effect, _textCache);
                    Parent.ActionManager.ExecuteAction(action);
                }
            }
            else if (fmgEntryGroup.Effect == null && fmgEntryGroup.SupportsEffect)
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Effect"));

                ImGui.TableSetColumnIndex(1);

                if (ImGui.Button($"{LOC.Get("TEXT_Contents_Add_Effect_Entry")}##addEffectEntry", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    Parent.ActionHandler.AddEffectEntry(selectedFmgWrapper, selectedEntry);
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

            ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_ID"));

            ImGui.TableSetColumnIndex(1);

            DPI.ApplyInputWidth(textboxWidth);
            if (ImGui.InputInt($"##fmgEntryIdInputBasic", ref curId))
            {
                _idCache = curId;
            }

            var idCommit = ImGui.IsItemDeactivatedAfterEdit();

            if (idCommit)
            {
                // Same duplicate-ID rule as grouped mode (this path had no check)
                ApplyIdChange(new List<FMG.Entry> { entry }, _idCache);
            }

            DisplayIdError();

            // Text
            var curText = entry.Text;

            // Correct contents if the entry.Text is null
            if (curText == null)
                curText = "";

            var height = (textboxHeight + ImGui.CalcTextSize(curText).Y) * DPI.UIScale();

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);

            ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Text"));

            ImGui.TableSetColumnIndex(1);

            if (ImGui.InputTextMultiline($"##fmgTextInputBasic", ref curText, GetEditBufferSize(curText), new Vector2(-1, height)))
            {
                _textCache = curText;
            }

            var textCommit = ImGui.IsItemDeactivatedAfterEdit();
                ShowLengthTooltip(curText);
                ShowPreview("basic", curText);
            if (textCommit)
            {
                var action = new ChangeFmgEntryText(Parent, Parent.Selection.SelectedContainerWrapper, entry, _textCache);
                Parent.ActionManager.ExecuteAction(action);
            }

            ImGui.EndTable();
        }
    }
}

