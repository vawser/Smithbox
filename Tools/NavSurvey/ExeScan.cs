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
        else if (args[2] == "fn")
        {
            var va = Convert.ToUInt64(args[3], 16);
            foreach (var ins in Exe.Function(va, args.Length > 4 ? int.Parse(args[4]) : 600)) Console.WriteLine(Exe.Format(ins));
        }
    }
}
