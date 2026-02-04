using Hexa.NET.ImGui;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Application;

public static class Test_MSB
{
    public static List<MismatchData> MismatchedMaps = new List<MismatchData>();

    public static int SelectedMap = -1;

    public static bool DisplaySizeDiffsOnly = false;

    public static void Display(ProjectEntry project)
    {
        var windowSize = DPI.GetWindowSize(Smithbox.Instance._context);
        var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
        var sectionHeight = windowSize.Y * 0.3f;
        var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

        if (ImGui.Button("Check all Maps for Byte-Perfect Match", DPI.StandardButtonSize))
        {
            Run();
        }

        ImGui.SameLine();

        ImGui.Checkbox("Display Size Diffs Only", ref DisplaySizeDiffsOnly);

        ImGui.Separator();

        ImGui.Columns(2);

        int index = 0;

        ImGui.BeginChild("mapSection", sectionSize, ImGuiChildFlags.Borders);

        foreach (var entry in MismatchedMaps)
        {
            if(DisplaySizeDiffsOnly)
            {
                if(entry.ByteDiff == 0)
                {
                    index++;
                    continue;
                }
            }

            if(ImGui.Selectable($"{entry.Name}##curMap{index}"))
            {
                SelectedMap = index;
            }

            index++;
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild("dataSection", sectionSize, ImGuiChildFlags.Borders);

        if(SelectedMap != -1)
        {
            var curMap = MismatchedMaps[SelectedMap];

            ImGui.Text($"Source Bytes: {curMap.SrcBytes}");
            ImGui.Text($"Write Bytes: {curMap.WriteBytes}");
            ImGui.Text($"Byte Difference: {curMap.ByteDiff}");
        }

        ImGui.EndChild();
    }

    public static bool Run()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        MismatchedMaps = Test_MSB_Util.GetMsbMismatches(curProject);

        return true;
    }
}

