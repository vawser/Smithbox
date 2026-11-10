using Microsoft.Extensions.Logging;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Developer;

public static class IconLayouts
{
    public static string BuildFolder = "";

    public static void Apply()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

    }

    public static void GenerateIconLayouts_DS2()
    {
        TraverseDS2Icons("tex\\icon", "ic_", "/menu/tex/icon", 128, 256, "Item", true);

        TraverseDS2Icons("tex\\icon\\bonfire_area", "ic_area_", "/menu/tex/icon/bonfire_area", 256, 128, "Bonfire Area");

        TraverseDS2Icons("tex\\icon\\bonfire_list", "ic_list_", "/menu/tex/icon/bonfire_list", 256, 128, "Bonfire List");

        TraverseDS2Icons("tex\\icon\\charamaking", "ic_cm_", "/menu/tex/icon/charamaking", 128, 128, "Character Menu");

        TraverseDS2Icons("tex\\icon\\effect", "ei_", "/menu/tex/icon/effect", 64, 64, "Effect");

        TraverseDS2Icons("tex\\icon\\item_category", "ic_ca_", "/menu/tex/icon/item_category", 64, 64, "Item Category");

        // Assuming english here for simplicity
        TraverseDS2Icons("tex\\icon\\mapname\\english", "map_name_", "/menu/tex/icon/mapname/english", 1024, 64, "Map Names");

        TraverseDS2Icons("tex\\icon\\vow", "vi_", "/menu/tex/icon/vow", 64, 64, "Vow");
    }

    public static void TraverseDS2Icons(string path, string prefix, string imagePath, int width, int height, string type, bool special = false)
    {
        var searchPath = $"F:\\SteamLibrary\\steamapps\\common\\Dark Souls II Scholar of the First Sin\\Game\\menu\\{path}";

        foreach (var file in Directory.EnumerateFiles(searchPath))
        {
            var filename = Path.GetFileNameWithoutExtension(file);

            if (filename.StartsWith(prefix))
            {
                var idStr = filename.Replace(prefix, "");

                if (long.TryParse(idStr, out long id))
                {
                    // Adjust the width and height based on the ID (for the items, where the sizing is varied
                    if (special)
                    {
                        width = 128;
                        height = 256;

                        // Armor
                        if (id >= 20000000 && id < 30000000)
                        {
                            // Gaunlets
                            if (id % 100 == 2)
                            {
                                width = 64;
                                height = 128;
                            }
                        }

                        if (id >= 30000000 && id < 1000000000)
                        {
                            width = 64;
                            height = 128;
                        }

                        if (id >= 1800000000 && id < 4000000000)
                        {
                            width = 64;
                            height = 128;
                        }
                        if (id >= 4000000000)
                        {
                            width = 128;
                            height = 128;
                        }
                    }

                    GenerateDS2IconLayout(imagePath, filename, id, width, height, type);
                }
            }
        }
    }

    public static void GenerateDS2IconLayout(string imagePath, string filename, long id, int width, int height, string type)
    {
        if (!Path.Exists(BuildFolder))
        {
            Smithbox.Log<DeveloperPanel>(LOC.Get("DEV_Tool_Icon_Layout_Invalid_Folder", BuildFolder), LogLevel.Error);
            return;
        }

        var header = $@"<TextureAtlas imagePath=""{imagePath}/{filename}.tpf"" width=""{width}"" height=""{height}"" type=""{type}"">";
        var footer = @"</TextureAtlas>";

        List<string> lines = new();

        lines.Add(header);

        var line = $@"    <SubTexture name=""ICON_{id}.png"" x=""0"" y=""0"" width=""{width}""  height=""{height}"" originalWidth=""{width}"" originalHeight=""{height}"" half=""0""/>";

        lines.Add(line);

        lines.Add(footer);

        var outputFile = Path.Combine(BuildFolder, $"{filename}.layout");

        File.WriteAllLines(outputFile, lines);
    }

    /// <summary>
    /// DS3 MENU_Icon sheets (/menu/01_common.tpf): 160 px cells, 12 x 12. A sheet MENU_Icon_XXYYY starts at XX * 1000 + YYY * 144,
    /// checked against the vanilla params' icon IDs (6000-6019 and 7000-7044 are used, on 06000 and 07000).
    /// Grids keep 12 rows even where the vanilla sheet is shorter, so a mod that enlarges a sheet still lines up.
    /// </summary>
    public static void GenerateIconLayouts_DS3()
    {
        foreach (var (sheet, idStart) in new (string, int)[]
        {
            ("MENU_Icon_00000", 0), ("MENU_Icon_00001", 144),
            ("MENU_Icon_01000", 1000), ("MENU_Icon_01001", 1144), ("MENU_Icon_01002", 1288),
            ("MENU_Icon_02000", 2000),
            ("MENU_Icon_03000", 3000), ("MENU_Icon_03001", 3144), ("MENU_Icon_03002", 3288), ("MENU_Icon_03003", 3432),
            ("MENU_Icon_04000", 4000), ("MENU_Icon_04001", 4144),
            ("MENU_Icon_05000", 5000), ("MENU_Icon_06000", 6000), ("MENU_Icon_07000", 7000),
            ("MENU_Icon_08000", 8000), ("MENU_Icon_09000", 9000),
        })
        {
            GenerateIconLayout(sheet, idStart, 12, 12, 160, 2048);
        }
    }

    /// <summary>
    /// BB MENU_Icon sheets (/menu/common.tpf): 80 px cells, 12 x 12 on 1024 sheets; 06001 and 07001 are 32 px cells, 32 x 4.
    /// The 03001-04002 gem sheets number on from 3000 without a gap. The ID ranges are unchanged from the earlier layouts;
    /// accessory (rune) icons 4000-4043, GemEffect and SpEffect (8800+) icons are not placed by these grids
    /// (see Documentation/Claude/Icon Preview Mod Support.md).
    /// </summary>
    public static void GenerateIconLayouts_BB()
    {
        foreach (var (sheet, idStart) in new (string, int)[]
        {
            ("MENU_Icon_00001", 0), ("MENU_Icon_00002", 144), ("MENU_Icon_00003", 288),
            ("MENU_Icon_01001", 1000),
            ("MENU_Icon_02001", 2000), ("MENU_Icon_02002", 2144),
            ("MENU_Icon_03001", 3000), ("MENU_Icon_03002", 3144), ("MENU_Icon_03003", 3288), ("MENU_Icon_03004", 3432),
            ("MENU_Icon_03005", 3576), ("MENU_Icon_03006", 3720), ("MENU_Icon_03007", 3864), ("MENU_Icon_03008", 4008),
            ("MENU_Icon_03009", 4152), ("MENU_Icon_04001", 4296), ("MENU_Icon_04002", 4440),
            ("MENU_Icon_05001", 5000),
        })
        {
            GenerateIconLayout(sheet, idStart, 12, 12, 80, 1024, sheet == "MENU_Icon_04002" ? BB_RuneCells() : null);
        }

        GenerateIconLayout("MENU_Icon_06001", 6000, 32, 4, 32, 1024);
        GenerateIconLayout("MENU_Icon_07001", 0, 32, 4, 32, 1024);
    }

    /// <summary>
    /// BB rune icons, as (icon id, cell index on MENU_Icon_04002). GemeffectParam.gemIconIdOffset 1200-1254 are the
    /// Caryll runes, 13 per tier in sheet order from cell 56 (cell = offset - 1144); the covenant runes 1013-1015 and
    /// 1049-1051 sit 200 below their cells (the cyan ones). Found by matching rune effects to glyphs; see
    /// Documentation/Claude/Icon Preview Mod Support.md.
    /// </summary>
    public static List<(int Id, int Cell)> BB_RuneCells()
    {
        var cells = new List<(int Id, int Cell)>();

        for (int offset = 1200; offset <= 1254; offset++)
            cells.Add((offset, offset - 1144));

        foreach (var offset in new[] { 1013, 1014, 1015, 1049, 1050, 1051 })
            cells.Add((offset, offset + 200 - 1144));

        return cells;
    }

    /// <summary>
    /// Writes a grid layout: width x height cells of iconIncrement pixels, numbered from idStart row by row.
    /// The crop is the cell size (it was a fixed 160, which cropped 2 x 2 cells on BB's 80 px sheets).
    /// extraCells adds further sub-textures on the same grid, as (icon id, cell index).
    /// </summary>
    public static void GenerateIconLayout(string filename, int idStart, int width, int height, int iconIncrement, int resolution, List<(int Id, int Cell)> extraCells = null)
    {
        if (!Path.Exists(BuildFolder))
        {
            Smithbox.Log<DeveloperPanel>($"Folder doesn't exist: {BuildFolder}", LogLevel.Error);
            return;
        }

        var header = $@"<TextureAtlas imagePath=""{filename}.tif"" width=""{resolution}"" height=""{resolution}"">";
        var footer = @"</TextureAtlas>";

        List<string> lines = new();

        lines.Add(header);

        var curId = idStart;
        var curX = 0;
        var curY = 0;

        // Rows
        for (int i = 0; i < height; i++)
        {
            // Icon in Row
            for (int k = 0; k < width; k++)
            {
                var idStr = "";
                if (curId < 10)
                {
                    idStr = $"0000{curId}";
                }
                else if (curId >= 10 && curId < 100)
                {
                    idStr = $"000{curId}";
                }
                else if (curId >= 100 && curId < 1000)
                {
                    idStr = $"00{curId}";
                }
                else if (curId >= 1000 && curId < 10000)
                {
                    idStr = $"0{curId}";
                }
                else
                {
                    idStr = $"{curId}";
                }

                var line = $@"    <SubTexture name=""ICON_{idStr}.png"" x=""{curX}"" y=""{curY}"" width=""{iconIncrement}""  height=""{iconIncrement}"" originalWidth=""{iconIncrement}"" originalHeight=""{iconIncrement}"" half=""0""/>";

                lines.Add(line);

                curX = (iconIncrement * (k + 1));
                curId = curId + 1;
            }

            curX = 0;
            curY = (iconIncrement * (i + 1));
        }

        if (extraCells != null)
        {
            foreach (var (id, cell) in extraCells)
            {
                var x = cell % width * iconIncrement;
                var y = cell / width * iconIncrement;
                lines.Add($@"    <SubTexture name=""ICON_{id:D5}.png"" x=""{x}"" y=""{y}"" width=""{iconIncrement}""  height=""{iconIncrement}"" originalWidth=""{iconIncrement}"" originalHeight=""{iconIncrement}"" half=""0""/>");
            }
        }

        lines.Add(footer);

        var outputFile = Path.Combine(BuildFolder, $"{filename}.layout");

        File.WriteAllLines(outputFile, lines);
    }
}