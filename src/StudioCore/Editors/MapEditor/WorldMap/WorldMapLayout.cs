using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.WorldMap;

public class WorldMapLayout
{
    public List<WorldMapTile> Tiles { get; set; }

    private string Prefix = "";
    private float XOffset;
    private float YOffset;

    public WorldMapLayout(string prefix, int xOffset, int yOffset)
    {
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

    private void GenerateTiles(List<int> rows, List<int> cols, string tileID, float increment)
    {
        var mapList = ResourceMapLocator.GetFullMapList();

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
                    var newTile = new WorldMapTile(id, CurX, CurY, increment, increment);
                    Tiles.Add(newTile);
                }

                CurY = CurY + increment;
            }

            CurX = CurX + increment;
            CurY = YOffset;
        }
    }

    public void ConstructSmallTiles()
    {
        var smallRows = new List<int>()
        {
            32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59
        };
        var smallCols = new List<int>()
        {
            63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33, 32, 31, 30
        };

        GenerateTiles(smallRows, smallCols, "00", 124);
    }

    public void ConstructMediumTiles()
    {
        var mediumRows = new List<int>()
        {
            16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29
        };
        var mediumCols = new List<int>()
        {
            31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15
        };

        GenerateTiles(mediumRows, mediumCols, "01", 248);

    }
    public void ConstructLargeTiles()
    {
        var largeRows = new List<int>()
        {
            8, 9, 10, 11, 12, 13, 14
        };
        var largeCols = new List<int>()
        {
            15, 14, 13, 12, 11, 10, 9, 8, 7
        };

        GenerateTiles(largeRows, largeCols, "02", 496);
    }
}

public class WorldMapTile
{
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public WorldMapTile(string name, float curX, float curY, float width, float height)
    {
        Name = name;
        X = curX;
        Y = curY;
        Width = width;
        Height = height;
    }
}