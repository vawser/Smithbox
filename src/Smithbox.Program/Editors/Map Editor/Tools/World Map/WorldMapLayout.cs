using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class WorldMapLayout
{
    public MapEditorView View;

    public List<WorldMapTile> Tiles { get; set; }

    /// <summary>
    /// The grid of each tile size, so a tile's place can be found whether or not it has an MSB
    /// </summary>
    public Dictionary<WorldMapTileType, WorldMapGrid> Grids { get; } = new();

    private string Prefix = "";
    private float XOffset;
    private float YOffset;

    public WorldMapLayout(MapEditorView view, string prefix, int xOffset, int yOffset)
    {
        View = view;
        Prefix = prefix;
        XOffset = xOffset;
        YOffset = yOffset;
        Tiles = new List<WorldMapTile>();
    }

    private static string PadNumber(int num)
    {
        if (num < 10)
        {
            return $"0{num}";
        }

        return num.ToString();
    }

    /// <summary>
    /// The ER tiles placed where the game's map puts them, in L0 map pixels (one per metre): the centre of block
    /// mAA_28_64_00 is at (128, 128), x east, y south (see WorldMapTileSet). A tile of size type k (0 small, 1 medium,
    /// 2 large) covers 2^k small tiles, so tile (x, y) starts at small tile (x * 2^k, y * 2^k).
    /// </summary>
    public void GenerateExactTiles(List<int> xs, List<int> ys, WorldMapTileType type)
    {
        var mapList = MsbUtils.GetFullMapList(View.Project);
        var k = type switch { WorldMapTileType.Medium => 1, WorldMapTileType.Large => 2, _ => 0 };
        var span = 1 << k;
        var size = 256f * span;

        // Tile (x, y): left = (x * span - 28) * 256, top = (64 - (y + 1) * span + 1) * 256 = 65 * 256 - size - y * size
        var grid = new WorldMapGrid(-28 * 256f, 65 * 256f - size, 0, 0, size);
        Grids[type] = grid;

        foreach (var x in xs)
        {
            foreach (var y in ys)
            {
                var id = $"m{Prefix}_{PadNumber(x)}_{PadNumber(y)}_{PadNumber(k)}";

                if (!mapList.Contains(id))
                    continue;

                var origin = grid.TileOrigin(x, y);
                Tiles.Add(new WorldMapTile(id, origin.X, origin.Y, size, size, type));
            }
        }
    }

    public void GenerateTiles(List<int> rows, List<int> cols, int tileOffset, float increment, WorldMapTileType type, List<int> tileIdVariants = null,
        int xLargeOffset = -1, int yLargeOffset = -1, 
        int xMediumOffset = -1, int yMediumOffset = -1, 
        int xSmallOffset = -1, int ySmallOffset = -1)
    {
        var mapList = MsbUtils.GetFullMapList(View.Project);

        float CurX = XOffset;
        float CurY = YOffset;

        if(type is WorldMapTileType.Large)
        {
            if(xLargeOffset != -1)
            {
                CurX = xLargeOffset;
            }
            if (yLargeOffset != -1)
            {
                CurY = yLargeOffset;
            }
        }

        if (type is WorldMapTileType.Medium)
        {
            if (xMediumOffset != -1)
            {
                CurX = xMediumOffset;
            }
            if (yMediumOffset != -1)
            {
                CurY = yMediumOffset;
            }
        }

        if (type is WorldMapTileType.Small)
        {
            if (xSmallOffset != -1)
            {
                CurX = xSmallOffset;
            }
            if (ySmallOffset != -1)
            {
                CurY = ySmallOffset;
            }
        }

        if (rows.Count > 0 && cols.Count > 0)
            Grids[type] = new WorldMapGrid(CurX, CurY, rows[0], cols[0], increment);

        foreach (var row in rows)
        {
            foreach (var col in cols)
            {
                // Pad the < 10 numbers so they fit the map ID format properly: mAA_BB_CC_DD
                string padRow = row.ToString();
                string padCol = col.ToString();

                if (row < 10)
                    padRow = PadNumber(row);

                if (col < 10)
                    padCol = PadNumber(col);

                var baseTileID = 0;
                baseTileID += tileOffset;

                var strTileID = "00";
                if(baseTileID < 10)
                {
                    strTileID = $"0{baseTileID}";
                }
                else
                {
                    strTileID = $"{baseTileID}";
                }

                var id = $"m{Prefix}_{padRow}_{padCol}_{strTileID}";

                if (tileIdVariants == null)
                {
                    //Smithbox.Log(this, $"{type}: {id}");

                    // Only include if the map id is an actual map
                    if (mapList.Contains(id))
                    {
                        var newTile = new WorldMapTile(id, CurX, CurY, increment, increment, type);
                        Tiles.Add(newTile);
                    }
                }
                else
                {
                    foreach(var variant in tileIdVariants)
                    {
                        baseTileID = variant;
                        baseTileID += tileOffset;

                        strTileID = "00";
                        if (baseTileID < 10)
                        {
                            strTileID = $"0{baseTileID}";
                        }
                        else
                        {
                            strTileID = $"{baseTileID}";
                        }

                        id = $"m{Prefix}_{padRow}_{padCol}_{strTileID}";

                        // Only include if the map id is an actual map
                        if (mapList.Contains(id))
                        {
                            var newTile = new WorldMapTile(id, CurX, CurY, increment, increment, type);
                            Tiles.Add(newTile);
                        }
                    }
                }

                CurY = CurY + increment;
            }

            CurX = CurX + increment;

            if (type is WorldMapTileType.Large)
            {
                if (yLargeOffset != -1)
                {
                    CurY = yLargeOffset;
                }
                else
                {
                    CurY = YOffset;
                }
            }

            if (type is WorldMapTileType.Medium)
            {
                if (yMediumOffset != -1)
                {
                    CurY = yMediumOffset;
                }
                else
                {
                    CurY = YOffset;
                }
            }

            if (type is WorldMapTileType.Small)
            {
                if (ySmallOffset != -1)
                {
                    CurY = ySmallOffset;
                }
                else
                {
                    CurY = YOffset;
                }
            }
        }
    }
}

/// <summary>
/// Where the tiles of one size sit on the world map image. The layouts list the first index of a map ID
/// (mAA_XX_YY_DD) going right and the second going down, with XX rising and YY falling by one per tile.
/// </summary>
public readonly record struct WorldMapGrid(float StartX, float StartY, int FirstX, int FirstY, float Size)
{
    /// <summary>The top left of tile (x, y) on the image, before zoom</summary>
    public Vector2 TileOrigin(int x, int y) => new(StartX + (x - FirstX) * Size, StartY + (FirstY - y) * Size);
}

public class WorldMapTile
{
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }

    public WorldMapTileType TileType { get; set; }

    public WorldMapTile(string name, float curX, float curY, float width, float height, WorldMapTileType type)
    {
        Name = name;
        X = curX;
        Y = curY;
        Width = width;
        Height = height;
        TileType = type;
    }
}
