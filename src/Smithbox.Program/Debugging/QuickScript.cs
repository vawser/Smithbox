using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.DebugNS;

public class QuickScript
{
    public static void ApplyQuickScript()
    {

    }

    public void GenerateIconLayouts_BB()
    {
        GenerateIconLayout("MENU_Icon_00001", 0, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_00002", 144, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_00003", 288, 12, 12, 80, 1024);

        GenerateIconLayout("MENU_Icon_01001", 1000, 12, 12, 80, 1024);

        GenerateIconLayout("MENU_Icon_02001", 2000, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_02002", 2144, 12, 12, 80, 1024);

        GenerateIconLayout("MENU_Icon_03001", 3000, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_03002", 3144, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_03003", 3288, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_03004", 3432, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_03005", 3576, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_03006", 3720, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_03007", 3864, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_03008", 4008, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_03009", 4152, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_04001", 4296, 12, 12, 80, 1024);
        GenerateIconLayout("MENU_Icon_04002", 4440, 12, 12, 80, 1024);

        GenerateIconLayout("MENU_Icon_05001", 5000, 12, 12, 80, 1024);

        GenerateIconLayout("MENU_Icon_06001", 6000, 32, 4, 32, 1024);

        GenerateIconLayout("MENU_Icon_07001", 0, 32, 4, 32, 1024);
    }

    public void GenerateIconLayouts_DS3()
    {
        GenerateIconLayout("MENU_Icon_00000", 0, 12, 12, 160, 2048);
        GenerateIconLayout("MENU_Icon_00001", 144, 12, 12, 160, 2048);

        GenerateIconLayout("MENU_Icon_01000", 1000, 12, 12, 160, 2048);
        GenerateIconLayout("MENU_Icon_01001", 1144, 12, 12, 160, 2048);
        GenerateIconLayout("MENU_Icon_01002", 1288, 12, 12, 160, 2048);

        GenerateIconLayout("MENU_Icon_02000", 2000, 12, 12, 160, 2048);

        GenerateIconLayout("MENU_Icon_03000", 3000, 12, 12, 160, 2048);
        GenerateIconLayout("MENU_Icon_03001", 3144, 12, 12, 160, 2048);
        GenerateIconLayout("MENU_Icon_03002", 3288, 12, 12, 160, 2048);
        GenerateIconLayout("MENU_Icon_03003", 3432, 12, 12, 160, 2048);

        GenerateIconLayout("MENU_Icon_04000", 4000, 12, 12, 160, 2048);
        GenerateIconLayout("MENU_Icon_04001", 4144, 12, 12, 160, 2048);

        GenerateIconLayout("MENU_Icon_05000", 5000, 12, 12, 160, 2048);

        GenerateIconLayout("MENU_Icon_06000", 5000, 12, 12, 160, 2048);

        GenerateIconLayout("MENU_Icon_07000", 5000, 12, 12, 160, 2048);

        GenerateIconLayout("MENU_Icon_08000", 5000, 12, 12, 160, 2048);

        GenerateIconLayout("MENU_Icon_09000", 5000, 12, 12, 160, 2048);
    }

    public void GenerateIconLayout(string filename, int idStart, int width, int height, int iconIncrement, int resolution)
    {
        var outputDir = @"C:\Users\benja\Programming\C#\Smithbox\src\Smithbox.Data\Assets\PARAM\BB\Icon Layouts";

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

        var outputFile = Path.Combine(outputDir, $"{filename}.layout");

        File.WriteAllLines(outputFile, lines);
    }

    public static void ConvertToOldStyleRowNames(string type, string group)
    {
        var outputDir = @"C:\Users\benja\Programming\C#\_temp";

        var sourceFilepath = @$"{AppContext.BaseDirectory}/Assets/PARAM/{type}";
        sourceFilepath = Path.Combine(sourceFilepath, $"{group}.json");


        RowNameStore store = null;

        try
        {
            var filestring = File.ReadAllText(sourceFilepath);

            store = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.RowNameStore);

            if (store == null)
            {
                throw new Exception($"JsonConvert returned null.");
            }
        }
        catch (Exception e)
        {
        }

        if (store == null)
            return;

        if (store.Params == null)
            return;

        foreach (var param in store.Params)
        {
            var filename = $"{param.Name}.json";

            var writeFolder = Path.Combine(outputDir, type, group);
            if (!Directory.Exists(writeFolder))
            {
                Directory.CreateDirectory(writeFolder);
            }

            var writePath = Path.Combine(outputDir, type, group, filename);

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true,
                IncludeFields = true
            };
            var json = JsonSerializer.Serialize(param, typeof(RowNameParam), options);

            File.WriteAllText(writePath, json);
        }
    }
    public static void SplitAliases()
    {

        string aliasesDir = @$"G:\Creative\GitHub\Smithbox\src\Smithbox.Data\Assets\Aliases";
        var games = Directory.GetDirectories(aliasesDir);
        foreach (string gameDir in games)
        {
            string sourceFile = File.ReadAllText(Path.Combine(gameDir, "Aliases.json"));
            AliasStore store = JsonSerializer.Deserialize(sourceFile, SmithboxSerializerContext.Default.AliasStore);
            foreach ((AliasType aliasType, List<AliasEntry> entries) in store)
            {
                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IncludeFields = true
                };
                var json = JsonSerializer.Serialize(entries, typeof(List<AliasEntry>), options);

                File.WriteAllText(Path.Combine(gameDir, $"{aliasType.ToString()}.json"), json);
            }

            File.Delete(Path.Combine(gameDir, "Aliases.json"));
        }
    }
}
