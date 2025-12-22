using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

public class WorldMapLayout
{
    public MapEditorScreen Editor;

    public List<WorldMapTile> Tiles { get; set; }

    private string Prefix = "";
    private float XOffset;
    private float YOffset;

    public WorldMapLayout(MapEditorScreen editor, string prefix, int xOffset, int yOffset)
    {
        Editor = editor;
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

    public void GenerateTiles(List<int> rows, List<int> cols, int tileOffset, float increment, WorldMapTileType type, List<int> tileIdVariants = null,
        int xLargeOffset = -1, int yLargeOffset = -1, 
        int xMediumOffset = -1, int yMediumOffset = -1, 
        int xSmallOffset = -1, int ySmallOffset = -1)
    {
        var mapList = MsbUtils.GetFullMapList(Editor.Project);

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
                    //TaskLogs.AddLog($"{type}: {id}");

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
