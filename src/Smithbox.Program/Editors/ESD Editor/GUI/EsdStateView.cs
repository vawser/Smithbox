using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// The state view: the selected machine's states as blocks. Each state is a hat block, with its entry / while / exit
/// commands, then its conditions as C-blocks (the transition's test as reporter chips, its target, its pass commands
/// and nested subconditions). Read-only in P1; see Claude/Documentation/ESD Block Editor Analysis.md.
/// </summary>
public class EsdStateView
{
    private readonly EsdEditorView View;

    // Rows of the machine on show, rebuilt when the machine changes
    private EsdFileWrapper CachedFile;
    private long? CachedMachine;
    private List<EsdRow> Rows = new();
    private HashSet<long> Unreached = new();
    private int CalledBy;

    private const float RowGap = 2f;

    public EsdStateView(EsdEditorView view)
    {
        View = view;
    }

    private EsdKindCatalog Catalog => View.Selection.SelectedFile is { } f ? View.Project.Handler.EsdData?.Catalog?.For(f.Kind) : null;

    private void Refresh()
    {
        var selection = View.Selection;
        if (selection.SelectedFile == CachedFile && selection.SelectedMachine == CachedMachine)
            return;

        CachedFile = selection.SelectedFile;
        CachedMachine = selection.SelectedMachine;

        var machine = selection.Machine;
        Rows = EsdLayout.Build(machine);
        Unreached = machine != null ? EsdLayout.Unreached(machine) : new();
        CalledBy = CachedFile?.Esd != null && CachedMachine is long id
            ? EsdLayout.Callers(CachedFile.Esd, id).Sum(c => c.Calls)
            : 0;
    }

    public void Draw()
    {
        Refresh();
        DrawHeader();

        ImGui.BeginChild("esdStates", Vector2.Zero, ImGuiChildFlags.Borders, ImGuiWindowFlags.HorizontalScrollbar);

        if (View.Selection.Machine == null)
        {
            ImGui.TextDisabled(LOC.Get("ESD_States_No_Machine"));
        }
        else
        {
            HandleKeys();
            DrawRows();
        }

        ImGui.EndChild();
    }

    private void DrawHeader()
    {
        GUI.TitleHeader(LOC.Get("ESD_States_Header"), LOC.Get("ESD_States_Header_TT"));

        ImGui.BeginChild("esdStatesHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        var selection = View.Selection;

        ImGui.BeginDisabled(!selection.CanGoBack);
        if (ImGui.Button($"{Icons.ArrowLeft}##esdBack"))
            selection.GoBack();
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("ESD_States_Back_TT"));

        if (selection.Machine is { } machine && selection.SelectedMachine is long id)
        {
            ImGui.SameLine();
            ImGui.Text(LOC.Get("ESD_States_Summary", EsdFormat.MachineLabel(selection.SelectedFile, id), machine.Count));

            if (CalledBy > 0)
            {
                ImGui.SameLine();
                ImGui.TextDisabled(LOC.Get("ESD_States_Called_By", CalledBy));
                GUI.Tooltip(LOC.Get("ESD_States_Called_By_TT"));
            }

            if (Unreached.Count > 0)
            {
                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ESD_States_Unreached", Unreached.Count));
                GUI.Tooltip(LOC.Get("ESD_States_Unreached_TT"));
            }

            if (Catalog == null)
            {
                ImGui.SameLine();
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("ESD_States_No_Catalogue"));
                GUI.Tooltip(LOC.Get("ESD_States_No_Catalogue_TT"));
            }
        }

        ImGui.EndChild();
    }

    private bool Selectable(EsdRow row) => row.Kind is EsdRowKind.State or EsdRowKind.Command or EsdRowKind.Condition;

    private int SelectedRow()
    {
        for (var r = 0; r < Rows.Count; r++)
            if (View.Selection.IsSelected(Rows[r]))
                return r;
        return -1;
    }

    private void HandleKeys()
    {
        if (!ImGui.IsWindowFocused() || ImGui.IsAnyItemActive())
            return;

        var selection = View.Selection;
        var at = SelectedRow();

        void Step(int dir)
        {
            var r = at;
            do { r += dir; } while (r >= 0 && r < Rows.Count && !Selectable(Rows[r]));
            if (r >= 0 && r < Rows.Count)
            {
                selection.SelectRow(Rows[r]);
                selection.ScrollToItem = true;
            }
        }

        if (ImGui.IsKeyPressed(ImGuiKey.DownArrow))
            Step(1);
        if (ImGui.IsKeyPressed(ImGuiKey.UpArrow) && at > 0)
            Step(-1);
        if (ImGui.IsKeyPressed(ImGuiKey.Enter) && at >= 0)
            Follow(Rows[at]);
        if (ImGui.IsKeyPressed(ImGuiKey.Backspace))
            selection.GoBack();
    }

    /// <summary>Double-click / Enter: a transition goes to its target state, a call to its machine</summary>
    private void Follow(EsdRow row)
    {
        var selection = View.Selection;

        if (row.Kind == EsdRowKind.Condition && row.Condition.TargetState is long target)
            selection.GoToState(target);
        else if (row.Kind == EsdRowKind.Command && EsdLayout.KindOf(row.Command) == EsdCommandKind.CallMachine)
            selection.GoToMachine(row.Command.CommandID);
    }

    private float RowHeight(EsdRow row, float lineH, float blockH)
    {
        return row.Kind switch
        {
            EsdRowKind.State => blockH + 6f,
            EsdRowKind.Section => lineH + 2f,
            EsdRowKind.ConditionEnd => 7f * DPI.UIScale(),
            _ => blockH
        };
    }

    private void DrawRows()
    {
        var selection = View.Selection;
        var drawList = ImGui.GetWindowDrawList();

        var lineH = ImGui.GetTextLineHeight();
        var padY = 3f;
        var blockH = lineH + padY * 2 + 4f;
        var indent = 18f * DPI.UIScale();
        var armW = 10f * DPI.UIScale();
        var stateGap = 10f * DPI.UIScale();

        var origin = ImGui.GetCursorScreenPos();
        var width = ImGui.GetContentRegionAvail().X;
        var x0 = origin.X + 4f;
        var x1 = origin.X + Math.Max(width, 420f * DPI.UIScale()) - 4f;

        // Row tops as a prefix sum, with a gap before every state but the first
        var tops = new float[Rows.Count + 1];
        for (var r = 0; r < Rows.Count; r++)
        {
            var gap = Rows[r].Kind == EsdRowKind.State && r > 0 ? stateGap : 0f;
            tops[r] += gap;
            tops[r + 1] = tops[r] + RowHeight(Rows[r], lineH, blockH) + RowGap;
        }

        var totalH = tops[^1] + 8f;
        var scrollY = ImGui.GetScrollY();
        var viewH = ImGui.GetWindowHeight();

        if (selection.ScrollToItem)
        {
            selection.ScrollToItem = false;
            var at = SelectedRow();
            if (at >= 0)
            {
                var y = tops[at];
                if (y < scrollY || y + blockH > scrollY + viewH)
                    ImGui.SetScrollY(Math.Max(0, y - viewH / 3));
            }
        }

        // Rows on screen
        var first = Math.Max(0, Array.BinarySearch(tops, scrollY) is var b && b >= 0 ? b : ~b - 1);
        var last = first;
        while (last < Rows.Count - 1 && tops[last + 1] < scrollY + viewH)
            last++;

        float RowX(int r) => x0 + Rows[r].Depth * indent;

        // Condition C-block arms crossing the view: from under the header to its foot
        for (var r = 0; r < Rows.Count; r++)
        {
            var row = Rows[r];
            if (row.Kind != EsdRowKind.Condition || row.EndRow < 0 || row.EndRow < first || r > last)
                continue;

            var color = ImGui.GetColorU32(ConditionColor(row));
            var x = RowX(r);
            drawList.AddRectFilled(new Vector2(x, origin.Y + tops[r] + blockH), new Vector2(x + armW, origin.Y + tops[row.EndRow]), color);
        }

        for (var r = first; r <= last && r < Rows.Count; r++)
        {
            var row = Rows[r];
            var min = new Vector2(RowX(r), origin.Y + tops[r]);
            var h = RowHeight(row, lineH, blockH);
            var max = new Vector2(x1, min.Y + h);

            switch (row.Kind)
            {
                case EsdRowKind.State:
                    DrawState(drawList, row, min, max, lineH, padY);
                    break;
                case EsdRowKind.Section:
                    DrawSection(drawList, row, min, lineH);
                    break;
                case EsdRowKind.Command:
                    DrawCommand(drawList, row, min, max, lineH, padY);
                    break;
                case EsdRowKind.Condition:
                    DrawCondition(drawList, row, min, max, lineH, padY);
                    break;
                case EsdRowKind.ConditionEnd:
                    var footMax = new Vector2(Math.Min(min.X + 140f * DPI.UIScale(), x1), max.Y);
                    drawList.AddRectFilled(min, footMax, ImGui.GetColorU32(ConditionColor(row)), 4f, ImDrawFlags.RoundCornersBottom);
                    break;
            }
        }

        // Reserve the whole column, so the scrollbar covers every row
        ImGui.SetCursorScreenPos(origin);
        ImGui.Dummy(new Vector2(x1 - origin.X + 4f, totalH));
    }

    private static Vector4 ConditionColor(EsdRow row) =>
        EsdExpr.IsAlways(row.Condition.Evaluator) ? EsdFormat.AlwaysColor : EsdFormat.ConditionColor;

    /// <summary>The block's button: selects on click, follows on double-click, and shows a tooltip</summary>
    private bool BlockButton(EsdRow row, Vector2 min, Vector2 max, string id)
    {
        ImGui.SetCursorScreenPos(min);
        ImGui.InvisibleButton(id, max - min);

        var hovered = ImGui.IsItemHovered();

        if (ImGui.IsItemClicked())
            View.Selection.SelectRow(row);

        if (hovered && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            Follow(row);

        return hovered;
    }

    private void Outline(ImDrawListPtr drawList, EsdRow row, Vector2 min, Vector2 max, float rounding, ImDrawFlags flags)
    {
        if (View.Selection.IsSelected(row))
            drawList.AddRect(min, max, ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), rounding, flags, 2f);
    }

    private void DrawState(ImDrawListPtr drawList, EsdRow row, Vector2 min, Vector2 max, float lineH, float padY)
    {
        var hovered = BlockButton(row, min, max, $"##esdState{row.StateId}");

        var isStart = Rows.Count > 0 && Rows[0].StateId == row.StateId;
        var color = isStart ? EsdFormat.StartStateColor : EsdFormat.StateColor;
        if (hovered)
            color = EsdFormat.Darken(color, 1.15f);

        drawList.AddRectFilled(min, max, ImGui.GetColorU32(color), 10f, ImDrawFlags.RoundCornersTop);
        Outline(drawList, row, min, max, 10f, ImDrawFlags.RoundCornersTop);

        drawList.PushClipRect(min, max, true);

        var pos = new Vector2(min.X + 10f, min.Y + (max.Y - min.Y - lineH) * 0.5f);
        var title = LOC.Get("ESD_States_State_Title", row.StateId);
        drawList.AddText(pos, ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), title);
        pos.X += ImGui.CalcTextSize(title).X + 10f;

        var file = View.Selection.SelectedFile;
        if (file != null && View.Selection.SelectedMachine is long m && file.StateNames.TryGetValue((m, row.StateId), out var name))
        {
            drawList.AddText(pos, ImGui.GetColorU32(UI.Current.ImGui_AliasName_Text), name);
            pos.X += ImGui.CalcTextSize(name).X + 10f;
        }

        var chipH = lineH + 2f;
        var chipY = min.Y + (max.Y - min.Y - chipH) * 0.5f;

        if (isStart)
            pos.X = TextChip(drawList, new Vector2(pos.X, chipY), LOC.Get("ESD_States_Start_Chip"), EsdFormat.TargetChipColor, true, chipH) + 6f;

        if (Unreached.Contains(row.StateId))
            TextChip(drawList, new Vector2(pos.X, chipY), LOC.Get("ESD_States_Unreached_Chip"), new Vector4(0.95f, 0.65f, 0.35f, 1f), true, chipH);

        drawList.PopClipRect();

        if (hovered)
            GUI.Tooltip(LOC.Get(isStart ? "ESD_States_State_Start_TT" : "ESD_States_State_TT",
                row.State.EntryCommands.Count, row.State.WhileCommands.Count, row.State.ExitCommands.Count, row.State.Conditions.Count));
    }

    private void DrawSection(ImDrawListPtr drawList, EsdRow row, Vector2 min, float lineH)
    {
        var text = EsdFormat.ListName(row.List);
        var max = new Vector2(min.X + ImGui.CalcTextSize(text).X + 16f, min.Y + lineH + 2f);
        drawList.AddRectFilled(min, max, ImGui.GetColorU32(EsdFormat.SectionColor), 3f);
        drawList.AddText(new Vector2(min.X + 8f, min.Y + 1f), ImGui.GetColorU32(new Vector4(1, 1, 1, 0.9f)), text);
    }

    private void DrawCommand(ImDrawListPtr drawList, EsdRow row, Vector2 min, Vector2 max, float lineH, float padY)
    {
        var cat = Catalog;
        var c = row.Command;
        var kind = EsdLayout.KindOf(c);
        var def = kind == EsdCommandKind.Game ? cat?.Command(c.CommandBank, c.CommandID) : null;

        var hovered = BlockButton(row, min, max, $"##esdCmd{row.StateId}_{row.GetHashCode()}");

        var color = kind switch
        {
            EsdCommandKind.CallMachine => EsdFormat.CallColor,
            EsdCommandKind.Return => EsdFormat.ReturnColor,
            EsdCommandKind.Game when def != null => EsdFormat.CommandColor,
            _ => EsdFormat.UnknownCommandColor
        };
        if (hovered)
            color = EsdFormat.Darken(color, 1.15f);

        drawList.AddRectFilled(min, max, ImGui.GetColorU32(color), 4f);
        Outline(drawList, row, min, max, 4f, ImDrawFlags.None);

        drawList.PushClipRect(min, max, true);

        var chipH = lineH + 2f;
        var y = min.Y + (max.Y - min.Y - lineH) * 0.5f;
        var chipY = min.Y + (max.Y - min.Y - chipH) * 0.5f;
        var x = min.X + 8f;

        var name = EsdFormat.CommandName(cat, c);
        drawList.AddText(new Vector2(x, y), ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), name);
        x += ImGui.CalcTextSize(name).X + 8f;

        for (var i = 0; i < c.Arguments.Count; i++)
        {
            var argDef = def != null && i < def.Args.Count ? def.Args[i] : null;
            var node = EsdExpr.Decode(c.Arguments[i]);
            x = node != null
                ? Chip(drawList, new Vector2(x, chipY), node, cat, argDef, 0, chipH, true) + 5f
                : TextChip(drawList, new Vector2(x, chipY), $"<{EsdFormat.Hex(c.Arguments[i])}>", EsdFormat.StatusChipColor, false, chipH) + 5f;
        }

        drawList.PopClipRect();

        if (hovered)
            CommandTooltip(cat, c, def, kind);
    }

    private void DrawCondition(ImDrawListPtr drawList, EsdRow row, Vector2 min, Vector2 max, float lineH, float padY)
    {
        var cat = Catalog;
        var c = row.Condition;

        var hovered = BlockButton(row, min, max, $"##esdCond{row.StateId}_{row.GetHashCode()}");

        var color = ConditionColor(row);
        if (hovered)
            color = EsdFormat.Darken(color, 1.15f);

        var flags = row.EndRow >= 0 ? ImDrawFlags.RoundCornersTop | ImDrawFlags.RoundCornersBottomRight : ImDrawFlags.RoundCornersAll;
        drawList.AddRectFilled(min, max, ImGui.GetColorU32(color), 4f, flags);
        Outline(drawList, row, min, max, 4f, flags);

        drawList.PushClipRect(min, max, true);

        var chipH = lineH + 2f;
        var y = min.Y + (max.Y - min.Y - lineH) * 0.5f;
        var chipY = min.Y + (max.Y - min.Y - chipH) * 0.5f;
        var x = min.X + 8f;
        var white = ImGui.GetColorU32(new Vector4(1, 1, 1, 1));

        var always = EsdExpr.IsAlways(c.Evaluator);
        var label = LOC.Get(always ? "ESD_States_Always" : "ESD_States_If");
        drawList.AddText(new Vector2(x, y), white, label);
        x += ImGui.CalcTextSize(label).X + 8f;

        if (!always)
        {
            var node = EsdExpr.Decode(c.Evaluator);
            x = node != null
                ? Chip(drawList, new Vector2(x, chipY), node, cat, null, 0, chipH, true) + 8f
                : TextChip(drawList, new Vector2(x, chipY), $"<{EsdFormat.Hex(c.Evaluator)}>", EsdFormat.StatusChipColor, false, chipH) + 8f;
        }

        if (c.TargetState is long target)
        {
            var arrow = LOC.Get("ESD_States_Goes_To");
            drawList.AddText(new Vector2(x, y), white, arrow);
            x += ImGui.CalcTextSize(arrow).X + 6f;
            x = TextChip(drawList, new Vector2(x, chipY), LOC.Get("ESD_States_State_Title", target), EsdFormat.TargetChipColor, true, chipH) + 8f;
        }
        else if (c.Subconditions.Count > 0)
        {
            var then = LOC.Get("ESD_States_Then");
            drawList.AddText(new Vector2(x, y), ImGui.GetColorU32(new Vector4(1, 1, 1, 0.75f)), then);
            x += ImGui.CalcTextSize(then).X + 8f;
        }

        if (row.SharedCount > 1)
            TextChip(drawList, new Vector2(x, chipY), LOC.Get("ESD_States_Shared_Chip", row.SharedCount), EsdFormat.SharedChipColor, true, chipH);

        drawList.PopClipRect();

        if (hovered)
        {
            var text = always ? LOC.Get("ESD_States_Always_TT") : EsdFormat.ExprText(cat, c.Evaluator);
            var lines = new List<string> { text };
            if (c.TargetState is long t)
                lines.Add(LOC.Get("ESD_States_Goes_To_TT", t));
            else if (c.Subconditions.Count > 0)
                lines.Add(LOC.Get("ESD_States_Then_TT"));
            if (row.SharedCount > 1)
                lines.Add(LOC.Get("ESD_States_Shared_TT", row.SharedCount));
            GUI.Tooltip(string.Join("\n", lines));
        }
    }

    private void CommandTooltip(EsdKindCatalog cat, ESD.CommandCall c, EsdCallDef def, EsdCommandKind kind)
    {
        var lines = new List<string> { $"{c.CommandBank}:{c.CommandID}  {EsdFormat.CommandName(cat, c)}" };

        switch (kind)
        {
            case EsdCommandKind.CallMachine:
                lines.Add(LOC.Get("ESD_States_Call_TT"));
                break;
            case EsdCommandKind.Return:
                lines.Add(LOC.Get("ESD_States_Return_TT"));
                break;
            case EsdCommandKind.Game when def == null:
                lines.Add(LOC.Get(cat == null ? "ESD_States_No_Catalogue_TT" : "ESD_States_Unknown_Command_TT"));
                break;
            case EsdCommandKind.Other:
                lines.Add(LOC.Get("ESD_States_Other_Bank_TT"));
                break;
        }

        if (!string.IsNullOrEmpty(def?.Comment))
            lines.Add(def.Comment);

        for (var i = 0; i < c.Arguments.Count; i++)
        {
            var argDef = def != null && i < def.Args.Count ? def.Args[i] : null;
            var node = EsdExpr.Decode(c.Arguments[i]);
            var value = node != null ? EsdFormat.ArgText(cat, def, i, node) : $"<{EsdFormat.Hex(c.Arguments[i])}>";
            lines.Add(argDef != null ? $"  {argDef.Name} ({argDef.Type}): {value}" : $"  arg{i}: {value}");
        }

        if (def != null && (c.Arguments.Count < def.RequiredArgs || c.Arguments.Count > def.Args.Count))
            lines.Add(LOC.Get("ESD_States_Arg_Count_TT", c.Arguments.Count, def.RequiredArgs, def.Args.Count));

        GUI.Tooltip(string.Join("\n", lines));
    }

    /// <summary>A rounded chip holding text; returns its right edge</summary>
    private static float TextChip(ImDrawListPtr drawList, Vector2 pos, string text, Vector4 color, bool darkText, float h)
    {
        var w = ImGui.CalcTextSize(text).X + 10f;
        drawList.AddRectFilled(pos, new Vector2(pos.X + w, pos.Y + h), ImGui.GetColorU32(color), h * 0.5f);
        var tc = darkText ? new Vector4(0.08f, 0.08f, 0.08f, 1f) : new Vector4(1, 1, 1, 1);
        drawList.AddText(new Vector2(pos.X + 5f, pos.Y + (h - ImGui.GetTextLineHeight()) * 0.5f), ImGui.GetColorU32(tc), text);
        return pos.X + w;
    }

    /// <summary>
    /// An expression as nested reporter chips: a call holds its args, an operator its two sides. With draw false it
    /// only measures. Returns the right edge.
    /// </summary>
    private static float Chip(ImDrawListPtr drawList, Vector2 pos, EsdExprNode n, EsdKindCatalog cat, EsdArgDef argDef, int depth, float h, bool draw)
    {
        // Markers that don't change the value draw as what they mark
        while (n.Kind == EsdExprKind.Unary && n.Op is 0xA6 or 0xB7)
            n = n.Children[0];

        var inner = Math.Max(h - 4f, ImGui.GetTextLineHeight());
        var textY = pos.Y + (h - ImGui.GetTextLineHeight()) * 0.5f;
        var innerY = pos.Y + (h - inner) * 0.5f;

        switch (n.Kind)
        {
            case EsdExprKind.Call:
                {
                    var id = n.FunctionId;
                    var def = id != null ? cat?.Function(id.Value) : null;
                    var name = id != null ? EsdFormat.FunctionName(cat, id.Value) : "call";
                    var color = def != null ? EsdFormat.FunctionChipColor : EsdFormat.UnknownFunctionChipColor;
                    return Composite(drawList, pos, h, color, draw, (x, d) =>
                    {
                        x = Label(drawList, x, textY, name, d) + 4f;
                        for (var k = 1; k < n.Children.Count; k++)
                        {
                            var ad = def != null && k - 1 < def.Args.Count ? def.Args[k - 1] : null;
                            x = Chip(drawList, new Vector2(x, innerY), n.Children[k], cat, ad, depth + 1, inner, d) + 3f;
                        }
                        return x;
                    });
                }
            case EsdExprKind.Binary:
                return Composite(drawList, pos, h, EsdFormat.OperatorChipColor, draw, (x, d) =>
                {
                    x = Chip(drawList, new Vector2(x, innerY), n.Children[0], cat, null, depth + 1, inner, d) + 4f;
                    x = Label(drawList, x, textY, EsdExpr.BinaryName(n.Op), d) + 4f;
                    return Chip(drawList, new Vector2(x, innerY), n.Children[1], cat, null, depth + 1, inner, d);
                });
            case EsdExprKind.Unary when n.Op is >= 0xA7 and <= 0xAE:
                return Composite(drawList, pos, h, EsdFormat.RegisterChipColor, draw, (x, d) =>
                {
                    x = Label(drawList, x, textY, $"r{n.Op - 0xA7} =", d) + 4f;
                    return Chip(drawList, new Vector2(x, innerY), n.Children[0], cat, null, depth + 1, inner, d);
                });
            case EsdExprKind.Unary when n.Op == 0xB8 && EsdExpr.Constant(n.Children[0]) is long argIndex:
                return Leaf(drawList, pos, h, $"arg{argIndex}", EsdFormat.ArgChipColor, true, draw);
            case EsdExprKind.Unary:
                return Composite(drawList, pos, h, EsdFormat.StatusChipColor, draw, (x, d) =>
                {
                    x = Label(drawList, x, textY, n.Op == 0xB8 ? "arg" : $"op{n.Op:X2}", d) + 4f;
                    return Chip(drawList, new Vector2(x, innerY), n.Children[0], cat, null, depth + 1, inner, d);
                });
            case EsdExprKind.Leaf:
                return Leaf(drawList, pos, h, EsdExpr.Text(n), EsdFormat.StatusChipColor, false, draw);
            case EsdExprKind.Int when argDef?.Enum != null && cat?.FindEnum(argDef.Enum)?.ValueName(n.IntValue) is { } enumName:
                return Leaf(drawList, pos, h, enumName, EsdFormat.EnumChipColor, true, draw);
            default:
                return Leaf(drawList, pos, h, EsdExpr.Text(n), EsdFormat.LiteralChipColor, true, draw);
        }
    }

    private static float Composite(ImDrawListPtr drawList, Vector2 pos, float h, Vector4 color, bool draw, Func<float, bool, float> content)
    {
        // Measure first, so the background goes under the children
        var right = content(pos.X + 6f, false) + 6f;
        if (!draw)
            return right;

        drawList.AddRectFilled(pos, new Vector2(right, pos.Y + h), ImGui.GetColorU32(color), h * 0.5f);
        content(pos.X + 6f, true);
        return right;
    }

    private static float Leaf(ImDrawListPtr drawList, Vector2 pos, float h, string text, Vector4 color, bool darkText, bool draw)
    {
        if (!draw)
            return pos.X + ImGui.CalcTextSize(text).X + 10f;
        return TextChip(drawList, pos, text, color, darkText, h);
    }

    private static float Label(ImDrawListPtr drawList, float x, float y, string text, bool draw)
    {
        if (draw)
            drawList.AddText(new Vector2(x, y), ImGui.GetColorU32(new Vector4(1, 1, 1, 1)), text);
        return x + ImGui.CalcTextSize(text).X;
    }
}
