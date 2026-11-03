using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
public static class Match
{
    static string Id(IMsbRegion r, string game) => game == "AC6"
        ? ((MSB_AC6.Region.EnvMapEffectBox)r).EnvMapEffectBox_UnkT32.ToString()
        : Regex.Match(r.Name, @"\d+$").Value;

    // args: game root mapId
    public static void Run(string game, string root, string mapId)
    {
        VirtualFileSystem vfs = game == "ER" ? ArchiveBinderVirtualFileSystem.FromGameFolder(root, Andre.Core.Game.ER) : null;
        Memory<byte> Read(string rel) => vfs != null ? vfs.ReadFileOrThrow(rel) : File.ReadAllBytes(Path.Combine(root, rel.TrimStart('/')));
        IEnumerable<string> List(string dir, string pattern) => vfs != null
            ? vfs.GetFileNamesMatching(dir, pattern).Select(p => p.Replace((char)92, (char)47))
            : Directory.GetFiles(Path.Combine(root, dir.TrimStart('/'))).Select(Path.GetFileName).Where(f => Regex.IsMatch(f, pattern)).Select(f => dir + "/" + f);

        var msbBytes = Read($"/map/mapstudio/{mapId}.msb.dcx");
        IMsb msb = game switch { "ER" => MSBE.Read(msbBytes), "NR" => MSB_NR.Read(msbBytes), _ => MSB_AC6.Read(msbBytes) };
        var boxes = msb.Regions.GetEntries().Where(r => r.GetType().Name.Contains("EffectBox")).ToList();

        string mapDir = $"/map/{mapId.Substring(0, 3)}/{mapId}";
        var vols = new Dictionary<string, (string file, int w, int h, int d)>();
        foreach (var p in List(mapDir, @".*envmap.*_high.*\.tpf(bnd)?\.dcx$"))
        {
            var tpfs = p.Contains("tpfbnd") ? BND4.Read(Read(p)).Files.Select(f => TPF.Read(f.Bytes)) : new[] { TPF.Read(Read(p)) };
            foreach (var t in tpfs.SelectMany(t => t.Textures).Where(t => t.Name.EndsWith("_W")))
            {
                var b = t.Bytes.ToArray().AsSpan();
                int h = BitConverter.ToInt32(b.Slice(12, 4)), w = BitConverter.ToInt32(b.Slice(16, 4)), d = BitConverter.ToInt32(b.Slice(24, 4));
                var key = Regex.Match(t.Name, @"GIIV(\d+)_(\d+)").Value;
                vols[key] = (Path.GetFileName(p), w, h, d);
            }
        }
        var ivs = new Dictionary<string, IVINFO>();
        foreach (var f in BND4.Read(Read($"{mapDir}/{mapId}_high.ivinfobnd.dcx")).Files)
            ivs[Regex.Match(f.Name, @"GIIV(\d+)_(\d+)").Value] = IVINFO.Read(f.Bytes);

        Console.WriteLine($"[{game} {mapId}] boxes={boxes.Count} volumes={vols.Count} ivinfo={ivs.Count}");
        foreach (var r in boxes)
        {
            var num = Id(r, game);
            var size = r.Shape is MSB.Shape.Box bx ? $"{bx.Width}x{bx.Height}x{bx.Depth}" : r.Shape?.GetType().Name;
            dynamic dr = r;
            string extra = "";
            try { extra = $" fade={dr.FadeDistance} prio={dr.Priority} diff={dr.AmbientDiffuseIntensity} spec={dr.AmbientSpecularIntensity}"; } catch { }
            var matches = vols.Where(v => v.Key.StartsWith("GIIV" + num.PadLeft(4, '0'))).Select(v => $"{v.Key}={v.Value.w}x{v.Value.h}x{v.Value.d}@{v.Value.file}");
            Console.WriteLine($"  {r.Name} box={size} rot={r.Rotation}{extra} -> {string.Join(", ", matches.Take(3))}{(matches.Count() > 3 ? $" (+{matches.Count() - 3})" : "")}");
        }
        var boxNums = boxes.Select(r => Id(r, game).PadLeft(4, '0')).ToHashSet();
        Console.WriteLine($"  boxes without a volume: {boxNums.Count(n => !vols.Keys.Any(k => k.Substring(4, 4) == n))} of {boxNums.Count} distinct ids");
        var orphan = vols.Keys.Where(k => !boxNums.Contains(k.Substring(4, 4))).ToList();
        Console.WriteLine($"  volumes without a box: {orphan.Count} {string.Join(" ", orphan.Take(8))}");
        Console.WriteLine($"  ivinfo without a volume: {ivs.Keys.Count(k => !vols.ContainsKey(k))}");
    }
}
