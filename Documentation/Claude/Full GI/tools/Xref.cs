using Iced.Intel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// Finds code that references a file offset (RIP-relative), and disassembles around each hit
public static class Xref
{
    record Section(string Name, uint Va, uint VSize, uint Raw, uint RawSize);
    static ulong ImageBase;
    static List<Section> Sections = new();
    static byte[] B;

    static void LoadPe(string path)
    {
        B = File.ReadAllBytes(path);
        int pe = BitConverter.ToInt32(B, 0x3C);
        int nsec = BitConverter.ToUInt16(B, pe + 6);
        int optSize = BitConverter.ToUInt16(B, pe + 20);
        ImageBase = BitConverter.ToUInt64(B, pe + 24 + 24);
        int sec = pe + 24 + optSize;
        for (int i = 0; i < nsec; i++, sec += 40)
            Sections.Add(new(System.Text.Encoding.ASCII.GetString(B, sec, 8).TrimEnd('\0'), BitConverter.ToUInt32(B, sec + 12), BitConverter.ToUInt32(B, sec + 8), BitConverter.ToUInt32(B, sec + 20), BitConverter.ToUInt32(B, sec + 16)));
    }
    public static ulong OffToVa(long off) { var s = Sections.First(s => off >= s.Raw && off < s.Raw + s.RawSize); return ImageBase + s.Va + (ulong)(off - s.Raw); }
    static long VaToOff(ulong va) { var rva = va - ImageBase; var s = Sections.FirstOrDefault(s => rva >= s.Va && rva < s.Va + Math.Max(s.VSize, s.RawSize)); return s == null ? -1 : s.Raw + (long)(rva - s.Va); }

    public static void Find(string path, long[] targets, int before, int after)
    {
        LoadPe(path);
        Console.WriteLine("sections: " + string.Join(" ", Sections.Select(s => $"{s.Name}@{s.Va:X}+{s.RawSize:X}")));
        var targetVas = targets.Select(OffToVa).ToHashSet();
        foreach (var s in Sections.Where(s => s.RawSize > 0 && s.Name is not (".rdata" or ".data" or ".pdata" or ".rsrc" or ".reloc")))
        {
            var reader = new ByteArrayCodeReader(B, (int)s.Raw, (int)s.RawSize);
            var dec = Decoder.Create(64, reader, ImageBase + s.Va);
            var window = new Queue<Instruction>();
            var pending = new List<(Instruction hit, List<Instruction> ctx, int left)>();
            while (reader.CanReadByte)
            {
                dec.Decode(out var ins);
                foreach (var p in pending.ToList()) { p.ctx.Add(ins); if (p.left <= 1) { Print(p.hit, p.ctx); pending.Remove(p); } else { pending.Remove(p); pending.Add((p.hit, p.ctx, p.left - 1)); } }
                if (ins.IsIPRelativeMemoryOperand && targetVas.Contains(ins.IPRelativeMemoryAddress))
                {
                    var ctx = window.ToList(); ctx.Add(ins);
                    pending.Add((ins, ctx, after));
                }
                window.Enqueue(ins); if (window.Count > before) window.Dequeue();
            }
        }
    }
    static void Print(Instruction hit, List<Instruction> ctx)
    {
        var f = new MasmFormatter(); f.Options.FirstOperandCharIndex = 10;
        var o = new StringOutput();
        Console.WriteLine($"==== xref at {hit.IP:X} -> {hit.IPRelativeMemoryAddress:X}");
        foreach (var i in ctx) { f.Format(i, o); Console.WriteLine($"{(i.IP == hit.IP ? ">>" : "  ")} {i.IP:X}  {o.ToStringAndReset()}{Annot(i)}"); }
    }
    static string Annot(Instruction i)
    {
        if (!i.IsIPRelativeMemoryOperand) return "";
        long off = VaToOff(i.IPRelativeMemoryAddress);
        if (off < 0 || off + 8 > B.Length) return "";
        // UTF-16 or ASCII string preview
        var sb = new System.Text.StringBuilder();
        for (long k = off; k + 1 < B.Length && sb.Length < 60; k += 2) { char c = (char)(B[k] | B[k + 1] << 8); if (c < 0x20 || c > 0x7E) break; sb.Append(c); }
        if (sb.Length >= 3) return $"   ; L\"{sb}\"";
        sb.Clear();
        for (long k = off; k < B.Length && sb.Length < 60; k++) { char c = (char)B[k]; if (c < 0x20 || c > 0x7E) break; sb.Append(c); }
        return sb.Length >= 4 ? $"   ; \"{sb}\"" : "";
    }
}
