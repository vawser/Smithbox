using SoulsFormats;
using System;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// One mAA_XX_YY_02.mapinfotexbnd.dcx: a BND4 holding two 256x256 24-bit BMPs. The game only loads a BMP that is
/// 'BM', 24 bits per pixel, uncompressed, 256x256 and stored bottom-up, and only from a file whose name stem is the
/// tile (slot 0) or the tile + "_01" (slot 1), so the editor keeps each file's header and replaces the pixels only.
/// </summary>
public class MapInfoTexTile
{
    public const int Size = 256;
    public const int Stride = Size * 3;
    public const int PixelBytes = Stride * Size;

    public string Name;
    public string Path;
    public string Area;
    public int X;
    public int Y;

    public BND4 Binder;

    /// <summary>The binder file of each slot, null when the binder has none</summary>
    public BinderFile[] Files = new BinderFile[2];

    /// <summary>The pixel data of each slot as stored: rows bottom-up, B, G, R per pixel</summary>
    public byte[][] Pixels = new byte[2][];

    private readonly int[] PixelOffsets = new int[2];

    /// <summary>Why a slot can't be edited, or null</summary>
    public string[] SlotErrors = new string[2];

    public bool Modified;

    /// <summary>Bumped on every change, so the overlay textures know to refresh</summary>
    public int Version;

    public bool IsDlc => Area == "m61";

    private static readonly Regex NamePattern = new(@"^(m\d\d)_(\d\d)_(\d\d)_02$", RegexOptions.IgnoreCase);

    public static bool TryParseName(string name, out string area, out int x, out int y)
    {
        area = null;
        x = 0;
        y = 0;

        var match = NamePattern.Match(name ?? "");

        if (!match.Success)
            return false;

        area = match.Groups[1].Value.ToLowerInvariant();
        x = int.Parse(match.Groups[2].Value);
        y = int.Parse(match.Groups[3].Value);
        return true;
    }

    public static MapInfoTexTile Read(string name, string path, Memory<byte> data)
    {
        if (!TryParseName(name, out var area, out var x, out var y))
            return null;

        var tile = new MapInfoTexTile
        {
            Name = name,
            Path = path,
            Area = area,
            X = x,
            Y = y,
            Binder = BND4.Read(data)
        };

        foreach (var file in tile.Binder.Files)
        {
            var slot = SlotOf(name, file.Name);

            if (slot < 0 || tile.Files[slot] != null)
                continue;

            tile.Files[slot] = file;
        }

        for (var slot = 0; slot < 2; slot++)
        {
            if (tile.Files[slot] == null)
            {
                tile.SlotErrors[slot] = "MAP_MapInfo_Error_No_Slot";
                continue;
            }

            var bytes = tile.Files[slot].Bytes.ToArray();

            if (!IsGameBmp(bytes, out var offset))
            {
                tile.SlotErrors[slot] = "MAP_MapInfo_Error_Bad_Bmp";
                continue;
            }

            tile.PixelOffsets[slot] = offset;
            tile.Pixels[slot] = bytes.AsSpan(offset, PixelBytes).ToArray();
        }

        return tile;
    }

    /// <summary>
    /// The slot the game loads a binder file into: the stem is the tile name (slot 0), or the tile name and "_NN"
    /// with NN below 2. -1 for any other file, which the game ignores.
    /// </summary>
    private static int SlotOf(string tile, string fileName)
    {
        var stem = System.IO.Path.GetFileNameWithoutExtension((fileName ?? "").Replace('\\', '/'));

        if (stem.Equals(tile, StringComparison.OrdinalIgnoreCase))
            return 0;

        if (stem.Length == tile.Length + 3 && stem.StartsWith(tile + "_", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(stem.AsSpan(tile.Length + 1), out var n) && n is >= 0 and < 2)
            return n;

        return -1;
    }

    /// <summary>The checks the game's loader makes; a top-down BMP (negative height) is rejected too</summary>
    private static bool IsGameBmp(byte[] b, out int pixelOffset)
    {
        pixelOffset = 0;

        if (b.Length < 0x36 || b[0] != 'B' || b[1] != 'M')
            return false;

        pixelOffset = BitConverter.ToInt32(b, 0x0A);

        return BitConverter.ToUInt16(b, 0x1C) == 24
            && BitConverter.ToInt32(b, 0x1E) == 0
            && BitConverter.ToInt32(b, 0x12) == Size
            && BitConverter.ToInt32(b, 0x16) == Size
            && pixelOffset >= 0x36
            && pixelOffset + PixelBytes <= b.Length;
    }

    public bool HasLayer(MapInfoLayer layer) => Pixels[MapInfoLayers.Slot(layer)] != null;

    /// <summary>The index of a pixel's layer byte; px goes west to east, py north to south</summary>
    public static int Index(MapInfoLayer layer, int px, int py) =>
        (Size - 1 - py) * Stride + px * 3 + MapInfoLayers.ByteOffset(layer);

    public byte Get(MapInfoLayer layer, int px, int py)
    {
        var pixels = Pixels[MapInfoLayers.Slot(layer)];
        return pixels == null ? MapInfoLayers.None : pixels[Index(layer, px, py)];
    }

    /// <summary>The binder with the edited pixels written into each BMP, compressed as it was read</summary>
    public byte[] Write()
    {
        for (var slot = 0; slot < 2; slot++)
        {
            if (Files[slot] == null || Pixels[slot] == null)
                continue;

            var bytes = Files[slot].Bytes.ToArray();
            Pixels[slot].CopyTo(bytes, PixelOffsets[slot]);
            Files[slot].Bytes = bytes;
        }

        return Binder.Write();
    }
}
