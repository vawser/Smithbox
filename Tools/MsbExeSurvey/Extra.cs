using Iced.Intel;
public static class Extra
{
    static List<(ulong Begin, ulong End)> _funcs;

    /// <summary>
    /// The function containing an address, from the .pdata unwind table (chained entries walked to their root)
    /// </summary>
    public static List<(ulong Begin, ulong End)> Funcs
    {
        get
        {
            if (_funcs != null) return _funcs;
            var pdata = Exe.Sections.First(s => s.Name == ".pdata");
            _funcs = new();
            for (long fo = pdata.Raw; fo + 12 <= pdata.Raw + pdata.VSize; fo += 12)
            {
                var b = BitConverter.ToUInt32(Exe.Image, (int)fo);
                var e = BitConverter.ToUInt32(Exe.Image, (int)fo + 4);
                if (b == 0) continue;
                _funcs.Add((Exe.ImageBase + b, Exe.ImageBase + e));
            }
            _funcs.Sort();
            return _funcs;
        }
    }

    public static ulong FuncStart(ulong va)
    {
        var f = Funcs;
        int lo = 0, hi = f.Count - 1;
        while (lo <= hi)
        {
            var m = (lo + hi) / 2;
            if (f[m].Begin <= va) lo = m + 1; else hi = m - 1;
        }
        if (hi < 0) return va;
        // walk back over chained fragments: a fragment directly preceded by another whose end touches it
        var start = f[hi].Begin;
        while (hi > 0 && f[hi - 1].End >= start - 0x10 && !PrecededByPadding(start)) { hi--; start = f[hi].Begin; }
        return start;
    }

    static bool PrecededByPadding(ulong va)
    {
        var fo = Exe.FileOffset(va);
        return fo > 0 && (Exe.Image[fo - 1] == 0xCC || Exe.Image[fo - 1] == 0x90 || Exe.Image[fo - 1] == 0xC3);
    }

    public static void Run(string mode, string[] args)
    {
        switch (mode)
        {
            case "start":
                foreach (var a in args.Skip(2))
                    Console.WriteLine($"{a} -> {FuncStart(Convert.ToUInt64(a.Replace("0x", ""), 16)):X}");
                return;
            case "flow":
                {
                    // flow <exe> <outDir> [iterations]
                    var dir = args[2];
                    Directory.CreateDirectory(dir);
                    if (args.Length > 4) Flow.TraceFn = Convert.ToUInt64(args[4], 16);
                    Flow.Run(args.Length > 3 ? int.Parse(args[3]) : 6);
                    var sb = new System.Text.StringBuilder();
                    foreach (var g in Flow.Accesses.GroupBy(a => (a.Kind, a.Off)).OrderBy(g => g.Key.Kind).ThenBy(g => g.Key.Off))
                    {
                        var sizes = string.Join("/", g.Select(a => a.Size).Distinct().OrderBy(x => x));
                        var rw = (g.Any(a => !a.Write) ? "R" : "") + (g.Any(a => a.Write) ? "W" : "");
                        var fns = g.Select(a => a.Func).Distinct().ToList();
                        sb.AppendLine($"{g.Key.Kind}\t0x{g.Key.Off:X}\t{sizes}\t{rw}\t{g.Count()}\t{fns.Count}\t{string.Join(" ", fns.Take(12).Select(f => f.ToString("X")))}");
                    }
                    File.WriteAllText(Path.Combine(dir, "fields.tsv"), sb.ToString());

                    sb.Clear();
                    foreach (var a in Flow.Accesses.OrderBy(a => a.Kind).ThenBy(a => a.Off).ThenBy(a => a.IP))
                        sb.AppendLine($"{a.Kind}\t0x{a.Off:X}\t{a.Size}\t{(a.Write ? "W" : "R")}\t{a.IP:X}\t{a.Func:X}\t{a.Ctx}\t{a.Text}");
                    File.WriteAllText(Path.Combine(dir, "accesses.tsv"), sb.ToString());

                    sb.Clear();
                    foreach (var (fn, p) in Flow.Params.OrderBy(k => k.Key))
                        sb.AppendLine($"{fn:X}\t{string.Join(",", p.Select(x => x?.ToString() ?? "-"))}\tret={(Flow.Returns.TryGetValue(fn, out var r) ? r?.ToString() : "-")}");
                    foreach (var (fn, r) in Flow.Returns.Where(k => !Flow.Params.ContainsKey(k.Key)).OrderBy(k => k.Key))
                        sb.AppendLine($"{fn:X}\t-\tret={r}");
                    File.WriteAllText(Path.Combine(dir, "functions.tsv"), sb.ToString());
                    Report.Write(dir);
                    return;
                }
            case "idiom":
                {
                    // idiom <exe> <off1,off2,...> : loads chained through these offsets, then the final field read
                    var chain = args[2].Split(',').Select(s => Convert.ToInt32(s, 16)).ToArray();
                    var defs = new Dictionary<Iced.Intel.Register, (int Depth, ulong IP)>();
                    var counts = new Dictionary<int, List<ulong>>();
                    ulong lastFn = 0;
                    foreach (var ins in Exe.Sweep())
                    {
                        if (ins.FlowControl != Iced.Intel.FlowControl.Next && ins.FlowControl != Iced.Intel.FlowControl.ConditionalBranch) { defs.Clear(); continue; }
                        var mem = -1;
                        for (var i = 0; i < ins.OpCount; i++) if (ins.GetOpKind(i) == Iced.Intel.OpKind.Memory) mem = i;
                        var dst = ins.OpCount > 0 && ins.Op0Kind == Iced.Intel.OpKind.Register ? ins.Op0Register.GetFullRegister() : Iced.Intel.Register.None;
                        if (mem >= 0 && ins.MemoryBase != Iced.Intel.Register.None && ins.MemoryIndex == Iced.Intel.Register.None)
                        {
                            var b = ins.MemoryBase.GetFullRegister();
                            var d = (int)(long)ins.MemoryDisplacement64;
                            var depth = defs.TryGetValue(b, out var bd) ? bd.Depth : 0;
                            if (depth == chain.Length)
                            {
                                if (!counts.TryGetValue(d, out var l)) counts[d] = l = new();
                                l.Add(ins.IP);
                            }
                            if (ins.Mnemonic == Iced.Intel.Mnemonic.Mov && mem == 1 && dst != Iced.Intel.Register.None && ins.Op0Register.GetSize() == 8)
                            {
                                if (depth < chain.Length && d == chain[depth] && (depth > 0 || true)) { defs[dst] = (depth + 1, ins.IP); continue; }
                            }
                        }
                        if (dst != Iced.Intel.Register.None) defs.Remove(dst);
                    }
                    foreach (var (d, l) in counts.OrderBy(k => k.Key))
                        Console.WriteLine($"0x{d:X}\t{l.Count}\t{string.Join(" ", l.Take(8).Select(x => x.ToString("X")))}");
                    return;
                }
            case "paramdef":
                foreach (var f in Paramdef.Offsets(args[2])) Console.WriteLine($"0x{f.Off:X}{(f.Bit >= 0 ? "." + f.Bit : "")}	{f.Type}	{f.Name}	{f.Display}");
                return;
            case "disp":
                {
                    // disp <exe> <displacement> : instructions with that memory displacement, and the function they are in
                    var d = Convert.ToInt64(args[2].Replace("0x", ""), 16);
                    foreach (var ins in Exe.Sweep().Concat(Exe.Sweep(1)))
                        for (var i = 0; i < ins.OpCount; i++)
                            if (ins.GetOpKind(i) == OpKind.Memory && ins.MemoryBase != Register.RIP && (long)ins.MemoryDisplacement64 == d)
                            {
                                Console.WriteLine($"{Exe.Format(ins)}    in {FuncStart(ins.IP):X}");
                                break;
                            }
                    return;
                }
            case "stub":
                foreach (var a in args.Skip(2))
                {
                    var va = Convert.ToUInt64(a.Replace("0x", ""), 16);
                    var fo = Exe.FileOffset(va);
                    var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, 16));
                    dec.IP = va; dec.Decode(out var j);
                    Console.WriteLine($"== {Exe.Format(j)}");
                    var list = j.Op0Kind == OpKind.NearBranch64 ? Arxan.Expand(j) : null;
                    if (list == null) { Console.WriteLine("   unresolved"); continue; }
                    foreach (var i in list) Console.WriteLine("   " + Exe.Format(i));
                }
                return;
            case "diff":
                {
                    // diff <exeA> <exeB>: section headers of both, and the ranges of B's sections that differ from A
                    var a = Exe.Image; var aSecs = Exe.Sections.ToList(); var aBase = Exe.ImageBase;
                    Exe.Sections.Clear();
                    Exe.Load(args[2]);
                    var b = Exe.Image;
                    Console.WriteLine($"A {a.Length} bytes, base {aBase:X}; B {b.Length} bytes, base {Exe.ImageBase:X}");
                    foreach (var s in aSecs) Console.WriteLine($"A {s.Name,-8} va={s.VA:X} vsize={s.VSize:X} raw={s.Raw:X} rawsize={s.RawSize:X}");
                    foreach (var s in Exe.Sections) Console.WriteLine($"B {s.Name,-8} va={s.VA:X} vsize={s.VSize:X} raw={s.Raw:X} rawsize={s.RawSize:X}");
                    foreach (var sb in Exe.Sections)
                    {
                        var sa = aSecs.FirstOrDefault(x => x.VA == sb.VA);
                        if (sa == null) { Console.WriteLine($"{sb.Name}: only in B"); continue; }
                        var n = (int)Math.Min(sa.RawSize, sb.RawSize);
                        long diff = 0; var ranges = new List<(long, long)>(); long start = -1;
                        for (var i = 0; i < n; i++)
                        {
                            var d = a[sa.Raw + i] != b[sb.Raw + i];
                            if (d) { diff++; if (start < 0) start = i; }
                            else if (start >= 0 && i - start > 0) { ranges.Add((start, i)); start = -1; }
                        }
                        if (start >= 0) ranges.Add((start, n));
                        // merge ranges closer than 64 bytes
                        var merged = new List<(long S, long E)>();
                        foreach (var r in ranges)
                            if (merged.Count > 0 && r.Item1 - merged[^1].E < 64) merged[^1] = (merged[^1].S, r.Item2); else merged.Add(r);
                        Console.WriteLine($"{sb.Name}: {diff} bytes differ in {merged.Count} ranges");
                        foreach (var r in merged.OrderByDescending(r => r.E - r.S).Take(12))
                            Console.WriteLine($"   {Exe.ImageBase + sb.VA + (ulong)r.S:X}..{Exe.ImageBase + sb.VA + (ulong)r.E:X} ({r.E - r.S} bytes)");
                    }
                    return;
                }
            case "raw":
                {
                    // raw <exe> <va> <count> [filter]: linear disassembly that doesn't stop at returns
                    var va = Convert.ToUInt64(args[2].Replace("0x", ""), 16);
                    var fo = Exe.FileOffset(va);
                    var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, 0x40000));
                    dec.IP = va;
                    var n = args.Length > 3 ? int.Parse(args[3]) : 200;
                    for (var i = 0; i < n; i++)
                    {
                        dec.Decode(out var ins);
                        var s = Exe.Format(ins);
                        if (args.Length <= 4 || s.Contains(args[4], StringComparison.OrdinalIgnoreCase)) Console.WriteLine(s);
                    }
                    return;
                }
            case "owner":
            case "identity":
            case "watch":
                Objects.Run(mode, args);
                return;
            case "layout":
                Layout.Dump(args[2]);
                return;
            case "fdis":
                {
                    var s = FuncStart(Convert.ToUInt64(args[2].Replace("0x", ""), 16));
                    foreach (var ins in Exe.Function(s, args.Length > 3 ? int.Parse(args[3]) : 4000))
                        Console.WriteLine(Exe.Format(ins));
                    return;
                }
        }
        Console.WriteLine($"unknown mode {mode}");
    }
}
