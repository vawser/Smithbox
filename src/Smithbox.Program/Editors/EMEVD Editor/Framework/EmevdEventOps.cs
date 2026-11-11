using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>A place an event, call or value was found: file name, event, line (-1 for the event itself), what</summary>
public record EmevdHit(string File, long EventId, int Line, string What);

/// <summary>What the Search tool looks for</summary>
public record EmevdQuery(long? Value, string Instruction, bool MatchFloats);

/// <summary>
/// Event-level operations and project scans for P5 (Claude/Documentation/EMEVD Block Editor Analysis.md): copying an
/// event, finding a free ID, the Initialize Event calls that start an event, and the value/instruction search. Pure, so
/// EmevdEditorCheck runs the same code.
/// </summary>
public static class EmevdEventOps
{
    /// <summary>
    /// A deep copy of an event under a new ID: new instructions (args, layer) and parameters, so editing either
    /// leaves the other alone. Names come from the .emeld, which isn't written, so the copy has none.
    /// </summary>
    public static EMEVD.Event Copy(EMEVD.Event ev, long newId)
    {
        return new EMEVD.Event(newId, ev.RestBehavior)
        {
            Instructions = ev.Instructions
                .Select(i => new EMEVD.Instruction(i.Bank, i.ID, (byte[])(i.ArgData ?? Array.Empty<byte>()).Clone()) { Layer = i.Layer })
                .ToList(),
            Parameters = ev.Parameters
                .Select(p => new EMEVD.Parameter(p.InstructionIndex, p.TargetStartByte, p.SourceStartByte, p.ByteCount) { UnkID = p.UnkID })
                .ToList(),
            EmptyParametersOffset = ev.EmptyParametersOffset,
        };
    }

    /// <summary>The first ID from <paramref name="start"/> up that no event in the file has</summary>
    public static long NextFreeId(IEnumerable<EMEVD.Event> events, long start)
    {
        var used = events.Select(e => e.ID).ToHashSet();
        var id = Math.Max(0, start);
        while (used.Contains(id))
            id++;
        return id;
    }

    /// <summary>
    /// The Initialize Event lines in <paramref name="file"/> that start the event <paramref name="targetId"/>, where
    /// <paramref name="resolvesHere"/> says a call from this file to that ID reaches the event looked for (the game
    /// takes the calling file's own event first, then common_func's and common's). Calls whose target a parameter
    /// supplies aren't counted: the ID is only known when the event starts.
    /// </summary>
    public static IEnumerable<EmevdHit> CallsTo(EmevdCatalog catalog, string fileName, EMEVD file, long targetId, bool resolvesHere)
    {
        if (!resolvesHere)
            yield break;

        foreach (var ev in file.Events.ToList())
        {
            var script = EmevdScript.Build(catalog, ev);
            foreach (var line in script.Lines)
            {
                if (line.Kind == EmevdBlockKind.Call && !line.CallTargetIsParam && line.CallTarget == targetId)
                    yield return new EmevdHit(fileName, ev.ID, line.Index, line.Name);
            }
        }
    }

    /// <summary>
    /// The lines of an event matching a query: the instruction filter (key like 2003[66], or part of the name) and/or a
    /// value held by any decoded arg (the stored value: args a parameter writes hold a placeholder, so they're skipped).
    /// </summary>
    public static IEnumerable<EmevdHit> Search(EmevdCatalog catalog, string fileName, EMEVD.Event ev, EmevdQuery query)
    {
        var script = EmevdScript.Build(catalog, ev);
        var filter = query.Instruction?.Trim();

        foreach (var line in script.Lines)
        {
            if (!string.IsNullOrEmpty(filter)
                && !line.Key.Equals(filter, StringComparison.OrdinalIgnoreCase)
                && !line.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                continue;

            if (query.Value == null)
            {
                yield return new EmevdHit(fileName, ev.ID, line.Index, line.Name);
                continue;
            }

            if (!line.Decoded.Fits)
                continue;

            foreach (var arg in line.Decoded.Args)
            {
                if (line.Params.ContainsKey(arg.Offset))
                    continue;

                var match = arg.IsFloat
                    ? query.MatchFloats && arg.Float == query.Value.Value
                    : arg.Int == query.Value.Value || (arg.Type is EmevdArgType.U32 && arg.Int == unchecked((uint)query.Value.Value));

                if (match)
                {
                    yield return new EmevdHit(fileName, ev.ID, line.Index, $"{line.Name}: {ArgName(arg)}");
                    break;
                }
            }
        }
    }

    private static string ArgName(EmevdArg arg)
    {
        return arg.Repeat > 0 ? $"{arg.Def.DisplayName} {arg.Repeat + 1}" : arg.Def.DisplayName;
    }

    /// <summary>The Initialize Event lines of an event that start <paramref name="oldId"/>, for renumbering it</summary>
    public static List<EmevdLine> CallLines(EmevdScript script, long oldId)
    {
        return script.Lines.Where(l => l.Kind == EmevdBlockKind.Call && !l.CallTargetIsParam && l.CallTarget == oldId).ToList();
    }
}
