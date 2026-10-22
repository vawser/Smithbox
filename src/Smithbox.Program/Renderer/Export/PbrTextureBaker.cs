using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace StudioCore.Renderer.Export;

/// <summary>
/// Converts From's texture channel layout into the channels glTF/PBR renderers (Octane, Cycles, ...) expect.
/// Based on how the ER-era textures are laid out:
/// <list type="bullet">
/// <item>_n: R/G = tangent-space normal XY (Z is not stored), B = gloss (Smithbox's own viewport shades with
/// <c>roughness = 1 - normal.B</c>, so the same is used here).</item>
/// <item>_m: single channel metallic mask (e.g. a crown's is near 255, hair's is 0).</item>
/// </list>
/// All of this is a best-effort conversion for rendering use, not a byte-exact reproduction of the game's shaders.
/// </summary>
public static class PbrTextureBaker
{
    public class Bitmap
    {
        public int Width;
        public int Height;

        /// <summary>
        /// Tightly packed RGBA, 8 bits per channel.
        /// </summary>
        public byte[] Rgba;
    }

    /// <summary>
    /// Decodes a DDS file (BC1/BC4/BC5/BC7/...) into an RGBA bitmap.
    /// </summary>
    public static Bitmap Decode(byte[] ddsBytes)
    {
        var image = Pfim.Dds.Create(ddsBytes, new Pfim.PfimConfig());
        if (image.Compressed)
        {
            image.Decompress();
        }

        var width = image.Width;
        var height = image.Height;
        var bmp = new Bitmap { Width = width, Height = height, Rgba = new byte[width * height * 4] };
        var src = image.Data;
        var stride = image.Stride;

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var d = (y * width + x) * 4;

                switch (image.Format)
                {
                    case Pfim.ImageFormat.Rgba32:
                    {
                        var s = y * stride + x * 4;
                        bmp.Rgba[d] = src[s + 2];
                        bmp.Rgba[d + 1] = src[s + 1];
                        bmp.Rgba[d + 2] = src[s];
                        bmp.Rgba[d + 3] = src[s + 3];
                        break;
                    }
                    case Pfim.ImageFormat.Rgb24:
                    {
                        var s = y * stride + x * 3;
                        bmp.Rgba[d] = src[s + 2];
                        bmp.Rgba[d + 1] = src[s + 1];
                        bmp.Rgba[d + 2] = src[s];
                        bmp.Rgba[d + 3] = 255;
                        break;
                    }
                    case Pfim.ImageFormat.Rgb8:
                    {
                        var g = src[y * stride + x];
                        bmp.Rgba[d] = g;
                        bmp.Rgba[d + 1] = g;
                        bmp.Rgba[d + 2] = g;
                        bmp.Rgba[d + 3] = 255;
                        break;
                    }
                    default:
                        throw new NotSupportedException($"Unsupported pixel format: {image.Format}");
                }
            }
        }

        return bmp;
    }

    /// <summary>
    /// Standard tangent-space normal map: keeps X/Y and rebuilds Z (the source stores no Z, its blue channel is
    /// gloss). <paramref name="flipY"/> inverts the green channel for DirectX-vs-OpenGL convention differences.
    /// </summary>
    public static Bitmap BakeNormal(Bitmap normal, bool flipY)
    {
        var result = new Bitmap { Width = normal.Width, Height = normal.Height, Rgba = new byte[normal.Rgba.Length] };

        for (var i = 0; i < normal.Rgba.Length; i += 4)
        {
            var r = normal.Rgba[i];
            var g = normal.Rgba[i + 1];

            var x = r / 255f * 2f - 1f;
            var y = g / 255f * 2f - 1f;
            var z = MathF.Sqrt(MathF.Max(0f, 1f - x * x - y * y));

            result.Rgba[i] = r;
            result.Rgba[i + 1] = flipY ? (byte)(255 - g) : g;
            result.Rgba[i + 2] = (byte)MathF.Round((z * 0.5f + 0.5f) * 255f);
            result.Rgba[i + 3] = 255;
        }

        return result;
    }

    /// <summary>
    /// glTF metallic-roughness texture: R = ambient occlusion (none available, so white), G = roughness
    /// (1 - normal.B), B = metallic (the _m map's value, 0 if there isn't one). Either source may be null; the
    /// result takes the larger of the two sizes so no detail is thrown away.
    /// </summary>
    public static Bitmap BakeOrm(Bitmap normal, Bitmap metallic)
    {
        var width = Math.Max(normal?.Width ?? 0, metallic?.Width ?? 0);
        var height = Math.Max(normal?.Height ?? 0, metallic?.Height ?? 0);

        var result = new Bitmap { Width = width, Height = height, Rgba = new byte[width * height * 4] };

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var u = (x + 0.5f) / width;
                var v = (y + 0.5f) / height;
                var d = (y * width + x) * 4;

                float roughness = 1f;
                if (normal != null)
                {
                    roughness = 1f - Sample(normal, u, v, 2) / 255f;
                }

                float metal = 0f;
                if (metallic != null)
                {
                    metal = Sample(metallic, u, v, 0);
                }

                result.Rgba[d] = 255;
                result.Rgba[d + 1] = (byte)MathF.Round(Math.Clamp(roughness, 0f, 1f) * 255f);
                result.Rgba[d + 2] = (byte)MathF.Round(Math.Clamp(metal, 0f, 255f));
                result.Rgba[d + 3] = 255;
            }
        }

        return result;
    }

    /// <summary>
    /// Bilinear sample of one channel at normalised UV (clamped at the edges), so inputs of different sizes can
    /// be combined.
    /// </summary>
    private static float Sample(Bitmap bmp, float u, float v, int channel)
    {
        var fx = Math.Clamp(u * bmp.Width - 0.5f, 0f, bmp.Width - 1);
        var fy = Math.Clamp(v * bmp.Height - 0.5f, 0f, bmp.Height - 1);

        var x0 = (int)MathF.Floor(fx);
        var y0 = (int)MathF.Floor(fy);
        var x1 = Math.Min(x0 + 1, bmp.Width - 1);
        var y1 = Math.Min(y0 + 1, bmp.Height - 1);
        var tx = fx - x0;
        var ty = fy - y0;

        float P(int px, int py) => bmp.Rgba[(py * bmp.Width + px) * 4 + channel];

        var top = P(x0, y0) * (1 - tx) + P(x1, y0) * tx;
        var bottom = P(x0, y1) * (1 - tx) + P(x1, y1) * tx;
        return top * (1 - ty) + bottom * ty;
    }

    public static void SavePng(Bitmap bmp, string pathNoExtension)
    {
        using var image = Image.LoadPixelData<Rgba32>(bmp.Rgba, bmp.Width, bmp.Height);
        image.SaveAsPng($"{pathNoExtension}.png");
    }
}
