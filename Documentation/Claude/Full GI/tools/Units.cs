using DirectXTexNet;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
// Mean luminance of each volume's L0 (and of the linear SH over all normals, which is L0) decoded with base 2 and
// base e, against the IVINFO ShLuminance.w
public static class Units
{
    public static unsafe void Run(string ivBnd, string[] tpfs)
    {
        var iv = BND4.Read(ivBnd).Files.ToDictionary(
            f => System.Text.RegularExpressions.Regex.Match(f.Name, @"GIIV\d+_\d+").Value, f => IVINFO.Read(f.Bytes));
        var tex = new Dictionary<string, TPF.Texture>();
        foreach (var p in tpfs)
        {
            if (p.Contains("tpfbnd")) foreach (var f in BND4.Read(p).Files) foreach (var t in TPF.Read(f.Bytes).Textures) tex[t.Name] = t;
            else foreach (var t in TPF.Read(p).Textures) tex[t.Name] = t;
        }
        foreach (var kv in iv.Take(12))
        {
            var w = tex.Values.FirstOrDefault(t => t.Name.EndsWith(kv.Key + "_W"));
            if (w == null) continue;
            var bytes = w.Bytes.ToArray();
            double sum2 = 0, sumE = 0; long n = 0;
            fixed (byte* pb = bytes)
            {
                using var img = TexHelper.Instance.LoadFromDDSMemory((IntPtr)pb, bytes.Length, DDS_FLAGS.NONE);
                using var dec = img.Decompress(DXGI_FORMAT.R32G32B32A32_FLOAT);
                for (int i = 0; i < dec.GetImageCount(); i++)
                {
                    var im = dec.GetImage(i); float* px = (float*)im.Pixels;
                    for (long k = 0; k < (long)im.Width * im.Height; k++)
                    {
                        float r = px[k * 4], g = px[k * 4 + 1], b = px[k * 4 + 2], a = px[k * 4 + 3];
                        double luma = 0.299 * (r - 0.5) + 0.587 * (g - 0.5) + 0.114 * (b - 0.5);
                        sum2 += luma * Math.Pow(2, (a - 0.5) * 20) * 2;
                        sumE += luma * Math.Exp((a - 0.5) * 20) * 2;
                        n++;
                    }
                }
            }
            Console.WriteLine($"{kv.Key}: ivinfo.w={kv.Value.ShLuminance.W:G4}  meanL0 base2={sum2 / n:G4} (ratio {kv.Value.ShLuminance.W / (sum2 / n):G3})  baseE={sumE / n:G4} (ratio {kv.Value.ShLuminance.W / (sumE / n):G3})");
        }
    }
}
