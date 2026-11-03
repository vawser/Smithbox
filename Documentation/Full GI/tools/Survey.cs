using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
public static class Survey
{
    public static void Run(string game, string root)
    {
        var sizes = new Dictionary<int, int>(); var i0 = new Dictionary<int, int>(); var i4 = new Dictionary<int, int>(); var tail = new Dictionary<float, int>();
        var bndInfo = new Dictionary<string, int>();
        int files = 0, wNeg = 0, wZero = 0, bnds = 0; float minW = float.MaxValue, maxW = 0, maxRatio = 0;
        var nameNotMatch = 0; int[] pos = new int[3], dom = new int[3]; int nW = 0;
        foreach (var p in Directory.EnumerateFiles(root, "*.ivinfobnd.dcx", SearchOption.AllDirectories))
        {
            bnds++;
            var bnd = BND4.Read(p);
            string k = $"v={bnd.Version} fmt={bnd.Format} unicode={bnd.Unicode} ext={bnd.Extended} big={bnd.BigEndian} bitbig={bnd.BitBigEndian} dcx={bnd.Compression}";
            bndInfo[k] = bndInfo.GetValueOrDefault(k) + 1;
            for (int idx = 0; idx < bnd.Files.Count; idx++)
            {
                var f = bnd.Files[idx];
                if (f.ID != idx) nameNotMatch++;
                var b = f.Bytes.ToArray(); files++;
                sizes[b.Length] = sizes.GetValueOrDefault(b.Length) + 1;
                if (b.Length < 28) continue;
                int a = BitConverter.ToInt32(b, 0), c = BitConverter.ToInt32(b, 4);
                i0[a] = i0.GetValueOrDefault(a) + 1; i4[c] = i4.GetValueOrDefault(c) + 1;
                float x = BitConverter.ToSingle(b, 8), y = BitConverter.ToSingle(b, 12), z = BitConverter.ToSingle(b, 16), w = BitConverter.ToSingle(b, 20), t = BitConverter.ToSingle(b, 24);
                tail[t] = tail.GetValueOrDefault(t) + 1;
                if (w > 0) { pos[0] += x > 0 ? 1 : 0; pos[1] += y > 0 ? 1 : 0; pos[2] += z > 0 ? 1 : 0; dom[Array.IndexOf(new[]{MathF.Abs(x),MathF.Abs(y),MathF.Abs(z)}, new[]{MathF.Abs(x),MathF.Abs(y),MathF.Abs(z)}.Max())]++; nW++; }
                if (w < 0) wNeg++; if (w == 0) wZero++;
                if (w > 0) { minW = Math.Min(minW, w); maxW = Math.Max(maxW, w); maxRatio = Math.Max(maxRatio, MathF.Sqrt(x * x + y * y + z * z) / w); }
            }
        }
        Console.WriteLine($"[{game}] bnds={bnds} files={files} idMismatch={nameNotMatch}");
        foreach (var kv in bndInfo) Console.WriteLine($"  bnd {kv.Key}: {kv.Value}");
        Console.WriteLine("  sizes: " + string.Join(", ", sizes.Select(kv => $"{kv.Key}x{kv.Value}")));
        Console.WriteLine("  @0: " + string.Join(", ", i0.Select(kv => $"{kv.Key}x{kv.Value}")));
        Console.WriteLine("  @4: " + string.Join(", ", i4.Select(kv => $"{kv.Key}x{kv.Value}")));
        Console.WriteLine("  @24: " + string.Join(", ", tail.Select(kv => $"{kv.Key}x{kv.Value}")));
        Console.WriteLine($"  positive x/y/z = {pos[0]*100/nW}% {pos[1]*100/nW}% {pos[2]*100/nW}%  dominant axis x/y/z = {dom[0]*100/nW}% {dom[1]*100/nW}% {dom[2]*100/nW}%");
        Console.WriteLine($"  w<0={wNeg} w==0={wZero} wRange=[{minW},{maxW}] max|xyz|/w={maxRatio}");
    }
}
