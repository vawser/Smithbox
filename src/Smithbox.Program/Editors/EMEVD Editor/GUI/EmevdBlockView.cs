using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// The selected event as a column of Scratch-style blocks (Claude/Documentation/EMEVD Block Editor Analysis.md, §5
/// Option A): a hat block for the event header, then one block per instruction. P1 is flat: skips and gotos are drawn
/// as brackets in the margin, coloured red when they run past the end or miss their label.
///
/// Every row has the same height, so only the rows on screen are drawn (the Vulkan ImGui renderer can't take more
/// than 65,535 vertices in one draw list, and ER's longest events have thousands of instructions).
/// </summary>
public class EmevdBlockView
{
    private readonly EmevdEditorView View;

    // The script of the selected event, rebuilt when the event changes
    private EMEVD.Event CachedEvent;
    private EmevdScript Script;

    // Initialize Event targets, resolved when first drawn
    private readonly Dictionary<int, (EmevdFileWrapper file, EMEVD.Event ev)> CallTargets = new();

    private const float LaneWidth = 7f;
    private const float RowGap = 2f;

    public EmevdBlockView(EmevdEditorView view)
    {
        View = view;
    }

    private EmevdCatalog Catalog => View.Project.Handler.EmevdData?.Catalog;

    /// <summary>The script for the selected event, for the Properties window</summary>
    public EmevdScript CurrentScript => Script;

    public void Draw()
    {
        var selection = View.Selection;
        var ev = selection.SelectedEvent;

        if (ev != CachedEvent)
        {
            CachedEvent = ev;
            Script = ev != null ? EmevdScript.Build(Catalog, ev) : null;
            CallTargets.Clear();
        }

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

        var selection = View.Selection;
        var count = Script.Lines.Count;

        if (ImGui.IsKeyPressed(ImGuiKey.DownArrow) && count > 0)
        {
            selection.SelectInstruction(Math.Min(selection.InstructionIndex + 1, count - 1));
            selection.ScrollToInstruction = true;
        }

        if (ImGui.IsKeyPressed(ImGuiKey.UpArrow) && selection.InstructionIndex >= 0)
        {
            selection.SelectInstruction(selection.InstructionIndex - 1);
            selection.ScrollToInstruction = true;
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
        var rowH = blockH + RowGap;
        var hatH = lineH * 2 + padY * 3 + 8f;

        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;

        var indexW = ImGui.CalcTextSize(Script.Lines.Count.ToString()).X + 8f;
        var lanes = Math.Min(Script.LaneCount, 12);
        var marginW = indexW + lanes * LaneWidth + 6f;
        var blockX0 = origin.X + marginW;
        var blockX1 = origin.X + Math.Max(width, marginW + 200f) - 4f;

        var linesTop = origin.Y + hatH + 6f;
        var totalH = hatH + 6f + Script.Lines.Count * rowH + 8f;

        var scrollY = ImGui.GetScrollY();
        var viewH = ImGui.GetWindowHeight();

        if (selection.ScrollToInstruction)
        {
            selection.ScrollToInstruction = false;

            if (selection.InstructionIndex >= 0)
            {
                var y = hatH + 6f + selection.InstructionIndex * rowH;
                if (y < scrollY || y + rowH > scrollY + viewH)
                    ImGui.SetScrollY(Math.Max(0, y - viewH / 3));
            }
        }

        // The hat block
        DrawHat(drawList, new Vector2(origin.X + indexW, origin.Y + 2f), new Vector2(blockX1, origin.Y + hatH), lineH, padY);

        // Rows on screen
        var first = Math.Max(0, (int)((scrollY - hatH - 6f) / rowH) - 1);
        var last = Math.Min(Script.Lines.Count - 1, (int)((scrollY + viewH - hatH) / rowH) + 1);

        for (var i = first; i <= last; i++)
        {
            var line = Script.Lines[i];
            var top = linesTop + i * rowH;

            DrawIndex(drawList, line, new Vector2(origin.X, top + padY));
            DrawBlock(drawList, line, new Vector2(blockX0, top), new Vector2(blockX1, top + blockH), padY);
        }

        // Brackets for the spans that cross the rows on screen
        foreach (var span in Script.Spans)
        {
            if (span.To < first || span.From > last)
                continue;

            // Past the lanes there's room for, brackets share the outermost one
            var lane = Math.Min(span.Lane, lanes - 1);
            var x = origin.X + indexW + (lanes - 1 - lane) * LaneWidth + 2f;
            var y0 = linesTop + span.From * rowH + blockH * 0.5f;
            var y1 = linesTop + span.To * rowH + (span.IsGoto ? blockH * 0.5f : blockH);

            var color = ImGui.GetColorU32(span.Broken ? EmevdFormat.BrokenSpanColor
                : span.IsGoto ? EmevdFormat.GotoSpanColor
                : EmevdFormat.SpanColor);

            drawList.AddLine(new Vector2(blockX0 - 1f, y0), new Vector2(x, y0), color, 1.5f);
            drawList.AddLine(new Vector2(x, y0), new Vector2(x, y1), color, 1.5f);
            drawList.AddLine(new Vector2(x, y1), new Vector2(blockX0 - 1f, y1), color, 1.5f);
        }

        // Reserve the whole column, so the scrollbar covers every row
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(Math.Max(width, marginW + 200f), totalH));
    }

    private void DrawHat(ImDrawListPtr drawList, Vector2 min, Vector2 max, float lineH, float padY)
    {
        var selection = View.Selection;
        var ev = Script.Event;
        var selected = selection.InstructionIndex < 0;

        ImGui.SetCursorScreenPos(min);
        ImGui.InvisibleButton("##emevdHat", max - min);

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

    private void DrawBlock(ImDrawListPtr drawList, EmevdLine line, Vector2 min, Vector2 max, float padY)
    {
        var selection = View.Selection;
        var selected = selection.InstructionIndex == line.Index;
        var lineH = ImGui.GetTextLineHeight();

        ImGui.SetCursorScreenPos(min);
        ImGui.InvisibleButton($"##emevdLine{line.Index}", max - min);

        var hovered = ImGui.IsItemHovered();

        if (ImGui.IsItemClicked())
            selection.SelectInstruction(line.Index);

        if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            FollowCall(line.Index);

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

        if (selected)
            drawList.AddRect(min, max, ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), 4f, ImDrawFlags.None, 2f);

        var white = ImGui.GetColorU32(new Vector4(1, 1, 1, 1));
        var dim = ImGui.GetColorU32(new Vector4(1, 1, 1, 0.65f));

        drawList.PushClipRect(min, max, true);

        var x = min.X + (line.Kind == EmevdBlockKind.Condition ? h * 0.5f + 2f : 8f);
        var y = min.Y + padY;

        var name = line.Name;
        drawList.AddText(new Vector2(x, y), line.Decoded.Def?.HasName == false ? dim : white, name);
        x += ImGui.CalcTextSize(name).X + 10f;

        if (line.Decoded.Fits)
        {
            foreach (var arg in line.Decoded.Args)
            {
                if (x > max.X)
                    break;

                if (CFG.Current.EmevdEditor_Show_Arg_Names)
                {
                    var argName = EmevdFormat.ArgName(arg);
                    drawList.AddText(new Vector2(x, y), dim, argName);
                    x += ImGui.CalcTextSize(argName).X + 4f;
                }

                if (line.Params.TryGetValue(arg.Offset, out var p))
                    x = Chip(drawList, new Vector2(x, y), EmevdScript.SlotName(p.SourceStartByte, p.ByteCount), EmevdFormat.ParamChipColor, true, lineH) + 8f;
                else
                    x = Chip(drawList, new Vector2(x, y), EmevdFormat.Value(Catalog, arg), EmevdFormat.Darken(color, 0.6f), false, lineH) + 8f;
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

        if (hovered)
            BlockTooltip(line);
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
