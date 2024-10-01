using ImGuiNET;
using SoulsFormats;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MSB_AC6;

namespace StudioCore.Tests;

public static class Test_FLVER2_BytePerfect
{
    public static List<string> Output = new List<string>();

    public static void Display()
    {
        var buttonSize = new Vector2(ImGui.GetWindowWidth(), 32);

        if (ImGui.Button("Check Loaded Model for Byte Perfectness", buttonSize))
        {
            Run();
        }

        ImGui.Separator();

        // Info
        foreach(var entry in Output)
        {
            ImGui.Text(entry);
        }
    }

    public static void Run()
    {
        Output = new List<string>();

        var currentFlver = Smithbox.EditorHandler.ModelEditor.ResourceHandler.GetCurrentInternalFile();

        if(currentFlver == null)
        {
            Output.Add("No FLVER loaded");
            return;
        }

        var name = currentFlver.Name;
        var newBytes = currentFlver.CurrentFLVER.Write();
        var oldBytes = currentFlver.InitialFlverBytes;

        Output.Add($"Old Bytes: {oldBytes.Length}");
        Output.Add($"New Bytes: {newBytes.Length}");

        var outputDir = $"{Smithbox.ProjectRoot}\\_flverTest\\";

        // Cleanup old files
        if(Directory.Exists(outputDir))
        {
            Directory.Delete(outputDir, true);
        }

        Directory.CreateDirectory(outputDir);

        File.WriteAllBytes($"{outputDir}\\Original_{name}.flver", oldBytes);
        File.WriteAllBytes($"{outputDir}\\Saved_{name}.flver", newBytes);
    }
}
