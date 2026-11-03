using SoulsFormats;
using System;
using System.IO;
using System.Linq;
public static class Shaders
{
    public static void Extract(string src, string outDir)
    {
        foreach (var p in Directory.GetFiles(src, "*.dcx").Where(p => p.Contains("shaderbnd") || p.Contains("shaderbdlebnd")))
        {
            var dir = Path.Combine(outDir, Path.GetFileName(p).Split('.')[0]);
            int n = Dump(BND4.Read(p), dir);
            Console.WriteLine($"{Path.GetFileName(p)}: {n} files");
        }
    }
    static int Dump(BND4 bnd, string dir)
    {
        int n = 0;
        Directory.CreateDirectory(dir);
        foreach (var f in bnd.Files)
        {
            var name = f.Name.Replace((char)92, (char)47).Split((char)47).Last();
            if (BND4.IsRead(f.Bytes, out BND4 inner)) { n += Dump(inner, Path.Combine(dir, name)); continue; }
            File.WriteAllBytes(Path.Combine(dir, name), f.Bytes.ToArray()); n++;
        }
        return n;
    }
}
