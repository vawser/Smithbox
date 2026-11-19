using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace StudioCore.Utilities.Remote;

/// <summary>
///     Draws labels (part names) into an image, for screenshot labels=true. Own bitmap font (ASCII 32-126, 8x13,
///     rasterized from DejaVu Sans Mono Bold 12 px, plus German umlauts and sharp s), because Smithbox does not ship
///     ImageSharp.Drawing.
/// </summary>
internal static class ViewportLabels
{
    private const int GlyphW = 8;
    private const int GlyphH = 13;
    private const int Advance = 7;
    private const int Pad = 2;

    private static readonly byte[] Font = Convert.FromBase64String(
        "AAAAAAAAAAAAAAAAAAAICAgICAgACAgAAAAANjY2AAAAAAAAAAAAAABoLH40NH8SGgAAAAAIPC4OHDgoOjwICAAABgsLZhgyWFgwAAAAABwGBg5eW3M2fgAAAAAICAgAAAAAAAAAAAAQGAgMDAwMDAgYEAAADAwIGBgYGBgIDAwAAAAIKjw8KggAAAAAAAAAAAAICAh/CAgIAAAAAAAAAAAAAAAYGAwMAAAAAAAAPDwAAAAAAAAAAAAAAAAAABgYAAAAACAgEBAYCAwEBgIAAAAcNiZmbmYmNhwAAAAAHhgYGBgYGBh+AAAAABwyMDA4GAwGPgAAAAAcMjAwHDAgMh4AAAAAMDg8NDYyfjAwAAAAAD4GBh4wICAyHAAAAAA8JgY+NmZmNhwAAAAAPjAwOBgYDAwMAAAAABw2JjYcNiI2PAAAAAAcNjIydjwgMhwAAAAAAAAAGBgAABgYAAAAAAAAABgYAAAYGAwMAAAAAGA4Dg44YAAAAAAAAAAAAH8AfwAAAAAAAAAAAx54cB4DAAAAAAA8MjAYCAgACAgAAAAAADxme21NTW97BjwAABwcHDQ2PiJiYwAAAAA+MmIyPmJiYj4AAAAAOAwGBgYGBgw4AAAAAB42ZmZmZmY2HgAAAAB+BgYGPgYGBn4AAAAAfgYGBj4GBgYGAAAAADwGBgYGdmZmPAAAAAAmJiYmPiYmJiYAAAAAPhgYGBgYGBg+AAAAADwwMDAwMDAwHgAAAABiMh4eHh4yMmIAAAAABgYGBgYGBgZ+AAAAAHd3d397a2NjYwAAAABmZm5uanpycnIAAAAAHDZmYmNiZjYcAAAAAD5mZmY+BgYGBgAAAAAcNmZiY2JmNjwwAAAAPjYmNh42NmZmAAAAABwmBg48MGAyHAAAAAB/GBgYGBgYGBgAAAAAYmJiYmJiYjY8AAAAAGNiJjY2NBwcHAAAAABDQ2t7f3Z2NjYAAAAAYzY8HBgcPjZjAAAAAGMmNjwcGBgYGAAAAAB+cDA4HAwOBn4AAAA8DAwMDAwMDAwMPAAAAAIGBAwIGBAQMCAAABwYGBgYGBgYGBgcAAAAHDxiAAAAAAAAAAAAAAAAAAAAAAAAAAAAfwQIAAAAAAAAAAAAAAAAAAA+IGB+ZnZ+AAAABgYGPjZmZmY2PgAAAAAAADwGBgYGBjwAAAAgICA8NjIjMjY8AAAAAAAAHDZifgIGPAAAADgYCD4ICAgICAgAAAAAAAA8NiIiIjY8IDIcBgYGPjYmJiYmJgAAABgYAB4YGBgYGH4AAAAYGAAeGBgYGBgYGBgOBgYGNj4eHjY2ZgAAAA8MDAwMDAwMDDgAAAAAAAA/a2tra2trAAAAAAAAPjYmJiYmJgAAAAAAABw2YmJiNhwAAAAAAAA+NmZmZjY+BgYGAAAAPDYyIzI2PCAgIAAAAHwMDAQEBAQAAAAAAAAcJgY8MDI8AAAAAAwMPgwMDAwMOAAAAAAAADY2NjY2Nj4AAAAAAABiJjY2HBwcAAAAAAAAQ0Nren42NgAAAAAAADY2HBgcNmYAAAAAAABjJjY0PBwYGAwOAAAAPjA4HAwGPgAAADgYGAgMDgwIGBg4AAAICAgICAgICAgICAgADggICBg4GAgICA4AAAAAAAAAAE44AAAAAAA=");

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

        for (var i = 0; i < text.Length; i++)
        {
            var glyph = Glyph(text[i]);

            for (var gy = 0; gy < GlyphH; gy++)
            {
                var bits = glyph[gy];

                for (var gx = 0; gx < GlyphW; gx++)
                {
                    if ((bits >> gx & 1) == 0)
                        continue;

                    for (var sy = 0; sy < scale; sy++)
                    for (var sx = 0; sx < scale; sx++)
                        Set(image, box.X + Pad + (i * Advance + gx) * scale + sx, box.Y + Pad + gy * scale + sy, fg, 1f);
                }
            }
        }
    }

    private static readonly byte[] SharpS = { 0x0, 0x1c, 0x36, 0x36, 0x16, 0x36, 0x66, 0x66, 0x36, 0x16, 0x6, 0x0, 0x0 };

    /// <summary>The rows of a character; umlauts as the base letter with two dots, sharp s separately, otherwise ?</summary>
    private static byte[] Glyph(char c)
    {
        if (c == 'ß')
            return SharpS;

        var basis = c switch { 'ä' => 'a', 'ö' => 'o', 'ü' => 'u', 'Ä' => 'A', 'Ö' => 'O', 'Ü' => 'U', _ => c };
        if (basis is < ' ' or > '~')
            basis = '?';

        var g = new byte[GlyphH];
        Array.Copy(Font, (basis - ' ') * GlyphH, g, 0, GlyphH);

        if (basis != c)
            g[char.IsUpper(c) ? 0 : 1] |= 0b0010_0100;

        return g;
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
