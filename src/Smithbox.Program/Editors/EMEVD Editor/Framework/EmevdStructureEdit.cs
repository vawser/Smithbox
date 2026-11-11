using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>Where a unit of nodes sits: the list holding it, its first index and node count</summary>
public record struct EmevdUnit(List<EmevdNode> List, int Index, int Count);

/// <summary>An instruction copied with the parameters that write it, for copy/paste and duplicate</summary>
public class EmevdClipLine
{
    public int Bank;
    public int ID;
    public byte[] Args;
    public uint? Layer;
    /// <summary>Parameters writing this instruction: target byte, source byte, size, UnkID</summary>
    public List<(long Target, long Source, int Size, int UnkID)> Params = new();

    public EMEVD.Instruction Make()
    {
        return new EMEVD.Instruction(Bank, ID, (byte[])Args.Clone()) { Layer = Layer };
    }
}

/// <summary>A structural edit worked out: the event's new instruction and parameter lists, and the skip counts it rewrites</summary>
public class EmevdEditResult
{
    public List<EMEVD.Instruction> Instructions;
    public List<EMEVD.Parameter> Parameters;
    public List<(EMEVD.Instruction Instr, int Offset, byte[] Bytes)> Patches = new();
}

/// <summary>
/// Structural edits of an event (P4): delete, move, insert, paste. Edits are made on a copy of the structured tree
/// (EmevdStructure), then compiled:
/// - structured skips (C-block headers, else jumps, exits) get the counts the new tree needs;
/// - raw skips keep their target instruction: the count is recomputed to land on the same line (or the next surviving
///   one if it was deleted), so lines inserted between a raw skip and its target are skipped too;
/// - parameters follow their instruction; those of deleted instructions go, and pasted ones come along.
/// Labels are IDs, so gotos need no rewriting; a goto C-block moves with its closing label when nothing else jumps
/// to it (<see cref="UnitOf"/>). Instruction objects are reused; only skip-count bytes change.
/// </summary>
public static class EmevdStructureEdit
{
    public static List<EmevdNode> CopyTree(List<EmevdNode> nodes)
    {
        return nodes.Select(n => new EmevdNode
        {
            Kind = n.Kind,
            Line = n.Line,
            Conditions = n.Conditions.ToList(),
            Body = CopyTree(n.Body),
            ElseLine = n.ElseLine,
            Else = CopyTree(n.Else),
            ExitLevel = n.ExitLevel,
        }).ToList();
    }

    /// <summary>The list and index of the node a line belongs to (as its line, else jump or condition), with the lists above it</summary>
    public static bool Find(List<EmevdNode> nodes, int line, out List<EmevdNode> list, out int index, List<(List<EmevdNode> list, int index)> path = null)
    {
        for (var i = 0; i < nodes.Count; i++)
        {
            var n = nodes[i];
            if (n.Line == line || n.ElseLine == line || n.Conditions.Contains(line))
            {
                list = nodes;
                index = i;
                return true;
            }

            path?.Add((nodes, i));

            if (Find(n.Body, line, out list, out index, path) || Find(n.Else, line, out list, out index, path))
                return true;

            path?.RemoveAt(path.Count - 1);
        }

        list = null;
        index = -1;
        return false;
    }

    /// <summary>Every line of some nodes, in file order</summary>
    public static List<int> LinesOf(IEnumerable<EmevdNode> nodes)
    {
        var lines = new List<int>();
        foreach (var n in nodes)
        {
            lines.AddRange(n.Conditions);
            lines.Add(n.Line);
            lines.AddRange(LinesOf(n.Body));
            if (n.ElseLine >= 0)
                lines.Add(n.ElseLine);
            lines.AddRange(LinesOf(n.Else));
        }
        return lines;
    }

    /// <summary>
    /// The unit an edit of a line acts on: its node (a whole C-block for a header or else line, with its conditions),
    /// plus, for a goto C-block, the label closing it when no goto outside the unit lands there.
    /// </summary>
    public static EmevdUnit UnitOf(EmevdScript script, List<EmevdNode> tree, int line)
    {
        if (!Find(tree, line, out var list, out var index))
            return default;

        var node = list[index];
        var count = 1;

        if (node.Kind == EmevdNodeKind.Goto && index + 1 < list.Count && list[index + 1] is { Kind: EmevdNodeKind.Line } next
            && script.Lines[next.Line].LabelId >= 0)
        {
            var inside = LinesOf(list.GetRange(index, 1)).ToHashSet();
            var label = next.Line;

            var landsHere = script.Lines.Where(l => l.Kind == EmevdBlockKind.Goto && TargetOf(script, l) == label).ToList();
            if (landsHere.Count > 0 && landsHere.All(l => inside.Contains(l.Index)))
                count = 2;
        }

        return new EmevdUnit(list, index, count);
    }

    /// <summary>The line a goto lands on: the first matching label after it, or -1</summary>
    public static int TargetOf(EmevdScript script, EmevdLine line)
    {
        for (var j = line.Index + 1; j < script.Lines.Count; j++)
        {
            if (script.Lines[j].LabelId == line.GotoLabel)
                return j;
        }
        return -1;
    }

    /// <summary>The unit's nodes taken out of the tree</summary>
    public static List<EmevdNode> Take(EmevdUnit unit)
    {
        var nodes = unit.List.GetRange(unit.Index, unit.Count);
        unit.List.RemoveRange(unit.Index, unit.Count);
        return nodes;
    }

    /// <summary>
    /// Moves the unit one step: past its neighbour (a whole C-block counts as one step), or out of its block when it's
    /// first or last in it. Returns false at the top or bottom of the event.
    /// </summary>
    public static bool Step(EmevdScript script, List<EmevdNode> tree, int line, bool down)
    {
        var path = new List<(List<EmevdNode> list, int index)>();
        if (!Find(tree, line, out _, out _, path))
            return false;

        var unit = UnitOf(script, tree, line);
        if (unit.List == null)
            return false;

        if (!down && unit.Index > 0)
        {
            var nodes = Take(unit);
            unit.List.InsertRange(unit.Index - 1, nodes);
            return true;
        }

        if (down && unit.Index + unit.Count < unit.List.Count)
        {
            var nodes = Take(unit);
            unit.List.InsertRange(unit.Index + 1, nodes);
            return true;
        }

        // Out of the enclosing block: just before it, or just after it
        if (path.Count == 0)
            return false;

        var (parentList, parentIndex) = path[^1];
        var moved = Take(unit);
        parentList.InsertRange(down ? parentIndex + 1 : parentIndex, moved);
        return true;
    }

    /// <summary>The lines of the unit, copied with the parameters that write them</summary>
    public static List<EmevdClipLine> Copy(EmevdScript script, EmevdUnit unit)
    {
        var lines = LinesOf(unit.List.GetRange(unit.Index, unit.Count));
        return lines.Select(l =>
        {
            var instr = script.Lines[l].Instr;
            var clip = new EmevdClipLine { Bank = instr.Bank, ID = instr.ID, Args = (byte[])(instr.ArgData ?? Array.Empty<byte>()).Clone(), Layer = instr.Layer };
            foreach (var p in script.Event.Parameters.Where(p => p.InstructionIndex == l))
                clip.Params.Add((p.TargetStartByte, p.SourceStartByte, p.ByteCount, p.UnkID));
            return clip;
        }).ToList();
    }

    // ---------------------------------------------------------------- several units at once

    /// <summary>
    /// The units a set of selected lines act on, each named by its lead line (the node's own line), in file order. A
    /// unit inside another selected unit (a line in a selected C-block's body) is left out: it goes with that block.
    /// </summary>
    public static List<int> UnitLeads(EmevdScript script, List<EmevdNode> tree, IEnumerable<int> lines)
    {
        var units = new List<(int lead, int first, HashSet<int> lines)>();

        foreach (var line in lines.Distinct())
        {
            var unit = UnitOf(script, tree, line);
            if (unit.List == null)
                continue;

            var lead = unit.List[unit.Index].Line;
            if (units.Any(u => u.lead == lead))
                continue;

            var unitLines = LinesOf(unit.List.GetRange(unit.Index, unit.Count));
            units.Add((lead, unitLines.Min(), unitLines.ToHashSet()));
        }

        // Biggest first, so a block's own lines are covered before its inner units are looked at
        var kept = new List<(int lead, int first, HashSet<int> lines)>();
        foreach (var u in units.OrderByDescending(u => u.lines.Count))
        {
            if (!kept.Any(k => k.lines.Contains(u.lead)))
                kept.Add(u);
        }

        return kept.OrderBy(u => u.first).Select(u => u.lead).ToList();
    }

    public static bool DeleteMany(EmevdScript script, List<EmevdNode> tree, List<int> leads)
    {
        var any = false;

        foreach (var lead in leads)
        {
            var unit = UnitOf(script, tree, lead);
            if (unit.List == null)
                continue;

            Take(unit);
            any = true;
        }

        return any;
    }

    /// <summary>The lines of every unit, in file order, with their parameters</summary>
    public static List<EmevdClipLine> CopyMany(EmevdScript script, List<EmevdNode> tree, List<int> leads)
    {
        var lines = new List<EmevdClipLine>();

        foreach (var lead in leads)
        {
            var unit = UnitOf(script, tree, lead);
            if (unit.List != null)
                lines.AddRange(Copy(script, unit));
        }

        return lines;
    }

    /// <summary>Puts a copy of each unit right after it. Returns the new lines, in the order of their line numbers.</summary>
    public static List<EmevdClipLine> DuplicateMany(EmevdScript script, List<EmevdNode> tree, List<int> leads)
    {
        var added = new List<EmevdClipLine>();

        foreach (var lead in leads)
        {
            var unit = UnitOf(script, tree, lead);
            if (unit.List == null)
                continue;

            var copy = Copy(script, unit);
            var nodes = Enumerable.Range(0, copy.Count)
                .Select(k => new EmevdNode { Kind = EmevdNodeKind.Line, Line = script.Lines.Count + added.Count + k })
                .ToList();

            unit.List.InsertRange(unit.Index + unit.Count, nodes);
            added.AddRange(copy);
        }

        return added;
    }

    /// <summary>
    /// Steps every unit once, the top one first going up and the bottom one first going down, so adjacent units move
    /// together. False if any can't move (the edit is then dropped whole).
    /// </summary>
    public static bool StepMany(EmevdScript script, List<EmevdNode> tree, List<int> leads, bool down)
    {
        foreach (var lead in down ? Enumerable.Reverse(leads) : leads)
        {
            if (!Step(script, tree, lead, down))
                return false;
        }

        return leads.Count > 0;
    }

    /// <summary>
    /// Moves every unit, in file order and next to each other, into <paramref name="dest"/> before
    /// <paramref name="before"/> (null: at the end). False if the place is inside one of them.
    /// </summary>
    public static bool MoveMany(EmevdScript script, List<EmevdNode> tree, List<int> leads, List<EmevdNode> dest, EmevdNode before)
    {
        var moving = new List<EmevdNode>();

        foreach (var lead in leads)
        {
            var unit = UnitOf(script, tree, lead);
            if (unit.List == null)
                return false;

            moving.AddRange(unit.List.GetRange(unit.Index, unit.Count));
        }

        if (moving.Count == 0 || moving.Any(n => Within(n, dest)) || (before != null && moving.Contains(before)))
            return false;

        DeleteMany(script, tree, leads);

        var index = before == null ? dest.Count : dest.IndexOf(before);
        dest.InsertRange(index < 0 ? dest.Count : index, moving);
        return true;
    }

    /// <summary>Whether a list is one of the node's bodies or else parts, at any depth</summary>
    public static bool Within(EmevdNode node, List<EmevdNode> list)
    {
        return ReferenceEquals(node.Body, list) || ReferenceEquals(node.Else, list)
            || node.Body.Any(n => Within(n, list)) || node.Else.Any(n => Within(n, list));
    }

    /// <summary>
    /// Works out the event after an edit. <paramref name="tree"/> is the edited copy of the script's tree; lines from
    /// script.Lines.Count on are the new instructions in <paramref name="added"/>, in order.
    /// </summary>
    public static EmevdEditResult Apply(EmevdCatalog catalog, EmevdScript script, List<EmevdNode> tree, List<EmevdClipLine> added)
    {
        added ??= new();
        var n = script.Lines.Count;

        // A script over the old instructions plus the new ones, so compile can see the new lines' kinds
        var newInstrs = added.Select(a => a.Make()).ToList();
        var extEvent = new EMEVD.Event(script.Event.ID, script.Event.RestBehavior)
        {
            Instructions = script.Event.Instructions.Concat(newInstrs).ToList(),
            Parameters = script.Event.Parameters.ToList(),
        };
        var ext = EmevdScript.Build(catalog, extEvent);

        var (order, counts) = EmevdStructure.Compile(ext, tree);
        var pos = new Dictionary<int, int>();
        for (var i = 0; i < order.Count; i++)
            pos[order[i]] = i;

        var result = new EmevdEditResult { Instructions = order.Select(i => ext.Lines[i].Instr).ToList() };

        foreach (var l in order)
        {
            var line = ext.Lines[l];
            if (line.Kind != EmevdBlockKind.Skip || line.Params.ContainsKey(line.FlowArgOffset))
                continue;

            int count;
            if (counts.TryGetValue(l, out var c))
            {
                count = c;
            }
            else if (l < n && line.SkipCount >= 0)
            {
                // Raw: land on the same instruction, or the next one that's still there
                var target = order.Count;
                for (var t = l + 1 + line.SkipCount; t < n; t++)
                {
                    if (pos.TryGetValue(t, out var p))
                    {
                        target = p;
                        break;
                    }
                }

                count = target - pos[l] - 1;
                if (count < 0)
                    continue;
            }
            else
            {
                continue;
            }

            if (count == line.SkipCount)
                continue;

            var arg = line.Decoded.Args.FirstOrDefault(a => a.Offset == line.FlowArgOffset);
            if (arg.Def != null && EmevdDecoded.EncodeInt(arg.Type, count, out var bytes))
                result.Patches.Add((line.Instr, arg.Offset, bytes));
        }

        // Parameters follow their instruction
        var parameters = new List<EMEVD.Parameter>();
        foreach (var p in script.Event.Parameters)
        {
            if (pos.TryGetValue((int)p.InstructionIndex, out var at))
                parameters.Add(new EMEVD.Parameter(at, p.TargetStartByte, p.SourceStartByte, p.ByteCount) { UnkID = p.UnkID });
        }

        for (var a = 0; a < added.Count; a++)
        {
            if (!pos.TryGetValue(n + a, out var at))
                continue;

            foreach (var (target, source, size, unk) in added[a].Params)
                parameters.Add(new EMEVD.Parameter(at, target, source, size) { UnkID = unk });
        }

        // Stable, so an event whose instructions didn't move keeps its parameter order
        result.Parameters = parameters.OrderBy(p => p.InstructionIndex).ToList();

        return result;
    }

    /// <summary>A new instruction from the layout: its required args at their defaults</summary>
    public static EmevdClipLine NewInstruction(EmevdInstrDef def)
    {
        var args = def.Args.Where(a => !a.Optional).ToList();
        var types = args.Select(a => a.Type).ToList();
        var (offsets, size) = EmevdDecoded.Offsets(types);
        var data = new byte[size];

        for (var i = 0; i < args.Count; i++)
        {
            byte[] bytes;
            if (args[i].Type == EmevdArgType.F32)
            {
                bytes = EmevdDecoded.EncodeFloat((float)args[i].Default);
            }
            else
            {
                var (min, max) = EmevdDecoded.Range(args[i].Type);
                EmevdDecoded.EncodeInt(args[i].Type, Math.Clamp((long)args[i].Default, min, max), out bytes);
            }

            bytes.CopyTo(data, offsets[i]);
        }

        return new EmevdClipLine { Bank = def.Bank, ID = def.ID, Args = data };
    }
}
