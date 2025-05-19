using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools.WorldMap;

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

        return num.ToString(); ;
    }

    public void GenerateTiles(List<int> rows, List<int> cols, string tileID, float increment, MapTileType type)
    {
        var mapList = MapLocator.GetFullMapList(Editor.Project);

        float CurX = XOffset;
        float CurY = YOffset;

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

                var id = $"m{Prefix}_{padRow}_{padCol}_{tileID}";

                // Only include if the map id is an actual map
                if (mapList.Contains(id))
                {
                    var newTile = new WorldMapTile(id, CurX, CurY, increment, increment, type);
                    Tiles.Add(newTile);
                }

                CurY = CurY + increment;
            }

            CurX = CurX + increment;
            CurY = YOffset;
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

    public MapTileType TileType { get; set; }

    public WorldMapTile(string name, float curX, float curY, float width, float height, MapTileType type)
    {
        Name = name;
        X = curX;
        Y = curY;
        Width = width;
        Height = height;
        TileType = type;
    }
}

public enum MapTileType
{
    Small,
    Medium,
    Large
}