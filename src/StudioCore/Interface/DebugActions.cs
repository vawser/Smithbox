using DotNext.IO;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Formats;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static SoapstoneLib.SoulsObject;

namespace StudioCore.Interface;

public static class DebugActions
{
    public static List<MSB_AC6> maps = new List<MSB_AC6>();
    public static List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();
    private static MapPropertyCache _propCache;

    public static void LoadMsbData()
    {
        var mapDir = $"{Project.GameRootDirectory}/map/mapstudio/";

        foreach(var entry in Directory.EnumerateFiles(mapDir))
        {
            if (entry.Contains(".msb.dcx"))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                ResourceDescriptor ad = ResourceMapLocator.GetMapMSB(name);
                if (ad.AssetPath != null)
                {
                    resMaps.Add(ad);
                }
            }
        }

        foreach(var res in resMaps)
        {
            var msb = MSB_AC6.Read(res.AssetPath);
            maps.Add(msb);
        }
    }

    public static void SearchInMsbForValue(int searchValue)
    {
        foreach(var map in maps)
        {
            var enemies = map.Parts.Enemies;
            foreach(var ene in enemies)
            {
                var prop = ene.GetType();
                PropertyInfo[] properties = prop.GetProperties();
                foreach(var field in properties)
                {
                    TaskLogs.AddLog($"{field.Name}");
                }
            }
        }
    }

    public static void ForceCrash()
    {
        var badArray = new int[2];
        var crash = badArray[5];
    }

    public static void DumpFlverLayouts()
    {
        if (PlatformUtils.Instance.SaveFileDialog("Save Flver layout dump", new[] { FilterStrings.TxtFilter },
                 out var path))
        {
            using (StreamWriter file = new(path))
            {
                foreach (KeyValuePair<string, FLVER2.BufferLayout> mat in FlverResource.MaterialLayouts)
                {
                    file.WriteLine(mat.Key + ":");
                    foreach (FLVER.LayoutMember member in mat.Value)
                    {
                        file.WriteLine($@"{member.Index}: {member.Type.ToString()}: {member.Semantic.ToString()}");
                    }

                    file.WriteLine();
                }
            }
        }
    }

    public static void ReadShoeboxFile()
    {
        string sourcePath = $@"F:\\SteamLibrary\\steamapps\\common\\ELDEN RING\\Game\\menu\\hi\01_common.sblytbnd.dcx";

        ShoeboxLayoutContainer container = new ShoeboxLayoutContainer(sourcePath);
        foreach (var layout in container.Layouts)
        {
            foreach (var texAtlas in layout.Value.TextureAtlases)
            {
                foreach (var subText in texAtlas.SubTextures)
                {
                    TaskLogs.AddLog($"{subText.Name}");
                    TaskLogs.AddLog($"{subText.X}");
                    TaskLogs.AddLog($"{subText.Y}");
                    TaskLogs.AddLog($"{subText.Width}");
                    TaskLogs.AddLog($"{subText.Height}");
                    TaskLogs.AddLog($"{subText.Half}");
                }
            }
        }
    }

    public static void DumpUncompressedFiles()
    {
        string sourcePath = "F:\\SteamLibrary\\steamapps\\common\\ARMORED CORE VI FIRES OF RUBICON\\Game\\script";
        string destPath = "C:\\Users\\benja\\Programming\\C#\\Smithbox\\Dump";
        string ext = $"*.luabnd.dcx";

        foreach (string path in Directory.GetFiles(sourcePath, ext))
        {
            TaskLogs.AddLog($"{path}");

            var bnd = BND4.Read(path);
            foreach(var file in bnd.Files)
            {
                var name = Path.GetFileName(Path.GetFileName(file.Name));

                File.WriteAllBytes($@"{destPath}\\lua\\{name}", file.Bytes.ToArray());
            }
        }
    }

    private static string sourceMap = "";
    private static string sourcePath = "";
    private static string destPath = "";

    public static void CollectTextures()
    {
        ImGui.Text("Collect Textures");

        ImGui.InputText("Source Map:", ref sourceMap, 1024);

        ImGui.InputText("Destination:", ref destPath, 1024);
        ImGui.SameLine();
        if (ImGui.Button("Select##destSelect"))
        {
            if (PlatformUtils.Instance.OpenFolderDialog("Choose destination directory", out var path))
            {
                destPath = path;
            }
        }

        if (ImGui.Button("Collect"))
        {
            List<string> sourcePaths = new List<string>
            {
                $"{Project.GameRootDirectory}\\map\\{sourceMap}\\{sourceMap}_0000-tpfbhd",
                $"{Project.GameRootDirectory}\\map\\{sourceMap}\\{sourceMap}_0001-tpfbhd",
                $"{Project.GameRootDirectory}\\map\\{sourceMap}\\{sourceMap}_0002-tpfbhd",
                $"{Project.GameRootDirectory}\\map\\{sourceMap}\\{sourceMap}_0003-tpfbhd"
            };

            List<string> witchyEntries = new List<string>();

            foreach (var srcPath in sourcePaths)
            {
                List<string> newEntries = MoveTextures(srcPath, destPath);
                foreach (var entry in newEntries)
                {
                    witchyEntries.Add(entry);
                }
            }

            File.WriteAllLines(Path.Combine(destPath, "_entries.txt"), witchyEntries);
        }
    }

    private static List<string> MoveTextures(string pSrcPath, string pDstPath)
    {
        List<string> entries = new List<string>();

        if (Directory.Exists(pSrcPath))
        {
            foreach (var entry in Directory.GetDirectories(pSrcPath))
            {
                TaskLogs.AddLog($"{entry}");

                foreach (var fEntry in Directory.GetFiles(entry))
                {
                    var srcPath = fEntry;
                    var filename = Path.GetFileName(fEntry);
                    var dstPath = Path.Combine(pDstPath, filename);

                    if (fEntry.Contains(".dds"))
                    {
                        TaskLogs.AddLog($"{fEntry}");

                        var format = 0;
                        // Color
                        if (fEntry.Contains("_a.dds"))
                        {
                            TaskLogs.AddLog($"Color");
                            format = 0;
                        }
                        // Metallic
                        if (fEntry.Contains("_m.dds"))
                        {
                            TaskLogs.AddLog($"Metallic");
                            format = 103;
                        }
                        // Reflectance
                        if (fEntry.Contains("_r.dds"))
                        {
                            TaskLogs.AddLog($"Reflectance");
                            format = 0;
                        }
                        // Normal
                        if (fEntry.Contains("_n.dds"))
                        {
                            TaskLogs.AddLog($"Normal");
                            format = 106;
                        }
                        // Normal
                        if (fEntry.Contains("_v.dds"))
                        {
                            TaskLogs.AddLog($"Volume");
                            format = 104;
                        }

                        if (File.Exists(srcPath))
                        {
                            entries.Add($"<texture>\r\n      <name>{filename}</name>\r\n      <format>{format}</format>\r\n      <flags1>0x00</flags1>\r\n    </texture>");

                            File.Copy(srcPath, dstPath, true);
                        }
                    }
                }
            }
        }

        return entries;
    }

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

    /// <summary>
    /// Builds a .layout file for the ER World Map
    /// </summary>
    public static void CalcWorldMapLayout()
    {
        var path = $"{AppContext.BaseDirectory}/layout_export.txt";

        var printStr = "";
        printStr = printStr + BuildTileLayout(printStr, "02", 496, largeTileDict, largeCols, largeRows);
        printStr = printStr + BuildTileLayout(printStr, "01", 248, mediumTileDict, mediumCols, mediumRows);
        printStr = printStr + BuildTileLayout(printStr, "00", 124, smallTileDict, smallCols, smallRows);

        File.WriteAllText(path, printStr);
    }

    private static string BuildTileLayout(string existingStr, string postfix, int size, Dictionary<int, List<int>> dict, List<int> cols, List<int> rows)
    {
        var xOff = 480;
        var yOff = 55;

        int rowIndex = 0;
        int colIndex = 0;
        int curX = 0 + xOff;
        int curY = 0 + yOff;

        // Each row
        foreach (var entry in dict)
        {
            string AA = "60";
            string DD = postfix;

            var CC = PadNumber(cols[colIndex]);

            // Row X
            foreach (var section in entry.Value)
            {
                string BB = PadNumber(rows[rowIndex]);

                // Only print if the truth table is true
                if (section == 1)
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
        if(num < 10)
        {
            return $"0{num}";
        }

        return num.ToString(); ;
    }
}
