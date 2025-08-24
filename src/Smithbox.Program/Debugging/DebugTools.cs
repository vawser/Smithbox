using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using Octokit;
using ProcessMemoryUtilities.Native;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Debug.Dumpers;
using StudioCore.Debug.Generators;
using StudioCore.Editors.TextEditor.Enums;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.DebugNS;

public class DebugTools
{
    private Smithbox BaseEditor;

    public bool ShowTaskWindow;
    public bool ShowImGuiDemo;

    public bool ShowParamValidator;
    public bool ShowMapValidator;
    public bool ShowTimeActValidator;

    public bool ShowFlverMaterialLayoutDumper;

    public bool ShowDokuWikiGenerator;
    public bool ShowFileDictionaryGenerator;
    public bool ShowWorldMapLayoutGenerator;

    public bool ShowTest_UniqueParamInsertion;
    public bool ShowTest_BHV;
    public bool ShowTest_BTL;
    public bool ShowTest_FLVER2;
    public bool ShowTest_MSB_AC6;
    public bool ShowTest_MSB_ACFA;
    public bool ShowTest_MSB_ACV;
    public bool ShowTest_MSB_ACVD;
    public bool ShowTest_MSB_ER;
    public bool ShowTest_MSB_NR;

    public DebugTools(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void DisplayMenu()
    {
        // Only display these tools this in Debug builds
#if DEBUG
        if (ImGui.BeginMenu("Debugging"))
        {
            // Quick action for testing stuff
            if (ImGui.MenuItem($"Quick Test"))
            {
                QuickTest();
            }
            if (ImGui.MenuItem($"Tasks"))
            {
                ShowTaskWindow = !ShowTaskWindow;
            }
            if (ImGui.MenuItem($"ImGui Demo"))
            {
                ShowImGuiDemo = !ShowImGuiDemo;
            }
            if (ImGui.BeginMenu("Tools"))
            {
                if (ImGui.BeginMenu("Validators"))
                {
                    if (ImGui.MenuItem($"Param"))
                    {
                        ShowParamValidator = !ShowParamValidator;
                    }
                    if (ImGui.MenuItem($"Map"))
                    {
                        ShowMapValidator = !ShowMapValidator;
                    }
                    if (ImGui.MenuItem($"Time Act"))
                    {
                        ShowTimeActValidator = !ShowTimeActValidator;
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Generators"))
                {
                    if (ImGui.MenuItem($"DokuWiki"))
                    {
                        ShowDokuWikiGenerator = !ShowDokuWikiGenerator;
                    }
                    if (ImGui.MenuItem($"File Dictionary"))
                    {
                        ShowFileDictionaryGenerator = !ShowFileDictionaryGenerator;
                    }
                    if (ImGui.MenuItem($"World Map Layout"))
                    {
                        ShowWorldMapLayoutGenerator = !ShowWorldMapLayoutGenerator;
                    }
                    ImGui.EndMenu();
                }
                if (ImGui.BeginMenu("Dumpers"))
                {
                    if (ImGui.MenuItem($"FLVER Material Layout"))
                    {
                        ShowFlverMaterialLayoutDumper = !ShowFlverMaterialLayoutDumper;
                    }
                    ImGui.EndMenu();
                }
                ImGui.EndMenu();
            }
            if (ImGui.BeginMenu("Tests"))
            {
                if (ImGui.MenuItem($"Unique Param Insertion"))
                {
                    ShowTest_UniqueParamInsertion = !ShowTest_UniqueParamInsertion;
                }
                if (ImGui.MenuItem($"BHV"))
                {
                    ShowTest_BHV = !ShowTest_BHV;
                }
                if (ImGui.MenuItem($"BTL"))
                {
                    ShowTest_BTL = !ShowTest_BTL;
                }
                if (ImGui.MenuItem($"FLVER2"))
                {
                    ShowTest_FLVER2 = !ShowTest_FLVER2;
                }
                if (ImGui.MenuItem($"MSB_AC6"))
                {
                    ShowTest_MSB_AC6 = !ShowTest_MSB_AC6;
                }
                if (ImGui.MenuItem($"MSB_ACFA"))
                {
                    ShowTest_MSB_ACFA = !ShowTest_MSB_ACFA;
                }
                if (ImGui.MenuItem($"MSB_ACV"))
                {
                    ShowTest_MSB_ACV = !ShowTest_MSB_ACV;
                }
                if (ImGui.MenuItem($"MSB_ACVD"))
                {
                    ShowTest_MSB_ACVD = !ShowTest_MSB_ACVD;
                }
                if (ImGui.MenuItem($"MSB_ER"))
                {
                    ShowTest_MSB_ER = !ShowTest_MSB_ER;
                }
                if (ImGui.MenuItem($"MSB_ERN"))
                {
                    ShowTest_MSB_NR = !ShowTest_MSB_NR;
                }
                ImGui.EndMenu();
            }
            ImGui.EndMenu();
        }
#endif
    }

    public void Display()
    {
        if (ShowTaskWindow)
        {
            if (ImGui.Begin("Task Viewer", ImGuiWindowFlags.AlwaysAutoResize))
            {
                TaskViewer.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowImGuiDemo)
        {
            ImGui.ShowDemoWindow();
        }
        if (ShowParamValidator)
        {
            if (ImGui.Begin("Param Validation", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ParamValidator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowMapValidator)
        {
            if (ImGui.Begin("Map Validation", ImGuiWindowFlags.AlwaysAutoResize))
            {
                MapValidator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowFlverMaterialLayoutDumper)
        {
            if (ImGui.Begin("FLVER Material Layout Dumper", ImGuiWindowFlags.AlwaysAutoResize))
            {
                FlverMaterialLayoutDumper.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowDokuWikiGenerator)
        {
            if (ImGui.Begin("DokuWiki Generator", ImGuiWindowFlags.AlwaysAutoResize))
            {
                DokuWikiGenerator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowFileDictionaryGenerator)
        {
            if (ImGui.Begin("File Dictionary Generator", ImGuiWindowFlags.AlwaysAutoResize))
            {
                FileDictionaryGenerator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowWorldMapLayoutGenerator)
        {
            if (ImGui.Begin("World Map Layout Generator", ImGuiWindowFlags.AlwaysAutoResize))
            {
                WorldMapLayoutGenerator.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_UniqueParamInsertion)
        {
            if (ImGui.Begin("Unique Param Insertion", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ParamUniqueInserter.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_BHV)
        {
            if (ImGui.Begin("BHV", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_BHV.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_BTL)
        {
            if (ImGui.Begin("BTL", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_BTL.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_FLVER2)
        {
            if (ImGui.Begin("FLVER2", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_FLVER2.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_MSB_AC6)
        {
            if (ImGui.Begin("MSB_AC6", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_AC6.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_MSB_ACFA)
        {
            if (ImGui.Begin("MSB_ACFA", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_ACFA.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_MSB_ACV)
        {
            if (ImGui.Begin("MSB_ACV", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_ACV.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_MSB_ACVD)
        {
            if (ImGui.Begin("MSB_ACVD", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_ACVD.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_MSB_ER)
        {
            if (ImGui.Begin("MSB_ER", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_ER.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
        if (ShowTest_MSB_NR)
        {
            if (ImGui.Begin("MSB_NR", ImGuiWindowFlags.AlwaysAutoResize))
            {
                Test_MSB_NR.Display(BaseEditor, BaseEditor.ProjectManager.SelectedProject);
                ImGui.End();
            }
        }
    }

    public void QuickTest()
    {
        GenerateIconLayouts_BB();
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
            for(int k = 0; k < width; k++)
            {
                var idStr = "";
                if(curId < 10)
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

        foreach(var param in store.Params)
        {
            var filename = $"{param.Name}.json";

            var writeFolder = Path.Combine(outputDir, type, group);
            if(!Directory.Exists(writeFolder))
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