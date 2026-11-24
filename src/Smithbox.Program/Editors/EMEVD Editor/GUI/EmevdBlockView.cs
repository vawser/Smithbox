using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The selected event as a column of Scratch-style blocks (Claude/Documentation/EMEVD and ESD.md (EMEVD Editor), §5
/// Option A): a hat block for the event header, then one block per instruction.
///
/// Structured (P3, the default): skips and gotos that nest are C-blocks holding the lines they jump over, with an
/// Else part where the body ends with an unconditional jump past one; jumps to the end of an enclosing block are
/// marked as exits; conditions sit just above the flow that evaluates them. Jumps that don't nest keep a bracket in
/// the margin, red when they run past the end or miss their label. Flat (View menu): every line at depth 0 with every
/// bracket, as in P1. Rows stay in file order either way, so selection and Up/Down work by line.
///
/// Only the rows on screen are drawn (the Vulkan ImGui renderer can't take more than 65,535 vertices in one draw
/// list, and the longest vanilla events have over 900 lines); row tops are a prefix sum, searched for the first row.
/// </summary>
public class EmevdBlockView
{
    private readonly EmevdEditorView View;

    // The script of the selected event and its layout, rebuilt when the event, its file's revision or the layout
    // mode changes
    private EMEVD.Event CachedEvent;
    private int CachedRevision = -1;
    private bool CachedStructured;
    private EmevdScript Script;

    private List<EmevdRow> Rows = new();
    private int[] LineRow = Array.Empty<int>();

    // Collapsed C-blocks, by their header instruction (instructions keep their identity through edits and undo); a
    // hidden line maps to the row of the header hiding it
    private readonly HashSet<EMEVD.Instruction> Collapsed = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<int, int> HiddenRow = new();
    private bool LayoutDirty;
    private readonly Dictionary<int, int> ExitLevels = new();
    private List<EmevdSpan> DrawnSpans = new();
    private int DrawnLanes;

    // The arg chip popup: which line and arg it edits
    private int PopupLine = -1;
    private int PopupArg = -1;
    private bool OpenPopup;

    // Initialize Event targets, resolved when first drawn
    private readonly Dictionary<int, (EmevdFileWrapper file, EMEVD.Event ev)> CallTargets = new();

    // MustSub calls shown as the event they run (DS2 / SotFS), by line; null where a call can't be shown so
    private readonly Dictionary<int, EmevdInlined> InlinedCalls = new();
    private bool CachedInline;

    // The popup edits a call arg from its in-place form: the name of the arg it sets there
    private string PopupSetsName;

    // The filter bar: blocks whose names and values don't match draw grey; Go To steps through the matching lines.
    // Matches are worked out per line (so a line hidden in a collapsed block can still be found), then per row.
    private string FilterText = "";
    private bool FilterExact;
    private EmevdScript FilterScriptFor;
    private string FilterFor;
    private bool FilterExactFor;
    private bool FilterInlineFor;
    private bool[] LineMatches;
    private List<EmevdRow> FilterRowsFor;
    private bool[] RowMatches;
    private int MatchCount;

    private bool Filtering => !string.IsNullOrWhiteSpace(FilterText);

    // Comment bubbles (EmevdComments): drawn in a column right of the blocks, level with the block they're on
    private EmevdComments Comments => View.Project.Handler.EmevdData?.Comments;
    private List<EmevdRow> CommentRowsFor;
    private EMEVD.Event CommentEventFor;
    private int CommentVersion = -1;
    private bool HasComments;

    // The open comment editor: the block it's on (the event or an instruction), and where
    private object EditCommentBlock;
    private EmevdFileWrapper EditCommentFile;
    private EMEVD.Event EditCommentEvent;
    private int EditCommentRow = NoRow;
    private string CommentText = "";
    private bool FocusComment;

    // The hat's "row" for a comment; NoRow: the block has no row (hidden in a collapsed block, or gone)
    private const int HatRow = -1;
    private const int NoRow = int.MinValue;

    private const float LaneWidth = 7f;
    private const float RowGap = 2f;

    public EmevdBlockView(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdCatalog Catalog => View.Project.Handler.EmevdData?.Catalog;

    /// <summary>
    /// The script for the selected event, rebuilt when the event or its file's revision changed. The Properties
    /// window calls this too, so it's current even while the Script window is hidden behind a tab.
    /// </summary>
    public EmevdScript CurrentScript
    {
        get
        {
            var selection = View.Selection;
            var ev = selection.SelectedEvent;
            var revision = selection.SelectedFile?.Revision ?? -1;
            var structured = CFG.Current.EmevdEditor.EmevdEditor_Structured_View;

            if (ev != CachedEvent || revision != CachedRevision || structured != CachedStructured)
            {
                if (ev != CachedEvent)
                    PopupLine = -1;

                CachedEvent = ev;
                CachedRevision = revision;
                CachedStructured = structured;
                Script = ev != null ? EmevdScript.Build(Catalog, ev) : null;
                CallTargets.Clear();
                InlinedCalls.Clear();
                BuildLayout(structured);
            }
            else if (LayoutDirty)
            {
                BuildLayout(structured);
            }

            LayoutDirty = false;
            return Script;
        }
    }

    /// <summary>The rows, which line each row is, the exits, and the brackets still drawn</summary>
    private void BuildLayout(bool structured)
    {
        ExitLevels.Clear();
        HiddenRow.Clear();

        if (Script == null)
        {
            Rows = new();
            LineRow = Array.Empty<int>();
            DrawnSpans = new();
            DrawnLanes = 0;
            return;
        }

        HashSet<int> structuredLines = null;

        if (structured)
        {
            Script.Nodes = EmevdStructure.Decompile(Script);
            var lines = Script.Lines;
            Rows = EmevdStructure.Rows(Script.Nodes, l => Collapsed.Contains(lines[l].Instr), HiddenRow);
            structuredLines = EmevdStructure.StructuredLines(Script.Nodes);
            CollectExits(Script.Nodes);
        }
        else
        {
            Script.Nodes = null;
            Rows = EmevdStructure.FlatRows(Script);
        }

        LineRow = new int[Script.Lines.Count];
        for (var r = 0; r < Rows.Count; r++)
        {
            if (Rows[r].Kind == EmevdRowKind.Line)
                LineRow[Rows[r].Line] = r;
        }

        foreach (var (line, row) in HiddenRow)
            LineRow[line] = row;

        // Brackets for the jumps the C-blocks don't show, laid out among themselves
        DrawnSpans = Script.Spans
            .Where(s => structuredLines == null || !structuredLines.Contains(s.From))
            .Select(s => new EmevdSpan { From = s.From, To = s.To, IsGoto = s.IsGoto, Broken = s.Broken })
            .ToList();
        DrawnLanes = EmevdScript.AssignLanes(DrawnSpans);
    }

    private void CollectExits(List<EmevdNode> nodes)
    {
        foreach (var n in nodes)
        {
            if (n.ExitLevel >= 0)
                ExitLevels[n.Line] = n.ExitLevel;

            foreach (var child in n.Children)
                CollectExits(child);
        }
    }

    // ---------------------------------------------------------------- collapsing

    public bool IsCollapsed(int line) => Script != null && line >= 0 && line < Script.Lines.Count && Collapsed.Contains(Script.Lines[line].Instr);

    /// <summary>Collapses or expands the C-block headed by a line (takes effect next frame)</summary>
    public void SetCollapsed(int line, bool collapse)
    {
        if (Script == null || line < 0 || line >= Script.Lines.Count)
            return;

        var instr = Script.Lines[line].Instr;
        if (collapse ? Collapsed.Add(instr) : Collapsed.Remove(instr))
            LayoutDirty = true;
    }

    /// <summary>Collapses (or expands) every C-block of the selected event</summary>
    public void SetAllCollapsed(bool collapse)
    {
        if (Script?.Nodes == null)
            return;

        void Walk(List<EmevdNode> nodes)
        {
            foreach (var n in nodes)
            {
                if (n.IsBlock)
                    SetCollapsed(n.Line, collapse);

                foreach (var child in n.Children)
                    Walk(child);
            }
        }

        Walk(Script.Nodes);
    }

    /// <summary>Expands the blocks hiding a line, so it can be shown and selected; true if any was collapsed</summary>
    private bool Reveal(int line)
    {
        if (Script?.Nodes == null || !HiddenRow.ContainsKey(line))
            return false;

        var path = new List<(List<EmevdNode> list, int index)>();
        if (!EmevdStructureEdit.Find(Script.Nodes, line, out var list, out var index, path))
            return false;

        foreach (var (l, i) in path)
            SetCollapsed(l[i].Line, false);

        // An else line is in its own node
        if (list[index].Line != line)
            SetCollapsed(list[index].Line, false);

        return true;
    }

    /// <summary>The line the row after (or before) a line's row shows, skipping feet, dividers and hidden lines</summary>
    private int StepLine(int line, int dir)
    {
        var r = line >= 0 && line < LineRow.Length ? LineRow[line] : -1;

        for (r += dir; r >= 0 && r < Rows.Count; r += dir)
        {
            if (Rows[r].Kind == EmevdRowKind.Line)
                return Rows[r].Line;
        }

        return dir > 0 ? line : -1;
    }

    public void Draw()
    {
        _ = CurrentScript;
        Comments?.EnsureLoaded(View.Selection.SelectedFile);

        DrawHeader();
        UpdateMatches();
        DrawFilterBar();

        // Scrolls sideways when a narrow window puts the comment column past its right edge
        ImGui.BeginChild("emevdScript", Vector2.Zero, ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);

        if (Script == null)
        {
            ImGui.TextDisabled(LOC.Get("EMEVD_Script_No_Event"));
        }
        else
        {
            HandleKeys();
            DrawScript();
        }

        ImGui.EndChild();
    }

    private void DrawHeader()
    {
        GUI.TitleHeader(LOC.Get("EMEVD_Script_Header"), LOC.Get("EMEVD_Script_Header_TT"));

        ImGui.BeginChild("emevdScriptHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        var selection = View.Selection;

        ImGui.BeginDisabled(!selection.CanGoBack);
        if (ImGui.Button($"{Icons.ArrowLeft}##emevdBack"))
            selection.GoBack();
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("EMEVD_Script_Back_TT"));

        if (Script != null)
        {
            ImGui.SameLine();
            ImGui.Text(LOC.Get("EMEVD_Script_Summary", Script.Lines.Count, Script.Slots.Count));

            if (Script.UndecodedLines > 0)
            {
                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Script_Undecoded", Script.UndecodedLines));
                GUI.Tooltip(LOC.Get("EMEVD_Script_Undecoded_TT"));
            }

            if (Script.BadParams > 0)
            {
                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Script_Bad_Params", Script.BadParams));
                GUI.Tooltip(LOC.Get("EMEVD_Script_Bad_Params_TT"));
            }

            var broken = Script.Spans.Count(s => s.Broken);
            if (broken > 0)
            {
                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Script_Broken_Spans", broken));
                GUI.Tooltip(LOC.Get("EMEVD_Script_Broken_Spans_TT"));
            }
        }

        ImGui.EndChild();
    }

    // ---------------------------------------------------------------- filter

    private void DrawFilterBar()
    {
        ImGui.BeginChild("emevdScriptFilter", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        ImGui.BeginDisabled(Script == null);

        ImGui.Checkbox("##emevdScriptFilterExact", ref FilterExact);
        GUI.Tooltip(LOC.Get("EDITOR_List_Filter_Exact_Match_TT"));

        ImGui.SameLine();
        var goToLabel = $"{LOC.Get("EMEVD_Filter_Go_To")}##emevdScriptFilterGoTo";
        var count = Filtering ? LOC.Get("EMEVD_Filter_Matches", MatchCount) : "";
        var style = ImGui.GetStyle();
        var rightW = ImGui.CalcTextSize(LOC.Get("EMEVD_Filter_Go_To")).X + style.FramePadding.X * 2 + style.ItemSpacing.X
            + (count.Length > 0 ? ImGui.CalcTextSize(count).X + style.ItemSpacing.X : 0f);
        ImGui.SetNextItemWidth(Math.Max(120f * DPI.UIScale(), ImGui.GetContentRegionAvail().X - rightW));
        ImGui.InputTextWithHint("##emevdScriptFilterText", LOC.Get("EMEVD_Filter_Hint"), ref FilterText, 255);
        GUI.Tooltip(LOC.Get("EMEVD_Filter_TT", EditorFilters.SplitChr, EditorFilters.OR_Chr));

        // The filter applies as it's typed; Enter in the field also goes to the next match
        var enterGoTo = ImGui.IsItemDeactivated() && ImGui.IsKeyPressed(ImGuiKey.Enter, false);

        ImGui.SameLine();
        ImGui.BeginDisabled(!Filtering);
        if ((ImGui.Button(goToLabel) || enterGoTo) && Filtering)
        {
            UpdateMatches();
            GoToNextMatch();
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("EMEVD_Filter_Go_To_TT"));

        if (count.Length > 0)
        {
            ImGui.SameLine();
            if (MatchCount == 0)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, count);
            else
                ImGui.TextDisabled(count);
        }

        ImGui.EndDisabled();
        ImGui.EndChild();
    }

    /// <summary>
    /// Recomputes which lines match the filter when the script or the filter changed, and which rows when the rows
    /// changed (collapsing a block rebuilds them)
    /// </summary>
    private void UpdateMatches()
    {
        if (Script == null)
        {
            LineMatches = null;
            RowMatches = null;
            FilterScriptFor = null;
            FilterRowsFor = null;
            MatchCount = 0;
            return;
        }

        var inline = CFG.Current.EmevdEditor.EmevdEditor_Inline_MustSub_Calls;
        var linesStale = !ReferenceEquals(FilterScriptFor, Script) || FilterFor != FilterText || FilterExactFor != FilterExact
            || FilterInlineFor != inline || LineMatches?.Length != Script.Lines.Count;

        if (linesStale)
        {
            FilterScriptFor = Script;
            FilterFor = FilterText;
            FilterExactFor = FilterExact;
            FilterInlineFor = inline;
            LineMatches = new bool[Script.Lines.Count];
            MatchCount = 0;
            FilterRowsFor = null;

            if (Filtering)
            {
                for (var l = 0; l < Script.Lines.Count; l++)
                {
                    LineMatches[l] = LineMatchesFilter(Script.Lines[l]);
                    if (LineMatches[l])
                        MatchCount++;
                }
            }
        }

        if (ReferenceEquals(FilterRowsFor, Rows) && RowMatches?.Length == Rows.Count)
            return;

        FilterRowsFor = Rows;
        RowMatches = new bool[Rows.Count];

        if (!Filtering)
            return;

        for (var r = 0; r < Rows.Count; r++)
        {
            if (Rows[r].Kind == EmevdRowKind.Line)
                RowMatches[r] = LineMatches[Rows[r].Line];
        }

        // A collapsed header stays lit when a line it hides matches
        foreach (var (line, row) in HiddenRow)
        {
            if (LineMatches[line] && row >= 0 && row < RowMatches.Length)
                RowMatches[row] = true;
        }

        // A C-block's foot and a switch's Default divider follow their header
        for (var r = 0; r < Rows.Count; r++)
        {
            if (Rows[r].Kind != EmevdRowKind.Line && Rows[r].Line >= 0 && Rows[r].Line < LineRow.Length)
                RowMatches[r] = RowMatches[LineRow[Rows[r].Line]];
        }
    }

    /// <summary>The names and values a block shows, matched against the filter</summary>
    private bool LineMatchesFilter(EmevdLine line)
    {
        var fields = new List<string>();

        void AddLine(EmevdLine l)
        {
            fields.Add(l.Name);
            fields.Add(l.Key);

            if (!l.Decoded.Fits)
                return;

            foreach (var arg in l.Decoded.Args)
            {
                fields.Add(EmevdFormat.ArgName(arg));
                fields.Add(l.Params.TryGetValue(arg.Offset, out var p)
                    ? EmevdScript.SlotName(p.SourceStartByte, p.ByteCount)
                    : EmevdFormat.Value(Catalog, arg));
            }
        }

        AddLine(line);

        // A MustSub call shown in place: what it's drawn as, and the event it runs
        if (Inlined(line) is { } inlined)
        {
            AddLine(inlined.Line);
            fields.Add(EmevdFormat.EventLabel(inlined.Target));
        }
        else if (line.Kind == EmevdBlockKind.Call && !line.CallTargetIsParam && line.CallTarget >= 0)
        {
            var (file, ev) = ResolveCall(line);
            if (ev != null)
            {
                fields.Add(EmevdFormat.EventLabel(ev));
                fields.Add(ev.ID.ToString());
                if (file != null && file != View.Selection.SelectedFile)
                    fields.Add(file.Name);
            }
        }

        fields.RemoveAll(string.IsNullOrEmpty);
        if (fields.Count == 0)
            return false;

        // Each field on its own (so exact matching works on a single name or value), and, when not exact, all of the
        // block's text together (so "a+b" can find a and b in different chips of the same block)
        if (fields.Any(f => EditorFilters.IsMatch(FilterText, f, FilterExact)))
            return true;
        return !FilterExact && EditorFilters.IsMatch(FilterText, string.Join("\n", fields), false);
    }

    private bool IsDimmed(int r) => Filtering && RowMatches != null && r < RowMatches.Length && !RowMatches[r];

    /// <summary>
    /// Selects the next matching line after the selected one (or from the top), wrapping round; a line hidden in a
    /// collapsed block is revealed by the scroll
    /// </summary>
    private void GoToNextMatch()
    {
        if (Script == null || MatchCount == 0 || LineMatches == null)
            return;

        var count = Script.Lines.Count;
        var start = View.Selection.InstructionIndex;

        for (var step = 1; step <= count; step++)
        {
            // start is -1 with the hat (or nothing) selected: the search begins at the first line
            var l = ((start + step) % count + count) % count;
            if (!LineMatches[l])
                continue;

            View.Selection.SelectInstruction(l);
            View.Selection.ScrollToInstruction = true;
            return;
        }
    }

    /// <summary>
    /// Turns the vertices drawn since `from` grey (their brightness kept, squeezed toward mid-grey), so a block that
    /// doesn't match the filter reads as greyed out whatever colours it was drawn with (as the ESD Editor's state view)
    /// </summary>
    private static unsafe void Grey(ImDrawListPtr drawList, int from)
    {
        var vtx = drawList.VtxBuffer;
        var data = vtx.Data;
        for (var i = from; i < vtx.Size; i++)
        {
            var col = data[i].Col;
            var r = col & 0xFF;
            var g = (col >> 8) & 0xFF;
            var b = (col >> 16) & 0xFF;
            var a = col & 0xFF000000;
            var lum = (uint)Math.Clamp((r * 0.299f + g * 0.587f + b * 0.114f) * 0.45f + 45f, 0f, 255f);
            data[i].Col = a | (lum << 16) | (lum << 8) | lum;
        }
    }

    private void HandleKeys()
    {
        if (!ImGui.IsWindowFocused() || ImGui.IsAnyItemActive())
            return;

        var io = ImGui.GetIO();
        var selection = View.Selection;
        var count = Script.Lines.Count;

        // Ctrl+A: every line of the event
        if (io.KeyCtrl && !io.KeyAlt && ImGui.IsKeyPressed(ImGuiKey.A))
            selection.SelectLines(Enumerable.Range(0, count));

        // Alt/Ctrl + arrows are the reorder shortcuts (EmevdEditorScreen)
        if (io.KeyAlt || io.KeyCtrl)
            return;

        // Shift + arrows extend the selection from its anchor
        void Go(int index)
        {
            if (io.KeyShift && index >= 0)
                selection.SelectRange(index);
            else
                selection.SelectInstruction(index);

            selection.ScrollToInstruction = true;
        }

        // By rows, so collapsed blocks are stepped over
        if (ImGui.IsKeyPressed(ImGuiKey.DownArrow) && count > 0)
            Go(StepLine(selection.InstructionIndex, 1));

        if (ImGui.IsKeyPressed(ImGuiKey.UpArrow) && selection.InstructionIndex >= 0)
            Go(StepLine(selection.InstructionIndex, -1));

        // Left collapses the selected C-block (or goes to the block holding the line), Right expands it
        var at = selection.InstructionIndex;
        if (Script.Nodes != null && at >= 0 && at < LineRow.Length)
        {
            var header = Rows[LineRow[at]] is { EndRow: >= 0 } r && r.Line == at;

            if (ImGui.IsKeyPressed(ImGuiKey.LeftArrow))
            {
                if (header && !IsCollapsed(at))
                {
                    SetCollapsed(at, true);
                }
                else
                {
                    var path = new List<(List<EmevdNode> list, int index)>();
                    if (EmevdStructureEdit.Find(Script.Nodes, at, out _, out _, path) && path.Count > 0)
                        Go(path[^1].list[path[^1].index].Line);
                }
            }

            if (ImGui.IsKeyPressed(ImGuiKey.RightArrow) && header)
                SetCollapsed(at, false);
        }

        if (ImGui.IsKeyPressed(ImGuiKey.Enter))
            FollowCall(selection.InstructionIndex);

        if (ImGui.IsKeyPressed(ImGuiKey.Backspace))
            selection.GoBack();
    }

    private void DrawScript()
    {
        var selection = View.Selection;
        var drawList = ImGui.GetWindowDrawList();

        var lineH = ImGui.GetTextLineHeight();
        var padY = 3f;
        var blockH = lineH + padY * 2;
        var footH = 8f * DPI.UIScale();
        var indent = 18f * DPI.UIScale();
        var condIndent = 10f * DPI.UIScale();
        var armW = 10f * DPI.UIScale();
        var hatH = lineH * 2 + padY * 3 + 8f;

        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;

        var indexW = ImGui.CalcTextSize(Script.Lines.Count.ToString()).X + 8f;
        var lanes = Math.Min(DrawnLanes, 12);
        var marginW = indexW + lanes * LaneWidth + 6f;

        // The comment column takes the right of the view while the event has comments (or one is being written)
        var showComments = CFG.Current.EmevdEditor.EmevdEditor_Show_Comments && Comments != null && (EventHasComments() || EditCommentBlock != null);
        var commentW = 240f * DPI.UIScale();
        var commentGap = 16f * DPI.UIScale();
        var blocksW = showComments
            ? Math.Max(width - commentW - commentGap, marginW + 300f * DPI.UIScale())
            : Math.Max(width, marginW + 200f);

        var blockX0 = origin.X + marginW;
        var blockX1 = origin.X + blocksW - 4f;
        var cx0 = blockX1 + commentGap;
        var cx1 = cx0 + commentW;

        // Row tops (relative to the first row) as a prefix sum, since C-block feet are shorter than lines
        var tops = new float[Rows.Count + 1];
        for (var r = 0; r < Rows.Count; r++)
            tops[r + 1] = tops[r] + (Rows[r].Kind == EmevdRowKind.BlockEnd ? footH : Rows[r].Kind == EmevdRowKind.Section ? lineH + 2f : blockH) + RowGap;

        var linesTop = origin.Y + hatH + 6f;
        var totalH = hatH + 6f + tops[^1] + 8f;

        var scrollY = ImGui.GetScrollY();
        var viewH = ImGui.GetWindowHeight();

        // A line hidden in a collapsed block is shown first (search hits, Called By, Problems); scrolled to next frame
        if (selection.ScrollToInstruction && !Reveal(selection.InstructionIndex))
        {
            selection.ScrollToInstruction = false;

            if (selection.InstructionIndex >= 0 && selection.InstructionIndex < LineRow.Length)
            {
                var y = hatH + 6f + tops[LineRow[selection.InstructionIndex]];
                if (y < scrollY || y + blockH > scrollY + viewH)
                    ImGui.SetScrollY(Math.Max(0, y - viewH / 3));
            }
        }

        // The hat block
        DrawHat(drawList, new Vector2(origin.X + indexW, origin.Y + 2f), new Vector2(blockX1, origin.Y + hatH), lineH, padY);

        // Rows on screen: the first row ending below the top of the view, to the first starting below its bottom
        var viewTop = scrollY - hatH - 6f;
        var first = Math.Max(0, Array.BinarySearch(tops, viewTop) is var b && b >= 0 ? b : ~b - 1);
        var last = first;
        while (last < Rows.Count - 1 && tops[last + 1] < viewTop + viewH)
            last++;

        float RowX(int r) => blockX0 + Rows[r].Depth * indent + (Rows[r].ConditionOf >= 0 ? condIndent : 0f);

        // C-block arms: from under each header to its foot, for the blocks crossing the view
        for (var r = 0; r < Rows.Count; r++)
        {
            var row = Rows[r];
            if (row.EndRow < 0 || row.EndRow < first || r > last)
                continue;

            var color = ImGui.GetColorU32(EmevdFormat.BlockColor(Script.Lines[row.Line]));
            var x = blockX0 + row.Depth * indent;
            var armVtx = drawList.VtxBuffer.Size;
            drawList.AddRectFilled(new Vector2(x, linesTop + tops[r] + blockH), new Vector2(x + armW, linesTop + tops[row.EndRow]), color);
            if (IsDimmed(r))
                Grey(drawList, armVtx);
        }

        for (var r = first; r <= last && r < Rows.Count; r++)
        {
            var rowVtx = drawList.VtxBuffer.Size;
            DrawRow(r);
            if (IsDimmed(r))
                Grey(drawList, rowVtx);
        }

        void DrawRow(int r)
        {
            var row = Rows[r];
            var top = linesTop + tops[r];
            var x = RowX(r);

            // A switch's "Default" divider, over the arm like an else row
            if (row.Kind == EmevdRowKind.Section)
            {
                var color = ImGui.GetColorU32(EmevdFormat.BlockColor(Script.Lines[row.Line]));
                var h = lineH + 2f;
                var text = LOC.Get("EMEVD_Script_Default");
                var max = new Vector2(Math.Min(x + ImGui.CalcTextSize(text).X + 16f, blockX1), top + h);
                drawList.AddRectFilled(new Vector2(x, top), max, color, 3f, ImDrawFlags.RoundCornersRight);
                drawList.AddText(new Vector2(x + 8f, top + 1f), ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), text);

                ImGui.SetCursorScreenPos(new Vector2(x, top));
                ImGui.InvisibleButton($"##emevdSection{r}", new Vector2(blockX1 - x, h));
                GUI.Tooltip(LOC.Get("EMEVD_Script_Default_TT"));
                DropTarget(View.Structure.TargetOf(row));
                return;
            }

            if (row.Kind == EmevdRowKind.BlockEnd)
            {
                var color = ImGui.GetColorU32(EmevdFormat.BlockColor(Script.Lines[row.Line]));
                var footMax = new Vector2(Math.Min(x + 160f * DPI.UIScale(), blockX1), top + footH);
                drawList.AddRectFilled(new Vector2(x, top), footMax, color, 4f, ImDrawFlags.RoundCornersBottom);

                // Dropping on a foot puts the block at the end of that C-block
                ImGui.SetCursorScreenPos(new Vector2(x, top));
                ImGui.InvisibleButton($"##emevdFoot{r}", new Vector2(blockX1 - x, footH));
                GUI.Tooltip(LOC.Get("EMEVD_Script_Foot_TT"));
                DropTarget(View.Structure.TargetOf(row));
                return;
            }

            var line = Script.Lines[row.Line];

            // Conditions: a strip joining them to the flow that evaluates them
            if (row.ConditionOf >= 0)
            {
                drawList.AddRectFilled(new Vector2(x - condIndent + 2f, top), new Vector2(x - condIndent + 5f, top + blockH + RowGap),
                    ImGui.GetColorU32(EmevdFormat.BlockColor(Script.Lines[Rows[row.ConditionOf].Line])));
            }

            DrawIndex(drawList, line, new Vector2(origin.X, top + padY));
            DrawBlock(drawList, line, new Vector2(x, top), new Vector2(blockX1, top + blockH), padY, row.IsElse || row.IsCase, row);
        }

        // Brackets for the jumps the C-blocks don't show, crossing the rows on screen
        foreach (var span in DrawnSpans)
        {
            var fromRow = LineRow[span.From];
            var toRow = LineRow[span.To];

            if (toRow < first || fromRow > last)
                continue;

            // Past the lanes there's room for, brackets share the outermost one
            var lane = Math.Min(span.Lane, lanes - 1);
            var x = origin.X + indexW + (lanes - 1 - lane) * LaneWidth + 2f;
            var y0 = linesTop + tops[fromRow] + blockH * 0.5f;
            var y1 = linesTop + tops[toRow] + (span.IsGoto ? blockH * 0.5f : blockH);

            var color = ImGui.GetColorU32(span.Broken ? EmevdFormat.BrokenSpanColor
                : span.IsGoto ? EmevdFormat.GotoSpanColor
                : EmevdFormat.SpanColor);

            drawList.AddLine(new Vector2(RowX(fromRow) - 1f, y0), new Vector2(x, y0), color, 1.5f);
            drawList.AddLine(new Vector2(x, y0), new Vector2(x, y1), color, 1.5f);
            drawList.AddLine(new Vector2(x, y1), new Vector2(RowX(toRow) - 1f, y1), color, 1.5f);
        }

        if (showComments)
        {
            var editorH = Math.Max(blockH, lineH * 6f + 8f);
            var editorBottom = DrawCommentColumn(drawList, tops, first, last, origin, linesTop, hatH, lineH, editorH, blockX1, cx0, cx1);
            totalH = Math.Max(totalH, editorBottom - origin.Y + 8f);
        }
        else if (EditCommentBlock != null && !CFG.Current.EmevdEditor.EmevdEditor_Show_Comments)
        {
            // Comments hidden while one was open
            EndCommentEdit(true);
        }

        // Reserve the whole column, so the scrollbar covers every row
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(showComments ? cx1 - origin.X + 4f : blocksW, totalH));

        DrawArgPopup();
    }

    // ---------------------------------------------------------------- comments

    /// <summary>The block a row comments on: its instruction; null for feet and dividers</summary>
    private object AnchorOf(int r) =>
        r == HatRow ? Script?.Event
        : r >= 0 && r < Rows.Count && Rows[r].Kind == EmevdRowKind.Line ? Script.Lines[Rows[r].Line].Instr
        : null;

    /// <summary>Whether the event's header or a row on show has a comment, cached by rows and comment version</summary>
    private bool EventHasComments()
    {
        var comments = Comments;
        if (comments == null || Script == null)
            return false;

        if (!ReferenceEquals(CommentRowsFor, Rows) || !ReferenceEquals(CommentEventFor, Script.Event) || CommentVersion != comments.Version)
        {
            CommentRowsFor = Rows;
            CommentEventFor = Script.Event;
            CommentVersion = comments.Version;
            HasComments = comments.Has(Script.Event) || Enumerable.Range(0, Rows.Count).Any(r => comments.Has(AnchorOf(r)));
        }

        return HasComments;
    }

    /// <summary>Opens the in-place editor on a line's comment (a new one if it has none); -1 is the event's header</summary>
    public void BeginComment(int line)
    {
        if (Script == null || Comments == null || line >= Script.Lines.Count)
            return;

        var block = line < 0 ? (object)Script.Event : Script.Lines[line].Instr;

        if (EditCommentBlock != null && !ReferenceEquals(EditCommentBlock, block))
            EndCommentEdit(true);

        // A line hidden in a collapsed block is shown first
        if (line >= 0)
            Reveal(line);

        EditCommentBlock = block;
        EditCommentFile = View.Selection.SelectedFile;
        EditCommentEvent = Script.Event;
        EditCommentRow = line < 0 ? HatRow : LineRow[line];
        CommentText = Comments.Get(block) ?? "";
        FocusComment = true;
        CFG.Current.EmevdEditor.EmevdEditor_Show_Comments = true;
    }

    /// <summary>
    /// Closes the editor. keepChange: the editor went away without the field being left (another event was
    /// selected, its block was collapsed away, the comments were hidden): a typed change is kept rather than lost.
    /// </summary>
    private void EndCommentEdit(bool keepChange)
    {
        if (keepChange && EditCommentBlock != null && CommentText != (Comments?.Get(EditCommentBlock) ?? ""))
            Comments?.Set(EditCommentFile, EditCommentBlock, CommentText);

        EditCommentBlock = null;
        EditCommentFile = null;
        EditCommentEvent = null;
        EditCommentRow = NoRow;
        FocusComment = false;
    }

    /// <summary>The open editor's row: where it was opened, else wherever its block is now (an edit moves rows)</summary>
    private int CommentEditRow()
    {
        if (ReferenceEquals(EditCommentBlock, Script.Event))
            return HatRow;

        if (EditCommentRow >= 0 && ReferenceEquals(AnchorOf(EditCommentRow), EditCommentBlock))
            return EditCommentRow;

        for (var r = 0; r < Rows.Count; r++)
        {
            if (ReferenceEquals(AnchorOf(r), EditCommentBlock))
                return r;
        }

        return NoRow;
    }

    /// <summary>
    /// The comment column: a bubble level with the header and each commented block on screen, then the editor if one
    /// is open. Returns the bottom of what it drew past the rows (the editor can reach below the last row).
    /// </summary>
    private float DrawCommentColumn(ImDrawListPtr drawList, float[] tops, int first, int last, Vector2 origin, float linesTop, float hatH,
        float lineH, float editorH, float x1, float cx0, float cx1)
    {
        var comments = Comments;
        var bottom = 0f;

        var editRow = NoRow;
        Vector2 editMin = default, editMax = default;
        if (EditCommentBlock != null)
        {
            editRow = CommentEditRow();

            if (View.Selection.SelectedFile != EditCommentFile || !ReferenceEquals(Script.Event, EditCommentEvent) || (editRow == NoRow && !LayoutDirty))
            {
                EndCommentEdit(true);
            }
            else if (editRow != NoRow)
            {
                EditCommentRow = editRow;
                editMin = new Vector2(cx0, editRow == HatRow ? origin.Y + 2f : linesTop + tops[editRow]);
                editMax = new Vector2(cx1, editMin.Y + editorH);
            }
            // else: the block is being revealed (its rows are rebuilt next frame)
        }

        // A bubble clicked below opens its editor next frame, where its place is known
        var editBlock = EditCommentBlock;

        var bubbleColor = ImGui.GetColorU32(EmevdFormat.CommentColor);
        var textColor = ImGui.GetColorU32(new Vector4(0.10f, 0.09f, 0.05f, 1f));
        var lineColor = ImGui.GetColorU32(EmevdFormat.Darken(EmevdFormat.CommentColor, 0.7f));

        void Bubble(int r, float top, float h)
        {
            var block = AnchorOf(r);
            var text = comments.Get(block);
            if (text == null || (EditCommentBlock != null && r == editRow))
                return;

            var line = r == HatRow ? -1 : Rows[r].Line;
            var min = new Vector2(cx0, top);
            var max = new Vector2(cx1, top + h);
            var midY = (min.Y + max.Y) * 0.5f;

            // A line from the block to the bubble's tail
            drawList.AddLine(new Vector2(x1 + 2f, midY), new Vector2(cx0 - 6f, midY), lineColor, 1f);
            drawList.AddTriangleFilled(new Vector2(cx0, midY - 5f), new Vector2(cx0 - 7f, midY), new Vector2(cx0, midY + 5f), bubbleColor);
            drawList.AddRectFilled(min, max, bubbleColor, 6f);

            var selected = line < 0 ? View.Selection.InstructionIndex < 0 : View.Selection.IsSelected(line);
            if (selected)
                drawList.AddRect(min, max, ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), 6f, ImDrawFlags.None, 2f);

            drawList.PushClipRect(min, max, true);
            var preview = FirstLine(text, cx1 - cx0 - 16f, out var cut);
            drawList.AddText(new Vector2(min.X + 8f, min.Y + (h - lineH) * 0.5f), textColor, cut ? preview + "…" : preview);
            drawList.PopClipRect();

            // A bubble under the open editor doesn't take clicks
            if (EditCommentBlock != null && editRow != NoRow && min.Y < editMax.Y && max.Y > editMin.Y)
                return;

            ImGui.SetCursorScreenPos(min);
            ImGui.InvisibleButton($"##emevdComment{r}", max - min);
            if (ImGui.IsItemHovered())
                GUI.Tooltip(LOC.Get("EMEVD_Comment_Bubble_TT", text));
            if (ImGui.IsItemClicked())
            {
                View.Selection.SelectInstruction(line);
                BeginComment(line);
            }

            if (ImGui.BeginPopupContextItem($"##emevdComment{r}_menu"))
            {
                if (ImGui.MenuItem(LOC.Get("EMEVD_Comment_Edit")))
                    BeginComment(line);
                if (ImGui.MenuItem(LOC.Get("EMEVD_Comment_Remove")))
                    comments.Set(View.Selection.SelectedFile, block, null);
                ImGui.EndPopup();
            }
        }

        Bubble(HatRow, origin.Y + 2f, hatH - 2f);
        for (var r = first; r <= last && r < Rows.Count; r++)
            Bubble(r, linesTop + tops[r], tops[r + 1] - tops[r] - RowGap);

        if (EditCommentBlock != null && ReferenceEquals(EditCommentBlock, editBlock) && editRow != NoRow)
            bottom = DrawCommentEditor(drawList, editMin, editMax, x1, lineH, lineColor);

        return bottom;
    }

    /// <summary>The open comment's text box, over its bubble's place. Leaving it after an edit keeps the text; Escape discards.</summary>
    private float DrawCommentEditor(ImDrawListPtr drawList, Vector2 min, Vector2 max, float x1, float lineH, uint lineColor)
    {
        var midY = min.Y + lineH * 0.5f + 4f;
        drawList.AddLine(new Vector2(x1 + 2f, midY), new Vector2(min.X - 6f, midY), lineColor, 1f);

        if (FocusComment)
        {
            // A narrow view puts the column past the right edge: bring the editor into view
            var winX = ImGui.GetWindowPos().X;
            var winW = ImGui.GetWindowWidth();
            if (max.X > winX + winW - ImGui.GetStyle().ScrollbarSize)
                ImGui.SetScrollFromPosX(max.X - winX + 8f, 1f);
        }

        ImGui.SetCursorScreenPos(min);
        var focusing = FocusComment;
        if (FocusComment)
        {
            ImGui.SetKeyboardFocusHere();
            FocusComment = false;
        }

        ImGui.PushStyleColor(ImGuiCol.FrameBg, EmevdFormat.Darken(EmevdFormat.CommentColor, 0.35f));
        ImGui.InputTextMultiline("##emevdCommentEdit", ref CommentText, 4096, max - min);
        ImGui.PopStyleColor();

        var hovered = ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenBlockedByActiveItem);
        var active = ImGui.IsItemActive();
        var clickedAway = !hovered && (ImGui.IsMouseClicked(ImGuiMouseButton.Left) || ImGui.IsMouseClicked(ImGuiMouseButton.Right));

        if (GUI.InputCommitted())
        {
            Comments?.Set(EditCommentFile, EditCommentBlock, CommentText);
            EndCommentEdit(false);
        }
        else if (ImGui.IsItemDeactivated())
        {
            if (ImGui.IsKeyPressed(ImGuiKey.Escape, false) || clickedAway)
                EndCommentEdit(false);
            else
                FocusComment = true; // Focus taken away without the user leaving (a closing menu hands focus back to its window): take it back
        }
        else if (!active && !focusing && clickedAway)
        {
            // Never focused (or focus didn't take), and the user clicked elsewhere: nothing was typed
            EndCommentEdit(false);
        }

        var hintPos = new Vector2(min.X, max.Y + 2f);
        drawList.AddText(hintPos, ImGui.GetColorU32(ImGuiCol.TextDisabled), LOC.Get("EMEVD_Comment_Hint"));
        return hintPos.Y + lineH;
    }

    /// <summary>The comment items of a block's menu: add / edit / remove; line -1 is the event's header</summary>
    private void CommentItems(int line)
    {
        var comments = Comments;
        if (comments == null || Script == null || line >= Script.Lines.Count)
            return;

        var block = line < 0 ? (object)Script.Event : Script.Lines[line].Instr;
        var has = comments.Has(block);

        if (ImGui.MenuItem(LOC.Get(has ? "EMEVD_Comment_Edit" : "EMEVD_Comment_Add")))
            BeginComment(line);
        GUI.Tooltip(LOC.Get("EMEVD_Comment_Add_TT"));

        if (has && ImGui.MenuItem(LOC.Get("EMEVD_Comment_Remove")))
            comments.Set(View.Selection.SelectedFile, block, null);
    }

    /// <summary>The text's first line, cut to fit a width; cut is true when anything was left out</summary>
    private static string FirstLine(string text, float width, out bool cut)
    {
        var nl = text.IndexOfAny(new[] { '\r', '\n' });
        var line = nl >= 0 ? text[..nl] : text;
        cut = nl >= 0 && text[nl..].Trim().Length > 0;

        var ellipsis = ImGui.CalcTextSize("…").X;
        if (ImGui.CalcTextSize(line).X <= width - (cut ? ellipsis : 0f))
            return line;

        cut = true;
        var lo = 0;
        var hi = line.Length;
        while (lo < hi)
        {
            var mid = (lo + hi + 1) / 2;
            if (ImGui.CalcTextSize(line[..mid]).X <= width - ellipsis)
                lo = mid;
            else
                hi = mid - 1;
        }
        return line[..lo].TrimEnd();
    }

    /// <summary>Edits the arg whose chip was clicked: its value and the parameter slot that writes it</summary>
    private void DrawArgPopup()
    {
        if (OpenPopup)
        {
            ImGui.OpenPopup("##emevdArgPopup");
            OpenPopup = false;
        }

        if (!ImGui.BeginPopup("##emevdArgPopup"))
            return;

        if (PopupLine < 0 || PopupLine >= Script.Lines.Count || PopupArg < 0 || PopupArg >= Script.Lines[PopupLine].Decoded.Args.Count)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        var line = Script.Lines[PopupLine];
        var arg = line.Decoded.Args[PopupArg];
        var width = 220f * DPI.UIScale();

        ImGui.Text(EmevdFormat.ArgName(arg));
        ImGui.SameLine();
        ImGui.TextDisabled($"{EmevdDecoded.TypeName(arg.Type)} +{arg.Offset:X2}");

        // Opened from a MustSub call in place: this call arg is what the event's arg receives
        if (PopupSetsName != null)
            ImGui.TextDisabled(LOC.Get("EMEVD_Script_Inline_Sets", PopupSetsName));

        if (line.FlowArgOffset == arg.Offset && line.Kind is EmevdBlockKind.Skip or EmevdBlockKind.Goto)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EMEVD_Edit_Flow_Arg_Warning"));

        View.ArgEditor.Value("##popupValue", line, arg, width);

        var e = Catalog?.FindEnum(arg.Def.EnumKey);
        if (e != null && e.Values.Count > 0)
        {
            ImGui.SameLine();
            View.ArgEditor.RawInt("##popupRaw", line, arg, 90f * DPI.UIScale());
        }

        ImGui.TextDisabled(LOC.Get("EMEVD_Edit_Slot"));
        ImGui.SameLine();
        View.ArgEditor.Slot("##popupSlot", Script, line, arg, width);

        // What the value refers to, with its link
        if (View.RoleLinks.Describe(line, arg) is { } refers)
        {
            View.RoleLinks.Button("popupRoleLink", line, arg);
            View.RoleLinks.FlagMarker(line, arg);
            ImGui.SameLine();
            ImGui.TextDisabled(refers);
        }

        ImGui.EndPopup();
    }

    private void DrawHat(ImDrawListPtr drawList, Vector2 min, Vector2 max, float lineH, float padY)
    {
        var selection = View.Selection;
        var ev = Script.Event;
        var selected = selection.InstructionIndex < 0;

        ImGui.SetCursorScreenPos(min);
        ImGui.InvisibleButton("##emevdHat", max - min);
        DropTarget(View.Structure.TopTarget());

        var hovered = ImGui.IsItemHovered();
        if (ImGui.IsItemClicked() || ImGui.IsItemClicked(ImGuiMouseButton.Right))
            selection.SelectInstruction(-1);

        if (ImGui.BeginPopupContextItem("##emevdHatMenu"))
        {
            CommentItems(-1);
            ImGui.EndPopup();
        }

        var color = EmevdFormat.HatColor;
        if (hovered)
            color = EmevdFormat.Darken(color, 1.15f);

        drawList.AddRectFilled(min, max, ImGui.GetColorU32(color), 10f, ImDrawFlags.RoundCornersTop);

        if (selected)
            drawList.AddRect(min, max, ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), 10f, ImDrawFlags.RoundCornersTop, 2f);

        var white = ImGui.GetColorU32(new Vector4(1, 1, 1, 1));
        var dim = ImGui.GetColorU32(new Vector4(1, 1, 1, 0.7f));

        drawList.PushClipRect(min, max, true);

        var pos = new Vector2(min.X + 10f, min.Y + padY + 2f);
        var title = LOC.Get("EMEVD_Script_Hat_Title", ev.ID);
        drawList.AddText(pos, white, title);
        pos.X += ImGui.CalcTextSize(title).X + 10f;

        if (!string.IsNullOrEmpty(ev.Name))
            drawList.AddText(pos, ImGui.GetColorU32(UI.Current.ImGui_AliasName_Text), ev.Name);

        pos = new Vector2(min.X + 10f, min.Y + lineH + padY * 2 + 4f);
        var rest = LOC.Get("EMEVD_Script_Hat_Rest", EmevdFormat.RestBehavior(ev.RestBehavior));
        drawList.AddText(pos, dim, rest);
        pos.X += ImGui.CalcTextSize(rest).X + 12f;

        if (Script.Slots.Count > 0)
        {
            var label = LOC.Get("EMEVD_Script_Hat_Params");
            drawList.AddText(pos, dim, label);
            pos.X += ImGui.CalcTextSize(label).X + 6f;

            foreach (var (source, size) in Script.Slots)
                pos.X = Chip(drawList, pos, EmevdScript.SlotName(source, size), EmevdFormat.ParamChipColor, true, lineH) + 4f;
        }

        drawList.PopClipRect();

        if (hovered)
            GUI.Tooltip(LOC.Get("EMEVD_Script_Hat_TT"));
    }

    private void DrawIndex(ImDrawListPtr drawList, EmevdLine line, Vector2 pos)
    {
        drawList.AddText(pos, ImGui.GetColorU32(ImGuiCol.TextDisabled), line.Index.ToString());
    }

    /// <param name="isElse">The line is a C-block's else jump: drawn as "Else", without its args</param>
    private void DrawBlock(ImDrawListPtr drawList, EmevdLine line, Vector2 min, Vector2 max, float padY, bool isElse, EmevdRow row)
    {
        var selection = View.Selection;
        var lineH = ImGui.GetTextLineHeight();

        ImGui.SetCursorScreenPos(min);
        ImGui.InvisibleButton($"##emevdLine{line.Index}", max - min);

        var hovered = ImGui.IsItemHovered();
        var clicked = ImGui.IsItemClicked();
        var deactivated = ImGui.IsItemDeactivated();
        var io = ImGui.GetIO();
        var plain = !io.KeyCtrl && !io.KeyShift;

        // A C-block header's caret collapses or expands it, instead of selecting
        var toggleW = row.EndRow >= 0 ? lineH + 2f : 0f;
        if (clicked && toggleW > 0 && ImGui.GetMousePos().X < min.X + toggleW)
        {
            SetCollapsed(line.Index, !row.Collapsed);
            clicked = false;
        }

        // Right-click keeps a selection that holds the line, so its menu acts on all of it
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && !selection.IsSelected(line.Index))
            selection.SelectInstruction(line.Index);

        // Drag and drop act on the last item, so before the context menu draws items of its own
        DragSource(line);
        DropTarget(View.Structure.TargetOf(row));
        BlockContextMenu(line.Index);

        if (clicked)
        {
            DragStarted = false;

            if (io.KeyCtrl)
                selection.Toggle(line.Index);
            else if (io.KeyShift)
                selection.SelectRange(line.Index);
            else if (selection.IsSelected(line.Index) && selection.Count > 1)
                PendingSingle = line.Index; // decided on release: a drag moves the whole selection
            else
                selection.SelectInstruction(line.Index);
        }

        if (deactivated && PendingSingle == line.Index)
        {
            if (!DragStarted)
                selection.SelectInstruction(line.Index);

            PendingSingle = -1;
        }

        var selected = selection.IsSelected(line.Index);
        var primary = selection.InstructionIndex == line.Index;

        if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            FollowCall(line.Index);

        var mouse = ImGui.GetMousePos();
        var hoveredArg = -1;

        // A MustSub call in place: drawn as the event's instruction, its values the call's
        var inlined = isElse ? null : Inlined(line);
        var shown = inlined?.Line ?? line;

        var color = EmevdFormat.BlockColor(shown);
        if (hovered)
            color = EmevdFormat.Darken(color, 1.2f);

        var col = ImGui.GetColorU32(color);
        var h = max.Y - min.Y;

        switch (line.Kind)
        {
            case EmevdBlockKind.Condition:
            {
                // Hexagonal ends, like Scratch's boolean reporters
                var n = h * 0.5f;
                drawList.AddRectFilled(new Vector2(min.X + n, min.Y), new Vector2(max.X - n, max.Y), col);
                drawList.AddTriangleFilled(new Vector2(min.X + n, min.Y), new Vector2(min.X + n, max.Y), new Vector2(min.X, min.Y + n), col);
                drawList.AddTriangleFilled(new Vector2(max.X - n, min.Y), new Vector2(max.X, min.Y + n), new Vector2(max.X - n, max.Y), col);
                break;
            }
            case EmevdBlockKind.End when line.IsCap:
                drawList.AddRectFilled(min, max, col, 8f, ImDrawFlags.RoundCornersBottom);
                break;
            case EmevdBlockKind.Label:
                drawList.AddRectFilled(min, new Vector2(min.X + 6f, max.Y), ImGui.GetColorU32(EmevdFormat.GotoSpanColor));
                drawList.AddRectFilled(new Vector2(min.X + 6f, min.Y), max, col, 3f, ImDrawFlags.RoundCornersRight);
                break;
            default:
                drawList.AddRectFilled(min, max, col, 4f);
                break;
        }

        // The primary line (shown in Properties) gets the full outline; the rest of a multi-selection a lighter one
        if (selected)
            drawList.AddRect(min, max, ImGui.GetColorU32(new Vector4(1, 1, 1, primary ? 1f : 0.6f)), 4f, ImDrawFlags.None, primary ? 2f : 1.5f);

        var white = ImGui.GetColorU32(new Vector4(1, 1, 1, 1));
        var dim = ImGui.GetColorU32(new Vector4(1, 1, 1, 0.65f));

        drawList.PushClipRect(min, max, true);

        var x = min.X + (line.Kind == EmevdBlockKind.Condition ? h * 0.5f + 2f : 8f);
        var y = min.Y + padY;

        if (toggleW > 0)
        {
            drawList.AddText(new Vector2(min.X + 5f, y), white, row.Collapsed ? Icons.CaretRight : Icons.CaretDown);
            x = Math.Max(x, min.X + toggleW + 2f);
        }

        // In place, a call-coloured strip on the left says it's still a call underneath
        if (inlined != null)
        {
            drawList.AddRectFilled(min, new Vector2(min.X + 4f, max.Y), ImGui.GetColorU32(EmevdFormat.CallColor), 4f, ImDrawFlags.RoundCornersLeft);
            x += 2f;
        }

        var name = !isElse ? shown.Name : row.IsCase ? LOC.Get("EMEVD_Script_Case") : LOC.Get("EMEVD_Script_Else");
        drawList.AddText(new Vector2(x, y), shown.Decoded.Def?.HasName == false && !isElse ? dim : white, name);
        x += ImGui.CalcTextSize(name).X + 10f;

        if (row.IsSwitch)
            x = Chip(drawList, new Vector2(x, y), LOC.Get("EMEVD_Script_Switch"), EmevdFormat.Darken(color, 0.5f), false, lineH) + 8f;

        if (ExitLevels.TryGetValue(line.Index, out var exitLevel))
        {
            x = Chip(drawList, new Vector2(x, y), LOC.Get("EMEVD_Script_Exit", exitLevel + 1), EmevdFormat.Darken(color, 0.5f), false, lineH) + 8f;
        }

        if (row.Collapsed)
            x = Chip(drawList, new Vector2(x, y), LOC.Get("EMEVD_Script_Collapsed", row.Hidden), EmevdFormat.Darken(color, 0.5f), false, lineH) + 8f;

        if (isElse)
        {
            drawList.AddText(new Vector2(x, y), dim, row.IsCase ? line.Name : line.Key);
        }
        else if (shown.Decoded.Fits)
        {
            for (var a = 0; a < shown.Decoded.Args.Count; a++)
            {
                var arg = shown.Decoded.Args[a];

                if (x > max.X)
                    break;

                if (CFG.Current.EmevdEditor.EmevdEditor_Show_Arg_Names)
                {
                    var argName = EmevdFormat.ArgName(arg);
                    drawList.AddText(new Vector2(x, y), dim, argName);
                    x += ImGui.CalcTextSize(argName).X + 4f;
                }

                // In place, an arg the MustSub event fixes isn't the call's to change: drawn fainter
                var fixedArg = inlined != null && inlined.CallArg[a] < 0;

                var chipX = x;
                var chipHovered = hovered && mouse.X >= chipX && mouse.Y >= min.Y && mouse.Y <= max.Y;
                var chipColor = EmevdFormat.Darken(color, fixedArg ? 0.8f : chipHovered ? 0.45f : 0.6f);

                if (shown.Params.TryGetValue(arg.Offset, out var p))
                    x = Chip(drawList, new Vector2(x, y), EmevdScript.SlotName(p.SourceStartByte, p.ByteCount), EmevdFormat.ParamChipColor, true, lineH);
                else
                    x = Chip(drawList, new Vector2(x, y), EmevdFormat.Value(Catalog, arg), chipColor, false, lineH);

                if (chipHovered && mouse.X <= x)
                    hoveredArg = a;

                x += 8f;
            }

            if (inlined != null)
                drawList.AddText(new Vector2(x, y), dim, LOC.Get("EMEVD_Script_Inline_Via", EmevdFormat.EventLabel(inlined.Target)));
            else if (line.Kind == EmevdBlockKind.Call)
                DrawCallTarget(drawList, line, new Vector2(x, y), dim);
        }
        else
        {
            var raw = $"{line.Key}  {EmevdFormat.Hex(line.Instr.ArgData)}";
            drawList.AddText(new Vector2(x, y), dim, raw);
        }

        if (line.Instr.Layer != null)
        {
            var layer = LOC.Get("EMEVD_Script_Layer", line.Instr.Layer.Value.ToString("X"));
            var w = ImGui.CalcTextSize(layer).X + 8f;
            Chip(drawList, new Vector2(max.X - w - 4f, y), layer, EmevdFormat.Darken(color, 0.5f), false, lineH);
        }

        drawList.PopClipRect();

        // In place, a chip edits the call arg it comes from; one the MustSub event fixes isn't editable here
        var popupArg = inlined == null ? hoveredArg : hoveredArg >= 0 ? inlined.CallArg[hoveredArg] : -1;

        if (clicked && plain && popupArg >= 0 && selection.Count <= 1)
        {
            PopupLine = line.Index;
            PopupArg = popupArg;
            PopupSetsName = inlined != null ? EmevdFormat.ArgName(shown.Decoded.Args[hoveredArg]) : null;
            OpenPopup = true;
        }

        if (hovered && hoveredArg >= 0)
        {
            // The chip's arg, and what its value refers to (a param row name, a text entry, a map entity…)
            var chipArg = shown.Decoded.Args[hoveredArg];
            var refers = View.RoleLinks.Describe(shown, chipArg);
            var text = popupArg >= 0
                ? LOC.Get("EMEVD_Script_Chip_TT", EmevdFormat.ArgName(chipArg))
                : LOC.Get("EMEVD_Script_Inline_Fixed_TT", EmevdFormat.ArgName(chipArg), EmevdFormat.EventLabel(inlined.Target));
            GUI.Tooltip(refers != null ? $"{refers}\n{text}" : text);
        }
        else if (hovered)
            BlockTooltip(line, inlined);
    }

    // ---------------------------------------------------------------- structural edits

    private int DragLine = -1;

    // A plain press on a line of a multi-selection: it becomes the only selected line on release, unless it was dragged
    private int PendingSingle = -1;
    private bool DragStarted;

    private const string BlockPayload = "EMEVD_BLOCK";
    public const string PalettePayload = "EMEVD_PALETTE";

    /// <summary>Dragging a block moves it with its C-block body (structured view only)</summary>
    private unsafe void DragSource(EmevdLine line)
    {
        if (Script?.Nodes == null || !ImGui.BeginDragDropSource())
            return;

        DragLine = line.Index;
        DragStarted = true;
        byte dummy = 0;
        ImGui.SetDragDropPayload(BlockPayload, &dummy, 1);

        var selection = View.Selection;
        ImGui.Text(selection.IsSelected(line.Index) && selection.Count > 1
            ? LOC.Get("EMEVD_Script_Drag_Many", selection.Count)
            : LOC.Get("EMEVD_Script_Drag", line.Name));
        ImGui.EndDragDropSource();
    }

    /// <summary>Accepts a dragged block (move) or palette instruction (insert) at a target</summary>
    private unsafe void DropTarget(EmevdDropTarget target)
    {
        if (target == null || !ImGui.BeginDragDropTarget())
            return;

        var block = ImGui.AcceptDragDropPayload(BlockPayload);
        if (!block.IsNull && DragLine >= 0)
        {
            View.Structure.Move(DragLine, target);
            DragLine = -1;
        }

        var palette = ImGui.AcceptDragDropPayload(PalettePayload);
        var paletteTool = View.ToolWindow.PaletteTool;
        if (!palette.IsNull && paletteTool.DragDef != null)
        {
            View.Structure.Insert(paletteTool.DragDef, target);
            paletteTool.DragDef = null;
        }

        ImGui.EndDragDropTarget();
    }

    private void BlockContextMenu(int line)
    {
        if (!ImGui.BeginPopupContextItem($"##emevdBlockMenu{line}"))
            return;

        var s = View.Structure;
        EditItems(s);

        // The comment is the clicked line's, even in a multi-selection
        ImGui.Separator();
        CommentItems(line);

        ImGui.EndPopup();
    }

    /// <summary>The structural edit menu items, shared by the block menu and the Edit menu</summary>
    public static void EditItems(EmevdStructureEditor s)
    {
        if (ImGui.MenuItem(LOC.Get("EMEVD_Edit_Op_Up"), InputManager.GetHint(KeybindID.Reorder_Up), false, s.HasLine))
            s.Step(false);
        if (ImGui.MenuItem(LOC.Get("EMEVD_Edit_Op_Down"), InputManager.GetHint(KeybindID.Reorder_Down), false, s.HasLine))
            s.Step(true);

        ImGui.Separator();

        if (ImGui.MenuItem(LOC.Get("EMEVD_Edit_Op_Copy"), InputManager.GetHint(KeybindID.Copy), false, s.HasLine))
            s.Copy();
        if (ImGui.MenuItem(LOC.Get("EMEVD_Edit_Op_Cut"), "", false, s.HasLine))
            s.Cut();
        if (ImGui.MenuItem(LOC.Get("EMEVD_Edit_Op_Paste"), InputManager.GetHint(KeybindID.Paste), false, s.CanPaste))
            s.Paste();
        if (ImGui.MenuItem(LOC.Get("EMEVD_Edit_Op_Duplicate"), InputManager.GetHint(KeybindID.Duplicate), false, s.HasLine))
            s.Duplicate();

        ImGui.Separator();

        if (ImGui.BeginMenu(LOC.Get("EMEVD_Edit_Op_Wrap_Menu"), s.HasLine))
        {
            var canWrap = s.CanWrap;
            if (!canWrap)
                ImGui.TextDisabled(LOC.Get("EMEVD_Edit_Wrap_Not_Adjacent"));

            WrapMenu(s, EmevdFlowKind.Skip, "EMEVD_Edit_Op_Wrap_Skip", canWrap);
            WrapMenu(s, EmevdFlowKind.Goto, "EMEVD_Edit_Op_Wrap_Goto", canWrap);
            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("EMEVD_Edit_Op_Wrap_TT"));

        ImGui.Separator();

        if (ImGui.MenuItem(LOC.Get("EMEVD_Edit_Op_Delete"), InputManager.GetHint(KeybindID.Delete), false, s.HasLine))
            s.Delete();
        GUI.Tooltip(LOC.Get("EMEVD_Edit_Op_Delete_TT"));
    }

    private static void WrapMenu(EmevdStructureEditor s, EmevdFlowKind kind, string label, bool enabled)
    {
        if (!ImGui.BeginMenu(LOC.Get(label), enabled))
            return;

        var defs = s.WrapDefs(kind);
        EmevdClassDef cls = null;

        foreach (var def in defs)
        {
            if (def.Class != cls)
            {
                if (cls != null)
                    ImGui.Separator();
                cls = def.Class;
                ImGui.TextDisabled(cls.DisplayName);
            }

            if (ImGui.MenuItem($"{def.DisplayName}##emevdWrap{def.Key}", def.Key))
                s.Wrap(def);
        }

        ImGui.EndMenu();
    }

    /// <summary>A rounded value box; returns the x after it</summary>
    private static float Chip(ImDrawListPtr drawList, Vector2 pos, string text, Vector4 color, bool darkText, float lineH)
    {
        var size = ImGui.CalcTextSize(text);
        var min = new Vector2(pos.X, pos.Y - 1f);
        var max = new Vector2(pos.X + size.X + 8f, pos.Y + lineH + 1f);

        drawList.AddRectFilled(min, max, ImGui.GetColorU32(color), lineH * 0.5f);
        drawList.AddText(new Vector2(pos.X + 4f, pos.Y), ImGui.GetColorU32(darkText ? new Vector4(0, 0, 0, 1) : new Vector4(1, 1, 1, 1)), text);

        return max.X;
    }

    private void DrawCallTarget(ImDrawListPtr drawList, EmevdLine line, Vector2 pos, uint dim)
    {
        if (line.CallTargetIsParam || line.CallTarget < 0)
            return;

        var (file, ev) = ResolveCall(line);

        var text = ev == null
            ? LOC.Get("EMEVD_Script_Call_Missing")
            : file == View.Selection.SelectedFile
                ? $"{Icons.LongArrowRight} {EmevdFormat.EventLabel(ev)}"
                : $"{Icons.LongArrowRight} {file.Name}: {EmevdFormat.EventLabel(ev)}";

        drawList.AddText(pos, ev == null ? ImGui.GetColorU32(UI.Current.ImGui_Warning_Text_Color) : dim, text);
    }

    /// <summary>
    /// A MustSub call as the SpEffectMustSub event it runs, when the View toggle is on and the target is one
    /// instruction; null otherwise (the call is drawn as itself)
    /// </summary>
    private EmevdInlined Inlined(EmevdLine line)
    {
        var on = CFG.Current.EmevdEditor.EmevdEditor_Inline_MustSub_Calls;
        if (on != CachedInline)
        {
            InlinedCalls.Clear();
            CachedInline = on;
        }

        if (!on || line.Kind != EmevdBlockKind.Call || line.CallTargetIsParam || line.CallTarget < 0)
            return null;

        if (InlinedCalls.TryGetValue(line.Index, out var cached))
            return cached;

        var (file, ev) = ResolveCall(line);
        var inlined = file != null && file.Name.Equals(EmevdBank.MustSubFile, StringComparison.OrdinalIgnoreCase)
            ? EmevdInline.Build(Catalog, line, ev)
            : null;

        InlinedCalls[line.Index] = inlined;
        return inlined;
    }

    public (EmevdFileWrapper file, EMEVD.Event ev) ResolveCall(EmevdLine line)
    {
        if (CallTargets.TryGetValue(line.Index, out var target))
            return target;

        var bank = View.Project.Handler.EmevdData?.PrimaryBank;
        target = bank?.ResolveEvent(View.Selection.SelectedFile, line.CallTarget) ?? (null, null);
        CallTargets[line.Index] = target;

        return target;
    }

    /// <summary>Jumps to the event an Initialize Event line starts</summary>
    public void FollowCall(int index)
    {
        if (Script == null || index < 0 || index >= Script.Lines.Count)
            return;

        var line = Script.Lines[index];
        if (line.Kind != EmevdBlockKind.Call || line.CallTargetIsParam || line.CallTarget < 0)
            return;

        var (file, ev) = ResolveCall(line);
        View.Selection.JumpTo(file, ev);
    }

    private void BlockTooltip(EmevdLine line, EmevdInlined inlined = null)
    {
        // In place: which MustSub event this is, and the call it stands for
        if (inlined != null)
        {
            var target = inlined.Line;
            GUI.Tooltip(LOC.Get("EMEVD_Script_Inline_TT", target.Key, target.Name, EmevdFormat.EventLabel(inlined.Target), line.Key, line.Name));
            return;
        }

        var def = line.Decoded.Def;
        var text = def == null
            ? LOC.Get("EMEVD_Script_Block_No_Layout_TT", line.Key)
            : LOC.Get("EMEVD_Script_Block_TT", line.Key, def.DisplayName, def.Class.DisplayName, def.Confidence);

        if (!line.Decoded.Fits && def != null)
            text += "\n" + line.Decoded.Problem;

        if (line.Kind == EmevdBlockKind.Call && !line.CallTargetIsParam && line.CallTarget >= 0)
            text += "\n" + LOC.Get("EMEVD_Script_Block_Call_TT");

        GUI.Tooltip(text);
    }
}
