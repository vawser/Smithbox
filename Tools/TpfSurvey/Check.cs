using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SoulsFormats;
using StudioCore.Editors.TextureViewer;
using System.Diagnostics;

/// <summary>
/// Checks the SoulsFormats Switch 2 path against every DB_NT texture: the TPF reader, the block linear layout
/// against the stored data size, the header's block height against the hardware rule, and the headerizer output
/// (DirectXTex must read it; ASTC output is counted for the decoder's error colour).
/// </summary>
static class Check
{
    public static void Run(string root, int limit)
    {
        var files = Directory.EnumerateFiles(root, "*.tpf.dcx", SearchOption.AllDirectories)
            .Select(p => (Path.GetRelativePath(root, p), (Func<byte[]>)(() => File.ReadAllBytes(p)))).ToList();
        foreach (var bhd in Directory.EnumerateFiles(root, "*.tpfbhd", SearchOption.AllDirectories))
        {
            var bxf = BXF4.Read(File.ReadAllBytes(bhd), File.ReadAllBytes(Path.ChangeExtension(bhd, ".tpfbdt")));
            foreach (var f in bxf.Files)
            {
                var bytes = f.Bytes.ToArray();
                files.Add((Path.GetRelativePath(root, bhd) + ":" + f.Name, () => bytes));
            }
        }

        int textures = 0, readFail = 0, sizeMismatch = 0, bhMismatch = 0, headerizeFail = 0, ddsFail = 0, astcTextures = 0;
        long astcPixels = 0, astcErrorPixels = 0;
        var astcErrorFiles = new List<string>();
        var sw = new Stopwatch();

        foreach (var (name, load) in files.Take(limit))
        {
            TPF tpf;
            try { tpf = TPF.Read(load()); }
            catch (Exception e) { readFail++; Console.WriteLine($"READ FAIL {name}: {e.Message}"); continue; }

            foreach (var tex in tpf.Textures)
            {
                textures++;
                var h = tex.Header;
                if (!Switch2Headerizer.TryGetFormatInfo(h.DXGIFormat, out int bw, out int bh, out int bpb))
                {
                    Console.WriteLine($"UNKNOWN FORMAT {h.DXGIFormat} {name}:{tex.Name}");
                    continue;
                }

                int rows = (h.Height + bh - 1) / bh;
                if (Switch2Headerizer.GetBlockHeightLog2(rows) != h.Unk2)
                {
                    bhMismatch++;
                    if (bhMismatch <= 10)
                        Console.WriteLine($"BLOCK HEIGHT {name}:{tex.Name} {h.Width}x{h.Height} dxgi {h.DXGIFormat} header {h.Unk2} computed {Switch2Headerizer.GetBlockHeightLog2(rows)}");
                }

                int layerSize = Switch2Headerizer.GetLayerLayout(h.Width, h.Height, Math.Max(1, (int)tex.Mipmaps), bw, bh, bpb, h.Unk2);
                int layers = Switch2Headerizer.GetLayerCount(tex);
                // Arrays and cubes pad every layer, even an array of one
                bool padded = tex.Type == TPF.TexType.Cubemap || tex.Type == TPF.TexType.TextureArray;
                long expected = padded ? (long)Switch2Headerizer.GetLayerStride(layerSize, h.Unk2) * layers : layerSize;
                if (expected != tex.Bytes.Length)
                {
                    sizeMismatch++;
                    if (sizeMismatch <= 20)
                        Console.WriteLine($"SIZE {name}:{tex.Name} {h.Width}x{h.Height} mips {tex.Mipmaps} type {tex.Type} count {h.TextureCount} dxgi {h.DXGIFormat} bh {h.Unk2}: data 0x{tex.Bytes.Length:X} layout 0x{expected:X}");
                }

                byte[] dds;
                try
                {
                    sw.Start();
                    dds = tex.Headerize();
                    sw.Stop();
                }
                catch (Exception e)
                {
                    sw.Stop();
                    headerizeFail++;
                    Console.WriteLine($"HEADERIZE FAIL {name}:{tex.Name}: {e.Message}");
                    continue;
                }

                if (AstcDecoder.TryGetBlockSize(h.DXGIFormat, out _, out _, out _))
                {
                    astcTextures++;
                    // Top mip of the first layer, straight after the DX10 header
                    int count = h.Width * h.Height;
                    int errors = 0;
                    for (int i = 0; i < count; i++)
                    {
                        int p = 0x94 + i * 4;
                        if (dds[p] == 0xFF && dds[p + 1] == 0 && dds[p + 2] == 0xFF && dds[p + 3] == 0xFF)
                            errors++;
                    }
                    astcPixels += count;
                    astcErrorPixels += errors;
                    if (errors > 0 && astcErrorFiles.Count < 20)
                        astcErrorFiles.Add($"{name}:{tex.Name} {errors}/{count}");
                }

                using var image = DdsDecoder.DecodeToImage(dds, out var error);
                if (image == null)
                {
                    ddsFail++;
                    if (ddsFail <= 10)
                        Console.WriteLine($"DDS FAIL {name}:{tex.Name} dxgi {h.DXGIFormat}: {error}");
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine($"{files.Count} TPFs, {textures} textures");
        Console.WriteLine($"  TPF read failures:        {readFail}");
        Console.WriteLine($"  block height mismatches:  {bhMismatch}");
        Console.WriteLine($"  layout size mismatches:   {sizeMismatch}");
        Console.WriteLine($"  headerize failures:       {headerizeFail}");
        Console.WriteLine($"  DirectXTex read failures: {ddsFail}");
        Console.WriteLine($"  ASTC textures: {astcTextures}, error-colour texels {astcErrorPixels}/{astcPixels}");
        foreach (var f in astcErrorFiles)
            Console.WriteLine($"    {f}");
        Console.WriteLine($"  headerize time {sw.Elapsed.TotalSeconds:F1}s");
    }

    /// <summary>Writes the top mip of each texture in a TPF as a PNG.</summary>
    public static void Preview(string path, string outDir)
    {
        Directory.CreateDirectory(outDir);
        var tpf = TPF.Read(DCX.Decompress(File.ReadAllBytes(path)).ToArray());
        foreach (var tex in tpf.Textures)
        {
            var dds = tex.Headerize();
            using var image = DdsDecoder.DecodeToImage(dds, out var error);
            if (image == null)
            {
                Console.WriteLine($"{tex.Name}: {error}");
                continue;
            }
            var file = Path.Combine(outDir, tex.Name + ".png");
            image.SaveAsPng(file);
            Console.WriteLine($"{tex.Name}: dxgi {tex.Header.DXGIFormat} {tex.Header.Width}x{tex.Header.Height} -> {file}");
        }
    }
}


static class RawDump
{
    public static void Run(string root, string file, string texName, int count)
    {
        var tpf = TPF.Read(DCX.Decompress(File.ReadAllBytes(Path.Combine(root, file))).ToArray());
        var t = tpf.Textures.First(x => x.Name == texName);
        Console.WriteLine($"{t.Name} {t.Header.Width}x{t.Header.Height} mips {t.Mipmaps} dxgi {t.Header.DXGIFormat} bh {t.Header.Unk2} unk3 0x{t.Header.Unk3:X} size 0x{t.Bytes.Length:X}");
        for (int i = 0; i < Math.Min(count, t.Bytes.Length); i += 16)
            Console.WriteLine($"{i:X5}: " + string.Join(" ", t.Bytes.Skip(i).Take(16).Select(b => b.ToString("X2"))));
    }
}
