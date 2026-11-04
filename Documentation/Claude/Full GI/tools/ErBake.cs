using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
// erbake <ER Game dir> <map regex>: mean IVINFO ShLuminance (w and the L1 length) of each bake, from the high ivinfobnds
public static class ErBake
{
    public static void Run(string folder, string mapRe)
    {
        using var vfs = ArchiveBinderVirtualFileSystem.FromGameFolder(folder, Andre.Core.Game.ER);
        var re = new Regex(@"_GIIV(\d{4})_(\d{2})\.ivinfo$", RegexOptions.IgnoreCase);
        foreach (var path in vfs.GetFileNamesMatchingRecursive("/map", mapRe + @".*_high\.ivinfobnd\.dcx$").Distinct().OrderBy(x => x))
        {
            var bnd = BND4.Read(vfs.ReadFileOrThrow(path));
            var byBake = new SortedDictionary<int, List<IVINFO>>();
            foreach (var f in bnd.Files)
            {
                var m = re.Match(f.Name); if (!m.Success) continue;
                var bake = int.Parse(m.Groups[2].Value);
                if (!byBake.TryGetValue(bake, out var l)) byBake[bake] = l = new();
                l.Add(IVINFO.Read(f.Bytes));
            }
            Console.WriteLine($"== {System.IO.Path.GetFileName(path)}");
            foreach (var kv in byBake)
            {
                var w = kv.Value.Select(i => i.ShLuminance.W).Where(x => x > 0).DefaultIfEmpty(0).Average();
                var l1 = kv.Value.Select(i => Math.Sqrt(i.ShLuminance.X * i.ShLuminance.X + i.ShLuminance.Y * i.ShLuminance.Y + i.ShLuminance.Z * i.ShLuminance.Z)).Average();
                var up = kv.Value.Select(i => i.ShLuminance.Y).Average();
                Console.WriteLine($"   bake {kv.Key:D2}: n={kv.Value.Count} meanL0={w:0.#####} meanL1={l1:0.#####} meanY={up:0.#####}");
            }
        }
    }
}
