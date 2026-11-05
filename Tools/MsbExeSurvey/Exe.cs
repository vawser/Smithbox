using Iced.Intel;
using System.Text;

/// <summary>
/// The exe image: sections, address mapping, RTTI vtables, strings and a linear sweep over .text.
/// </summary>
public static class Exe
{
    public sealed class Section { public string Name; public uint VA, VSize, Raw, RawSize; }

    public static byte[] Image;
    public static ulong ImageBase;
    public static List<Section> Sections = new();
    public static Dictionary<ulong, string> VtableClass = new();
    public static Dictionary<ulong, List<(string Class, int Index)>> VirtualOf = new();

    public static void Load(string exe)
    {
        Image = File.ReadAllBytes(exe);
        var pe = BitConverter.ToInt32(Image, 0x3C);
        var count = BitConverter.ToUInt16(Image, pe + 6);
        var optSize = BitConverter.ToUInt16(Image, pe + 20);
        var opt = pe + 24;
        ImageBase = BitConverter.ToUInt64(Image, opt + 24);
        var sec = opt + optSize;
        for (var i = 0; i < count; i++)
        {
            var o = sec + i * 40;
            Sections.Add(new Section
            {
                Name = Encoding.ASCII.GetString(Image, o, 8).TrimEnd('\0'),
                VSize = BitConverter.ToUInt32(Image, o + 8),
                VA = BitConverter.ToUInt32(Image, o + 12),
                RawSize = BitConverter.ToUInt32(Image, o + 16),
                Raw = BitConverter.ToUInt32(Image, o + 20),
            });
        }
    }

    public static long FileOffset(ulong va)
    {
        var rva = va - ImageBase;
        foreach (var s in Sections)
            if (rva >= s.VA && rva < s.VA + Math.Max(s.VSize, s.RawSize))
                return rva - s.VA < s.RawSize ? (long)(s.Raw + (rva - s.VA)) : -1;
        return -1;
    }

    public static ulong VaOfFile(long fo)
    {
        foreach (var s in Sections)
            if (fo >= s.Raw && fo < s.Raw + s.RawSize)
                return ImageBase + s.VA + (ulong)(fo - s.Raw);
        return 0;
    }

    public static bool IsCode(ulong va)
    {
        var rva = va - ImageBase;
        return Sections.Any(s => s.Name == ".text" && rva >= s.VA && rva < s.VA + s.VSize);
    }

    public static ulong ReadU64(ulong va) { var f = FileOffset(va); return f <= 0 ? 0 : BitConverter.ToUInt64(Image, (int)f); }
    public static int ReadI32(ulong va) { var f = FileOffset(va); return f <= 0 ? 0 : BitConverter.ToInt32(Image, (int)f); }

    public static void LoadRtti()
    {
        var rdata = Sections.First(s => s.Name == ".rdata");
        for (long fo = rdata.Raw; fo + 16 < rdata.Raw + rdata.RawSize; fo += 8)
        {
            var colVA = BitConverter.ToUInt64(Image, (int)fo);
            if (colVA < ImageBase || colVA > ImageBase + 0x8000000) continue;
            var col = FileOffset(colVA);
            if (col <= 0 || col + 24 > Image.Length) continue;
            if (BitConverter.ToUInt32(Image, (int)col) != 1) continue;
            var selfRva = BitConverter.ToUInt32(Image, (int)col + 20);
            if (ImageBase + selfRva != colVA) continue;

            var td = FileOffset(ImageBase + BitConverter.ToUInt32(Image, (int)col + 12));
            if (td <= 0) continue;
            var name = ReadCString(td + 16);
            if (name == null || !name.StartsWith(".?AV")) continue;

            var vt = VaOfFile(fo) + 8;
            var cls = Demangle(name);
            if (BitConverter.ToUInt32(Image, (int)col + 4) != 0) cls += $"@{BitConverter.ToUInt32(Image, (int)col + 4):X}";
            VtableClass[vt] = cls;

            for (var i = 0; i < 400; i++)
            {
                var fn = ReadU64(vt + (ulong)i * 8);
                if (!IsCode(fn)) break;
                if (i > 0 && VtableClass.ContainsKey(vt + (ulong)i * 8)) break;
                if (!VirtualOf.TryGetValue(fn, out var l)) VirtualOf[fn] = l = new();
                l.Add((cls, i));
            }
        }
    }

    public static string ReadCString(long fo)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 300 && fo + i < Image.Length; i++)
        {
            var c = Image[fo + i];
            if (c == 0) return sb.ToString();
            sb.Append((char)c);
        }
        return null;
    }

    static string Demangle(string n)
    {
        n = n.Substring(4).TrimEnd('@');
        var parts = n.Split('@', StringSplitOptions.RemoveEmptyEntries);
        Array.Reverse(parts);
        return string.Join("::", parts);
    }

    public static string ReadString(ulong va)
    {
        var fo = FileOffset(va);
        if (fo <= 0) return null;
        var sb = new StringBuilder();
        for (var i = 0; i < 120 && fo + i < Image.Length; i++)
        {
            var c = Image[fo + i];
            if (c == 0) break;
            if (c < 0x20 || c > 0x7e) { sb.Clear(); break; }
            sb.Append((char)c);
        }
        if (sb.Length >= 4) return sb.ToString();
        sb.Clear();
        for (var i = 0; i < 240 && fo + i + 1 < Image.Length; i += 2)
        {
            var c = BitConverter.ToUInt16(Image, (int)fo + i);
            if (c == 0) break;
            if (c < 0x20 || c > 0x7e) { sb.Clear(); break; }
            sb.Append((char)c);
        }
        return sb.Length >= 3 ? "u:" + sb : null;
    }

    /// <summary>
    /// Every ASCII and UTF-16 string in the data sections containing the text
    /// </summary>
    public static List<(ulong VA, string Text)> FindStrings(string needle)
    {
        var result = new List<(ulong, string)>();
        foreach (var s in Sections.Where(s => s.Name != ".text"))
        {
            var a = Encoding.ASCII.GetBytes(needle);
            var u = Encoding.Unicode.GetBytes(needle);
            for (long fo = s.Raw; fo < s.Raw + s.RawSize - u.Length; fo++)
            {
                if (Match(fo, a))
                {
                    var st = fo; while (st > s.Raw && Image[st - 1] >= 0x20 && Image[st - 1] < 0x7f) st--;
                    result.Add((VaOfFile(st), ReadCString(st)));
                }
                else if (Match(fo, u))
                {
                    var st = fo; while (st > s.Raw + 1 && Image[st - 1] == 0 && Image[st - 2] >= 0x20 && Image[st - 2] < 0x7f) st -= 2;
                    result.Add((VaOfFile(st), ReadString(VaOfFile(st))));
                }
            }
        }
        return result.DistinctBy(x => x.Item1).ToList();
    }

    static bool Match(long fo, byte[] b)
    {
        for (var i = 0; i < b.Length; i++) if (Image[fo + i] != b[i]) return false;
        return true;
    }

    /// <summary>
    /// Linear sweep over .text
    /// </summary>
    public static IEnumerable<Instruction> Sweep(int section = 0)
    {
        var text = Sections.Where(s => s.Name == ".text").OrderBy(s => s.VA).ElementAt(section);
        var len = (int)Math.Min(text.RawSize, text.VSize);
        var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Image, (int)text.Raw, len));
        decoder.IP = ImageBase + text.VA;
        var end = decoder.IP + (ulong)len;
        while (decoder.IP < end)
        {
            decoder.Decode(out var ins);
            yield return ins;
        }
    }

    public static ulong RefTarget(in Instruction ins)
    {
        if (ins.IsIPRelativeMemoryOperand) return ins.IPRelativeMemoryAddress;
        if (ins.Op0Kind == OpKind.NearBranch64) return ins.NearBranchTarget;
        return 0;
    }

    public static IEnumerable<Instruction> Function(ulong start, int max = 4000)
    {
        var fo = FileOffset(start);
        var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Image, (int)fo, (int)Math.Min(0x20000, Image.Length - fo)));
        decoder.IP = start;
        ulong furthest = start;
        for (var i = 0; i < max; i++)
        {
            decoder.Decode(out var ins);
            yield return ins;
            if (ins.FlowControl == FlowControl.ConditionalBranch || ins.FlowControl == FlowControl.UnconditionalBranch)
                if (ins.Op0Kind == OpKind.NearBranch64 && ins.NearBranchTarget > furthest && ins.NearBranchTarget < start + 0x8000)
                    furthest = ins.NearBranchTarget;
            if ((ins.Mnemonic == Mnemonic.Ret || ins.Mnemonic == Mnemonic.Int3 || (ins.Mnemonic == Mnemonic.Jmp && ins.Op0Kind != OpKind.NearBranch64)
                 || (ins.Mnemonic == Mnemonic.Jmp && ins.NearBranchTarget < start)) && ins.NextIP > furthest)
                yield break;
        }
    }

    public static string Annotate(in Instruction ins)
    {
        var a = RefTarget(ins);
        if (a == 0) return "";
        if (ins.Op0Kind == OpKind.NearBranch64)
        {
            if (VirtualOf.TryGetValue(a, out var v)) return "  ; " + v[0].Class + "#" + v[0].Index;
            return "";
        }
        if (VtableClass.TryGetValue(a, out var c)) return "  ; vtable " + c;
        var s = ReadString(a);
        if (s != null) return $"  ; \"{s}\"";
        var fo = FileOffset(a);
        if (fo > 0 && fo + 8 < Image.Length)
        {
            var q = BitConverter.ToUInt64(Image, (int)fo);
            var extra = IsCode(q) && VirtualOf.TryGetValue(q, out var vv) ? $" -> {vv[0].Class}#{vv[0].Index}" : "";
            return $"  ; [{a:X}] f={BitConverter.ToSingle(Image, (int)fo):0.####} i={BitConverter.ToInt32(Image, (int)fo)}{extra}";
        }
        return "";
    }

    static readonly MasmFormatter Fmt = new() { Options = { FirstOperandCharIndex = 8 } };
    public static string Format(in Instruction ins)
    {
        var o = new StringOutput();
        Fmt.Format(ins, o);
        return $"{ins.IP:X}  {o.ToStringAndReset()}{Annotate(ins)}";
    }
}
