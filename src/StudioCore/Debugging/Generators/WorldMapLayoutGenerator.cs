using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Debug.Generators;

/// <summary>
/// Used to generate the .layout file that drives the World Map feature
/// </summary>
public static class WorldMapLayoutGenerator
{
    // Small Tiles - Row IDs
    public static List<int> smallRows = new List<int>()
    {
        32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58, 59
    };

    // Small Tiles - Col IDs
    public static List<int> smallCols = new List<int>()
    {
        63, 62, 61, 60, 59, 58, 57, 56, 55, 54, 53, 52, 51, 50, 49, 48, 47, 46, 45, 44, 43, 42, 41, 40, 39, 38, 37, 36, 35, 34, 33, 32, 31, 30
    };

    // Small Tiles - Truth Table for when to add a layout entry
    public static Dictionary<int, List<int>> smallTileDict = new Dictionary<int, List<int>>()
    {
        {  1, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }},
        {  2, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 }},
        {  3, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        {  4, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        {  5, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        {  6, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        {  7, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        {  8, new List<int> { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        {  9, new List<int> { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        { 10, new List<int> { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        { 11, new List<int> { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        { 12, new List<int> { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        { 13, new List<int> { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        { 14, new List<int> { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 }},
        { 15, new List<int> { 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }},
        { 16, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0 }},
        { 17, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 18, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 19, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 20, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 21, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 22, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 23, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 24, new List<int> { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 25, new List<int> { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 26, new List<int> { 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 27, new List<int> { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 28, new List<int> { 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 }},
        { 29, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 }},
        { 30, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }},
        { 31, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }},
        { 32, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }},
        { 33, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }},
        { 34, new List<int> { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }}
    };

    // Medium Tiles
    public static List<int> mediumRows = new List<int>()
    {
        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29
    };

    public static List<int> mediumCols = new List<int>()
    {
        31, 30, 29, 28, 27, 26, 25, 24, 23, 22, 21, 20, 19, 18, 17, 16, 15
    };

    public static Dictionary<int, List<int>> mediumTileDict = new Dictionary<int, List<int>>()
    {
        {  1, new List<int> { 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0 }},
        {  2, new List<int> { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }},
        {  3, new List<int> { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }},
        {  4, new List<int> { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }},
        {  5, new List<int> { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }},
        {  6, new List<int> { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }},
        {  7, new List<int> { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }},
        {  8, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0 }},
        {  9, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }},
        { 10, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }},
        { 11, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }},
        { 12, new List<int> { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }},
        { 13, new List<int> { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }},
        { 14, new List<int> { 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0 }},
        { 15, new List<int> { 0, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 0, 0, 0 }},
        { 16, new List<int> { 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 }},
        { 17, new List<int> { 0, 0, 0, 0, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0 }},
    };

    // Large Tiles
    public static List<int> largeRows = new List<int>()
    {
        8, 9, 10, 11, 12, 13, 14
    };

    public static List<int> largeCols = new List<int>()
    {
        15, 14, 13, 12, 11, 10, 9, 8, 7
    };

    public static Dictionary<int, List<int>> largeTileDict = new Dictionary<int, List<int>>()
    {
        {  1, new List<int> { 0, 0, 1, 1, 1, 1, 1 }},
        {  2, new List<int> { 0, 1, 1, 1, 1, 1, 1 }},
        {  3, new List<int> { 1, 1, 1, 1, 1, 1, 1 }},
        {  4, new List<int> { 1, 1, 1, 1, 1, 1, 1 }},
        {  5, new List<int> { 1, 1, 1, 1, 1, 1, 0 }},
        {  6, new List<int> { 1, 1, 1, 1, 1, 1, 0 }},
        {  7, new List<int> { 0, 1, 1, 1, 1, 1, 0 }},
        {  8, new List<int> { 0, 0, 1, 1, 1, 0, 0 }},
        {  9, new List<int> { 0, 0, 1, 1, 0, 0, 0 }},
    };

    public static string _folderPath = "";
    public static string exportPath = $"F:\\SteamLibrary\\steamapps\\common\\ELDEN RING\\Game\\layout_export.txt";

    public static bool GenerateDLC = false;

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        var inputWidth = 400.0f;

        if (ImGui.BeginTable($"worldMapGenTable", 3, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Input", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Action", ImGuiTableColumnFlags.WidthStretch);

            // Project Path
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);

            ImGui.AlignTextToFramePadding();
            ImGui.Text("Export Path");
            UIHelper.Tooltip("The folder path to place the exported world map.");

            ImGui.TableSetColumnIndex(1);

            ImGui.SetNextItemWidth(inputWidth);
            ImGui.InputText("##exportFolderPath", ref _folderPath, 255);

            ImGui.TableSetColumnIndex(2);

            if (ImGui.Button("Select##exportFolderPathSelect"))
            {
                var newFolderPath = "";
                var result = PlatformUtils.Instance.OpenFolderDialog("Select Export Folder", out newFolderPath);

                if (result)
                {
                    _folderPath = newFolderPath;
                    exportPath = $"{_folderPath}//layout_export.txt";
                }
            }

            ImGui.EndTable();
        }

        if (File.Exists(_folderPath))
        {
            ImGui.Checkbox("Generate DLC Map", ref GenerateDLC);

            if (ImGui.Button("Generate World Map"))
            {
                if(GenerateDLC)
                {
                    CalcSOTEWorldMapLayout();
                }
                else
                {
                    CalcWorldMapLayout();
                }
            }
        }
    }

    public static void CalcSOTEWorldMapLayout()
    {
        var xOffset = 350; // 
        var yOffset = 1150; // 

        var printStr = "";
        printStr = printStr + BuildTileLayout(xOffset, yOffset, "61", printStr, "02", 496, largeTileDict, largeCols, largeRows, false);
        printStr = printStr + BuildTileLayout(xOffset, yOffset, "61", printStr, "01", 248, mediumTileDict, mediumCols, mediumRows, false);
        printStr = printStr + BuildTileLayout(xOffset, yOffset, "61", printStr, "00", 124, smallTileDict, smallCols, smallRows, false);

        File.WriteAllText(exportPath, printStr);
    }

    /// <summary>
    /// Builds a .layout file for the ER World Map
    /// </summary>
    public static void CalcWorldMapLayout()
    {
        var printStr = "";
        printStr = printStr + BuildTileLayout(480, 55, "60", printStr, "02", 496, largeTileDict, largeCols, largeRows);
        printStr = printStr + BuildTileLayout(480, 55, "60", printStr, "01", 248, mediumTileDict, mediumCols, mediumRows);
        printStr = printStr + BuildTileLayout(480, 55, "60", printStr, "00", 124, smallTileDict, smallCols, smallRows);

        File.WriteAllText(exportPath, printStr);
    }

    private static string BuildTileLayout(int xOffset, int yOffset, string mapPrefix, string existingStr, string postfix, int size, Dictionary<int, List<int>> dict, List<int> cols, List<int> rows, bool respectTruthTable = true)
    {
        var xOff = xOffset;
        var yOff = yOffset;

        int rowIndex = 0;
        int colIndex = 0;
        int curX = 0 + xOff;
        int curY = 0 + yOff;

        // Each row
        foreach (var entry in dict)
        {
            string AA = mapPrefix;
            string DD = postfix;

            var CC = PadNumber(cols[colIndex]);

            // Row X
            foreach (var section in entry.Value)
            {
                string BB = PadNumber(rows[rowIndex]);

                // Only print if the truth table is true
                if (respectTruthTable && section == 1 || !respectTruthTable)
                {
                    existingStr = existingStr + $"\t<SubTexture name=\"m{AA}_{BB}_{CC}_{DD}.png\" x=\"{curX}\" y=\"{curY}\" width=\"{size}\" height=\"{size}\"/>\n";
                }

                rowIndex = rowIndex + 1;
                curX = curX + size;
            }

            curX = 0 + xOff;
            rowIndex = 0;
            colIndex = colIndex + 1;
            curY = curY + size;
        }

        return existingStr;
    }

    private static string PadNumber(int num)
    {
        if (num < 10)
        {
            return $"0{num}";
        }

        return num.ToString(); ;
    }
}
