using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.IO;
using System.Linq;
// Dumps the raw header ID and type data of ER/NR regions of one type, straight from the bytes
public static class Raw
{
    public static void Run(string game, string root, string mapId, int regionType)
    {
        Memory<byte> data;
        if (game == "ER")
        {
            using var vfs = ArchiveBinderVirtualFileSystem.FromGameFolder(root, Andre.Core.Game.ER);
            data = vfs.ReadFileOrThrow($"/map/mapstudio/{mapId}.msb.dcx").ToArray();
        }
        else data = File.ReadAllBytes(Path.Combine(root, "map", "mapstudio", mapId + ".msb.dcx"));
        var br = new BinaryReaderEx(false, DCX.Decompress(data).ToArray());
        br.Position = 0x10;
        for (int p = 0; p < 3; p++)
        {
            int version = br.ReadInt32(); int count = br.ReadInt32(); long nameOff = br.ReadInt64();
            long[] offs = br.ReadInt64s(count - 1); long next = br.ReadInt64();
            string name = br.GetUTF16(nameOff);
            if (p == 2)
            {
                foreach (var o in offs)
                {
                    br.Position = o;
                    long nOff = br.ReadInt64(); int type = br.ReadInt32(); int id = br.ReadInt32();
                    if (type != regionType) continue;
                    br.Position = o + 0x58; long typeOff = br.ReadInt64(); long s98 = br.ReadInt64();
                    string rname = br.GetUTF16(o + nOff);
                    var bytes = br.GetBytes(o + typeOff, (int)(s98 - typeOff));
                    var ints = Enumerable.Range(0, bytes.Length / 4).Select(i => BitConverter.ToInt32(bytes, i * 4).ToString("X8"));
                    Console.WriteLine($"{rname,-12} id={id,-4} typeLen=0x{bytes.Length:X} {string.Join(" ", ints)}");
                }
            }
            br.Position = next;
        }
    }
}
