using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Utilities;
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

    private TextReferenceLookup ReferenceLookup;

    public TextContents(TextEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;

        ReferenceLookup = new TextReferenceLookup(view, project);
    }

    private static bool ShowReference => CFG.Current.TextEditor_Contents_Show_Reference;

    /// <summary>
    /// The bordered header strip under the title, as in the File List and Text Entries panels,
    /// holding the panel's toggles (they used to sit at the top of the contents area)
    /// </summary>
    private void DisplayHeader()
    {
        ImGui.BeginChild("textEditor_Contents_Section_Header", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        // Preview toggle
        var previewIcon = CFG.Current.TextEditor_Contents_Show_Text_Preview ? Icons.Eye : Icons.EyeSlash;
        if (ImGui.Button($"{previewIcon}##toggleTextPreview", DPI.IconButtonSize))
        {
            CFG.Current.TextEditor_Contents_Show_Text_Preview = !CFG.Current.TextEditor_Contents_Show_Text_Preview;
        }
        GUI.Tooltip(LOC.Get("TEXT_Contents_Preview_Toggle_TT",
            LOC.Get(CFG.Current.TextEditor_Contents_Show_Text_Preview ? "TEXT_Contents_Preview_Shown" : "TEXT_Contents_Preview_Hidden")));

        DisplayReferenceControls();

        ImGui.EndChild();
    }

    /// <summary>
    /// Reference toggle and source picker, beside the preview toggle
    /// </summary>
    private void DisplayReferenceControls()
    {
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Columns}##toggleReference", DPI.IconButtonSize))
        {
            CFG.Current.TextEditor_Contents_Show_Reference = !CFG.Current.TextEditor_Contents_Show_Reference;
        }
        GUI.Tooltip(LOC.Get("TEXT_Reference_Toggle_TT"));

        if (!ShowReference)
            return;

        ImGui.SameLine();

        DPI.ApplyInputWidth(180f);
        if (ImGui.BeginCombo("##referenceSource", ReferenceLookup.SourceLabel()))
        {
            if (ImGui.Selectable($"{LOC.Get("TEXT_Reference_Source_Vanilla")}##refVanilla",
                CFG.Current.TextEditor_Contents_Reference_Use_Vanilla))
            {
                CFG.Current.TextEditor_Contents_Reference_Use_Vanilla = true;
            }
            GUI.Tooltip(LOC.Get("TEXT_Reference_Source_Vanilla_TT"));

            foreach (var language in TextUtils.GetSupportedLanguages(Project))
            {
                var isCurrent = !CFG.Current.TextEditor_Contents_Reference_Use_Vanilla &&
                    CFG.Current.TextEditor_Contents_Reference_Category == language;

                if (ImGui.Selectable($"{LOC.Get(language.GetDisplayName())}##ref{language}", isCurrent))
                {
                    CFG.Current.TextEditor_Contents_Reference_Use_Vanilla = false;
                    CFG.Current.TextEditor_Contents_Reference_Category = language;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("TEXT_Reference_Source_TT"));
    }

    private int ColumnCount => ShowReference ? 3 : 2;

    private void SetupColumns()
    {
        ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthStretch);

        if (ShowReference)
        {
            ImGui.TableSetupColumn("Reference", ImGuiTableColumnFlags.WidthStretch);
        }
    }

    /// <summary>
    /// The read-only reference text for this row's entry, in the third column
    /// </summary>
    private void ReferenceCell(FMG.Entry entry)
    {
        if (!ShowReference)
            return;

        ImGui.TableSetColumnIndex(2);

        if (entry != null && ReferenceLookup.TryGetReference(entry, out var referenceText))
        {
            ImGui.PushTextWrapPos(0f);
            ImGui.TextUnformatted(referenceText ?? "<null>");
            ImGui.PopTextWrapPos();

            GUI.Tooltip(LOC.Get("TEXT_Reference_Cell_TT", ReferenceLookup.SourceLabel()));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
            ImGui.TextUnformatted(LOC.Get("TEXT_Reference_Not_Found", ReferenceLookup.SourceLabel()));
            ImGui.PopStyleColor();
        }
    }

    /// <summary>
    /// The main UI for the fmg entry view
    /// </summary>
    public void Display()
    {
        GUI.TitleHeader(
            LOC.Get("TEXT_Contents_Header_Contents"),
            LOC.Get("TEXT_Contents_Header_Contents_TT"));

        DisplayHeader();

        ImGui.BeginChild("FmgEntryContents", ImGuiChildFlags.Borders);

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
        // Markup problems are shown whether or not the preview is
        var warnings = TextMarkupPreview.GetWarnings(text);

        if (warnings != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, warnings);
        }

        if (!CFG.Current.TextEditor_Contents_Show_Text_Preview)
            return;

        TextMarkupPreview.Display(id, text);
    }

    /// <remarks>Also used by the Entry Creator's text boxes.</remarks>
    internal static nuint GetEditBufferSize(string text)
    {
        var byteCount = Encoding.UTF8.GetByteCount(text ?? "");
        return (nuint)Math.Max(2000, byteCount + 4096);
    }

    /// <summary>
    /// Limit a text box's grown height to half the panel (at least 150px), so a long description
    /// doesn't push the other fields off screen. The box scrolls inside past that.
    /// </summary>
    private static float CapBoxHeight(float height)
    {
        var max = Math.Max(150f * DPI.UIScale(), ImGui.GetWindowHeight() * 0.5f);

        return Math.Min(height, max);
    }

    /// <summary>
    /// Apply the committed text box edit to the entry. Skipped when the text ends up as it
    /// was (typed then deleted again): that used to add an undo step and mark the file modified.
    /// A null entry and an empty box count as the same, since the box shows null as "".
    /// </summary>
    private void CommitText(FMG.Entry target)
    {
        if (target == null)
            return;

        if (_textCache == (target.Text ?? ""))
            return;

        var action = new ChangeFmgEntryText(Parent, Parent.Selection.SelectedContainerWrapper, target, _textCache);
        Parent.ActionManager.ExecuteAction(action);
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

        // The ID change moves the entry to its sorted place; scroll the list to follow it
        Parent.TextEntryList.FocusSelection = true;
    }

    /// <summary>
    /// Group Editor: the ID, then one row per slot of the entry's group (up to 10, in the
    /// definition's order, labelled by what the slot means). A slot without an entry yet
    /// shows an Add button.
    /// </summary>
    public void DisplayGroupedTextInput(FMG.Entry entry, FmgEntryGroup fmgEntryGroup)
    {
        var windowWidth = ImGui.GetWindowWidth();
        var textboxWidth = windowWidth * 0.92f;

        if (ImGui.BeginTable($"fmgEditTableGrouped", ColumnCount, ImGuiTableFlags.SizingFixedFit))
        {
            SetupColumns();

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

                // Update the ID of every entry in the group once the input is exited
                if (GUI.InputCommitted())
                {
                    ApplyIdChange(fmgEntryGroup.Entries.Append(entry).Distinct().ToList(), _idCache);
                }

                DisplayIdError();
            }

            for (int i = 0; i < fmgEntryGroup.Slots.Count; i++)
            {
                var slot = fmgEntryGroup.Slots[i];

                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // TextUnformatted: the label can come from the group file, and Text reads '%' as a format
                ImGui.TextUnformatted(slot.Label);

                ImGui.TableSetColumnIndex(1);

                // No entry with this ID in the slot's FMG yet
                if (slot.Entry == null)
                {
                    if (ImGui.Button($"{LOC.Get("TEXT_Contents_Add_Group_Entry", slot.Label)}##addGroupEntry{i}", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                        Parent.ActionHandler.AddGroupEntry(slot, entry);
                    }

                    continue;
                }

                var curText = slot.Entry.Text ?? "";

                // A title is one line (growing for multi-line titles); other slots start taller
                var height = slot.IsTitle
                    ? CapBoxHeight(Math.Max(32f * DPI.UIScale(), ImGui.CalcTextSize(curText).Y + 16f * DPI.UIScale()))
                    : CapBoxHeight(100 * DPI.UIScale() + ImGui.CalcTextSize(curText).Y);

                if (ImGui.InputTextMultiline($"##fmgTextInput_Slot{i}", ref curText, GetEditBufferSize(curText), new Vector2(-1, height)))
                {
                    _textCache = curText;
                }

                var textCommit = GUI.InputCommitted();
                ShowLengthTooltip(curText);
                ShowPreview($"slot{i}", curText);
                ReferenceCell(slot.Entry);

                if (textCommit)
                {
                    CommitText(slot.Entry);
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

        if (ImGui.BeginTable($"fmgEditTableBasic", ColumnCount, ImGuiTableFlags.SizingFixedFit))
        {
            SetupColumns();

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

            var idCommit = GUI.InputCommitted();

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

            var height = CapBoxHeight(textboxHeight * DPI.UIScale() + ImGui.CalcTextSize(curText).Y);

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);

            ImGui.Text(LOC.Get("TEXT_Contents_Column_Header_Text"));

            ImGui.TableSetColumnIndex(1);

            if (ImGui.InputTextMultiline($"##fmgTextInputBasic", ref curText, GetEditBufferSize(curText), new Vector2(-1, height)))
            {
                _textCache = curText;
            }

            var textCommit = GUI.InputCommitted();
            ShowLengthTooltip(curText);
            ShowPreview("basic", curText);
            ReferenceCell(entry);

            if (textCommit)
            {
                CommitText(entry);
            }

            ImGui.EndTable();
        }
    }
}

