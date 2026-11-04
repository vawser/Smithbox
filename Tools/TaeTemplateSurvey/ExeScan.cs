using Iced.Intel;
using System.Text;

/// <summary>
/// Finds the switch statements of an exe that dispatch on the time act event types. MSVC builds a switch as a
/// bounds check and a table of offsets from the image base, sometimes behind a byte table that maps cases to
/// handlers. A switch whose live cases match the event types the game's time acts use is a dispatcher.
/// </summary>
public static class ExeScan
{
    public sealed class Section
    {
        public string Name;
        public uint VA, VSize, Raw, RawSize;
    }

    public sealed class Switch
    {
        public ulong Address;
        public long Bias;          // the value subtracted before the bounds check
        public uint Max;           // the largest index the bounds check lets through
        public ulong DefaultTarget;
        public Dictionary<long, ulong> Targets = new(); // case value -> handler
        public string Register;
    }

    public static byte[] Image;
    public static ulong ImageBase;
    public static List<Section> Sections = new();

    public static void Load(string exe)
    {
        Image = File.ReadAllBytes(exe);
        var pe = BitConverter.ToInt32(Image, 0x3C);
        var sectionCount = BitConverter.ToUInt16(Image, pe + 6);
        var optSize = BitConverter.ToUInt16(Image, pe + 20);
        var opt = pe + 24;
        ImageBase = BitConverter.ToUInt64(Image, opt + 24);
        var sec = opt + optSize;

        for (var i = 0; i < sectionCount; i++)
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

    public static bool IsCode(ulong va)
    {
        var rva = va - ImageBase;
        return Sections.Any(s => s.Name == ".text" && rva >= s.VA && rva < s.VA + s.VSize);
    }

    public static int ReadI32(ulong va) => BitConverter.ToInt32(Image, (int)FileOffset(va));
    public static byte ReadU8(ulong va) => Image[FileOffset(va)];

    /// <summary>
    /// Lists the switches between two addresses, with every case and its handler
    /// </summary>
    public static void SwitchesIn(string exe, ulong lo, ulong hi)
    {
        Load(exe);

        foreach (var sw in FindSwitches().Where(s => s.Address >= lo && s.Address < hi))
        {
            Console.WriteLine($"0x{sw.Address:X} reg={sw.Register} range={sw.Bias}..{sw.Bias + sw.Max} default=0x{sw.DefaultTarget:X}");
            foreach (var g in sw.Targets.Where(t => t.Value != sw.DefaultTarget).GroupBy(t => t.Value).OrderBy(g => g.Key))
                Console.WriteLine($"   0x{g.Key:X}: {string.Join(",", g.Select(x => x.Key))}");
        }
    }

    public static void Disasm(string exe, ulong start, int count)
    {
        Load(exe);
        Rtti.Load();
        var formatter = new MasmFormatter();
        formatter.Options.FirstOperandCharIndex = 8;
        var output = new StringOutput();
        var reader = new ByteArrayCodeReader(Image, (int)FileOffset(start), 0x10000);
        var decoder = Iced.Intel.Decoder.Create(64, reader);
        decoder.IP = start;

        for (var i = 0; i < count; i++)
        {
            decoder.Decode(out var ins);
            formatter.Format(ins, output);
            var extra = "";

            // floats and strings the instruction points at
            if (ins.IsIPRelativeMemoryOperand)
            {
                var a = ins.IPRelativeMemoryAddress;
                var fo = FileOffset(a);
                if (fo > 0 && fo + 8 < Image.Length)
                {
                    var f = BitConverter.ToSingle(Image, (int)fo);
                    var s = ReadString(a);
                    if (Rtti.VtableClass.TryGetValue(a, out var vcls)) s = "vtable " + vcls;
                    extra = s != null ? $"  ; \"{s}\"" : $"  ; [{a:X}] f={f:0.####} i={BitConverter.ToInt32(Image, (int)fo)}";
                }
            }

            Console.WriteLine($"{ins.IP:X}  {output.ToStringAndReset()}{extra}");
        }
    }

    public static string ReadString(ulong va)
    {
        var fo = FileOffset(va);
        if (fo <= 0) return null;
        var sb = new StringBuilder();

        // ASCII
        for (var i = 0; i < 80 && fo + i < Image.Length; i++)
        {
            var c = Image[fo + i];
            if (c == 0) break;
            if (c < 0x20 || c > 0x7e) { sb.Clear(); break; }
            sb.Append((char)c);
        }
        if (sb.Length >= 4) return sb.ToString();

        // UTF-16
        sb.Clear();
        for (var i = 0; i < 160 && fo + i + 1 < Image.Length; i += 2)
        {
            var c = BitConverter.ToUInt16(Image, (int)fo + i);
            if (c == 0) break;
            if (c < 0x20 || c > 0x7e) { sb.Clear(); break; }
            sb.Append((char)c);
        }
        return sb.Length >= 3 ? "u:" + sb : null;
    }

    public static void Run(string exe, string outFile, string cacheFile)
    {
        Load(exe);

        foreach (var s in Sections)
            Console.WriteLine($"{s.Name,-8} va=0x{s.VA:X} vsize=0x{s.VSize:X} raw=0x{s.Raw:X} rawsize=0x{s.RawSize:X}");

        var switches = FindSwitches();
        Console.WriteLine($"{switches.Count} switches");

        var used = cacheFile != null ? Cache.Read(cacheFile).GroupBy(e => e.Type).ToDictionary(g => g.Key, g => g.Count()) : new();
        var sb = new StringBuilder();

        // A dispatcher's live cases are event types, so score how many of its cases the game uses
        var scored = switches
            .Select(sw =>
            {
                var live = sw.Targets.Where(t => t.Value != sw.DefaultTarget).Select(t => t.Key).ToList();
                var hits = live.Count(v => used.ContainsKey((int)v));
                return (sw, live, hits);
            })
            .Where(x => x.live.Count >= 4)
            .OrderByDescending(x => x.hits)
            .ThenByDescending(x => (double)x.hits / x.live.Count)
            .ToList();

        foreach (var (sw, live, hits) in scored.Take(60))
        {
            sb.AppendLine($"0x{sw.Address:X} reg={sw.Register} bias={sw.Bias} range={sw.Bias}..{sw.Bias + sw.Max} live={live.Count} usedHits={hits} handlers={sw.Targets.Values.Distinct().Count()}");
            sb.AppendLine($"   cases: {string.Join(",", live.OrderBy(v => v).Take(200))}");
        }

        File.WriteAllText(outFile, sb.ToString());
        Console.WriteLine(string.Join("\n", sb.ToString().Split('\n').Take(40)));
    }

    public static List<Switch> FindSwitches()
    {
        var text = Sections.First(s => s.Name == ".text");
        var result = new List<Switch>();
        var start = ImageBase + text.VA;
        var reader = new ByteArrayCodeReader(Image, (int)text.Raw, (int)Math.Min(text.RawSize, text.VSize));
        var decoder = Iced.Intel.Decoder.Create(64, reader);
        decoder.IP = start;
        var end = start + Math.Min(text.RawSize, text.VSize);
        var ring = new Instruction[16];
        var n = 0;

        while (decoder.IP < end)
        {
            decoder.Decode(out var ins);
            ring[n++ % ring.Length] = ins;

            if (ins.Mnemonic != Mnemonic.Jmp || ins.Op0Kind != OpKind.Register)
                continue;

            var sw = TryMatch(ring, n);
            if (sw != null)
                result.Add(sw);
        }

        return result;
    }

    /// <summary>
    /// Walks back from a jmp reg over the instructions that compute its target
    /// </summary>
    static Switch TryMatch(Instruction[] ring, int n)
    {
        Instruction Back(int k) => ring[(n - 1 - k + ring.Length * 4) % ring.Length];

        var jmp = Back(0);
        var target = jmp.Op0Register.GetFullRegister();
        Register baseReg = Register.None;
        ulong tableRva = 0, byteTableRva = 0;
        bool hasByteTable = false;
        Register indexReg = Register.None;
        int k = 1;

        // add target, base
        for (; k < 6; k++)
        {
            var i = Back(k);
            if (i.Mnemonic == Mnemonic.Add && i.Op0Kind == OpKind.Register && i.Op0Register.GetFullRegister() == target && i.Op1Kind == OpKind.Register)
            {
                baseReg = i.Op1Register.GetFullRegister();
                break;
            }
        }
        if (baseReg == Register.None) return null;

        // mov target32, [base + index*4 + table]
        for (k++; k < 9; k++)
        {
            var i = Back(k);
            if (i.Mnemonic == Mnemonic.Mov && i.Op0Kind == OpKind.Register && i.Op0Register.GetFullRegister() == target && i.Op1Kind == OpKind.Memory
                && i.MemoryBase.GetFullRegister() == baseReg && i.MemoryIndexScale == 4)
            {
                tableRva = i.MemoryDisplacement64;
                indexReg = i.MemoryIndex.GetFullRegister();
                break;
            }
        }
        if (tableRva == 0) return null;

        // optional movzx index, byte [base + index + byteTable]
        int kk = k;
        for (k++; k < 12; k++)
        {
            var i = Back(k);
            if (i.Op0Kind == OpKind.Register && i.Op0Register.GetFullRegister() == indexReg)
            {
                if (i.Mnemonic == Mnemonic.Movzx && i.Op1Kind == OpKind.Memory && i.MemoryBase.GetFullRegister() == baseReg && i.MemoryIndexScale == 1)
                {
                    hasByteTable = true;
                    byteTableRva = i.MemoryDisplacement64;
                    indexReg = i.MemoryIndex.GetFullRegister();
                    kk = k;
                }
                break;
            }
        }

        // lea base, [imagebase] - it may come before or after the bounds check
        // cmp index, max ; ja default
        uint max = 0;
        ulong def = 0;
        bool found = false;
        long bias = 0;
        Register valueReg = indexReg;

        for (k = kk + 1; k < 16; k++)
        {
            var i = Back(k);
            if (i.Mnemonic == Mnemonic.Ja && i.Op0Kind == OpKind.NearBranch64)
            {
                var c = Back(k + 1);
                if (c.Mnemonic == Mnemonic.Cmp && c.Op0Kind == OpKind.Register && c.Op0Register.GetFullRegister() == indexReg
                    && (c.Op1Kind == OpKind.Immediate32 || c.Op1Kind == OpKind.Immediate8to32 || c.Op1Kind == OpKind.Immediate8))
                {
                    max = c.Immediate32;
                    def = i.NearBranchTarget;
                    found = true;

                    // a lea index, [value - bias] or add/sub index, imm before it
                    var b = Back(k + 2);
                    if (b.Mnemonic == Mnemonic.Lea && b.Op0Register.GetFullRegister() == indexReg && b.MemoryIndex == Register.None)
                    {
                        bias = -(long)(int)b.MemoryDisplacement64;
                        valueReg = b.MemoryBase.GetFullRegister();
                    }
                    else if (b.Mnemonic == Mnemonic.Add && b.Op0Kind == OpKind.Register && b.Op0Register.GetFullRegister() == indexReg && b.Op1Kind != OpKind.Register)
                        bias = -(long)(int)b.Immediate32;
                    else if (b.Mnemonic == Mnemonic.Sub && b.Op0Kind == OpKind.Register && b.Op0Register.GetFullRegister() == indexReg && b.Op1Kind != OpKind.Register)
                        bias = (int)b.Immediate32;
                    else if (b.Mnemonic == Mnemonic.Dec && b.Op0Kind == OpKind.Register && b.Op0Register.GetFullRegister() == indexReg)
                        bias = 1;
                    else if (b.Mnemonic == Mnemonic.Inc && b.Op0Kind == OpKind.Register && b.Op0Register.GetFullRegister() == indexReg)
                        bias = -1;
                }
                break;
            }
        }

        if (!found || max > 5000) return null;

        var sw = new Switch { Address = jmp.IP, Bias = bias, Max = max, DefaultTarget = def, Register = valueReg.ToString() };

        try
        {
            for (uint idx = 0; idx <= max; idx++)
            {
                uint slot = hasByteTable ? ReadU8(ImageBase + byteTableRva + idx) : idx;
                var off = ReadI32(ImageBase + tableRva + slot * 4);
                var t = ImageBase + (ulong)(uint)off;
                if (!IsCode(t)) return null;
                sw.Targets[bias + idx] = t;
            }
        }
        catch
        {
            return null;
        }

        return sw;
    }
}
