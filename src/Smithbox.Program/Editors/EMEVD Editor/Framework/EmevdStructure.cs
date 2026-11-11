namespace StudioCore.Editors.EmevdEditor;

public enum EmevdNodeKind
{
    /// <summary>One instruction, drawn as a plain block (including skips and gotos that don't nest)</summary>
    Line,
    /// <summary>A skip whose span covers whole nodes: a C-block holding the lines it skips</summary>
    Skip,
    /// <summary>A goto whose label follows at the same level: a C-block holding the lines it jumps over</summary>
    Goto,
    /// <summary>
    /// A dispatch table: a run of gotos (the first is the node's line, the rest are <see cref="EmevdNode.Body"/>) to
    /// labels after the run that don't nest, then a default part and one case per label
    /// </summary>
    Switch,
}

/// <summary>A node of an event's structured tree. Every instruction is in the tree exactly once, in order.</summary>
public class EmevdNode
{
    public EmevdNodeKind Kind;

    /// <summary>The instruction: the line itself, or the C-block's skip/goto</summary>
    public int Line;

    /// <summary>Condition lines just before a flow that evaluates them (Wait/Skip/End/Goto on a condition group)</summary>
    public List<int> Conditions = new();

    public List<EmevdNode> Body = new();

    /// <summary>The unconditional skip/goto that ends the body and jumps over the else part, or -1</summary>
    public int ElseLine = -1;

    /// <summary>The else part. For a goto, it starts with the label the header jumps to.</summary>
    public List<EmevdNode> Else = new();

    /// <summary>
    /// A line that jumps to the end of an enclosing block ("break"): 0 for the innermost, 1 for the one around it,
    /// and so on; -1 when it isn't one.
    /// </summary>
    public int ExitLevel = -1;

    /// <summary>A switch's default part: the lines after its gotos, before the first case label</summary>
    public List<EmevdNode> Default = new();

    /// <summary>A switch's cases, in file order; each starts with its label line</summary>
    public List<List<EmevdNode>> Cases = new();

    public bool IsBlock => Kind != EmevdNodeKind.Line;

    /// <summary>Every list of nodes inside this one, in file order (unused ones are empty)</summary>
    public List<List<EmevdNode>> Children
    {
        get
        {
            var list = new List<List<EmevdNode>>(3 + Cases.Count) { Body, Else, Default };
            list.AddRange(Cases);
            return list;
        }
    }
}

/// <summary>What a display row is: a line, the foot of a C-block, or a switch's "Default" divider</summary>
public enum EmevdRowKind { Line, BlockEnd, Section }

/// <summary>One row of the structured view: a line at a depth, or the foot of a C-block</summary>
public struct EmevdRow
{
    public EmevdRowKind Kind;
    public int Line;
    public int Depth;
    /// <summary>A C-block's header row: the index of its foot row</summary>
    public int EndRow;
    /// <summary>The line is the else of a C-block</summary>
    public bool IsElse;
    /// <summary>A condition evaluated by the flow at this row index, or -1</summary>
    public int ConditionOf;
    /// <summary>The line is the label starting a switch case</summary>
    public bool IsCase;
    /// <summary>The header of a switch</summary>
    public bool IsSwitch;
    /// <summary>A collapsed C-block's header: its inner lines have no rows</summary>
    public bool Collapsed;
    /// <summary>A collapsed header: how many lines it hides</summary>
    public int Hidden;
}

/// <summary>
/// Decompiles an event's flat instruction list into nested C-blocks (Claude/Documentation/EMEVD Block Editor
/// Analysis.md §4), and compiles the tree back.
///
/// - A skip of n lines (1000[03] included) becomes a C-block when its n lines are whole nodes inside the same parent.
///   If the body ends with 1000[03] Skip Unconditionally over the m lines right after it, those m lines are the
///   else part.
/// - A goto becomes a C-block running to the first matching label after it at the same level. If the body ends with
///   1000[103] Goto Unconditionally to a later label at that level, the lines between the two labels are the else
///   part (starting with the first label).
/// - A run of two or more gotos whose labels all follow the run and cross (a later goto's label after an earlier
///   one's, so goto C-blocks can't nest them) is a switch: the lines up to the first label are the default part, and
///   each label starts a case running to the next label. The switch ends where every part but the last ends with
///   1000[103] to one label after the last case ("break"), else at the end of its parent.
/// - Condition lines right before 1000[00]/[01]/[02]/[101] (wait, skip, end, goto on a condition group, uncompiled:
///   they evaluate the conditions registered before them) are drawn as that flow's conditions.
/// - Anything else stays a plain line: skips running past their parent, skips set by a parameter, unmatched gotos.
///
/// Instructions are found by bank and ID (same in all nine games' layouts), never by name. Every line is kept in
/// order, so compiling only has to recompute skip counts; the round-trip gate (EmevdEditorCheck structure) checks the
/// recomputed counts equal the file's on every vanilla event.
/// </summary>
public static class EmevdStructure
{
    private static bool IsSkipAlways(EmevdLine l) => l.Instr.Bank == 1000 && l.Instr.ID == 3;
    private static bool IsGotoAlways(EmevdLine l) => l.Instr.Bank == 1000 && l.Instr.ID == 103;

    private static bool Evaluates(EmevdLine l) => l.Instr.Bank == 1000 && l.Instr.ID is 0 or 1 or 2 or 101;

    /// <summary>A skip the structurer can use: its count is stored in the file, not supplied by a parameter</summary>
    private static bool UsableSkip(EmevdLine l) =>
        l.Kind == EmevdBlockKind.Skip && l.SkipCount > 0 && !l.Params.ContainsKey(l.FlowArgOffset);

    private static bool UsableGoto(EmevdLine l) =>
        l.Kind == EmevdBlockKind.Goto && l.GotoLabel >= 0 && !l.Params.ContainsKey(l.FlowArgOffset);

    public static List<EmevdNode> Decompile(EmevdScript script)
    {
        return Parse(script.Lines, 0, script.Lines.Count, new List<int> { script.Lines.Count });
    }

    /// <summary>The line a skip or goto lands on, or -1 (a goto lands on the first matching label after it)</summary>
    private static int TargetOf(List<EmevdLine> lines, EmevdLine line)
    {
        if (UsableSkip(line))
            return line.Index + 1 + line.SkipCount;

        if (UsableGoto(line))
            return FindLabel(lines, line.Index + 1, lines.Count, line.GotoLabel);

        return -1;
    }

    /// <param name="ends">The end of every enclosing range, outermost first; the last is <paramref name="end"/></param>
    private static List<EmevdNode> Parse(List<EmevdLine> lines, int start, int end, List<int> ends)
    {
        var nodes = new List<EmevdNode>();
        var i = start;

        while (i < end)
        {
            var line = lines[i];

            if (UsableSkip(line) && i + 1 + line.SkipCount <= end)
            {
                var bodyEnd = i + 1 + line.SkipCount;
                var node = new EmevdNode { Kind = EmevdNodeKind.Skip, Line = i, Body = Parse(lines, i + 1, bodyEnd, With(ends, bodyEnd)) };
                i = bodyEnd;

                // else: the body's last line skips the m lines right after the body
                if (node.Body.Count > 0 && node.Body[^1] is { Kind: EmevdNodeKind.Line } last && last.Conditions.Count == 0)
                {
                    var skip = lines[last.Line];
                    if (IsSkipAlways(skip) && UsableSkip(skip) && bodyEnd + skip.SkipCount <= end)
                    {
                        node.Body.RemoveAt(node.Body.Count - 1);
                        node.ElseLine = last.Line;
                        node.Else = Parse(lines, bodyEnd, bodyEnd + skip.SkipCount, With(ends, bodyEnd + skip.SkipCount));
                        i = bodyEnd + skip.SkipCount;
                    }
                }

                nodes.Add(node);
                continue;
            }

            if (UsableGoto(line) && TrySwitch(lines, i, end, ends, out var sw, out var swEnd))
            {
                nodes.Add(sw);
                i = swEnd;
                continue;
            }

            if (UsableGoto(line))
            {
                var label = FindLabel(lines, i + 1, end, line.GotoLabel);

                if (label >= 0)
                {
                    var node = new EmevdNode { Kind = EmevdNodeKind.Goto, Line = i, Body = Parse(lines, i + 1, label, With(ends, label)) };
                    i = label;

                    // else: the body's last line jumps to a later label; the label this goto lands on starts it
                    if (node.Body.Count > 0 && node.Body[^1] is { Kind: EmevdNodeKind.Line } last && last.Conditions.Count == 0)
                    {
                        var jump = lines[last.Line];
                        var endLabel = IsGotoAlways(jump) && UsableGoto(jump) ? FindLabel(lines, label + 1, end, jump.GotoLabel) : -1;

                        // The jump's own target must be past the label the header lands on
                        if (endLabel > label && FindLabel(lines, last.Line + 1, end, jump.GotoLabel) == endLabel)
                        {
                            node.Body.RemoveAt(node.Body.Count - 1);
                            node.ElseLine = last.Line;
                            node.Else = new List<EmevdNode> { new() { Kind = EmevdNodeKind.Line, Line = label } };
                            node.Else.AddRange(Parse(lines, label + 1, endLabel, With(ends, endLabel)));
                            i = endLabel;
                        }
                    }

                    nodes.Add(node);
                    continue;
                }
            }

            // A jump to the end of an enclosing range: a "break" out of that block
            var plain = new EmevdNode { Kind = EmevdNodeKind.Line, Line = i };
            var target = TargetOf(lines, line);
            var level = target >= 0 ? ends.LastIndexOf(target) : -1;

            if (level >= 0)
                plain.ExitLevel = ends.Count - 1 - level;

            nodes.Add(plain);
            i++;
        }

        AttachConditions(lines, nodes);
        return nodes;
    }

    /// <summary>
    /// A switch starting at goto <paramref name="start"/>: the run of gotos (each with the conditions just before it),
    /// their labels after the run, crossing; the default part and the cases. Ranges: the switch, and inside it each
    /// part, so exits count both (leaving a case and its switch is 2 levels).
    /// </summary>
    private static bool TrySwitch(List<EmevdLine> lines, int start, int end, List<int> ends, out EmevdNode node, out int switchEnd)
    {
        node = null;
        switchEnd = end;

        // The run: goto, then (conditions*, goto)*. Conditions only before a goto that evaluates them.
        var selectors = new List<(int Line, List<int> Conditions)> { (start, new()) };
        var j = start + 1;
        while (true)
        {
            var conds = new List<int>();
            var k = j;
            while (k < end && lines[k].Kind == EmevdBlockKind.Condition)
                conds.Add(k++);

            if (k >= end || !UsableGoto(lines[k]) || (conds.Count > 0 && !Evaluates(lines[k])))
                break;

            selectors.Add((k, conds));
            j = k + 1;
        }

        if (selectors.Count < 2)
            return false;

        var targets = selectors.Select(s => FindLabel(lines, s.Line + 1, end, lines[s.Line].GotoLabel)).ToList();

        // A run ending with Goto Unconditionally past every other label is the default jumping to the end ("break"),
        // not a case: it starts the default part. With one goto left, it's a goto C-block's else instead.
        if (IsGotoAlways(lines[selectors[^1].Line]) && targets[^1] > targets.Take(targets.Count - 1).Max())
        {
            selectors.RemoveAt(selectors.Count - 1);
            targets.RemoveAt(targets.Count - 1);

            if (selectors.Count < 2)
                return false;
        }

        var lastSelector = selectors[^1].Line;
        if (targets.Any(t => t <= lastSelector))
            return false;

        // Two gotos whose labels nest (the later goto's inside the earlier one's) already make nested goto C-blocks;
        // three or more to different labels are a dispatch table even then (nested, they'd be a staircase)
        var crossing = targets.Distinct().Count() >= 3;
        for (var a = 0; a < targets.Count && !crossing; a++)
        {
            for (var b = a + 1; b < targets.Count; b++)
            {
                if (targets[b] > targets[a])
                {
                    crossing = true;
                    break;
                }
            }
        }

        if (!crossing)
            return false;

        var labels = targets.Distinct().OrderBy(t => t).ToList();

        // "break": the default (if not empty) and every case but the last end with Goto Unconditionally to one label
        // after the last case
        var breakLabel = -1;
        var breaks = 0;
        var allBreak = true;
        var sections = new List<(int From, int To)> { (lastSelector + 1, labels[0]) };
        for (var c = 0; c + 1 < labels.Count; c++)
            sections.Add((labels[c], labels[c + 1]));

        foreach (var (from, to) in sections)
        {
            // An empty default falls into the first case; a case always holds at least its label
            if (to == from)
                continue;

            // A part ending the event never falls through, so needs no break
            var last = lines[to - 1];
            if (last.IsCap)
                continue;

            if (!IsGotoAlways(last) || !UsableGoto(last) || (breakLabel >= 0 && last.GotoLabel != breakLabel))
            {
                allBreak = false;
                break;
            }

            breakLabel = last.GotoLabel;
            breaks++;
        }

        if (allBreak && breaks > 0)
        {
            var at = FindLabel(lines, labels[^1] + 1, end, breakLabel);

            // Every break must land there, not on an earlier label of the same ID
            if (at > labels[^1] && sections.Where(s => s.To > s.From).All(s => FindLabel(lines, s.To, end, breakLabel) == at))
                switchEnd = at;
        }

        var inner = With(ends, switchEnd);
        node = new EmevdNode { Kind = EmevdNodeKind.Switch, Line = start };

        foreach (var (s, conds) in selectors.Skip(1))
            node.Body.Add(new EmevdNode { Kind = EmevdNodeKind.Line, Line = s, Conditions = conds });

        node.Default = Parse(lines, lastSelector + 1, labels[0], With(inner, labels[0]));

        for (var c = 0; c < labels.Count; c++)
        {
            var to = c + 1 < labels.Count ? labels[c + 1] : switchEnd;
            var caseNodes = new List<EmevdNode> { new() { Kind = EmevdNodeKind.Line, Line = labels[c] } };
            caseNodes.AddRange(Parse(lines, labels[c] + 1, to, With(inner, to)));
            node.Cases.Add(caseNodes);
        }

        return true;
    }

    /// <summary>The enclosing range ends plus a new innermost one</summary>
    private static List<int> With(List<int> ends, int end)
    {
        var list = new List<int>(ends.Count + 1);
        list.AddRange(ends);
        list.Add(end);
        return list;
    }

    private static int FindLabel(List<EmevdLine> lines, int from, int end, int label)
    {
        for (var j = from; j < end; j++)
        {
            if (lines[j].LabelId == label)
                return j;
        }

        return -1;
    }

    /// <summary>Moves the condition lines just before an evaluating flow into that flow's node</summary>
    private static void AttachConditions(List<EmevdLine> lines, List<EmevdNode> nodes)
    {
        for (var n = 0; n < nodes.Count; n++)
        {
            if (!Evaluates(lines[nodes[n].Line]))
                continue;

            var first = n;
            while (first > 0 && nodes[first - 1] is { Kind: EmevdNodeKind.Line, Conditions.Count: 0 } prev
                   && lines[prev.Line].Kind == EmevdBlockKind.Condition)
                first--;

            if (first == n)
                continue;

            nodes[n].Conditions.AddRange(nodes.GetRange(first, n - first).Select(c => c.Line));
            nodes.RemoveRange(first, n - first);
            n = first;
        }
    }

    /// <summary>
    /// Flattens the tree: the line order, and the skip count each structured skip needs for the tree as it is now.
    /// Unstructured lines keep their counts (they aren't in the result).
    /// </summary>
    public static (List<int> order, Dictionary<int, int> skipCounts) Compile(EmevdScript script, List<EmevdNode> nodes)
    {
        var order = new List<int>();
        var counts = new Dictionary<int, int>();
        var exits = new List<List<(int line, int pos)>> { new() };

        Flatten(script, nodes, order, counts, exits);
        Resolve(exits, order, counts);

        return (order, counts);
    }

    /// <summary>
    /// Flattens one range. Exit skips register in the list of the range they leave (exits[^1 - level]) and get their
    /// count when that range closes.
    /// </summary>
    private static void Flatten(EmevdScript script, List<EmevdNode> nodes, List<int> order, Dictionary<int, int> counts,
        List<List<(int line, int pos)>> exits)
    {
        foreach (var node in nodes)
        {
            order.AddRange(node.Conditions);
            order.Add(node.Line);

            // A switch is one range, holding one range per part (as Parse made them)
            if (node.Kind == EmevdNodeKind.Switch)
            {
                exits.Add(new());
                Flatten(script, node.Body, order, counts, exits);

                foreach (var part in node.Children.Skip(2))
                {
                    exits.Add(new());
                    Flatten(script, part, order, counts, exits);
                    Resolve(exits, order, counts);
                }

                Resolve(exits, order, counts);
                continue;
            }

            if (!node.IsBlock)
            {
                // An exit moved to fewer enclosing blocks than it leaves exits the outermost one (to the event end)
                if (node.ExitLevel >= 0 && script.Lines[node.Line].Kind == EmevdBlockKind.Skip)
                    exits[^(1 + Math.Min(node.ExitLevel, exits.Count - 1))].Add((node.Line, order.Count - 1));
                continue;
            }

            // The body range: in the file the else jump is its last line
            var bodyStart = order.Count;
            exits.Add(new());
            Flatten(script, node.Body, order, counts, exits);

            if (node.ElseLine >= 0)
                order.Add(node.ElseLine);

            // The header skips the body and the else jump, not the else part
            var headerCount = order.Count - bodyStart;
            Resolve(exits, order, counts);

            if (node.ElseLine >= 0)
            {
                var elseStart = order.Count;
                exits.Add(new());
                Flatten(script, node.Else, order, counts, exits);
                Resolve(exits, order, counts);

                if (node.Kind == EmevdNodeKind.Skip)
                    counts[node.ElseLine] = order.Count - elseStart;
            }

            if (node.Kind == EmevdNodeKind.Skip)
                counts[node.Line] = headerCount;
        }
    }

    /// <summary>Closes the innermost range: its exit skips land on the next line</summary>
    private static void Resolve(List<List<(int line, int pos)>> exits, List<int> order, Dictionary<int, int> counts)
    {
        foreach (var (line, pos) in exits[^1])
            counts[line] = order.Count - (pos + 1);

        exits.RemoveAt(exits.Count - 1);
    }

    /// <summary>The display rows: each line once, in file order, plus a foot row closing each C-block</summary>
    /// <param name="collapsed">Whether the C-block headed by a line is collapsed: only its header and foot get rows</param>
    /// <param name="hidden">Filled with each line a collapsed block hides, and the row of the header hiding it</param>
    public static List<EmevdRow> Rows(List<EmevdNode> nodes, Func<int, bool> collapsed = null, Dictionary<int, int> hidden = null)
    {
        var rows = new List<EmevdRow>();
        AddRows(nodes, 0, rows, collapsed, hidden);
        return rows;
    }

    private static void AddRows(List<EmevdNode> nodes, int depth, List<EmevdRow> rows, Func<int, bool> collapsed, Dictionary<int, int> hidden)
    {
        foreach (var node in nodes)
        {
            var headerRow = rows.Count + node.Conditions.Count;

            foreach (var c in node.Conditions)
                rows.Add(new EmevdRow { Kind = EmevdRowKind.Line, Line = c, Depth = depth, EndRow = -1, ConditionOf = headerRow });

            rows.Add(new EmevdRow
            {
                Kind = EmevdRowKind.Line, Line = node.Line, Depth = depth, EndRow = -1, ConditionOf = -1,
                IsSwitch = node.Kind == EmevdNodeKind.Switch,
            });

            if (!node.IsBlock)
                continue;

            if (collapsed != null && collapsed(node.Line))
            {
                var inner = EmevdStructureEdit.LinesOf(new[] { node }).Skip(node.Conditions.Count + 1).ToList();
                if (hidden != null)
                {
                    foreach (var l in inner)
                        hidden[l] = headerRow;
                }

                var h = rows[headerRow];
                h.Collapsed = true;
                h.Hidden = inner.Count;
                rows[headerRow] = h;
            }
            else if (node.Kind == EmevdNodeKind.Switch)
            {
                AddRows(node.Body, depth + 1, rows, collapsed, hidden);

                rows.Add(new EmevdRow { Kind = EmevdRowKind.Section, Line = node.Line, Depth = depth, EndRow = -1, ConditionOf = -1 });
                AddRows(node.Default, depth + 1, rows, collapsed, hidden);

                foreach (var part in node.Cases)
                {
                    rows.Add(new EmevdRow { Kind = EmevdRowKind.Line, Line = part[0].Line, Depth = depth, EndRow = -1, IsCase = true, ConditionOf = -1 });
                    AddRows(part.GetRange(1, part.Count - 1), depth + 1, rows, collapsed, hidden);
                }
            }
            else
            {
                AddRows(node.Body, depth + 1, rows, collapsed, hidden);

                if (node.ElseLine >= 0)
                {
                    rows.Add(new EmevdRow { Kind = EmevdRowKind.Line, Line = node.ElseLine, Depth = depth, EndRow = -1, IsElse = true, ConditionOf = -1 });
                    AddRows(node.Else, depth + 1, rows, collapsed, hidden);
                }
            }

            rows.Add(new EmevdRow { Kind = EmevdRowKind.BlockEnd, Line = node.Line, Depth = depth, EndRow = -1, ConditionOf = -1 });

            var header = rows[headerRow];
            header.EndRow = rows.Count - 1;
            rows[headerRow] = header;
        }
    }

    /// <summary>The flat rows of the P1 view: one per line, at depth 0</summary>
    public static List<EmevdRow> FlatRows(EmevdScript script)
    {
        return script.Lines.Select(l => new EmevdRow { Kind = EmevdRowKind.Line, Line = l.Index, EndRow = -1, ConditionOf = -1 }).ToList();
    }

    /// <summary>The lines that head or end a C-block (their brackets aren't drawn: the C-block shows the span)</summary>
    public static HashSet<int> StructuredLines(List<EmevdNode> nodes)
    {
        var set = new HashSet<int>();
        Collect(nodes, set);
        return set;

        static void Collect(List<EmevdNode> list, HashSet<int> set)
        {
            foreach (var n in list)
            {
                if (n.ExitLevel >= 0)
                    set.Add(n.Line);

                if (!n.IsBlock)
                    continue;

                set.Add(n.Line);
                if (n.ElseLine >= 0)
                    set.Add(n.ElseLine);

                // A switch shows its gotos' jumps as its cases
                if (n.Kind == EmevdNodeKind.Switch)
                {
                    foreach (var s in n.Body)
                        set.Add(s.Line);
                }

                foreach (var child in n.Children)
                    Collect(child, set);
            }
        }
    }
}
