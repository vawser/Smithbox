using Iced.Intel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// Finds calls/jumps to any address in [target, target+span) and prints the code before each
public static class Callers
{
    public static void Find(string path, ulong[] targets, int span, int before, int after)
    {
        var b = File.ReadAllBytes(path);
        int pe = BitConverter.ToInt32(b, 0x3C); int nsec = BitConverter.ToUInt16(b, pe + 6); int opt = BitConverter.ToUInt16(b, pe + 20);
        ulong ib = BitConverter.ToUInt64(b, pe + 48); int sec = pe + 24 + opt;
        var fmt = new MasmFormatter(); var o = new StringOutput();
        for (int i = 0; i < nsec; i++, sec += 40)
        {
            string name = System.Text.Encoding.ASCII.GetString(b, sec, 8).TrimEnd('\0');
            if (name != ".text") continue;
            uint va = BitConverter.ToUInt32(b, sec + 12), raw = BitConverter.ToUInt32(b, sec + 20), size = BitConverter.ToUInt32(b, sec + 16);
            var reader = new ByteArrayCodeReader(b, (int)raw, (int)size);
            var dec = Decoder.Create(64, reader, ib + va);
            var win = new Queue<Instruction>(); var pend = new List<(List<Instruction> ctx, int left)>();
            while (reader.CanReadByte)
            {
                dec.Decode(out var ins);
                for (int p = pend.Count - 1; p >= 0; p--) { pend[p].ctx.Add(ins); var l = pend[p].left - 1; if (l <= 0) { Dump(pend[p].ctx, fmt, o); pend.RemoveAt(p); } else pend[p] = (pend[p].ctx, l); }
                if ((ins.FlowControl is FlowControl.Call or FlowControl.UnconditionalBranch) && ins.Op0Kind == OpKind.NearBranch64
                    && targets.Any(t => ins.NearBranchTarget >= t && ins.NearBranchTarget < t + (ulong)span))
                { var ctx = win.ToList(); ctx.Add(ins); pend.Add((ctx, after)); Console.WriteLine($"==== {ins.IP:X} -> {ins.NearBranchTarget:X}"); }
                win.Enqueue(ins); if (win.Count > before) win.Dequeue();
            }
        }
    }
    static void Dump(List<Instruction> ctx, MasmFormatter f, StringOutput o)
    {
        foreach (var i in ctx) { f.Format(i, o); Console.WriteLine($"   {i.IP:X}  {o.ToStringAndReset()}"); }
    }
}
