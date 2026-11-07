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
            var wanted = new uint[] { 0x56414E4F, 0x434D564E };
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
        else if (args[2] == "fn")
        {
            var va = Convert.ToUInt64(args[3], 16);
            foreach (var ins in Exe.Function(va, args.Length > 4 ? int.Parse(args[4]) : 600)) Console.WriteLine(Exe.Format(ins));
        }
    }
}
