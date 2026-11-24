using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

public enum EmevdProblemKind
{
    /// <summary>A skip runs past the end of the event</summary>
    SkipPastEnd,
    /// <summary>A goto has no matching label after it</summary>
    GotoNoLabel,
    /// <summary>A parameter points at no instruction</summary>
    ParamNoInstruction,
    /// <summary>A parameter's target byte isn't the start of an arg, or it runs past the args</summary>
    ParamNotOnArg,
    /// <summary>No layout decodes the instruction</summary>
    Undecoded,
    /// <summary>An Initialize Event passes fewer argument bytes than the started event's parameters read</summary>
    CallTooFewArgs,
    /// <summary>An event flag arg holds a flag the game doesn't store (ER: its block isn't allocated)</summary>
    UnallocatedFlag,
    /// <summary>An Initialize Event starts an event that checks its own flag, but that flag isn't stored (ER)</summary>
    OwnFlagUnallocated,
    /// <summary>An Initialize Event gives its event the same own flag as another event started in the file (ER)</summary>
    OwnFlagShared,
}

public record EmevdProblem(EmevdProblemKind Kind, long EventId, int Line, string Detail);

/// <summary>
/// The checks pass (Claude/Documentation/EMEVD and ESD.md (EMEVD Editor) §4): the mistakes an edit can make that the
/// game wouldn't report, it would just behave differently. Pure, so EmevdEditorCheck runs it on every vanilla event
/// and around every test edit.
/// </summary>
public static class EmevdChecks
{
    /// <param name="findEvent">Finds the event an Initialize Event starts (own file, common_func, common), or null</param>
    /// <param name="isFlagStored">True when the game stores an event flag (ER's allocation), or null to skip the flag check</param>
    public static List<EmevdProblem> Run(EmevdScript script, Func<long, EMEVD.Event> findEvent = null, Func<long, bool> isFlagStored = null)
    {
        var problems = new List<EmevdProblem>();
        var id = script.Event.ID;

        foreach (var span in script.Spans.Where(s => s.Broken))
        {
            problems.Add(new(span.IsGoto ? EmevdProblemKind.GotoNoLabel : EmevdProblemKind.SkipPastEnd, id, span.From,
                span.IsGoto ? $"label {script.Lines[span.From].GotoLabel}" : $"{script.Lines[span.From].SkipCount}"));
        }

        foreach (var line in script.Lines.Where(l => !l.Decoded.Fits))
            problems.Add(new(EmevdProblemKind.Undecoded, id, line.Index, line.Key));

        foreach (var p in script.Event.Parameters)
        {
            if (p.InstructionIndex < 0 || p.InstructionIndex >= script.Lines.Count)
            {
                problems.Add(new(EmevdProblemKind.ParamNoInstruction, id, -1, $"{p.InstructionIndex}"));
                continue;
            }

            var line = script.Lines[(int)p.InstructionIndex];
            var length = line.Instr.ArgData?.Length ?? 0;

            if (p.TargetStartByte + p.ByteCount > length || (line.Decoded.Fits && !line.Decoded.Args.Any(a => a.Offset == p.TargetStartByte)))
                problems.Add(new(EmevdProblemKind.ParamNotOnArg, id, line.Index, $"+{p.TargetStartByte:X2}"));
        }

        if (findEvent != null)
        {
            foreach (var line in script.Lines.Where(l => l.Kind == EmevdBlockKind.Call && !l.CallTargetIsParam && l.CallTarget >= 0))
            {
                var target = findEvent(line.CallTarget);
                if (target == null || target.Parameters.Count == 0)
                    continue;

                var needed = target.Parameters.Max(p => p.SourceStartByte + p.ByteCount);
                var passed = (line.Instr.ArgData?.Length ?? 0) - (line.FlowArgOffset + 4);

                if (passed < needed)
                    problems.Add(new(EmevdProblemKind.CallTooFewArgs, id, line.Index, $"{passed}/{needed}"));
            }
        }

        // Fixed flag args only: a parameter's value comes from the Initialize Event that starts the event. Args that are
        // an offset from the event's own flag (Target Event Flag Type 2) depend on the slot it's started in, so they're skipped.
        if (isFlagStored != null)
        {
            foreach (var line in script.Lines.Where(l => l.Decoded.Fits && FlagTargetType(l) is -1 or 0))
                foreach (var arg in line.Decoded.Args.Where(a => a.Def?.Role == EmevdArgRole.EventFlag && !line.Params.ContainsKey(a.Offset)))
                {
                    var v = arg.Type == EmevdArgType.U32 && arg.Int == uint.MaxValue ? -1 : arg.Int;
                    if (v > 0 && !isFlagStored(v))
                        problems.Add(new(EmevdProblemKind.UnallocatedFlag, id, line.Index, $"{v}"));
                }
        }

        return problems;
    }

    /// <summary>
    /// The own-flag pass over one file (ER; Claude/Documentation/EMEVD and ESD.md (Elden Ring event flags), "Event IDs as flags"). Every event
    /// instance has an own flag that the game turns on when the instance first ends, and that one-shot events check
    /// (Target Event Flag Type 2) to skip themselves after a reload. For each Initialize Event / Initialize Common Event
    /// line whose started event checks its own flag, reports an own flag the game doesn't store, or one that another
    /// event started in this file also gets.
    /// </summary>
    /// <param name="fileName">The file's name (m10_00_00_00, common_func...): common events take their flag from its map</param>
    /// <param name="scripts">Every event of the file, in file order (the instruction index counts across the file)</param>
    /// <param name="findScript">The event an init starts (own file, common_func, common), decoded; null if not found</param>
    /// <param name="isFlagStored">True when the game stores an event flag</param>
    public static List<EmevdProblem> RunOwnFlags(string fileName, IReadOnlyList<EmevdScript> scripts,
        Func<long, EmevdScript> findScript, Func<long, bool> isFlagStored)
    {
        var starts = new List<(long Caller, int Line, long EventId, long Flag, bool Reads)>();
        var readsOwn = new Dictionary<long, bool>();
        var index = 0;

        foreach (var script in scripts)
        {
            foreach (var line in script.Lines)
            {
                var common = line.Instr.Bank == 2000 && line.Instr.ID == 6;
                if (line.Instr.Bank == 2000 && (line.Instr.ID == 0 || common) && line.Decoded.Fits && line.Instr.ArgData.Length >= 8
                    && !line.Params.Keys.Any(o => o < 8))
                {
                    var slot = BitConverter.ToInt16(line.Instr.ArgData, 0);
                    long id = BitConverter.ToUInt32(line.Instr.ArgData, 4);

                    // Event IDs up to 299 have no own flag (eldenring.exe 1405844E0)
                    if (id > 299 && OwnFlag(fileName, common, slot, id, index + line.Index) is long flag)
                    {
                        if (!readsOwn.TryGetValue(id, out var reads))
                            readsOwn[id] = reads = findScript(id)?.Lines.Any(l => FlagTargetType(l) == 2) == true;
                        starts.Add((script.Event.ID, line.Index, id, flag, reads));
                    }
                }
            }
            index += script.Lines.Count;
        }

        var problems = new List<EmevdProblem>();

        foreach (var s in starts.Where(s => s.Reads && !isFlagStored(s.Flag)))
            problems.Add(new(EmevdProblemKind.OwnFlagUnallocated, s.Caller, s.Line, $"{s.Flag} (event {s.EventId})"));

        foreach (var group in starts.GroupBy(s => s.Flag).Where(g => g.Select(s => s.EventId).Distinct().Count() > 1 && g.Any(s => s.Reads)))
        {
            foreach (var s in group.Where(s => s.Reads))
            {
                var other = group.First(o => o.EventId != s.EventId);
                problems.Add(new(EmevdProblemKind.OwnFlagShared, s.Caller, s.Line,
                    $"{s.Flag}: event {s.EventId}, also event {other.EventId} (event {other.Caller} line {other.Line})"));
            }
        }

        return problems;
    }

    /// <summary>
    /// The own flag ER gives an event instance, or null when it can't be worked out:
    /// - Initialize Event: event ID + slot when the slot is above 0, otherwise the event ID (140584700)
    /// - Initialize Common Event: in the calling map's thousands-digit-5 block, AABB5nnn for a legacy map, TbXXYY5nnn for an
    ///   m60 / m61 tile, where nnn is the init line's index across the whole file, mod 1000 (1405CC510); the event ID in
    ///   other maps and files. Maps ending above _09 are left out: the exe's map inputs for them weren't confirmed.
    /// </summary>
    public static long? OwnFlag(string fileName, bool common, int slot, long eventId, int fileIndex)
    {
        if (eventId == uint.MaxValue)
            return null;
        if (!common)
            return slot > 0 ? eventId + slot : eventId;

        var m = System.Text.RegularExpressions.Regex.Match(fileName ?? "", @"^m(\d\d)_(\d\d)_(\d\d)_(\d\d)$");
        if (!m.Success)
            return eventId;

        int area = int.Parse(m.Groups[1].Value), bb = int.Parse(m.Groups[2].Value),
            cc = int.Parse(m.Groups[3].Value), dd = int.Parse(m.Groups[4].Value);
        var nnn = fileIndex % 1000;

        if (area is 60 or 61)
            return dd > 9 ? null : ((((area == 60 ? 10 : 20) + dd) * 100L + bb) * 100 + cc) * 10000 + 5000 + nnn;
        if (area is >= 50 and <= 88)
            return eventId;
        return (area * 100L + bb) * 10000 + 5000 + nnn;
    }

    /// <summary>
    /// What a line's event flag args are, from its Target Event Flag Type arg: 0 = a flag ID, 1 = event ID (no flag in ER),
    /// 2 = an offset from the event's own flag (event ID + slot). -1 when the line has no such arg (its flags are IDs),
    /// null when a parameter sets it.
    /// </summary>
    public static int? FlagTargetType(EmevdLine line)
    {
        var type = line.Decoded.Args?.FirstOrDefault(a => a.Def?.Name == "Target Event Flag Type") ?? default;
        if (type.Def == null)
            return -1;
        if (line.Params.ContainsKey(type.Offset))
            return null;
        return (int)type.Int;
    }
}
