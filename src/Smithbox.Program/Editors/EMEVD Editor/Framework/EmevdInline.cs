using SoulsFormats;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// A call shown as the event it calls (DS2 / SotFS MustSub calls): the called event's one instruction with the
/// caller's values written in, as the game runs it.
/// </summary>
public class EmevdInlined
{
    /// <summary>A <see cref="CallArg"/> value: the caller supplies it, but at bytes no single call arg covers</summary>
    public const int Unaligned = -2;

    /// <summary>The called event's instruction, decoded with the caller's values in its parameterised args</summary>
    public EmevdLine Line;

    /// <summary>For each of <see cref="Line"/>'s args (by index): the call arg supplying it (by index), or -1 when the
    /// called event fixes it (shared by every caller)</summary>
    public int[] CallArg;

    public EMEVD.Event Target;
}

public static class EmevdInline
{
    /// <summary>
    /// The call as its target's instruction, or null when it can't be shown so: the target has more than one
    /// instruction, a parameter reads past the call's args, or the result doesn't decode. A MustSub call's args are
    /// slot, event ID, then the target's parameter block, so parameter source byte S is the call's byte
    /// (event ID offset + 4 + S).
    /// </summary>
    public static EmevdInlined Build(EmevdCatalog catalog, EmevdLine call, EMEVD.Event target)
    {
        if (target == null || target.Instructions.Count != 1 || call.Kind != EmevdBlockKind.Call
            || !call.Decoded.Fits || call.FlowArgOffset < 0)
            return null;

        var instr = target.Instructions[0];
        var callData = call.Instr.ArgData ?? Array.Empty<byte>();
        var data = (byte[])(instr.ArgData ?? Array.Empty<byte>()).Clone();
        var blockStart = call.FlowArgOffset + 4;
        var sourceOf = new Dictionary<int, int>();

        foreach (var p in target.Parameters)
        {
            if (p.InstructionIndex != 0)
                continue;

            var src = blockStart + (int)p.SourceStartByte;
            var dst = (int)p.TargetStartByte;

            if (src < blockStart || src + p.ByteCount > callData.Length || dst < 0 || dst + p.ByteCount > data.Length)
                return null;

            Array.Copy(callData, src, data, dst, p.ByteCount);
            sourceOf[dst] = src;
        }

        var substituted = new EMEVD.Instruction(instr.Bank, instr.ID, data) { Layer = instr.Layer };
        var decoded = EmevdDecoded.Decode(catalog?.Find(instr.Bank, instr.ID), substituted);

        if (!decoded.Fits)
            return null;

        var line = new EmevdLine
        {
            Index = call.Index,
            Instr = substituted,
            Decoded = decoded,
            Kind = EmevdBlockKind.Command,
        };

        var callArg = new int[decoded.Args.Count];
        for (var a = 0; a < decoded.Args.Count; a++)
        {
            callArg[a] = -1;
            if (sourceOf.TryGetValue(decoded.Args[a].Offset, out var src))
                callArg[a] = call.Decoded.Args.FindIndex(c => c.Offset == src) is >= 0 and var i ? i : EmevdInlined.Unaligned;

            // The calling event's own parameter over that call arg: the value comes from further up
            if (callArg[a] >= 0 && call.Params.TryGetValue(call.Decoded.Args[callArg[a]].Offset, out var p))
                line.Params[decoded.Args[a].Offset] = p;
        }

        return new EmevdInlined { Line = line, CallArg = callArg, Target = target };
    }
}
