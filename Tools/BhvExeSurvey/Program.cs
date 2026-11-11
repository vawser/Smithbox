using Iced.Intel;

// BhvExeSurvey <exe> <mode> [args]
//   strings <needle>      strings containing the text, with code references
//   classes <needle>      RTTI classes containing the text, with vtable functions
//   xref <va>             instructions referencing the address
//   fn <va> [max]         disassemble a function (Arxan stubs expanded inline when seen)
//   callers <va>          call/jmp sites targeting the function
var exe = args[0];
Exe.Load(exe);
var mode = args[1];
ulong Hex(string s) => Convert.ToUInt64(s.Replace("0x", ""), 16);

switch (mode)
{
    case "strings":
    {
        var hits = Exe.FindStrings(args[2]);
        var set = hits.Select(h => h.VA).ToHashSet();
        var refs = new Dictionary<ulong, List<ulong>>();
        foreach (var sec in new[] { 0, 1 })
            foreach (var ins in SafeSweep(sec))
            {
                var t = Exe.RefTarget(ins);
                if (t != 0 && set.Contains(t)) { if (!refs.TryGetValue(t, out var l)) refs[t] = l = new(); l.Add(ins.IP); }
            }
        foreach (var (va, text) in hits)
            Console.WriteLine($"{va:X} \"{text}\"  refs: {string.Join(" ", refs.GetValueOrDefault(va, new()).Select(x => x.ToString("X")))}");
        break;
    }
    case "classes":
    {
        Exe.LoadRtti();
        foreach (var (vt, cls) in Exe.VtableClass.Where(kv => kv.Value.Contains(args[2], StringComparison.OrdinalIgnoreCase)).OrderBy(kv => kv.Value))
        {
            var fns = new List<string>();
            for (int i = 0; i < 40; i++) { var f = Exe.ReadU64(vt + (ulong)i * 8); if (!Exe.IsCode(f)) break; fns.Add(f.ToString("X")); }
            Console.WriteLine($"{vt:X} {cls}: {string.Join(" ", fns)}");
        }
        break;
    }
    case "xref":
    {
        var target = Hex(args[2]);
        foreach (var sec in new[] { 0, 1 })
            foreach (var ins in SafeSweep(sec))
                if (Exe.RefTarget(ins) == target || ins.Immediate64OrZero() == target || (!ins.IsIPRelativeMemoryOperand && ins.MemoryDisplacement64 != 0 && (ins.MemoryDisplacement64 == target || unchecked((ulong)-(long)ins.MemoryDisplacement64) == target))) Console.WriteLine(Exe.Format(ins));
        break;
    }
    case "callers":
    {
        var target = Hex(args[2]);
        foreach (var sec in new[] { 0, 1 })
            foreach (var ins in SafeSweep(sec))
                if ((ins.Mnemonic == Mnemonic.Call || ins.Mnemonic == Mnemonic.Jmp) && ins.Op0Kind == OpKind.NearBranch64 && ins.NearBranchTarget == target)
                    Console.WriteLine(Exe.Format(ins));
        break;
    }
    case "strrange":
    {
        ulong a = Hex(args[2]), b = Hex(args[3]);
        long fo = Exe.FileOffset(a), end = Exe.FileOffset(b);
        while (fo < end)
        {
            var va = Exe.VaOfFile(fo);
            var s = Exe.ReadString(va);
            if (s != null) { Console.WriteLine($"{va:X} {s}"); fo += s.StartsWith("u:") ? (s.Length - 2) * 2 + 2 : s.Length + 1; }
            else fo++;
        }
        break;
    }
    case "range":
    {
        Exe.LoadRtti();
        ulong a = Hex(args[2]); int n = int.Parse(args[3]);
        var fo = Exe.FileOffset(a);
        var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, n * 16));
        dec.IP = a;
        for (int i = 0; i < n; i++) { dec.Decode(out var ins); Console.WriteLine(Exe.Format(ins)); }
        break;
    }
    case "hashsearch":
    {
        var map = new Dictionary<ulong, string>();
        foreach (var name in args.Skip(2))
            foreach (var p in new ulong[] { 37, 133 })
            {
                ulong h = 0; foreach (char ch in name.ToLowerInvariant().Replace('\\', '/')) h = h * p + ch;
                map.TryAdd(h, $"{name} x{p} 64"); map.TryAdd(h & 0xFFFFFFFF, $"{name} x{p} 32");
            }
        foreach (var sec in new[] { 0, 1 })
            foreach (var ins in SafeSweep(sec))
            {
                var im = ins.Immediate64OrZero();
                if (im != 0 && map.TryGetValue(im, out var n)) Console.WriteLine($"{Exe.Format(ins)}   <= {n}");
            }
        // also data sections
        foreach (var s in Exe.Sections.Where(s => s.Name != ".text"))
            for (long fo = s.Raw; fo + 8 <= s.Raw + s.RawSize; fo += 4)
            {
                var v = BitConverter.ToUInt64(Exe.Image, (int)fo);
                if (map.TryGetValue(v, out var n) && v > 0xFFFFFFFF) Console.WriteLine($"data {Exe.VaOfFile(fo):X} <= {n}");
            }
        break;
    }
    case "sigscan":
    {
        // score each pdata chunk by distinct BHV header displacements read off one base register
        var want = new HashSet<long> { 0x20, 0x24, 0x28, 0x2C, 0x30, 0x34, 0x36, 0x38, 0x3C };
        var results = new List<(int Score, ulong Begin, string Detail)>();
        foreach (var (begin, end) in Pdata.All)
        {
            var fo = Exe.FileOffset(begin); if (fo <= 0 || end - begin > 0x10000) continue;
            var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, (int)(end - begin)));
            dec.IP = begin;
            var byBase = new Dictionary<Register, HashSet<long>>(); bool word36 = false, has0A0 = false;
            while (dec.IP < end)
            {
                dec.Decode(out var ins);
                for (int i = 0; i < ins.OpCount; i++)
                {
                    if (ins.GetOpKind(i) == OpKind.Memory && ins.MemoryIndex == Register.None && ins.MemoryBase != Register.RSP && ins.MemoryBase != Register.RBP && ins.MemoryBase != Register.RIP)
                    {
                        long d = (long)ins.MemoryDisplacement64;
                        if (want.Contains(d)) { if (!byBase.TryGetValue(ins.MemoryBase, out var s)) byBase[ins.MemoryBase] = s = new(); s.Add(d); if ((d == 0x36 || d == 0x34) && ins.MemorySize == MemorySize.UInt16 || ins.MemorySize == MemorySize.Int16) word36 |= d == 0x36 || d == 0x34; }
                    }
                }
                if (ins.Immediate64OrZero() == 0xA0) has0A0 = true;
            }
            if (byBase.Count == 0) continue;
            var best = byBase.OrderByDescending(kv => kv.Value.Count).First();
            int score = best.Value.Count + (word36 ? 3 : 0) + (has0A0 ? 2 : 0);
            if (score >= 8) results.Add((score, begin, $"{best.Key} {string.Join(",", best.Value.OrderBy(x => x).Select(x => x.ToString("X")))} w36={word36} A0={has0A0}"));
        }
        foreach (var r in results.OrderByDescending(r => r.Score).Take(40)) Console.WriteLine($"{r.Score,3} {r.Begin:X} {r.Detail}");
        break;
    }
    case "cmpscan":
    {
        // chunks containing every given cmp [reg+disp],imm  (args: disp:imm ...)
        var pats = args.Skip(2).Select(a => a.Split(':')).Select(p => (D: Convert.ToInt64(p[0], 16), I: Convert.ToUInt64(p[1], 16))).ToList();
        foreach (var (begin, end) in Pdata.All)
        {
            var fo = Exe.FileOffset(begin); if (fo <= 0 || end - begin > 0x20000) continue;
            var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, (int)(end - begin)));
            dec.IP = begin; var hit = new HashSet<int>(); var lines = new List<string>();
            while (dec.IP < end)
            {
                dec.Decode(out var ins);
                if (ins.Mnemonic != Mnemonic.Cmp || ins.Op0Kind != OpKind.Memory) continue;
                for (int k = 0; k < pats.Count; k++)
                    if ((long)ins.MemoryDisplacement64 == pats[k].D && ins.Immediate64OrZero() == pats[k].I && !(ins.Immediate64OrZero() == 0 && ins.Op1Kind == OpKind.Register)) { hit.Add(k); lines.Add(Exe.Format(ins)); }
            }
            if (hit.Count == pats.Count) { Console.WriteLine($"{begin:X}-{end:X}"); foreach (var l in lines.Take(6)) Console.WriteLine("    " + l); }
        }
        break;
    }
    case "stridescan":
    {
        var res = new List<(int, ulong, string)>();
        foreach (var (begin, end) in Pdata.All)
        {
            var fo = Exe.FileOffset(begin); if (fo <= 0 || end - begin > 0x20000) continue;
            var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, (int)(end - begin)));
            dec.IP = begin; int lea20 = 0, strideA0 = 0; Register pending = Register.None; int since = 99;
            while (dec.IP < end)
            {
                dec.Decode(out var ins); since++;
                if (ins.Mnemonic == Mnemonic.Lea && ins.MemoryIndex != Register.None && ins.MemoryIndexScale == 1 && ins.MemoryDisplacement64 == 0x20) lea20++;
                if (ins.Mnemonic == Mnemonic.Lea && ins.MemoryIndex != Register.None && ins.MemoryIndex == ins.MemoryBase && ins.MemoryIndexScale == 4 && ins.MemoryDisplacement64 == 0) { pending = ins.Op0Register.GetFullRegister(); since = 0; }
                if (ins.Mnemonic == Mnemonic.Shl && since < 4 && ins.Op0Kind == OpKind.Register && ins.Op0Register.GetFullRegister() == pending && ins.Immediate64OrZero() == 5) strideA0++;
                if (ins.Mnemonic == Mnemonic.Imul && ins.OpCount == 3 && ins.GetImmediate(2) == 0xA0) strideA0++;
            }
            if (lea20 >= 2 && strideA0 >= 1) res.Add((lea20 + 3 * strideA0, begin, $"lea20={lea20} strideA0={strideA0} size={end - begin:X}"));
        }
        foreach (var r in res.OrderByDescending(r => r.Item1).Take(40)) Console.WriteLine($"{r.Item1,3} {r.Item2:X} {r.Item3}");
        break;
    }
    case "fnat":
    {
        Exe.LoadRtti();
        var va = Hex(args[2]);
        var f = Pdata.Containing(va);
        if (f == null) { Console.WriteLine("no pdata entry"); break; }
        // chained unwind entries split a function; widen to adjacent entries
        ulong begin = f.Value.Begin, end = f.Value.End;
        Console.WriteLine($"; function {begin:X}-{end:X}");
        var fo = Exe.FileOffset(begin);
        var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, (int)(end - begin)));
        dec.IP = begin;
        while (dec.IP < end) { dec.Decode(out var ins); Console.WriteLine((ins.IP <= va && va < ins.NextIP ? ">> " : "   ") + Exe.Format(ins)); }
        break;
    }
    case "fn":
    {
        Exe.LoadRtti();
        int max = args.Length > 3 ? int.Parse(args[3]) : 600;
        foreach (var ins in Exe.Function(Hex(args[2]), max))
        {
            Console.WriteLine(Exe.Format(ins));
            if (ins.Mnemonic == Mnemonic.Jmp && ins.Op0Kind == OpKind.NearBranch64 && Arxan.InStubs(ins.NearBranchTarget))
                foreach (var r in Arxan.Expand(ins)) Console.WriteLine("      | " + Exe.Format(r));
        }
        break;
    }
}

static IEnumerable<Instruction> SafeSweep(int sec)
{
    var count = Exe.Sections.Count(s => s.Name == ".text");
    return sec < count ? Exe.Sweep(sec) : Enumerable.Empty<Instruction>();
}

static class InsExt
{
    public static ulong Immediate64OrZero(this Instruction ins)
    {
        for (int i = 0; i < ins.OpCount; i++)
            if (ins.GetOpKind(i) is OpKind.Immediate64 or OpKind.Immediate32to64 or OpKind.Immediate32 or OpKind.Immediate8to32 or OpKind.Immediate8to64 or OpKind.Immediate16 or OpKind.Immediate8) return ins.GetImmediate(i);
        return 0;
    }
}
