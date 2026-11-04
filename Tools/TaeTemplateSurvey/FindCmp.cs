using Iced.Intel;

public static class FindCmp
{
    // findcmp <exe> <imm> : cmp dword [reg], imm with no displacement, as a check of an event's type
    public static void Run(string exe, long imm)
    {
        ExeScan.Load(exe);
        var text = ExeScan.Sections.First(s => s.Name == ".text");
        var start = ExeScan.ImageBase + text.VA;
        var d = Iced.Intel.Decoder.Create(64, new ByteArrayCodeReader(ExeScan.Image, (int)text.Raw, (int)text.VSize));
        d.IP = start;
        var f = Rtti.ParamTraceFunctions();
        while (d.IP < start + text.VSize)
        {
            d.Decode(out var i);
            if (i.Mnemonic == Mnemonic.Mov && i.Op0Kind == OpKind.Register && i.Op0Register == Register.EDX && i.Op1Kind == OpKind.Immediate32 && i.Immediate32 == imm)
            {
                var save = d.IP; d.Decode(out var n1); d.Decode(out var n2); d.IP = save;
                var call = n1.Mnemonic == Mnemonic.Call ? n1 : n2.Mnemonic == Mnemonic.Call ? n2 : default;
                if (call.Mnemonic == Mnemonic.Call && call.Op0Kind == OpKind.NearBranch64) Console.WriteLine($"mov edx,{imm} then call {call.NearBranchTarget:X} at {i.IP:X} in {f(i.IP):X}");
            }
            if (i.Mnemonic == Mnemonic.Cmp && i.Op0Kind == OpKind.Memory && i.MemoryDisplacement64 == 0 && i.MemoryIndex == Register.None
                && i.MemorySize.GetSize() == 4 && i.Op1Kind != OpKind.Register && (long)(int)i.GetImmediate(1) == imm)
                Console.WriteLine($"{i.IP:X} in {f(i.IP):X}");
        }
    }
}
