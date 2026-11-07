using Iced.Intel;

// exe <eldenring.exe> magic        : functions using the ONAV / NVMC magics as immediates
// exe <eldenring.exe> fn <va> [n]  : disassemble a function
static class ExeScan
{
    public static void Run(string[] args)
    {
        Exe.Load(args[1]);
        if (args[2] == "magic")
        {
            var wanted = args.Length > 3 ? args.Skip(3).Select(a => Convert.ToUInt32(a, 16)).ToArray() : new uint[] { 0x56414E4F, 0x434D564E };
            foreach (var ins in Exe.Sweep())
            {
                for (int i = 0; i < ins.OpCount; i++)
                {
                    var k = ins.GetOpKind(i);
                    if (k is OpKind.Immediate32 or OpKind.Immediate32to64 && wanted.Contains((uint)ins.GetImmediate(i)))
                        Console.WriteLine(Exe.Format(ins));
                }
            }
        }
        else if (args[2] == "str")
        {
            var targets = Exe.FindStrings(args[3]).Select(x => x.VA).ToHashSet();
            foreach (var ins in Exe.Sweep()) { var t = Exe.RefTarget(ins); if (targets.Contains(t)) Console.WriteLine(Exe.Format(ins)); }
        }
        else if (args[2] == "calls")
        {
            var t = Convert.ToUInt64(args[3], 16);
            foreach (var ins in Exe.Sweep()) if (Exe.RefTarget(ins) == t) Console.WriteLine(Exe.Format(ins));
        }
        else if (args[2] == "disp")
        {
            var d = Convert.ToUInt64(args[3], 16); var lo = Convert.ToUInt64(args[4], 16); var hi = Convert.ToUInt64(args[5], 16);
            foreach (var ins in Exe.Sweep()) if (ins.IP >= lo && ins.IP < hi && ins.MemoryDisplacement64 == d && ins.MemoryBase != Register.RSP && ins.MemoryBase != Register.RBP) Console.WriteLine(Exe.Format(ins));
        }
        else if (args[2] == "jt")
        {
            var t = Convert.ToUInt64(args[3], 16); int n = int.Parse(args[4]);
            for (int i = 0; i < n; i++) Console.WriteLine($"{i}: {Exe.ImageBase + (ulong)Exe.ReadI32(t + (ulong)i * 4):X}");
        }
        else if (args[2] == "strings")
        {
            foreach (var (va, t) in Exe.FindStrings(args[3])) Console.WriteLine($"{va:X} {t}");
        }
        else if (args[2] == "fnat")
        {
            var va = Convert.ToUInt64(args[3], 16);
            long fo = Exe.FileOffset(va);
            long p = fo;
            while (p > fo - 0x4000 && !(Exe.Image[p - 1] == 0xCC && Exe.Image[p] != 0xCC && (p % 16) == 0)) p--;
            var start = Exe.VaOfFile(p);
            Console.WriteLine($"; function at {start:X}");
            foreach (var ins in Exe.Function(start, args.Length > 4 ? int.Parse(args[4]) : 800)) Console.WriteLine(Exe.Format(ins));
        }
        else if (args[2] == "win")
        {
            var va = Convert.ToUInt64(args[3], 16); var before = Convert.ToUInt64(args[4], 16); var after = args.Length > 5 ? Convert.ToUInt64(args[5], 16) : 0x40UL;
            var fo = Exe.FileOffset(va - before);
            var dec = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(Exe.Image, (int)fo, (int)(before + after)));
            dec.IP = va - before;
            while (dec.IP < va + after) { dec.Decode(out var ins); Console.WriteLine(Exe.Format(ins)); }
        }
        else if (args[2] == "vt")
        {
            // vt <class substring> : the virtual functions of matching RTTI classes
            Exe.LoadRtti();
            foreach (var (vt, cls) in Exe.VtableClass.Where(x => x.Value.Contains(args[3])).OrderBy(x => x.Value))
            {
                Console.WriteLine($"{cls} vtable {vt:X}");
                for (int i = 0; i < 60; i++)
                {
                    var fn = Exe.ReadU64(vt + (ulong)i * 8);
                    if (!Exe.IsCode(fn) || (i > 0 && Exe.VtableClass.ContainsKey(vt + (ulong)i * 8))) break;
                    Console.WriteLine($"  [{i,2}] {fn:X}");
                }
            }
        }
        else if (args[2] == "bits")
        {
            // bits <hex mask,...> [min distinct] [window] : places that test several of the masks close together.
            // Byte tests on memory and on ah/bh.. are shifted to where the byte sits in a 32 bit value
            var masks = args[3].Split(',').Select(h => Convert.ToUInt32(h, 16)).ToHashSet();
            int minDistinct = args.Length > 4 ? int.Parse(args[4]) : 3;
            ulong window = args.Length > 5 ? Convert.ToUInt64(args[5], 16) : 0x200;
            var hits = new List<(ulong Ip, uint Mask, string Text)>();
            foreach (var ins in Exe.Sweep())
            {
                if (ins.Mnemonic is not (Mnemonic.Test or Mnemonic.And or Mnemonic.Bt or Mnemonic.Or or Mnemonic.Btr or Mnemonic.Bts) || ins.OpCount != 2)
                    continue;
                var k = ins.GetOpKind(1);
                if (k is not (OpKind.Immediate8 or OpKind.Immediate32 or OpKind.Immediate8to32 or OpKind.Immediate32to64 or OpKind.Immediate8to64 or OpKind.Immediate16))
                    continue;
                ulong imm = ins.GetImmediate(1);
                uint mask;
                if (ins.Mnemonic is Mnemonic.Bt or Mnemonic.Btr or Mnemonic.Bts) { if (imm >= 32) continue; mask = 1u << (int)imm; }
                else
                {
                    mask = (uint)imm;
                    int shift = 0;
                    if (ins.Op0Kind == OpKind.Register && ins.Op0Register is Register.AH or Register.BH or Register.CH or Register.DH) shift = 8;
                    else if (ins.Op0Kind == OpKind.Memory && ins.MemorySize == MemorySize.UInt8) shift = 8 * (int)(ins.MemoryDisplacement64 % 4);
                    else if (ins.Op0Kind == OpKind.Memory && ins.MemorySize == MemorySize.UInt16) shift = 8 * (int)(ins.MemoryDisplacement64 % 4);
                    mask = shift >= 32 ? 0 : (uint)(imm << shift);
                }
                if (masks.Contains(mask)) hits.Add((ins.IP, mask, Exe.Format(ins)));
            }
            // Clusters of hits that are close together
            var clusters = new List<List<(ulong Ip, uint Mask, string Text)>>();
            foreach (var h in hits.OrderBy(h => h.Ip))
            {
                if (clusters.Count > 0 && h.Ip - clusters[^1][^1].Ip <= window) clusters[^1].Add(h);
                else clusters.Add(new() { h });
            }
            foreach (var c in clusters.Where(c => c.Select(h => h.Mask).Distinct().Count() >= minDistinct).OrderByDescending(c => c.Select(h => h.Mask).Distinct().Count()).Take(40))
            {
                Console.WriteLine($"== {c[0].Ip:X}..{c[^1].Ip:X}: {string.Join(",", c.Select(h => h.Mask.ToString("X")).Distinct())}");
                foreach (var h in c.Take(14)) Console.WriteLine($"   {h.Text}");
            }
        }
        else if (args[2] == "fn")
        {
            var va = Convert.ToUInt64(args[3], 16);
            foreach (var ins in Exe.Function(va, args.Length > 4 ? int.Parse(args[4]) : 600)) Console.WriteLine(Exe.Format(ins));
        }
    }
}
