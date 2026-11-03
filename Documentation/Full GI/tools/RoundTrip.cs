using Andre.IO.VFS;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
public static class RoundTrip
{
    static int ok, fail, notIs;
    static void Check(string label, BND4 bnd)
    {
        foreach (var f in bnd.Files)
        {
            var src = f.Bytes.ToArray();
            if (!IVINFO.Is(src)) { notIs++; continue; }
            var iv = IVINFO.Read(src);
            var dst = iv.Write();
            if (src.SequenceEqual(dst)) ok++; else { fail++; if (fail < 5) Console.WriteLine($"MISMATCH {label} {f.Name}"); }
        }
    }
    public static void Run(string game, string root)
    {
        ok = fail = notIs = 0;
        if (game == "ER")
        {
            using var vfs = ArchiveBinderVirtualFileSystem.FromGameFolder(root, Andre.Core.Game.ER);
            foreach (var p in vfs.GetFileNamesMatchingRecursive("/map", @".*\.ivinfobnd\.dcx$").Distinct())
                Check(p, BND4.Read(vfs.ReadFileOrThrow(p)));
        }
        else
            foreach (var p in Directory.EnumerateFiles(root, "*.ivinfobnd.dcx", SearchOption.AllDirectories))
                Check(p, BND4.Read(p));
        Console.WriteLine($"[{game}] identical={ok} mismatched={fail} rejectedByIs={notIs}");
        var sample = IVINFO.Read(new IVINFO().Write());
        Console.WriteLine($"  default roundtrip: {sample}");
    }
}
