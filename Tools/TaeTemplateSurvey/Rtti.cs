using Iced.Intel;
using System.Text;

/// <summary>
/// Reads the MSVC RTTI of the exe: each vtable's class, and the class each virtual function belongs to.
/// Also works out which module class sits at each offset of a character's module bag (chr+190h), from the
/// code that builds it: a vtable written into a new object, and that object then stored into the bag.
/// </summary>
public static class Rtti
{
    public static Dictionary<ulong, string> VtableClass = new();          // vtable VA -> class
    public static Dictionary<ulong, (string Class, int Index)> VirtualOf = new(); // function -> first class & slot

    public static void Load()
    {
        var rdata = ExeScan.Sections.First(s => s.Name == ".rdata");
        var img = ExeScan.Image;
        var baseVA = ExeScan.ImageBase;

        for (long fo = rdata.Raw; fo + 16 < rdata.Raw + rdata.RawSize; fo += 8)
        {
            var colVA = BitConverter.ToUInt64(img, (int)fo);
            if (colVA < baseVA || colVA > baseVA + 0x6000000) continue;
            var col = ExeScan.FileOffset(colVA);
            if (col <= 0 || col + 24 > img.Length) continue;
            if (BitConverter.ToUInt32(img, (int)col) != 1) continue;
            var selfRva = BitConverter.ToUInt32(img, (int)col + 20);
            if (baseVA + selfRva != colVA) continue;
            if (BitConverter.ToUInt32(img, (int)col + 4) != 0) continue; // only the primary vtable

            var td = ExeScan.FileOffset(baseVA + BitConverter.ToUInt32(img, (int)col + 12));
            if (td <= 0) continue;
            var name = ReadCString(img, td + 16);
            if (name == null || !name.StartsWith(".?AV")) continue;

            var vt = ExeScan.ImageBase + (ulong)(fo - rdata.Raw + rdata.VA) + 8;
            var cls = Demangle(name);
            VtableClass[vt] = cls;

            for (var i = 0; i < 400; i++)
            {
                var f = ExeScan.FileOffset(vt + (ulong)i * 8);
                if (f <= 0) break;
                var fn = BitConverter.ToUInt64(img, (int)f);
                if (!ExeScan.IsCode(fn)) break;
                if (i > 0 && VtableClass.ContainsKey(vt + (ulong)i * 8)) break;
                VirtualOf.TryAdd(fn, (cls, i));
            }
        }
    }

    static string ReadCString(byte[] img, long fo)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < 300 && fo + i < img.Length; i++)
        {
            var c = img[fo + i];
            if (c == 0) return sb.ToString();
            sb.Append((char)c);
        }
        return null;
    }

    // ".?AVCSChrPhysicsModule@CS@@" -> "CS::CSChrPhysicsModule"
    static string Demangle(string n)
    {
        var body = n.Substring(4).TrimEnd('@');
        var parts = body.Split('@', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 1 && !body.Contains('?') ? string.Join("::", parts.Reverse()) : body;
    }

    /// <summary>
    /// Finds stores of a new module into a bag: the object a module vtable was written into, then stored at
    /// [bag+disp]. Constructors that aren't inlined are followed through their return value.
    /// </summary>
    public static void ModuleBag()
    {
        var moduleVtables = VtableClass.Where(v => v.Value.Contains("CSChr") && v.Value.EndsWith("Module")).ToDictionary(v => v.Key, v => v.Value);
        Console.WriteLine($"{VtableClass.Count} vtables, {moduleVtables.Count} chr module vtables");

        // constructors: functions that write a module vtable into rcx
        var ctorOf = new Dictionary<ulong, string>();
        var text = ExeScan.Sections.First(s => s.Name == ".text");
        var start = ExeScan.ImageBase + text.VA;
        var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(ExeScan.Image, (int)text.Raw, (int)text.VSize));
        decoder.IP = start;
        var all = new List<Instruction>();
        while (decoder.IP < start + text.VSize)
        {
            decoder.Decode(out var ins);
            all.Add(ins);
        }

        var functions = ParamTraceFunctions();

        for (var i = 0; i < all.Count; i++)
        {
            var ins = all[i];
            if (ins.Mnemonic != Mnemonic.Lea || !ins.IsIPRelativeMemoryOperand) continue;
            if (!moduleVtables.TryGetValue(ins.IPRelativeMemoryAddress, out var cls)) continue;
            var fn = functions(ins.IP);
            ctorOf.TryAdd(fn, cls);
        }

        Console.WriteLine($"{ctorOf.Count} functions write a chr module vtable");

        // the functions that call many of them build the bag
        var callers = new Dictionary<ulong, List<(ulong At, string Cls)>>();
        foreach (var ins in all)
        {
            if (ins.Mnemonic != Mnemonic.Call || ins.Op0Kind != OpKind.NearBranch64) continue;
            if (!ctorOf.TryGetValue(ins.NearBranchTarget, out var c)) continue;
            var f = functions(ins.IP);
            if (!callers.TryGetValue(f, out var l)) callers[f] = l = new();
            l.Add((ins.IP, c));
        }
        foreach (var (f, l) in callers.Where(c => c.Value.Select(x => x.Cls).Distinct().Count() >= 5).OrderByDescending(c => c.Value.Count))
        {
            Console.WriteLine($"builder 0x{f:X}: {l.Count} calls");
            foreach (var (at, c) in l) Console.WriteLine($"   0x{at:X} {c}");
        }

        // calls to a constructor, then the result stored into [reg+disp]
        var seen = new Dictionary<(string, ulong), int>();
        for (var i = 0; i < all.Count; i++)
        {
            var ins = all[i];
            if (ins.Mnemonic != Mnemonic.Call || ins.Op0Kind != OpKind.NearBranch64) continue;
            if (!ctorOf.TryGetValue(ins.NearBranchTarget, out var cls)) continue;

            var holder = Register.RAX;
            for (var k = i + 1; k < Math.Min(all.Count, i + 8); k++)
            {
                var n = all[k];
                if (n.Mnemonic == Mnemonic.Mov && n.Op0Kind == OpKind.Register && n.Op1Kind == OpKind.Register && n.Op1Register == holder)
                {
                    holder = n.Op0Register;
                    continue;
                }
                if (n.Mnemonic == Mnemonic.Mov && n.Op0Kind == OpKind.Memory && n.Op1Kind == OpKind.Register && n.Op1Register == holder
                    && n.MemoryBase != Register.RSP && n.MemoryIndex == Register.None && n.MemoryDisplacement64 <= 0x200)
                {
                    var key = (cls, n.MemoryDisplacement64);
                    seen[key] = seen.TryGetValue(key, out var c) ? c + 1 : 1;
                    break;
                }
                if (n.FlowControl != FlowControl.Next) break;
            }
        }

        foreach (var g in seen.GroupBy(s => s.Key.Item1).OrderBy(g => g.OrderByDescending(x => x.Value).First().Key.Item2))
            Console.WriteLine($"  {g.Key,-48} {string.Join(" ", g.OrderByDescending(x => x.Value).Select(x => $"+{x.Key.Item2:X}x{x.Value}"))}");
    }

    /// <summary>
    /// Votes for the class at each offset of the module bag: a module loaded from [bag+X] and passed as rcx
    /// to a function that is a virtual of a module class, or whose vtable is compared or called through
    /// </summary>
    public static Dictionary<ulong, string> VoteModuleBag(bool print)
    {
        var text = ExeScan.Sections.First(s => s.Name == ".text");
        var start = ExeScan.ImageBase + text.VA;
        var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(ExeScan.Image, (int)text.Raw, (int)text.VSize));
        decoder.IP = start;
        var ring = new Instruction[24];
        var n = 0;
        var votes = new Dictionary<ulong, Dictionary<string, int>>();

        while (decoder.IP < start + text.VSize)
        {
            decoder.Decode(out var ins);
            ring[n++ % ring.Length] = ins;
            if (ins.Mnemonic != Mnemonic.Call || ins.Op0Kind != OpKind.NearBranch64) continue;
            if (!VirtualOf.TryGetValue(ins.NearBranchTarget, out var v) || !v.Class.Contains("Module")) continue;

            // walk back: rcx <- [bag + X] <- [chr + 190h]
            Register want = Register.RCX;
            ulong? disp = null;
            for (var k = 1; k < ring.Length - 1 && k < n; k++)
            {
                var p = ring[(n - 1 - k) % ring.Length];
                if (p.Op0Kind != OpKind.Register || p.Op0Register.GetFullRegister() != want) continue;
                if (p.Mnemonic == Mnemonic.Mov && p.Op1Kind == OpKind.Register) { want = p.Op1Register.GetFullRegister(); continue; }
                if (p.Mnemonic == Mnemonic.Mov && p.Op1Kind == OpKind.Memory && p.MemoryIndex == Register.None && p.MemorySize.GetSize() == 8)
                {
                    if (disp == null)
                    {
                        disp = p.MemoryDisplacement64;
                        want = p.MemoryBase.GetFullRegister();
                        continue;
                    }
                    if (p.MemoryDisplacement64 == 0x190 && disp <= 0x200)
                    {
                        if (!votes.TryGetValue(disp.Value, out var d)) votes[disp.Value] = d = new();
                        d[v.Class] = d.TryGetValue(v.Class, out var c) ? c + 1 : 1;
                    }
                }
                break;
            }
        }

        var result = new Dictionary<ulong, string>();
        foreach (var (off, d) in votes.OrderBy(v => v.Key))
        {
            var best = d.OrderByDescending(x => x.Value).First();
            result[off] = best.Key;
            if (print)
                Console.WriteLine($"  +{off:X3} {best.Key,-44} ({string.Join(", ", d.OrderByDescending(x => x.Value).Take(3).Select(x => $"{x.Key.Replace("CS::", "")}x{x.Value}"))})");
        }
        return result;
    }

    /// <summary>
    /// The bag builder calls setSlot(bag, slot, module) after creating each module. The class is the module
    /// vtable written somewhere in the creating function or the functions it calls.
    /// </summary>
    public static Dictionary<ulong, string> BagSlots(ulong builder, ulong setSlot, ulong[] skip)
    {
        var result = new Dictionary<ulong, string>();
        var fo = ExeScan.FileOffset(builder);
        var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(ExeScan.Image, (int)fo, 0x2000));
        decoder.IP = builder;
        ulong lastCreate = 0;
        long slot = -1;

        for (var i = 0; i < 1200; i++)
        {
            decoder.Decode(out var ins);
            if (ins.Mnemonic == Mnemonic.Mov && ins.Op0Kind == OpKind.Register && ins.Op0Register == Register.EDX && ins.Op1Kind == OpKind.Immediate32) slot = ins.Immediate32;
            if (ins.Mnemonic == Mnemonic.Xor && ins.Op0Kind == OpKind.Register && ins.Op0Register == Register.EDX && ins.Op1Register == Register.EDX) slot = 0;
            if (ins.Mnemonic != Mnemonic.Call || ins.Op0Kind != OpKind.NearBranch64) continue;
            var t = ins.NearBranchTarget;
            if (t == setSlot)
            {
                var cls = FindVtable(lastCreate, 3, new HashSet<ulong>()) ?? $"? (create 0x{lastCreate:X})";
                result[(ulong)slot * 8] = cls;
                continue;
            }
            if (!skip.Contains(t)) lastCreate = t;
        }

        return result;
    }

    static string FindVtable(ulong fn, int depth, HashSet<ulong> seen)
    {
        if (depth < 0 || !seen.Add(fn)) return null;
        var fo = ExeScan.FileOffset(fn);
        if (fo <= 0) return null;
        var decoder = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(ExeScan.Image, (int)fo, 0x800));
        decoder.IP = fn;
        var calls = new List<ulong>();
        for (var i = 0; i < 300; i++)
        {
            decoder.Decode(out var ins);
            if (ins.Mnemonic == Mnemonic.Lea && ins.IsIPRelativeMemoryOperand && VtableClass.TryGetValue(ins.IPRelativeMemoryAddress, out var c) && c.EndsWith("Module"))
                return c;
            if (ins.Mnemonic == Mnemonic.Call && ins.Op0Kind == OpKind.NearBranch64) calls.Add(ins.NearBranchTarget);
            if (ins.Mnemonic == Mnemonic.Jmp && ins.Op0Kind == OpKind.NearBranch64 && (ins.NearBranchTarget < fn || ins.NearBranchTarget > fn + 0x800)) { calls.Add(ins.NearBranchTarget); break; }
            if (ins.FlowControl == FlowControl.Return || ins.Mnemonic == Mnemonic.Int3) break;
        }
        foreach (var c in calls)
            if (FindVtable(c, depth - 1, seen) is { } r) return r;
        return null;
    }

    static Func<ulong, ulong> ParamTraceFunctions()
    {
        var pdata = ExeScan.Sections.First(s => s.Name == ".pdata");
        var list = new List<(uint, uint)>();
        for (var o = pdata.Raw; o + 12 <= pdata.Raw + pdata.VSize; o += 12)
        {
            var b = BitConverter.ToUInt32(ExeScan.Image, (int)o);
            if (b == 0) break;
            list.Add((b, BitConverter.ToUInt32(ExeScan.Image, (int)o + 4)));
        }
        list.Sort();
        return va =>
        {
            var rva = (uint)(va - ExeScan.ImageBase);
            int lo = 0, hi = list.Count - 1;
            while (lo <= hi) { var m = (lo + hi) / 2; if (list[m].Item1 <= rva) lo = m + 1; else hi = m - 1; }
            return hi >= 0 ? ExeScan.ImageBase + list[hi].Item1 : va;
        };
    }
}
