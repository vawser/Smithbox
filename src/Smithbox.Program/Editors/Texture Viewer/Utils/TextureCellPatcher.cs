using Hexa.NET.DirectXTex;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using DxImage = Hexa.NET.DirectXTex.Image;

namespace StudioCore.Editors.TextureViewer;

/// <summary>An image to paste into one layout cell of a sheet.</summary>
public sealed record CellPatch(string Name, int X, int Y, int Width, int Height, byte[] ImageBytes, string Extension);

/// <summary>
/// Replaces the pixels of layout cells in a sheet (Texture Editor plan, T6 "replace icon", brought forward).
/// Only the compressed blocks a cell covers are re-encoded, on every mip level, and they are written into a copy of
/// the original DDS: the header and every other block keep their bytes, so the rest of the sheet is untouched
/// (re-encoding a whole 4096×2048 BC7 sheet would recompress every other icon). Images are scaled to the cell's
/// size. Works on 2D textures in 8-bit and BC1–BC5/BC7 formats.
/// </summary>
public static unsafe class TextureCellPatcher
{
    public static TextureEncodeResult Patch(byte[] dds, IReadOnlyList<CellPatch> patches)
    {
        var timer = Stopwatch.StartNew();
        var result = new TextureEncodeResult();

        var original = DirectXTex.CreateScratchImage();
        var top = DirectXTex.CreateScratchImage();
        var chain = DirectXTex.CreateScratchImage();

        try
        {
            TexMetadata meta;
            HexaGen.Runtime.HResult hr;
            fixed (byte* p = dds)
                hr = DirectXTex.LoadFromDDSMemory(p, (nuint)dds.Length, DDSFlags.None, &meta, &original);

            if (hr.IsFailure)
                return Fail(result, $"Failed to read the sheet (0x{hr.Value:X8})");

            if (meta.ArraySize != 1 || meta.Depth != 1 || meta.Dimension != TexDimension.Texture2D)
                return Fail(result, "Cells can only be replaced on 2D textures");

            int format = meta.Format;
            bool compressed = DirectXTex.IsCompressed(format);
            int bpp = (int)DirectXTex.BitsPerPixel(format);

            if (format is TextureEncoder.BC6H_UF16 or TextureEncoder.BC6H_SF16 || bpp == 0 || (!compressed && bpp % 8 != 0) || bpp > 32)
                return Fail(result, $"Cells can't be replaced in DXGI format {format}");

            int blockSize = compressed ? 4 : 1;
            int unitBytes = compressed ? bpp * 16 / 8 : bpp / 8;
            int work = DirectXTex.IsSRGB(format) ? TextureEncoder.R8G8B8A8_UNORM_SRGB : TextureEncoder.R8G8B8A8_UNORM;

            // The top level as RGBA, in the sheet's colour space (no sRGB conversion either way)
            var source = original.GetImage(0, 0, 0);
            hr = compressed
                ? DirectXTex.Decompress(source, work, &top)
                : DirectXTex.Convert(source, work, TexFilterFlags.Default, 0.5f, &top);
            if (hr.IsFailure)
                return Fail(result, $"Failed to decode the sheet (0x{hr.Value:X8})");

            var topImage = top.GetImage(0, 0, 0);

            // Paste each image into its cell
            foreach (var patch in patches)
            {
                var error = Paste(topImage, patch);
                if (error != null)
                    return Fail(result, $"{patch.Name}: {error}");
            }

            // Lower mips from the patched top
            int levels = (int)meta.MipLevels;
            if (levels > 1)
            {
                hr = DirectXTex.GenerateMipMaps(topImage, TexFilterFlags.Default, (nuint)levels, &chain, false);
                if (hr.IsFailure)
                    return Fail(result, $"Failed to generate mipmaps (0x{hr.Value:X8})");
            }

            // Where each level's data is in the DDS
            int dataOffset = dds.Length > 0x58 && dds[0x54] == 'D' && dds[0x55] == 'X' && dds[0x56] == '1' && dds[0x57] == '0' ? 0x94 : 0x80;
            var output = (byte[])dds.Clone();
            long offset = dataOffset;

            for (int level = 0; level < levels; level++)
            {
                int w = Math.Max(1, (int)meta.Width >> level);
                int h = Math.Max(1, (int)meta.Height >> level);

                nuint rowPitch, slicePitch;
                hr = DirectXTex.ComputePitch(format, (nuint)w, (nuint)h, &rowPitch, &slicePitch, CPFlags.None);
                if (hr.IsFailure)
                    return Fail(result, "Failed to compute the sheet's layout");

                if (offset + (long)slicePitch > output.Length)
                    return Fail(result, "The sheet's data is shorter than its header says");

                var levelImage = level == 0 ? topImage : chain.GetImage((nuint)level, 0, 0);

                foreach (var patch in patches)
                {
                    // The patch's area on this level, grown to whole blocks
                    int x0 = (patch.X >> level) / blockSize * blockSize;
                    int y0 = (patch.Y >> level) / blockSize * blockSize;
                    int x1 = Math.Min(w, CeilDiv(CeilShift(patch.X + patch.Width, level), blockSize) * blockSize);
                    int y1 = Math.Min(h, CeilDiv(CeilShift(patch.Y + patch.Height, level), blockSize) * blockSize);
                    x0 = Math.Clamp(x0, 0, w); y0 = Math.Clamp(y0, 0, h);

                    if (x1 <= x0 || y1 <= y0)
                        continue;

                    var error = EncodeRegion(levelImage, x0, y0, x1 - x0, y1 - y0, format, work, compressed, blockSize, unitBytes,
                        output, offset, (long)rowPitch, result);

                    if (error != null)
                        return Fail(result, error);
                }

                offset += (long)slicePitch;
            }

            result.Dds = output;
            result.Width = (int)meta.Width;
            result.Height = (int)meta.Height;
            result.MipLevels = levels;
            result.Format = format;
            return result;
        }
        catch (Exception e)
        {
            return Fail(result, e.Message);
        }
        finally
        {
            original.Release();
            top.Release();
            chain.Release();
            result.Milliseconds = timer.ElapsedMilliseconds;
        }
    }

    /// <summary>Scales the image to the cell and writes it into the RGBA top level.</summary>
    private static string Paste(DxImage* target, CellPatch patch)
    {
        Image<Rgba32> img;

        if (patch.Extension.Equals(".dds", StringComparison.OrdinalIgnoreCase))
        {
            img = DdsDecoder.DecodeToImage(patch.ImageBytes, out var error);
            if (img == null)
                return error;
        }
        else
        {
            img = SixLabors.ImageSharp.Image.Load<Rgba32>(patch.ImageBytes);
        }

        using (img)
        {
            if (patch.Width <= 0 || patch.Height <= 0)
                return "the cell is empty";

            if (img.Width != patch.Width || img.Height != patch.Height)
                img.Mutate(ctx => ctx.Resize(patch.Width, patch.Height));

            var pixels = new byte[patch.Width * patch.Height * 4];
            img.CopyPixelDataTo(pixels);

            int sheetW = (int)target->Width, sheetH = (int)target->Height;

            for (int y = 0; y < patch.Height; y++)
            {
                int ty = patch.Y + y;
                if (ty < 0 || ty >= sheetH)
                    continue;

                int sx = Math.Max(0, -patch.X);
                int tx = patch.X + sx;
                int count = Math.Min(patch.Width - sx, sheetW - tx);
                if (count <= 0)
                    continue;

                pixels.AsSpan((y * patch.Width + sx) * 4, count * 4)
                    .CopyTo(new Span<byte>(target->Pixels + (long)ty * (long)target->RowPitch + (long)tx * 4, count * 4));
            }
        }

        return null;
    }

    /// <summary>Encodes one block-aligned region of an RGBA level and writes its blocks into the DDS bytes.</summary>
    private static string EncodeRegion(DxImage* level, int x, int y, int w, int h, int format, int work, bool compressed,
        int blockSize, int unitBytes, byte[] output, long levelOffset, long levelRowPitch, TextureEncodeResult result)
    {
        var region = DirectXTex.CreateScratchImage();
        var encoded = DirectXTex.CreateScratchImage();

        try
        {
            var hr = region.Initialize2D(work, (nuint)w, (nuint)h, 1, 1, CPFlags.None);
            if (hr.IsFailure)
                return "Failed to allocate a region";

            var r = region.GetImage(0, 0, 0);
            for (int row = 0; row < h; row++)
            {
                new ReadOnlySpan<byte>(level->Pixels + (long)(y + row) * (long)level->RowPitch + (long)x * 4, w * 4)
                    .CopyTo(new Span<byte>(r->Pixels + (long)row * (long)r->RowPitch, w * 4));
            }

            DxImage* blocks;

            if (compressed)
            {
                bool bc7 = format is TextureEncoder.BC7_UNORM or TextureEncoder.BC7_UNORM_SRGB;
                bool done = false;

                if (bc7)
                {
                    lock (TextureEncoder.GpuLock)
                    {
                        var device = TextureEncoder.GetGpuDevice();
                        if (device != null)
                        {
                            hr = DirectXTex.Compress3((Hexa.NET.DirectXTex.ID3D11Device*)device.NativePointer, r, format, TexCompressFlags.Bc7Use3Subsets, 1.0f, &encoded);
                            done = !hr.IsFailure;
                            result.UsedGpu |= done;
                        }
                    }
                }

                if (!done)
                {
                    encoded.Release();
                    encoded = DirectXTex.CreateScratchImage();
                    hr = DirectXTex.Compress(r, format, TexCompressFlags.Parallel | (bc7 ? TexCompressFlags.Bc7Quick : TexCompressFlags.Default), 0.5f, &encoded);
                    if (hr.IsFailure)
                        return $"Failed to compress a region (0x{hr.Value:X8})";
                }

                blocks = encoded.GetImage(0, 0, 0);
            }
            else if (format != work)
            {
                hr = DirectXTex.Convert(r, format, TexFilterFlags.Default, 0.5f, &encoded);
                if (hr.IsFailure)
                    return $"Failed to convert a region (0x{hr.Value:X8})";
                blocks = encoded.GetImage(0, 0, 0);
            }
            else
            {
                blocks = r;
            }

            // Block rows (or pixel rows) into the level
            int unitsWide = CeilDiv(w, blockSize);
            int unitsHigh = CeilDiv(h, blockSize);
            int bx = x / blockSize, by = y / blockSize;

            for (int row = 0; row < unitsHigh; row++)
            {
                long dst = levelOffset + (long)(by + row) * levelRowPitch + (long)bx * unitBytes;
                new ReadOnlySpan<byte>(blocks->Pixels + (long)row * (long)blocks->RowPitch, unitsWide * unitBytes)
                    .CopyTo(output.AsSpan((int)dst, unitsWide * unitBytes));
            }

            return null;
        }
        finally
        {
            region.Release();
            encoded.Release();
        }
    }

    private static int CeilDiv(int a, int b) => (a + b - 1) / b;

    private static int CeilShift(int v, int level) => (v + (1 << level) - 1) >> level;

    private static TextureEncodeResult Fail(TextureEncodeResult result, string error)
    {
        result.Dds = null;
        result.Error = error;
        return result;
    }
}
