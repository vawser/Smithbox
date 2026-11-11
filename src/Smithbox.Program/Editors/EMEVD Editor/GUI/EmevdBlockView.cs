using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The selected event as a column of Scratch-style blocks (Claude/Documentation/EMEVD Block Editor Analysis.md, §5
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
    private readonly Dictionary<int, int> ExitLevels = new();
    private List<EmevdSpan> DrawnSpans = new();
    private int DrawnLanes;

    // The arg chip popup: which line and arg it edits
    private int PopupLine = -1;
    private int PopupArg = -1;
    private bool OpenPopup;

    // Initialize Event targets, resolved when first drawn
    private readonly Dictionary<int, (EmevdFileWrapper file, EMEVD.Event ev)> CallTargets = new();

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
            var structured = CFG.Current.EmevdEditor_Structured_View;

            if (ev != CachedEvent || revision != CachedRevision || structured != CachedStructured)
            {
                if (ev != CachedEvent)
                    PopupLine = -1;

                CachedEvent = ev;
                CachedRevision = revision;
                CachedStructured = structured;
                Script = ev != null ? EmevdScript.Build(Catalog, ev) : null;
                CallTargets.Clear();
                BuildLayout(structured);
            }

            return Script;
        }
    }

    /// <summary>The rows, which line each row is, the exits, and the brackets still drawn</summary>
    private void BuildLayout(bool structured)
    {
        ExitLevels.Clear();

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
            Rows = EmevdStructure.Rows(Script.Nodes);
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

            CollectExits(n.Body);
            CollectExits(n.Else);
        }
    }

    public void Draw()
    {
        _ = CurrentScript;

        DrawHeader();

        ImGui.BeginChild("emevdScript", Vector2.Zero, ImGuiChildFlags.Borders);

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

        if (ImGui.IsKeyPressed(ImGuiKey.DownArrow) && count > 0)
            Go(Math.Min(selection.InstructionIndex + 1, count - 1));

        if (ImGui.IsKeyPressed(ImGuiKey.UpArrow) && selection.InstructionIndex >= 0)
            Go(selection.InstructionIndex - 1);

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
        var blockX0 = origin.X + marginW;
        var blockX1 = origin.X + Math.Max(width, marginW + 200f) - 4f;

        // Row tops (relative to the first row) as a prefix sum, since C-block feet are shorter than lines
        var tops = new float[Rows.Count + 1];
        for (var r = 0; r < Rows.Count; r++)
            tops[r + 1] = tops[r] + (Rows[r].Kind == EmevdRowKind.BlockEnd ? footH : blockH) + RowGap;

        var linesTop = origin.Y + hatH + 6f;
        var totalH = hatH + 6f + tops[^1] + 8f;

        var scrollY = ImGui.GetScrollY();
        var viewH = ImGui.GetWindowHeight();

        if (selection.ScrollToInstruction)
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
            drawList.AddRectFilled(new Vector2(x, linesTop + tops[r] + blockH), new Vector2(x + armW, linesTop + tops[row.EndRow]), color);
        }

        for (var r = first; r <= last && r < Rows.Count; r++)
        {
            var row = Rows[r];
            var top = linesTop + tops[r];
            var x = RowX(r);

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
                continue;
            }

            var line = Script.Lines[row.Line];

            // Conditions: a strip joining them to the flow that evaluates them
            if (row.ConditionOf >= 0)
            {
                drawList.AddRectFilled(new Vector2(x - condIndent + 2f, top), new Vector2(x - condIndent + 5f, top + blockH + RowGap),
                    ImGui.GetColorU32(EmevdFormat.BlockColor(Script.Lines[Rows[row.ConditionOf].Line])));
            }

            DrawIndex(drawList, line, new Vector2(origin.X, top + padY));
            DrawBlock(drawList, line, new Vector2(x, top), new Vector2(blockX1, top + blockH), padY, row.IsElse, row);
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

        // Reserve the whole column, so the scrollbar covers every row
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(Math.Max(width, marginW + 200f), totalH));

        DrawArgPopup();
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
        if (ImGui.IsItemClicked())
            selection.SelectInstruction(-1);

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

        var color = EmevdFormat.BlockColor(line);
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

        var name = isElse ? LOC.Get("EMEVD_Script_Else") : line.Name;
        drawList.AddText(new Vector2(x, y), line.Decoded.Def?.HasName == false && !isElse ? dim : white, name);
        x += ImGui.CalcTextSize(name).X + 10f;

        if (ExitLevels.TryGetValue(line.Index, out var exitLevel))
        {
            x = Chip(drawList, new Vector2(x, y), LOC.Get("EMEVD_Script_Exit", exitLevel + 1), EmevdFormat.Darken(color, 0.5f), false, lineH) + 8f;
        }

        if (isElse)
        {
            drawList.AddText(new Vector2(x, y), dim, line.Key);
        }
        else if (line.Decoded.Fits)
        {
            for (var a = 0; a < line.Decoded.Args.Count; a++)
            {
                var arg = line.Decoded.Args[a];

                if (x > max.X)
                    break;

                if (CFG.Current.EmevdEditor_Show_Arg_Names)
                {
                    var argName = EmevdFormat.ArgName(arg);
                    drawList.AddText(new Vector2(x, y), dim, argName);
                    x += ImGui.CalcTextSize(argName).X + 4f;
                }

                var chipX = x;
                var chipHovered = hovered && mouse.X >= chipX && mouse.Y >= min.Y && mouse.Y <= max.Y;
                var chipColor = EmevdFormat.Darken(color, chipHovered ? 0.45f : 0.6f);

                if (line.Params.TryGetValue(arg.Offset, out var p))
                    x = Chip(drawList, new Vector2(x, y), EmevdScript.SlotName(p.SourceStartByte, p.ByteCount), EmevdFormat.ParamChipColor, true, lineH);
                else
                    x = Chip(drawList, new Vector2(x, y), EmevdFormat.Value(Catalog, arg), chipColor, false, lineH);

                if (chipHovered && mouse.X <= x)
                    hoveredArg = a;

                x += 8f;
            }

            if (line.Kind == EmevdBlockKind.Call)
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

        if (clicked && plain && hoveredArg >= 0 && selection.Count <= 1)
        {
            PopupLine = line.Index;
            PopupArg = hoveredArg;
            OpenPopup = true;
        }

        if (hovered && hoveredArg >= 0)
            GUI.Tooltip(LOC.Get("EMEVD_Script_Chip_TT", EmevdFormat.ArgName(line.Decoded.Args[hoveredArg])));
        else if (hovered)
            BlockTooltip(line);
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

        if (ImGui.MenuItem(LOC.Get("EMEVD_Edit_Op_Delete"), InputManager.GetHint(KeybindID.Delete), false, s.HasLine))
            s.Delete();
        GUI.Tooltip(LOC.Get("EMEVD_Edit_Op_Delete_TT"));
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

    private void BlockTooltip(EmevdLine line)
    {
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
