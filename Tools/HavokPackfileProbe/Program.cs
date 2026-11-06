using Andre.Core;
using Andre.IO.VFS;
using Havok;
using Havok.Formats.Packfile;
using SoulsFormats;
using System.Buffers.Binary;
using System.Text.RegularExpressions;

// Reads one packfile through the unified packfile engine, writes it, and prints the data section's local fixups and
// the hkSimpleArray/hkArray headers of the original and the rewrite side by side, to find where a round trip goes wrong.
//   HavokPackfileProbe <GAME> <bhd path in the VFS> <inner file regex>
// Run with the working directory at src/Smithbox.Data.

var gamePaths = File.ReadAllLines(Path.Combine("..", "..", "game_data.txt"))
    .Select(l => l.Split(':', 2)).Where(p => p.Length == 2)
    .ToDictionary(p => p[0].Trim(), p => p[1].Trim().Trim('"').Trim());
var game = Enum.Parse<Game>(args[0]);
var root = gamePaths[args[0]];
VirtualFileSystem fs;
try { fs = new CompundVirtualFileSystem([ArchiveBinderVirtualFileSystem.FromGameFolder(root, game), new RealVirtualFileSystem(root, false)]); }
catch { fs = new RealVirtualFileSystem(root, false); }

var layoutMode = args.Contains("--layout");
args = args.Where(a => a != "--layout").ToArray();

var container = fs.ReadFileOrThrow(args[1]);
IBinder binder;
if (args[1].EndsWith("bhd", StringComparison.OrdinalIgnoreCase))
{
    var bdt = fs.ReadFileOrThrow(Path.ChangeExtension(args[1], ".hkxbdt"));
    binder = BXF4.IsBHD(container) ? BXF4.Read(container, bdt) : BXF3.Read(container, bdt);
}
else
{
    var bnd = DCX.Is(container) ? DCX.Decompress(container).ToArray() : container.ToArray();
    binder = BND4.Is(bnd) ? BND4.Read(bnd) : BND3.Read(bnd);
}
var file = binder.Files.First(f => Regex.IsMatch(f.Name, args[2]));
var original = DCX.Is(file.Bytes) ? DCX.Decompress(file.Bytes).ToArray() : file.Bytes.ToArray();

if (layoutMode)
{
    LayoutProbe.Run(original, HavokProfile.FromGame(args[0])!, file.Name);
    return;
}

var profile = HavokProfile.FromGame(args[0]) is { Sdk: HavokSdk.Hk2014 } p ? p : HavokProfile.DS3;
var serializer = new HavokPackfileSerializer(Havok.hk2014.ClassSet.For(profile));
var written = serializer.Write(serializer.Read(original));
Console.WriteLine($"{file.Name}: original {original.Length} bytes, written {written.Length}");

foreach (var (label, bytes) in new[] { ("original", original), ("written", written) })
{
    // data section: the third section header
    int sectionStart = 0x40 + BinaryPrimitives.ReadInt16LittleEndian(bytes.AsSpan(0x3E)) + 2 * 0x40;
    uint start = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(sectionStart + 20));
    uint local = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(sectionStart + 24));
    uint global = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(sectionStart + 28));
    Console.WriteLine($"== {label}: data at 0x{start:X}, {local} bytes; local fixups:");
    for (uint q = start + local; q + 8 <= start + global; q += 8)
    {
        uint src = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan((int)q));
        if (src == 0xFFFFFFFF) continue;
        uint dst = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan((int)q + 4));
        uint count = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan((int)(start + src + 8)));
        uint cap = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan((int)(start + src + 12)));
        Console.WriteLine($"  src 0x{src:X5} -> dst 0x{dst:X5}  count {count,6} cap/flags 0x{cap:X8}");
    }
}
