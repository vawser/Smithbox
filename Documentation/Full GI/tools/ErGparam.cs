using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Linq;
using System.Collections;
// ergp <ER Game dir> <file regex> [group] [field regex]: lists drawparam files, or dumps the value IDs and times of matching fields
public static class ErGparam
{
    public static void Run(string folder, string fileRe, string group, string fieldRe)
    {
        using var vfs = ArchiveBinderVirtualFileSystem.FromGameFolder(folder, Andre.Core.Game.ER);
        var files = vfs.GetFileNamesMatchingRecursive("/param/drawparam", fileRe).Distinct().OrderBy(x => x).ToList();
        if (group == null) { foreach (var f in files) Console.WriteLine(f); Console.WriteLine($"{files.Count} files"); return; }
        var re = new System.Text.RegularExpressions.Regex(fieldRe ?? ".", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        foreach (var path in files)
        {
            var g = GPARAM.Read(vfs.ReadFileOrThrow(path));
            foreach (var p in g.Params.Where(p => p.Key.Contains(group, StringComparison.OrdinalIgnoreCase)))
                foreach (var f in p.Fields.Where(f => re.IsMatch(f.Key)))
                {
                    var vals = (IList)f.GetType().GetProperty("Values").GetValue(f);
                    var vs = string.Join("; ", vals.Cast<object>().Select(v => { var t = v.GetType(); return $"[{t.GetProperty("ID").GetValue(v)} t{t.GetProperty("TimeOfDay").GetValue(v)}]{t.GetProperty("Value").GetValue(v)}"; }));
                    Console.WriteLine($"{System.IO.Path.GetFileName(path)} | {f.Key} | n={vals.Count} | {vs}");
                }
        }
    }
}
