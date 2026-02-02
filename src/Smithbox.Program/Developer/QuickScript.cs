using Andre.Formats;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Pfim;
using SoulsFormats;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Application;

public class QuickScript
{
    public static string BuildFolder = "";

    public static void ApplyQuickScript(ProjectEntry curProject)
    {
        BuildFolder = $@"{CFG.Current.Developer_Smithbox_Build_Folder}\src\Smithbox.Data\Assets\PARAM\DS2S\Icon Layouts";

        GenerateIconLayouts_DS2();
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
                    if(special)
                    {
                        width = 128;
                        height = 256;

                        // Armor
                        if (id >= 20000000 && id < 30000000)
                        {
                            // Gaunlets
                            if(id % 100 == 2)
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
            TaskLogs.AddError($"Folder doesn't exist: {BuildFolder}");
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

    public static void GenerateIconLayout(string filename, int idStart, int width, int height, int iconIncrement, int resolution)
    {
        if (!Path.Exists(BuildFolder))
        {
            TaskLogs.AddError($"Folder doesn't exist: {BuildFolder}");
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

                var line = $@"    <SubTexture name=""ICON_{idStr}.png"" x=""{curX}"" y=""{curY}"" width=""160""  height=""160"" originalWidth=""160"" originalHeight=""160"" half=""0""/>";

                lines.Add(line);

                curX = (iconIncrement * (k + 1));
                curId = curId + 1;
            }

            curX = 0;
            curY = (iconIncrement * (i + 1));
        }

        lines.Add(footer);

        var outputFile = Path.Combine(BuildFolder, $"{filename}.layout");

        File.WriteAllLines(outputFile, lines);
    }
}
