using SoulsFormats;
using System.Buffers.Binary;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>One unpacked arg of an instruction</summary>
public struct EmevdArg
{
    /// <summary>The layout's arg; for a repeated last arg, the same definition each time</summary>
    public EmevdArgDef Def;
    public int Offset;
    public int Size;
    public EmevdArgType Type;
    /// <summary>0 for the defined args, 1.. for each extra repeat of the last one</summary>
    public int Repeat;
    /// <summary>Integer value (floats are in <see cref="Float"/>)</summary>
    public long Int;
    public float Float;

    public bool IsFloat => Type == EmevdArgType.F32;
}

/// <summary>
/// An instruction's args unpacked with its layout, the way Claude/Tools/EmedfSurvey's verify does it: the last arg
/// repeats when RepeatLastArg, otherwise a use may stop before its Optional args. Uses that fit no form keep
/// <see cref="Fits"/> false and are shown as raw bytes.
/// </summary>
public class EmevdDecoded
{
    public EmevdInstrDef Def;
    public List<EmevdArg> Args = new();
    public bool Fits;
    public string Problem;

    public static int SizeOf(EmevdArgType t) => t switch
    {
        EmevdArgType.U8 or EmevdArgType.S8 => 1,
        EmevdArgType.U16 or EmevdArgType.S16 => 2,
        _ => 4
    };

    public static string TypeName(EmevdArgType t) => t switch
    {
        EmevdArgType.U8 => "u8",
        EmevdArgType.U16 => "u16",
        EmevdArgType.U32 => "u32",
        EmevdArgType.S8 => "s8",
        EmevdArgType.S16 => "s16",
        EmevdArgType.S32 => "s32",
        _ => "f32"
    };

    /// <summary>Offsets with natural alignment, and the size padded to 4, as SoulsFormats PackArgs packs</summary>
    public static (List<int> offsets, int size) Offsets(IReadOnlyList<EmevdArgType> types)
    {
        var offsets = new List<int>(types.Count);
        var pos = 0;

        foreach (var t in types)
        {
            var size = SizeOf(t);
            pos = (pos + size - 1) / size * size;
            offsets.Add(pos);
            pos += size;
        }

        return (offsets, (pos + 3) / 4 * 4);
    }

    public static EmevdDecoded Decode(EmevdInstrDef def, EMEVD.Instruction instr)
    {
        var result = new EmevdDecoded { Def = def };
        var data = instr.ArgData ?? Array.Empty<byte>();

        if (def == null)
        {
            result.Problem = "No layout";
            return result;
        }

        var types = def.Args.Select(a => a.Type).ToList();
        var defs = def.Args.ToList();
        List<EmevdArgType> chosen = null;

        if (def.RepeatLastArg && types.Count > 0)
        {
            chosen = types.ToList();
            while (Offsets(chosen).size < data.Length)
            {
                chosen.Add(types[^1]);
                defs.Add(def.Args[^1]);
            }
        }
        else
        {
            var required = def.Args.TakeWhile(a => !a.Optional).Count();

            for (var n = types.Count; n >= required && chosen == null; n--)
            {
                var prefix = types.Take(n).ToList();
                if (Offsets(prefix).size == data.Length)
                    chosen = prefix;
            }
        }

        if (chosen == null || Offsets(chosen).size != data.Length)
        {
            result.Problem = $"{data.Length} bytes fit no form of the layout";
            return result;
        }

        var (offsets, _) = Offsets(chosen);

        for (var i = 0; i < chosen.Count; i++)
        {
            var off = offsets[i];
            var arg = new EmevdArg
            {
                Def = defs[i],
                Offset = off,
                Size = SizeOf(chosen[i]),
                Type = chosen[i],
                Repeat = Math.Max(0, i - (def.Args.Count - 1)),
            };

            var span = data.AsSpan(off);
            switch (chosen[i])
            {
                case EmevdArgType.U8: arg.Int = span[0]; break;
                case EmevdArgType.S8: arg.Int = (sbyte)span[0]; break;
                case EmevdArgType.U16: arg.Int = BinaryPrimitives.ReadUInt16LittleEndian(span); break;
                case EmevdArgType.S16: arg.Int = BinaryPrimitives.ReadInt16LittleEndian(span); break;
                case EmevdArgType.U32: arg.Int = BinaryPrimitives.ReadUInt32LittleEndian(span); break;
                case EmevdArgType.S32: arg.Int = BinaryPrimitives.ReadInt32LittleEndian(span); break;
                case EmevdArgType.F32:
                    arg.Float = BinaryPrimitives.ReadSingleLittleEndian(span);
                    arg.Int = BinaryPrimitives.ReadInt32LittleEndian(span);
                    break;
            }

            result.Args.Add(arg);
        }

        result.Fits = true;
        return result;
    }
}
