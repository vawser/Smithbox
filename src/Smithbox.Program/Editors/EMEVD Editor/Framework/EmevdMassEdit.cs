using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>How a mass edit changes the values it finds</summary>
public enum EmevdMassOp { Set, Add }

/// <summary>
/// What a mass edit looks for and does: integer args holding a value in [From, To], optionally only args of one role
/// (an event flag, an entity…), one param (for Param args) and/or one instruction; each is set to Operand, or has
/// Operand added.
/// </summary>
public record EmevdMassQuery(
    long From, long To,
    EmevdArgRole? Role, string ParamName, string Instruction,
    bool IncludeCallTargets,
    EmevdMassOp Op, long Operand);

/// <summary>One planned change: an arg of an instruction, its value now and after, or why it can't be made</summary>
public class EmevdMassChange
{
    public string File;
    public long EventId;
    public int Line;
    public EMEVD.Instruction Instr;
    public int Offset;
    public EmevdArgType Type;
    public long Old;
    public long New;
    /// <summary>The bytes the arg holds now, so a change made after the plan went stale can be seen</summary>
    public byte[] OldBytes;
    public byte[] NewBytes;
    public string What;
    /// <summary>The new value doesn't fit the arg's type (NewBytes null)</summary>
    public bool OutOfRange;
}

/// <summary>
/// The mass edit (P6, Claude/Documentation/EMEVD Block Editor Analysis.md): finds integer arg values and plans their
/// replacement. Pure, so EmevdEditorCheck runs the same code.
///
/// Never planned: skip counts and goto labels (the structure owns them), args a parameter writes (they hold
/// placeholders the game replaces), float args, and lines the layouts can't decode. Initialize Event targets only when
/// asked. u32 args match a negative number by its 32-bit form, as Search does (-1 finds 4294967295), and a negative
/// new value for a u32 arg is written the same way.
/// </summary>
public static class EmevdMassEdit
{
    public static IEnumerable<EmevdMassChange> Plan(EmevdCatalog catalog, string fileName, EMEVD.Event ev, EmevdMassQuery query)
    {
        var script = EmevdScript.Build(catalog, ev);
        var filter = query.Instruction?.Trim();
        var paramName = query.ParamName?.Trim();

        foreach (var line in script.Lines)
        {
            if (!line.Decoded.Fits)
                continue;

            if (!string.IsNullOrEmpty(filter)
                && !line.Key.Equals(filter, StringComparison.OrdinalIgnoreCase)
                && !line.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                continue;

            foreach (var arg in line.Decoded.Args)
            {
                if (arg.IsFloat || line.Params.ContainsKey(arg.Offset))
                    continue;

                // The structure's own args, and the called event unless asked
                if (arg.Offset == line.FlowArgOffset && line.Kind is EmevdBlockKind.Skip or EmevdBlockKind.Goto)
                    continue;
                if (arg.Offset == line.FlowArgOffset && line.Kind == EmevdBlockKind.Call && !query.IncludeCallTargets)
                    continue;

                if (query.Role != null && arg.Def.Role != query.Role)
                    continue;

                if (!string.IsNullOrEmpty(paramName) && !string.Equals(ParamOf(line, arg), paramName, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!Matches(arg, query.From, query.To))
                    continue;

                var signed = Signed(arg);
                var target = query.Op == EmevdMassOp.Set ? query.Operand : signed + query.Operand;

                // A u32 takes a negative value by its 32-bit form
                if (arg.Type == EmevdArgType.U32 && target < 0 && target >= int.MinValue)
                    target = unchecked((uint)(int)target);

                var change = new EmevdMassChange
                {
                    File = fileName,
                    EventId = ev.ID,
                    Line = line.Index,
                    Instr = line.Instr,
                    Offset = arg.Offset,
                    Type = arg.Type,
                    Old = arg.Int,
                    New = target,
                    OldBytes = line.Instr.ArgData.AsSpan(arg.Offset, arg.Size).ToArray(),
                    What = $"{line.Name}: {ArgName(arg)}",
                };

                if (EmevdDecoded.EncodeInt(arg.Type, target, out var bytes))
                    change.NewBytes = bytes;
                else
                    change.OutOfRange = true;

                // Setting a value to itself is no change
                if (change.NewBytes != null && change.NewBytes.AsSpan().SequenceEqual(change.OldBytes))
                    continue;

                yield return change;
            }
        }
    }

    /// <summary>The value in [from, to]; a u32 also by its signed form, so -1 finds 4294967295</summary>
    private static bool Matches(EmevdArg arg, long from, long to)
    {
        if (arg.Int >= from && arg.Int <= to)
            return true;

        return arg.Type == EmevdArgType.U32 && Signed(arg) is var s && s != arg.Int && s >= from && s <= to;
    }

    /// <summary>
    /// A u32 above 2^31-1 as its signed 32-bit form (4294967295 is -1, "none"); other types as they are. Adding to it
    /// and writing it back as a u32 gives the same bytes as unsigned arithmetic would.
    /// </summary>
    private static long Signed(EmevdArg arg)
    {
        return arg.Type == EmevdArgType.U32 && arg.Int > int.MaxValue ? unchecked((int)(uint)arg.Int) : arg.Int;
    }

    /// <summary>The param a Param arg refers to (picked by another arg when it has a selector), or null</summary>
    public static string ParamOf(EmevdLine line, EmevdArg arg)
    {
        var def = arg.Def;
        if (def.Role != EmevdArgRole.Param)
            return null;

        if (def.RoleSelector < 0)
            return def.ParamFor(0);

        var selector = line.Decoded.Args.FirstOrDefault(a => a.Offset == def.RoleSelector);
        return selector.Def != null ? def.ParamFor(selector.Int) : null;
    }

    private static string ArgName(EmevdArg arg)
    {
        return arg.Repeat > 0 ? $"{arg.Def.DisplayName} {arg.Repeat + 1}" : arg.Def.DisplayName;
    }

    /// <summary>The change still applies: the arg holds the bytes it had when it was planned</summary>
    public static bool StillCurrent(EmevdMassChange change)
    {
        var data = change.Instr.ArgData;
        return data != null && change.Offset + change.OldBytes.Length <= data.Length
            && data.AsSpan(change.Offset, change.OldBytes.Length).SequenceEqual(change.OldBytes);
    }
}
