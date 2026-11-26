using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.TextureViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Variant tool's colour shift (Model Variants Plan.md §6, V5): a variant's forked AET container gets the source
/// container's colour textures shifted in hue, saturation and value, re-encoded in each texture's own format, colour
/// space and mip count. Always made from the source's images, never the fork's, so shifts don't build on each other
/// and a zero shift puts the copy back as forked.
/// </summary>
public static class AssetVariantColourShift
{
    /// <summary>
    /// The outcome of building a shift: each fork file's new bytes and its bytes now (null when it doesn't exist), for
    /// an undoable write; the textures shifted, and any errors.
    /// </summary>
    public sealed class Result
    {
        public List<(string Path, byte[] Before, byte[] After)> Files { get; } = new();
        public int Shifted { get; set; }
        public List<string> Errors { get; } = new();
    }

    /// <summary>The colour shift of one pixel (sRGB values as stored; alpha kept).</summary>
    public static Rgba32 ShiftPixel(Rgba32 pixel, float hueDegrees, float saturation, float value)
    {
        float r = pixel.R / 255f, g = pixel.G / 255f, b = pixel.B / 255f;
        float max = MathF.Max(r, MathF.Max(g, b)), min = MathF.Min(r, MathF.Min(g, b));
        float delta = max - min;

        float h = 0;
        if (delta > 1e-6f)
        {
            if (max == r) h = 60f * (((g - b) / delta) % 6f);
            else if (max == g) h = 60f * ((b - r) / delta + 2f);
            else h = 60f * ((r - g) / delta + 4f);
        }

        float s = max <= 1e-6f ? 0 : delta / max;
        float v = max;

        h = ((h + hueDegrees) % 360f + 360f) % 360f;
        s = Math.Clamp(s * saturation, 0, 1);
        v = Math.Clamp(v * value, 0, 1);

        float c = v * s;
        float x = c * (1 - MathF.Abs(h / 60f % 2f - 1));
        float m = v - c;

        (float rr, float gg, float bb) = h switch
        {
            < 60 => (c, x, 0f),
            < 120 => (x, c, 0f),
            < 180 => (0f, c, x),
            < 240 => (0f, x, c),
            < 300 => (x, 0f, c),
            _ => (c, 0f, x)
        };

        return new Rgba32(
            (byte)Math.Clamp(MathF.Round((rr + m) * 255f), 0, 255),
            (byte)Math.Clamp(MathF.Round((gg + m) * 255f), 0, 255),
            (byte)Math.Clamp(MathF.Round((bb + m) * 255f), 0, 255),
            pixel.A);
    }

    /// <summary>A DDS shifted and encoded back into the same format, size and mip count. Null on failure.</summary>
    public static byte[] ShiftDds(TPF tpf, TPF.Texture texture, AssetVariantShift shift, out string error)
    {
        var info = TextureFormats.GetInfo(tpf, texture);

        if (!info.Readable)
        {
            error = LOC.Get("MAP_AssetVariant_Shift_Unreadable", texture.Name);
            return null;
        }

        using var image = DdsDecoder.DecodeToImage(texture.Bytes, out error);

        if (image == null)
            return null;

        image.ProcessPixelRows(rows =>
        {
            for (var y = 0; y < rows.Height; y++)
            {
                var row = rows.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                    row[x] = ShiftPixel(row[x], shift.Hue, shift.Saturation, shift.Value);
            }
        });

        using var png = new MemoryStream();
        image.SaveAsPng(png);

        // As the Texture Editor's import with the format kept: same DXGI format (and so colour space), header style
        // and mip count (a full chain stays one)
        var request = new TextureEncodeRequest
        {
            TargetFormat = info.Dxgi,
            ForceDx10Header = info.Dx10Header,
            LegacyFourCC = info.FourCC,
            MipLevels = info.MipLevels == TextureEncoder.FullChain(info.Width, info.Height) ? 0 : Math.Max(1, info.MipLevels),
        };

        var encoded = TextureEncoder.Encode(png.ToArray(), ".png", request);
        error = encoded.Error;
        return encoded.Dds;
    }

    /// <summary>
    /// The forked container (and its _l twin) built again from the source's: textures with a shifted suffix get the
    /// source image shifted, the others the source image as it is, each under the fork's name. Nothing is written: the
    /// result goes to an undoable action (<see cref="AssetVariantShiftAction" />). Background thread.
    /// </summary>
    public static Result Build(ProjectEntry project, AssetVariantLayout layout, string sourceContainer, string forkContainer, AssetVariantShift shift)
    {
        var result = new Result();

        foreach (var low in new[] { false, true })
        {
            var sourcePath = AssetVariantFork.AetPath(layout, sourceContainer, low);
            var targetPath = AssetVariantFork.AetPath(layout, forkContainer, low);

            if (!project.VFS.FS.FileExists(sourcePath))
                continue;

            try
            {
                var data = project.VFS.FS.ReadFile(sourcePath);
                if (data == null)
                {
                    result.Errors.Add(LOC.Get("MAP_AssetVariant_Error_Read", sourcePath));
                    continue;
                }

                // The source renamed, as the fork was made; then the chosen textures shifted
                var tpf = TPF.Read(AssetVariantFork.RenameTpf(data.Value, sourceContainer, forkContainer));

                if (!shift.IsIdentity)
                {
                    foreach (var texture in tpf.Textures)
                    {
                        // AET000_901_a (or the twin's AET000_901_a_l): the suffix after the container's name
                        var suffix = texture.Name.Length > 10 ? texture.Name[10..] : "";
                        if (low && suffix.EndsWith("_l", StringComparison.OrdinalIgnoreCase))
                            suffix = suffix[..^2];

                        if (!shift.Suffixes.Any(s => string.Equals(s, suffix, StringComparison.OrdinalIgnoreCase)))
                            continue;

                        var dds = ShiftDds(tpf, texture, shift, out var error);

                        if (dds == null)
                        {
                            result.Errors.Add($"{texture.Name}: {error}");
                            continue;
                        }

                        texture.Bytes = dds;
                        result.Shifted++;
                    }
                }

                // What the fork holds now, so undo can put it back
                var before = project.VFS.FS.FileExists(targetPath) ? project.VFS.FS.ReadFile(targetPath)?.ToArray() : null;
                result.Files.Add((targetPath, before, tpf.Write()));
            }
            catch (Exception e)
            {
                result.Errors.Add(LOC.Get("MAP_AssetVariant_Error_Write", targetPath, e.Message));
                Smithbox.LogError(typeof(AssetVariantColourShift), targetPath, e);
            }
        }

        return result;
    }
}
