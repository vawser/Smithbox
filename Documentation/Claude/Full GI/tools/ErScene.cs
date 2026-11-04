using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Linq;
using System.Collections.Generic;
// erscene <ER Game dir> <msb regex>: per map, the collisions' scene GPARAM sub IDs, against the drawparam files of its area
public static class ErScene
{
    public static void Run(string folder, string msbRe)
    {
        using var vfs = ArchiveBinderVirtualFileSystem.FromGameFolder(folder, Andre.Core.Game.ER);
        var gparams = vfs.GetFileNamesMatchingRecursive("/param/drawparam", @"^m.*\.gparam\.dcx$").Select(p => System.IO.Path.GetFileName(p).Replace(".gparam.dcx", "")).ToList();
        foreach (var path in vfs.GetFileNamesMatchingRecursive("/map/mapstudio", msbRe).Distinct().OrderBy(x => x))
        {
            MSBE msb; try { msb = MSBE.Read(vfs.ReadFileOrThrow(path)); } catch (Exception e) { Console.WriteLine($"{path}: {e.Message}"); continue; }
            var name = System.IO.Path.GetFileName(path).Substring(0, 12);
            var counts = new SortedDictionary<string, int>();
            foreach (var c in msb.Parts.Collisions)
            {
                var s = c.SceneGparamConfigStruct; if (s == null) continue;
                var k = $"base={s.GparamSubID_Base} o1={s.GparamSubID_Override1} o2={s.GparamSubID_Override2} o3={s.GparamSubID_Override3} t={s.TransitionTime}";
                counts[k] = counts.GetValueOrDefault(k) + 1;
            }
            var files = gparams.Where(g => g.StartsWith(name.Substring(0, 7))).OrderBy(x => x);
            Console.WriteLine($"== {name}: files {string.Join(" ", files)}");
            foreach (var kv in counts) Console.WriteLine($"   {kv.Value,4} {kv.Key}");
        }
    }
}
