using Iced.Intel;
using System;
using System.IO;
using System.Linq;
// Disassembles the whole function containing an address, using .pdata for its bounds
public static class Func
{
    public static void Dump(string path, ulong addr)
    {
        var b = File.ReadAllBytes(path);
        int pe = BitConverter.ToInt32(b, 0x3C); int nsec = BitConverter.ToUInt16(b, pe + 6); int opt = BitConverter.ToUInt16(b, pe + 20);
        ulong ib = BitConverter.ToUInt64(b, pe + 48); int sec = pe + 24 + opt;
        var secs = Enumerable.Range(0, nsec).Select(i => (name: System.Text.Encoding.ASCII.GetString(b, sec + i * 40, 8).TrimEnd('\0'), va: BitConverter.ToUInt32(b, sec + i * 40 + 12), raw: BitConverter.ToUInt32(b, sec + i * 40 + 20), size: BitConverter.ToUInt32(b, sec + i * 40 + 16))).ToList();
        long Off(uint rva) { var s = secs.First(s => rva >= s.va && rva < s.va + s.size); return s.raw + (rva - s.va); }
        // exception directory = data directory 3
        uint excRva = BitConverter.ToUInt32(b, pe + 24 + 112 + 3 * 8), excSize = BitConverter.ToUInt32(b, pe + 24 + 112 + 3 * 8 + 4);
        uint rva = (uint)(addr - ib), begin = 0, end = 0;
        long eo = Off(excRva);
        for (uint i = 0; i < excSize / 12; i++)
        {
            uint s0 = BitConverter.ToUInt32(b, (int)(eo + i * 12)), e0 = BitConverter.ToUInt32(b, (int)(eo + i * 12 + 4));
            if (rva >= s0 && rva < e0) { begin = s0; end = e0; break; }
        }
        if (begin == 0) { Console.WriteLine("no pdata entry"); return; }
        Console.WriteLine($"function {ib + begin:X}-{ib + end:X} ({end - begin} bytes)");
        var reader = new ByteArrayCodeReader(b, (int)Off(begin), (int)(end - begin));
        var dec = Decoder.Create(64, reader, ib + begin);
        var f = new MasmFormatter(); var o = new StringOutput();
        while (reader.CanReadByte) { dec.Decode(out var ins); f.Format(ins, o); Console.WriteLine($"   {ins.IP:X}  {o.ToStringAndReset()}"); }
    }
}
