using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
// erld <ER Game dir>: across map gparams, how often each LightSet field of IDs 100/1000/1100 differs from ID 0, and which IDs exist
public static class ErLightDiff
{
    public static void Run(string folder)
    {
        using var vfs = ArchiveBinderVirtualFileSystem.FromGameFolder(folder, Andre.Core.Game.ER);
        var files = vfs.GetFileNamesMatchingRecursive("/param/drawparam", @"m\d\d_\d\d_\d{4}\.gparam\.dcx$").Distinct().ToList();
        var counts = new SortedDictionary<string, int>(); var idSets = new Dictionary<string, int>(); int n = 0;
        foreach (var path in files)
        {
            GPARAM g; try { g = GPARAM.Read(vfs.ReadFileOrThrow(path)); } catch { continue; }
            var ls = g.Params.FirstOrDefault(p => p.Key == "LightSet ParamEditor"); if (ls == null) continue; n++;
            var ids = new SortedSet<int>();
            foreach (var f in ls.Fields)
            {
                var vals = ((IList)f.GetType().GetProperty("Values").GetValue(f)).Cast<object>()
                    .Select(v => (Id: (int)v.GetType().GetProperty("ID").GetValue(v), T: (float)v.GetType().GetProperty("TimeOfDay").GetValue(v), V: v.GetType().GetProperty("Value").GetValue(v)?.ToString())).ToList();
                foreach (var v in vals) ids.Add(v.Id);
                var b = vals.Where(v => v.Id == 0).GroupBy(v => v.T).ToDictionary(x => x.Key, x => x.First().V);
                foreach (var id in vals.Select(v => v.Id).Where(i => i != 0).Distinct())
                    if (vals.Where(v => v.Id == id).Any(v => !b.TryGetValue(v.T, out var bv) || bv != v.V))
                    { var k = $"{id,5} {f.Key}"; counts[k] = counts.GetValueOrDefault(k) + 1; }
            }
            var s = string.Join(",", ids); idSets[s] = idSets.GetValueOrDefault(s) + 1;
        }
        Console.WriteLine($"files with LightSet: {n}");
        foreach (var kv in idSets.OrderByDescending(k => k.Value)) Console.WriteLine($"ids {kv.Key}: {kv.Value}");
        foreach (var kv in counts) Console.WriteLine($"{kv.Value,4} {kv.Key}");
    }
}
