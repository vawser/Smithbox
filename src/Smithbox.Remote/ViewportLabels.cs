using Hexa.NET.ImGui;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Draws labels (part names) into an image, for screenshot labels=true and the plan/profile images. The text uses
///     Smithbox' own UI font (Assets/Fonts/RobotoMono-Light.ttf), rasterized with ImGui's font atlas, since Smithbox does
///     not ship ImageSharp.Drawing.
/// </summary>
internal static unsafe class ViewportLabels
{
    private const int GlyphH = 13;
    private const int Advance = 7;
    private const int Pad = 2;
    private const int MaxScale = 4;

    // ImFontGlyph as laid out in C (Colored:1, Visible:1, Codepoint:30 share one uint); Hexa.NET.ImGui's struct has
    // them as three fields, which shifts everything after them
    [StructLayout(LayoutKind.Sequential)]
    private struct NativeGlyph
    {
        public uint Bits;
        public float AdvanceX, X0, Y0, X1, Y1, U0, V0, U1, V1;
    }

    // One atlas with the font at 12 px per label scale (1-4), made on first use; null if the font could not be loaded
    private static bool AtlasTried;
    private static ImFontAtlasPtr Atlas;
    private static ImFontPtr[] Fonts;
    private static byte* AtlasPixels;
    private static int AtlasWidth, AtlasHeight;

    public readonly record struct Box(int X, int Y, int W, int H)
    {
        public bool Overlaps(Box o) => X < o.X + o.W && o.X < X + W && Y < o.Y + o.H && o.Y < Y + H;
    }

    public static Box Measure(string text, int x, int y, int scale) =>
        new(x, y, text.Length * Advance * scale + 2 * Pad, GlyphH * scale + 2 * Pad);

    /// <summary>
    ///     Finds room for a label next to the point (x, y): above right, otherwise below or to the left, without
    ///     covering labels already placed. False if nothing is free.
    /// </summary>
    public static bool TryPlace(List<Box> used, string text, int x, int y, int scale, int width, int height, out Box box)
    {
        var size = Measure(text, 0, 0, scale);
        var candidates = new (int, int)[]
        {
            (x + 4, y - size.H - 2), (x + 4, y + 4), (x - size.W - 4, y - size.H - 2), (x - size.W - 4, y + 4),
            (x - size.W / 2, y - 2 * size.H - 4), (x - size.W / 2, y + size.H + 6),
        };

        foreach (var (cx, cy) in candidates)
        {
            var b = new Box(Math.Clamp(cx, 0, Math.Max(0, width - size.W)), Math.Clamp(cy, 0, Math.Max(0, height - size.H)),
                size.W, size.H);

            if (!used.Exists(u => u.Overlaps(b)))
            {
                used.Add(b);
                box = b;
                return true;
            }
        }

        box = default;
        return false;
    }

    public static void Marker(Image<Rgba32> image, int x, int y, Rgba32 color)
    {
        for (var dy = -3; dy <= 3; dy++)
        for (var dx = -3; dx <= 3; dx++)
        {
            var edge = Math.Abs(dx) == 3 || Math.Abs(dy) == 3;
            Set(image, x + dx, y + dy, edge ? new Rgba32(0, 0, 0, 255) : color, 1f);
        }
    }

    public static void Line(Image<Rgba32> image, int x0, int y0, int x1, int y1, Rgba32 color)
    {
        var steps = Math.Max(Math.Abs(x1 - x0), Math.Abs(y1 - y0));

        for (var i = 0; i <= steps; i++)
        {
            var t = steps == 0 ? 0f : (float)i / steps;
            Set(image, (int)MathF.Round(x0 + (x1 - x0) * t), (int)MathF.Round(y0 + (y1 - y0) * t), color, 0.8f);
        }
    }

    public static void Text(Image<Rgba32> image, Box box, string text, Rgba32 fg, Rgba32 bg, float bgAlpha, int scale)
    {
        for (var y = box.Y; y < box.Y + box.H; y++)
        for (var x = box.X; x < box.X + box.W; x++)
            Set(image, x, y, bg, bgAlpha);

        if (!EnsureAtlas())
            return;

        var font = Fonts[Math.Clamp(scale, 1, MaxScale) - 1];

        for (var i = 0; i < text.Length; i++)
        {
            var g = (NativeGlyph*)font.FindGlyph(text[i]);
            if (g == null)
                continue;

            var u = (int)MathF.Round(g->U0 * AtlasWidth);
            var v = (int)MathF.Round(g->V0 * AtlasHeight);
            var w = (int)MathF.Round((g->U1 - g->U0) * AtlasWidth);
            var h = (int)MathF.Round((g->V1 - g->V0) * AtlasHeight);
            var left = box.X + Pad + i * Advance * scale + (int)MathF.Round(g->X0);
            var top = box.Y + Pad + (int)MathF.Round(g->Y0);

            for (var y = 0; y < h; y++)
            for (var x = 0; x < w; x++)
            {
                var alpha = AtlasPixels[(v + y) * AtlasWidth + u + x];
                if (alpha > 0)
                    Set(image, left + x, top + y, fg, alpha / 255f);
            }
        }
    }

    private static bool EnsureAtlas()
    {
        if (AtlasTried)
            return Fonts != null;

        AtlasTried = true;
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "RobotoMono-Light.ttf");

        if (!File.Exists(path))
        {
            Smithbox.LogWarning(typeof(ViewportLabels), $"Label font not found, labels are drawn without text: {path}");
            return false;
        }

        Atlas = ImGui.ImFontAtlas();
        var cfg = ImGui.ImFontConfig();
        // The light weight is too faint for small labels on busy images
        cfg.RasterizerMultiply = 1.4f;
        cfg.PixelSnapH = true;

        var fonts = new ImFontPtr[MaxScale];
        for (var s = 1; s <= MaxScale; s++)
            fonts[s - 1] = Atlas.AddFontFromFileTTF(path, 12f * s, cfg, Atlas.GetGlyphRangesDefault());

        byte* pixels;
        int width, height;
        Atlas.GetTexDataAsAlpha8(&pixels, &width, &height);
        cfg.Destroy();

        AtlasPixels = pixels;
        AtlasWidth = width;
        AtlasHeight = height;
        Fonts = fonts;
        return true;
    }

    private static void Set(Image<Rgba32> image, int x, int y, Rgba32 color, float alpha)
    {
        if (x < 0 || y < 0 || x >= image.Width || y >= image.Height)
            return;

        if (alpha >= 1f)
        {
            image[x, y] = color;
            return;
        }

        var d = image[x, y];
        image[x, y] = new Rgba32(
            (byte)(d.R + (color.R - d.R) * alpha),
            (byte)(d.G + (color.G - d.G) * alpha),
            (byte)(d.B + (color.B - d.B) * alpha),
            255);
    }
}
