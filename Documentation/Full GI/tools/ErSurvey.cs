using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Linq;
using System.Collections.Generic;
public static class ErSurvey
{
    public static void Run(string folder)
    {
        using var vfs = ArchiveBinderVirtualFileSystem.FromGameFolder(folder, Andre.Core.Game.ER);
        int bnds = 0, files = 0, bad = 0, wNeg = 0; var tails = new System.Collections.Generic.Dictionary<float, int>(); var dcx = new System.Collections.Generic.Dictionary<string, int>();
        bool dumped = false;
        foreach (var path in vfs.GetFileNamesMatchingRecursive("/map", @".*.ivinfobnd.dcx$").Distinct())
        {
            var data = vfs.ReadFileOrThrow(path);
            var bnd = BND4.Read(data);
            bnds++; dcx[bnd.Compression.ToString()] = dcx.GetValueOrDefault(bnd.Compression.ToString()) + 1;
            foreach (var f in bnd.Files)
            {
                var b = f.Bytes.ToArray(); files++;
                if (b.Length != 28 || BitConverter.ToInt32(b, 0) != 1 || BitConverter.ToInt32(b, 4) != 0) { bad++; Console.WriteLine($"odd {f.Name} len={b.Length}"); continue; }
                float t = BitConverter.ToSingle(b, 24); tails[t] = tails.GetValueOrDefault(t) + 1;
                if (BitConverter.ToSingle(b, 20) < 0) wNeg++;
                if (!dumped) { Console.WriteLine($"{path}: {f.Name} {BitConverter.ToString(b)}"); dumped = true; }
            }
        }
        Console.WriteLine($"[ER] bnds={bnds} files={files} odd={bad} w<0={wNeg} dcx={string.Join(",", dcx.Select(k => k.Key + "x" + k.Value))} @24={string.Join(",", tails.Select(k => k.Key + "x" + k.Value))}");
    }
}
