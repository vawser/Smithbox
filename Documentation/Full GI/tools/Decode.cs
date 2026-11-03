using DirectXTexNet;
using SoulsFormats;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
public static class Decode
{
    // args: mapdir mapname quality index(volume tag e.g. GIIV0500_00) tpfPattern(ac6|nr)
    public static unsafe void Run(string ivBnd, string[] tpfs)
    {
        var iv = BND4.Read(ivBnd);
        var ivByName = iv.Files.ToDictionary(f => Path.GetFileNameWithoutExtension(f.Name).Split('_').Skip(4).Aggregate((a, b) => a + "_" + b), f => f.Bytes.ToArray());
        var tex = new System.Collections.Generic.Dictionary<string, TPF.Texture>();
        foreach (var p in tpfs)
        {
            if (p.Contains("tpfbnd")) foreach (var f in BND4.Read(p).Files) foreach (var t in TPF.Read(f.Bytes).Textures) tex[t.Name] = t;
            else foreach (var t in TPF.Read(p).Textures) tex[t.Name] = t;
        }
        foreach (var kv in ivByName.Take(8))
        {
            var b = kv.Value;
            Console.WriteLine($"{kv.Key}: iv = ({BitConverter.ToSingle(b, 8):G5}, {BitConverter.ToSingle(b, 12):G5}, {BitConverter.ToSingle(b, 16):G5}, {BitConverter.ToSingle(b, 20):G5})");
            foreach (var ch in new[] { "W", "X", "Y", "Z" })
            {
                var t = tex.Values.FirstOrDefault(x => x.Name.EndsWith(kv.Key + "_" + ch));
                if (t == null) { Console.WriteLine($"   {ch}: missing"); continue; }
                var bytes = t.Bytes.ToArray();
                fixed (byte* pb = bytes)
                {
                    using var img = TexHelper.Instance.LoadFromDDSMemory((IntPtr)pb, bytes.Length, DDS_FLAGS.NONE);
                    using var dec = img.Decompress(DXGI_FORMAT.R32G32B32A32_FLOAT);
                    var md = dec.GetMetadata();
                    double[] sum = new double[4], mn = { 9, 9, 9, 9 }, mx = { -9, -9, -9, -9 }; long n = 0;
                    for (int i = 0; i < dec.GetImageCount(); i++)
                    {
                        var im = dec.GetImage(i);
                        float* px = (float*)im.Pixels;
                        for (long k = 0; k < (long)im.Width * im.Height; k++)
                            for (int c = 0; c < 4; c++) { double v = px[k * 4 + c]; sum[c] += v; mn[c] = Math.Min(mn[c], v); mx[c] = Math.Max(mx[c], v); if (c == 0) n++; }
                    }
                    Console.WriteLine($"   {ch} {md.Width}x{md.Height}x{md.Depth}: mean=({sum[0]/n:F4},{sum[1]/n:F4},{sum[2]/n:F4},{sum[3]/n:F4}) min=({mn[0]:F3},{mn[1]:F3},{mn[2]:F3},{mn[3]:F3}) max=({mx[0]:F3},{mx[1]:F3},{mx[2]:F3},{mx[3]:F3})");
                }
            }
        }
    }
}
