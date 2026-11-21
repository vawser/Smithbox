using Hexa.NET.DirectXTex;
using SoulsFormats;
using StudioCore.Editors.TextureViewer;
using System.Text.RegularExpressions;
using DxImage = Hexa.NET.DirectXTex.Image;

namespace StudioCore.Port;

public enum TextureJobKind { Copy, MetalAlbedo, Metallic }

/// <summary>One ported texture: its AET name and how it is made.</summary>
public sealed class TextureJob
{
    public string Aet;
    public TextureJobKind Kind;
    /// <summary>The DS3 texture (Copy, MetalAlbedo) or the albedo the metallic map sits with (Metallic).</summary>
    public string Source;
    /// <summary>The DS3 specular map (MetalAlbedo, Metallic).</summary>
    public string Spec;
}

/// <summary>
/// The AET textures of a port (Material Mapping.md §8.3), one TPF per container (asset/aet/aetGGG/aetGGG_NNN.tpf.dcx)
/// with the fields of an ER AET TPF, plus its low-detail twin (_l):
/// <list type="bullet">
/// <item>Copy: the DS3 DDS as it is (DS3 and ER use the same formats per role)</item>
/// <item>MetalAlbedo: a metal material's albedo, linear lerp(albedo, F0, metallic), re-encoded in the source's format</item>
/// <item>Metallic: BC4 from the specular map's linear F0 luma through MetallicRamp</item>
/// </list>
/// A container the project already has keeps the textures this run doesn't write. Each written TPF is read back.
/// </summary>
public static class PortTextures
{
    private const int R8G8B8A8_UNORM = 28, R8G8B8A8_UNORM_SRGB = 29, BC4_UNORM = 80;

    public static void Run(PortConfig config, PortGame output, Dictionary<string, BinderFile> textures, IReadOnlyCollection<TextureJob> jobs,
        int group, PortLog log, Action<float> progress, CancellationToken cancel)
    {
        // An ER AET TPF with a metallic map, for the TPF and per-suffix texture fields
        var templatePath = output.Paths.FirstOrDefault(p => p.Equals("/asset/aet/aet210/aet210_012.tpf.dcx", StringComparison.OrdinalIgnoreCase))
            ?? output.Paths.First(p => Regex.IsMatch(p, @"^/asset/aet/aet\d{3}/aet\d{3}_\d{3}\.tpf\.dcx$", RegexOptions.IgnoreCase));
        var template = TPF.Read(output.Read(templatePath));
        var mTemplate = template.Textures.FirstOrDefault(t => Suffix(t.Name) == "m");

        var containers = jobs.GroupBy(j => j.Aet[..10], StringComparer.OrdinalIgnoreCase).OrderBy(g => g.Key).ToList();
        int missing = 0, done = 0, checkedTextures = 0;

        foreach (var container in containers)
        {
            cancel.ThrowIfCancellationRequested();
            var path = $"/asset/aet/aet{group:000}/{container.Key.ToLowerInvariant()}.tpf.dcx";
            var lowPath = path.Replace(".tpf.dcx", "_l.tpf.dcx");

            var tpf = new TPF { Platform = template.Platform, Encoding = template.Encoding, Flag2 = template.Flag2, Compression = template.Compression };
            var made = new Dictionary<string, (TextureJob Job, double Mean)>(StringComparer.OrdinalIgnoreCase);

            foreach (var job in container.OrderBy(j => j.Aet))
            {
                byte format, flags1;
                byte[] dds;
                double mean = double.NaN;

                if (job.Kind == TextureJobKind.Metallic)
                {
                    dds = MetallicMap(config, textures, job.Spec, out mean);
                    if (dds == null) { missing++; log.Warn($"{job.Aet}: no specular map {job.Spec}"); continue; }
                    (format, flags1) = (mTemplate?.Format ?? 103, mTemplate?.Flags1 ?? 0);
                }
                else
                {
                    var src = Source(textures, job.Source);
                    if (src == null) { missing++; log.Warn($"{job.Aet}: {job.Source} is not in the DS3 area's texture binders"); continue; }
                    (format, flags1, dds) = (src.Format, src.Flags1, src.Bytes.ToArray());
                    if (job.Kind == TextureJobKind.MetalAlbedo)
                    {
                        dds = Recolour(config, textures, dds, job.Spec);
                        if (dds == null) { missing++; log.Warn($"{job.Aet}: {job.Source} could not be re-encoded"); continue; }
                    }
                }

                tpf.Textures.Add(new TPF.Texture(job.Aet, format, flags1, dds, tpf.Platform));
                made[job.Aet] = (job, mean);
            }

            if (tpf.Textures.Count > 0)
            {
                // Textures an earlier run put in this container stay
                Keep(output, path, tpf, made.Keys, "");
                output.Write(path, tpf.Write());

                var low = new TPF { Platform = tpf.Platform, Encoding = tpf.Encoding, Flag2 = tpf.Flag2, Compression = tpf.Compression };
                foreach (var t in tpf.Textures.Where(t => made.ContainsKey(t.Name)))
                {
                    var lowDds = LowMips(t.Bytes, LowCap(Suffix(t.Name)));
                    if (lowDds == null) { log.Warn($"{t.Name}: no low-detail mips (format not handled), the full texture is used"); lowDds = t.Bytes; }
                    low.Textures.Add(new TPF.Texture(t.Name + "_l", t.Format, t.Flags1, lowDds, low.Platform));
                }
                Keep(output, lowPath, low, made.Keys.Select(k => k + "_l"), "_l");
                output.Write(lowPath, low.Write());

                checkedTextures += Check(output, path, lowPath, made, textures, log);
            }

            progress((float)++done / containers.Count);
        }

        log.Line($"Textures: {jobs.Count} in {containers.Count} containers of aet{group:000}, {missing} missing; {checkedTextures} read back (with _l twins)");
    }

    /// <summary>Adds the textures of the project's existing copy that this run doesn't write.</summary>
    private static void Keep(PortGame output, string path, TPF tpf, IEnumerable<string> written, string suffix)
    {
        var existing = output.ReadProjectOnly(path);
        if (existing == null) return;
        var names = written.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var t in TPF.Read(existing).Textures.Where(t => !names.Contains(t.Name)))
            tpf.Textures.Add(t);
    }

    private static int Check(PortGame output, string path, string lowPath, Dictionary<string, (TextureJob Job, double Mean)> made,
        Dictionary<string, BinderFile> textures, PortLog log)
    {
        int count = 0;
        var tpf = TPF.Read(output.Read(path));
        var low = TPF.Read(output.Read(lowPath));
        var lowByName = low.Textures.ToDictionary(t => t.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var tex in tpf.Textures.Where(t => made.ContainsKey(t.Name)))
        {
            var (job, mean) = made[tex.Name];
            using var image = DdsDecoder.DecodeToImage(tex.Bytes, out var error);
            if (!log.Check(image != null, $"{tex.Name}: decodes ({error})")) continue;

            if (job.Kind == TextureJobKind.Copy)
                log.Check(tex.Bytes.AsSpan().SequenceEqual(Source(textures, job.Source).Bytes), $"{tex.Name}: byte-equal to {job.Source}");
            else if (job.Kind == TextureJobKind.Metallic)
            {
                double sum = 0;
                image.ProcessPixelRows(rows => { for (int y = 0; y < rows.Height; y++) foreach (var p in rows.GetRowSpan(y)) sum += p.R / 255.0; });
                log.Check(Math.Abs(sum / (image.Width * image.Height) - mean) < 0.02, $"{tex.Name}: metallic mean after BC4");
            }

            if (log.Check(lowByName.TryGetValue(tex.Name + "_l", out var l), $"{tex.Name}: _l twin"))
            {
                var h = new DDS(l.Bytes);
                // A twin cut from the main texture's own mips is over the cap only when the source has too few mips
                if (Math.Max(h.dwWidth, h.dwHeight) > LowCap(Suffix(tex.Name)))
                {
                    if (TailEquals(tex.Bytes, l.Bytes))
                        log.Warn($"{l.Name}: the source has too few mips to shrink to {LowCap(Suffix(tex.Name))}, so the twin stays {h.dwWidth}x{h.dwHeight}");
                    else
                        log.Fail($"{l.Name}: {h.dwWidth}x{h.dwHeight} is over its size cap");
                }
                log.Check(TailEquals(tex.Bytes, l.Bytes), $"{l.Name}: the main texture's mip tail");
            }
            count += 2;
        }
        return count;
    }

    private static string Suffix(string name) => name[(name.LastIndexOf('_') + 1)..].ToLowerInvariant();

    private static TPF.Texture Source(Dictionary<string, BinderFile> textures, string name) =>
        name != null && textures.TryGetValue(name, out var f) ? TPF.Read(f.Bytes).Textures.FirstOrDefault() : null;

    // ---- pixels -------------------------------------------------------------------------------------------------

    private static float ToSrgb(float v) => v <= 0.0031308f ? v * 12.92f : 1.055f * MathF.Pow(v, 1 / 2.4f) - 0.055f;

    private static float Metallic(PortConfig config, float r, float g, float b)
    {
        var f0 = 0.2126f * r + 0.7152f * g + 0.0722f * b;
        var t = Math.Clamp((f0 - config.MetallicRamp[0]) / (config.MetallicRamp[1] - config.MetallicRamp[0]), 0, 1);
        return t * t * (3 - 2 * t);
    }

    /// <summary>BC4 metallic map at the specular map's size (at least 4x4).</summary>
    private static byte[] MetallicMap(PortConfig config, Dictionary<string, BinderFile> textures, string spec, out double mean)
    {
        mean = 0;
        var (rgb, w, h) = PortPieces.Specular(textures, spec);
        if (rgb == null) return null;

        int ow = Math.Max(w, 4), oh = Math.Max(h, 4);
        var pixels = new byte[ow * oh * 4];
        double sum = 0;
        for (int y = 0; y < oh; y++)
            for (int x = 0; x < ow; x++)
            {
                var i = ((y * h / oh) * w + x * w / ow) * 3;
                var v = (byte)Math.Round(Metallic(config, rgb[i], rgb[i + 1], rgb[i + 2]) * 255);
                var o = (y * ow + x) * 4;
                pixels[o] = pixels[o + 1] = pixels[o + 2] = v;
                pixels[o + 3] = 255;
                sum += v / 255.0;
            }
        mean = sum / (ow * oh);
        return Encode(pixels, ow, oh, R8G8B8A8_UNORM, BC4_UNORM, false, "ATI1");
    }

    /// <summary>The albedo, linear lerp(albedo, F0, metallic), alpha kept, in the source's DXGI format and header style.</summary>
    private static byte[] Recolour(PortConfig config, Dictionary<string, BinderFile> textures, byte[] sourceDds, string spec)
    {
        var (rgb, sw, sh) = PortPieces.Specular(textures, spec);
        using var albedo = DdsDecoder.DecodeToImage(sourceDds, out _);
        if (rgb == null || albedo == null) return null;

        var dx10 = sourceDds.Length >= 148 && System.Text.Encoding.ASCII.GetString(sourceDds, 84, 4) == "DX10";
        var legacy = dx10 ? null : System.Text.Encoding.ASCII.GetString(sourceDds, 84, 4);
        var target = dx10 ? BitConverter.ToInt32(sourceDds, 128) : legacy switch { "DXT5" => 78, "DXT3" => 75, _ => 72 };

        int w = albedo.Width, h = albedo.Height;
        var pixels = new byte[w * h * 4];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                var p = albedo[x, y];
                var i = ((y * sh / h) * sw + x * sw / w) * 3;
                var m = Metallic(config, rgb[i], rgb[i + 1], rgb[i + 2]);
                var o = (y * w + x) * 4;
                byte Mix(byte c, float f0) => (byte)Math.Round(Math.Clamp(ToSrgb(PortPieces.ToLinear(c / 255f) * (1 - m) + f0 * m), 0, 1) * 255);
                pixels[o] = Mix(p.R, rgb[i]);
                pixels[o + 1] = Mix(p.G, rgb[i + 1]);
                pixels[o + 2] = Mix(p.B, rgb[i + 2]);
                pixels[o + 3] = p.A;
            }
        return Encode(pixels, w, h, R8G8B8A8_UNORM_SRGB, target, dx10, legacy);
    }

    /// <summary>RGBA8 pixels -> full mip chain -> block compressed DDS (CPU; BC7 in quick mode).</summary>
    private static unsafe byte[] Encode(byte[] pixels, int w, int h, int work, int target, bool forceDx10, string legacyFourCC)
    {
        var src = DirectXTex.CreateScratchImage();
        var mipped = DirectXTex.CreateScratchImage();
        var encoded = DirectXTex.CreateScratchImage();
        var blob = DirectXTex.CreateBlob();
        try
        {
            if (src.Initialize2D(work, (nuint)w, (nuint)h, 1, 1, CPFlags.None).IsFailure) return null;
            DxImage* img = src.GetImage(0, 0, 0);
            for (int y = 0; y < h; y++)
                pixels.AsSpan(y * w * 4, w * 4).CopyTo(new Span<byte>(img->Pixels + (long)y * (long)img->RowPitch, w * 4));

            int levels = 1;
            for (int s = Math.Max(w, h); s > 1; s >>= 1) levels++;
            if (DirectXTex.GenerateMipMaps(img, TexFilterFlags.Default, (nuint)levels, &mipped, false).IsFailure) return null;

            var meta = mipped.GetMetadata();
            var bc7 = target is 98 or 99;
            if (DirectXTex.Compress2(mipped.GetImages(), mipped.GetImageCount(), &meta, target,
                    TexCompressFlags.Parallel | (bc7 ? TexCompressFlags.Bc7Quick : TexCompressFlags.Default), 0.5f, &encoded).IsFailure) return null;

            var outMeta = encoded.GetMetadata();
            if (DirectXTex.SaveToDDSMemory2(encoded.GetImages(), encoded.GetImageCount(), &outMeta, forceDx10 ? DDSFlags.ForceDx10Ext : DDSFlags.None, &blob).IsFailure) return null;
            var dds = new ReadOnlySpan<byte>(blob.GetBufferPointer(), (int)blob.GetBufferSize()).ToArray();

            // DirectXTex writes BC4 as "BC4U"; the games (and SoulsFormats) spell it "ATI1"
            if (!forceDx10 && legacyFourCC?.Length == 4 && dds.Length > 0x58 && System.Text.Encoding.ASCII.GetString(dds, 0x54, 4) is "BC4U")
                System.Text.Encoding.ASCII.GetBytes(legacyFourCC, 0, 4, dds, 0x54);
            return dds;
        }
        finally
        {
            DirectXTex.BlobRelease(blob);
            src.Release();
            mipped.Release();
            encoded.Release();
        }
    }

    // ---- low-detail twins ---------------------------------------------------------------------------------------

    /// <summary>ER's _l twins: normals at most 128, everything else 256; smaller ones as they are.</summary>
    private static int LowCap(string suffix) => suffix == "n" ? 128 : 256;

    private static int HeaderSize(byte[] dds) => dds.Length >= 148 && System.Text.Encoding.ASCII.GetString(dds, 84, 4) == "DX10" ? 148 : 128;

    private static bool TailEquals(byte[] main, byte[] low)
    {
        var lowData = low.AsSpan(HeaderSize(low));
        var mainData = main.AsSpan(HeaderSize(main));
        return lowData.Length <= mainData.Length && mainData[^lowData.Length..].SequenceEqual(lowData);
    }

    /// <summary>
    /// The same texture from the first mip level whose larger side is within cap: header copied with the new size, mip
    /// count and linear size, pixel data the tail of the chain. Null for a format it can't size.
    /// </summary>
    private static byte[] LowMips(byte[] dds, int cap)
    {
        var header = HeaderSize(dds);
        int w = BitConverter.ToInt32(dds, 16), h = BitConverter.ToInt32(dds, 12), mips = Math.Max(1, BitConverter.ToInt32(dds, 28));
        var fourcc = System.Text.Encoding.ASCII.GetString(dds, 84, 4);
        var dxgi = header == 148 ? BitConverter.ToInt32(dds, 128) : 0;
        int block = fourcc switch
        {
            "DXT1" or "ATI1" or "BC4U" or "BC4S" => 8,
            "DXT3" or "DXT5" or "ATI2" or "BC5U" or "BC5S" => 16,
            "DX10" => dxgi switch { >= 70 and <= 72 or >= 79 and <= 81 => 8, >= 73 and <= 78 or >= 82 and <= 84 or >= 94 and <= 99 => 16, 28 or 29 or 87 or 88 or 91 or 93 => -4, _ => 0 },
            _ => 0
        };
        if (block == 0) return null;

        long Size(int lw, int lh) => block < 0 ? (long)lw * lh * -block : (long)Math.Max(1, (lw + 3) / 4) * Math.Max(1, (lh + 3) / 4) * block;

        int k = 0;
        long skip = 0;
        while (k < mips - 1 && Math.Max(Math.Max(1, w >> k), Math.Max(1, h >> k)) > cap) { skip += Size(Math.Max(1, w >> k), Math.Max(1, h >> k)); k++; }
        if (k == 0) return dds;

        int nw = Math.Max(1, w >> k), nh = Math.Max(1, h >> k);
        var result = new byte[dds.Length - skip];
        dds.AsSpan(0, header).CopyTo(result);
        dds.AsSpan(header + (int)skip).CopyTo(result.AsSpan(header));
        BitConverter.GetBytes(nh).CopyTo(result, 12);
        BitConverter.GetBytes(nw).CopyTo(result, 16);
        BitConverter.GetBytes((int)(block < 0 ? (long)nw * -block : Size(nw, nh))).CopyTo(result, 20);
        BitConverter.GetBytes(mips - k).CopyTo(result, 28);
        return result;
    }
}
