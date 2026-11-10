using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextureViewer;

/// <summary>A cell's rectangle in sheet pixels.</summary>
public readonly record struct CellRect(int X, int Y, int W, int H)
{
    public int Right => X + W;
    public int Bottom => Y + H;
    public long Area => (long)W * H;

    public static CellRect Of(SubTexture cell) => new(cell.XValue, cell.YValue, cell.WidthValue, cell.HeightValue);

    public bool Contains(float x, float y) => x >= X && y >= Y && x < Right && y < Bottom;

    public bool Overlaps(CellRect o) => X < o.Right && o.X < Right && Y < o.Bottom && o.Y < Bottom;

    /// <summary>One rectangle inside the other (or the same): an alias or a sub-region, not a packing error.</summary>
    public bool Nests(CellRect o) => (X >= o.X && Y >= o.Y && Right <= o.Right && Bottom <= o.Bottom) || (o.X >= X && o.Y >= Y && o.Right <= Right && o.Bottom <= Bottom);
}

/// <summary>A layout problem, with the cells to select when it is clicked.</summary>
public record LayoutIssue(string Kind, string Message, List<SubTexture> Cells);

/// <summary>
/// The rules the layout editor uses (Texture Editor plan, T5), without UI so TextureSurvey can test them:
/// alias groups, the next free grid slot and the next name for a cell, and the layout checks.
/// </summary>
public static class LayoutGeometry
{
    /// <summary>The cells with exactly the same rectangle as this one (the games alias icons this way, T0).</summary>
    public static List<SubTexture> AliasGroup(TextureAtlas atlas, SubTexture cell)
    {
        var rect = CellRect.Of(cell);
        return atlas.SubTextures.Where(c => CellRect.Of(c) == rect).ToList();
    }

    /// <summary>The cells under a point, smallest first (so a small cell inside a big one can be picked).</summary>
    public static List<SubTexture> CellsAt(TextureAtlas atlas, float x, float y)
    {
        return atlas.SubTextures.Where(c => CellRect.Of(c).Contains(x, y)).OrderBy(c => CellRect.Of(c).Area).ToList();
    }

    private static readonly Regex NameRx = new(@"^(.*?)(\d+)(\D*)$", RegexOptions.Compiled);

    /// <summary>
    /// A name for a new cell like the template: its prefix and suffix with the next free number, zero-padded the
    /// same (MENU_ItemIcon_00123.png → MENU_ItemIcon_{max+1:D5}.png); without a number, "{name}_1" and up.
    /// </summary>
    public static string NextName(TextureAtlas atlas, SubTexture template, IEnumerable<string> alsoTaken = null)
    {
        var taken = new HashSet<string>(atlas.SubTextures.Select(c => c.Name), StringComparer.OrdinalIgnoreCase);
        if (alsoTaken != null)
            foreach (var t in alsoTaken) taken.Add(t);

        var name = template?.Name ?? "Cell_0.png";
        var m = NameRx.Match(name);

        if (!m.Success)
        {
            var dot = name.LastIndexOf('.');
            var stem = dot > 0 ? name[..dot] : name;
            var ext = dot > 0 ? name[dot..] : "";
            for (int i = 1; ; i++)
                if (!taken.Contains($"{stem}_{i}{ext}"))
                    return $"{stem}_{i}{ext}";
        }

        var prefix = m.Groups[1].Value;
        var digits = m.Groups[2].Value.Length;
        var suffix = m.Groups[3].Value;

        long max = -1;
        foreach (var t in taken)
        {
            var tm = NameRx.Match(t);
            if (tm.Success && tm.Groups[1].Value.Equals(prefix, StringComparison.OrdinalIgnoreCase)
                && tm.Groups[3].Value.Equals(suffix, StringComparison.OrdinalIgnoreCase)
                && long.TryParse(tm.Groups[2].Value, out var v))
                max = Math.Max(max, v);
        }

        for (long id = max + 1; ; id++)
        {
            var candidate = $"{prefix}{id.ToString().PadLeft(digits, '0')}{suffix}";
            if (!taken.Contains(candidate))
                return candidate;
        }
    }

    /// <summary>
    /// The next free slot for a cell the size of the template, on the grid the same-size cells sit on: origin =
    /// their smallest x/y, stride = the most common gap between their columns/rows (else the size). Slots are tried
    /// column by column from the origin (down, then across); the first that is inside the sheet and overlaps no cell
    /// wins. Null when full.
    /// </summary>
    public static CellRect? NextSlot(TextureAtlas atlas, SubTexture template, int sheetWidth, int sheetHeight)
    {
        var size = template != null ? CellRect.Of(template) : new CellRect(0, 0, 64, 64);
        int w = Math.Max(1, size.W), h = Math.Max(1, size.H);

        var same = atlas.SubTextures.Select(CellRect.Of).Where(r => r.W == w && r.H == h).Distinct().ToList();

        int originX = same.Count > 0 ? same.Min(r => r.X) : 0;
        int originY = same.Count > 0 ? same.Min(r => r.Y) : 0;
        int strideX = Stride(same.Select(r => r.X), w);
        int strideY = Stride(same.Select(r => r.Y), h);

        var cells = atlas.SubTextures.Select(CellRect.Of).ToList();

        // Down each column, then across to the next one
        for (int x = originX; x + w <= sheetWidth; x += strideX)
        {
            for (int y = originY; y + h <= sheetHeight; y += strideY)
            {
                var slot = new CellRect(x, y, w, h);
                if (!cells.Any(c => c.Overlaps(slot)))
                    return slot;
            }
        }

        return null;
    }

    /// <summary>
    /// A free place for a w×h cell anywhere on the sheet (for new icons of any size): candidates are the sheet's
    /// corner and the right/bottom edges of the cells, rounded up to 'align' (4 keeps a cell on whole BC blocks, so
    /// its neighbours' blocks are never re-encoded). Tried down each column, then across, as Add Next Slot.
    /// Null when nothing fits.
    /// </summary>
    public static CellRect? FindFreeSpot(IEnumerable<CellRect> occupied, int w, int h, int sheetWidth, int sheetHeight, int align = 4)
    {
        if (w <= 0 || h <= 0 || w > sheetWidth || h > sheetHeight)
            return null;

        var cells = occupied.Where(r => r.W > 0 && r.H > 0).ToList();
        int Up(int v) => (v + align - 1) / align * align;

        var xs = new SortedSet<int> { 0 };
        var ys = new SortedSet<int> { 0 };
        foreach (var c in cells)
        {
            xs.Add(Up(c.Right));
            ys.Add(Up(c.Bottom));
        }

        foreach (var x in xs)
        {
            if (x + w > sheetWidth)
                break;

            foreach (var y in ys)
            {
                if (y + h > sheetHeight)
                    break;

                var spot = new CellRect(x, y, w, h);
                if (!cells.Any(c => c.Overlaps(spot)))
                    return spot;
            }
        }

        return null;
    }

    /// <summary>
    /// How a sheet's low twin stores its cells, from the cells both have: the most common low/hi width ratio
    /// (1 or 0.5 in the games, T0). 1 when they share no cells.
    /// </summary>
    public static double TwinScale(TextureAtlas hi, TextureAtlas low)
    {
        var lowByName = low.SubTextures.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var ratios = new Dictionary<double, int>();

        foreach (var c in hi.SubTextures)
        {
            if (c.WidthValue <= 0 || !lowByName.TryGetValue(c.Name, out var l))
                continue;

            var ratio = Math.Round((double)l.WidthValue / c.WidthValue * 2) / 2;
            ratios[ratio] = ratios.GetValueOrDefault(ratio) + 1;
        }

        return ratios.Count == 0 ? 1 : ratios.OrderByDescending(kv => kv.Value).First().Key;
    }

    /// <summary>
    /// The low scale (1 or 0.5) for a cell the low twin doesn't have yet: the majority of the shared cells in its
    /// name family (the name without its trailing number: MENU_ItemIcon_), else of the whole sheet. The games halve
    /// cells by choice, not by size (TextureSurvey: every size has both); the family guess is right for 94–98% of
    /// cells (ER 97.7%, NR 98.2%, AC6 98.5%, SDT 93.7%, leave-one-out), the sheet majority for 91–98%.
    /// </summary>
    public static double TwinScaleFor(TextureAtlas hi, TextureAtlas low, string cellName)
    {
        var lowByName = low.SubTextures.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        var family = NamePrefix(cellName);
        int familyHalved = 0, familyTotal = 0, sheetHalved = 0, sheetTotal = 0;

        foreach (var c in hi.SubTextures)
        {
            if (c.WidthValue < 4 || !lowByName.TryGetValue(c.Name, out var l) || c.Name.Equals(cellName, StringComparison.OrdinalIgnoreCase))
                continue;

            bool halved = l.WidthValue != c.WidthValue && l.WidthValue * 2 <= c.WidthValue + 2;
            sheetTotal++;
            if (halved) sheetHalved++;

            if (NamePrefix(c.Name) == family)
            {
                familyTotal++;
                if (halved) familyHalved++;
            }
        }

        if (familyTotal > 0)
            return familyHalved * 2 > familyTotal ? 0.5 : 1;

        return sheetTotal > 0 && sheetHalved * 2 > sheetTotal ? 0.5 : 1;
    }

    /// <summary>A cell name without its extension and trailing number: MENU_ItemIcon_00123.png → MENU_ItemIcon_.</summary>
    public static string NamePrefix(string name)
    {
        var dot = name.LastIndexOf('.');
        var stem = dot > 0 ? name[..dot] : name;
        return Regex.Replace(stem, @"\d+\D*$", "");
    }

    /// <summary>The most common positive difference between consecutive distinct positions, at least the size.</summary>
    private static int Stride(IEnumerable<int> positions, int size)
    {
        var sorted = positions.Distinct().OrderBy(p => p).ToList();
        var gaps = new Dictionary<int, int>();
        for (int i = 1; i < sorted.Count; i++)
        {
            var gap = sorted[i] - sorted[i - 1];
            if (gap >= size)
                gaps[gap] = gaps.GetValueOrDefault(gap) + 1;
        }

        return gaps.Count == 0 ? size : gaps.OrderByDescending(kv => kv.Value).ThenBy(kv => kv.Key).First().Key;
    }

    /// <summary>
    /// The layout checks: cells outside the sheet, empty cells, partial overlaps (aliases and nested cells are
    /// normal, T0), duplicate names, and an atlas size that isn't the sheet's.
    /// </summary>
    public static List<LayoutIssue> Check(TextureAtlas atlas, int sheetWidth, int sheetHeight)
    {
        var issues = new List<LayoutIssue>();
        var cells = atlas.SubTextures;

        if (sheetWidth > 0 && atlas.WidthValue > 0 && (atlas.WidthValue != sheetWidth || atlas.HeightValue != sheetHeight))
            issues.Add(new("AtlasSize", $"{atlas.WidthValue}x{atlas.HeightValue} / {sheetWidth}x{sheetHeight}", new()));

        foreach (var c in cells)
        {
            var r = CellRect.Of(c);
            if (r.W <= 0 || r.H <= 0)
                issues.Add(new("Empty", c.Name, [c]));
            else if (sheetWidth > 0 && (r.X < 0 || r.Y < 0 || r.Right > sheetWidth || r.Bottom > sheetHeight))
                issues.Add(new("OutOfBounds", $"{c.Name} ({r.X},{r.Y} {r.W}x{r.H})", [c]));
        }

        foreach (var g in cells.GroupBy(c => c.Name, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
            issues.Add(new("DuplicateName", $"{g.Key} x{g.Count()}", g.ToList()));

        var rects = cells.Select(c => (Cell: c, Rect: CellRect.Of(c))).Where(p => p.Rect.W > 0 && p.Rect.H > 0).ToList();
        for (int i = 0; i < rects.Count; i++)
        {
            for (int j = i + 1; j < rects.Count; j++)
            {
                var a = rects[i]; var b = rects[j];
                if (a.Rect.Overlaps(b.Rect) && !a.Rect.Nests(b.Rect))
                    issues.Add(new("Overlap", $"{a.Cell.Name} / {b.Cell.Name}", [a.Cell, b.Cell]));
            }
        }

        return issues;
    }
}
