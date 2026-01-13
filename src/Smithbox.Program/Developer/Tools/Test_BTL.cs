using Hexa.NET.ImGui;
using System.Collections.Generic;
using System.Numerics;

namespace StudioCore.Application;

public static class Test_BTL
{
    public static List<MismatchData> MismatchedBtls = new List<MismatchData>();

    public static int SelectedMap = -1;

    public static bool DisplaySizeDiffsOnly = false;

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        var windowSize = DPI.GetWindowSize(baseEditor._context);
        var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
        var sectionHeight = windowSize.Y * 0.3f;
        var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

        if (ImGui.Button("Check all BTLs for Byte-Perfect Match", DPI.StandardButtonSize))
        {
            Run(baseEditor);
        }

        ImGui.SameLine();

        ImGui.Checkbox("Display Size Diffs Only", ref DisplaySizeDiffsOnly);

        ImGui.Separator();

        ImGui.Columns(2);

        int index = 0;

        ImGui.BeginChild("mapSection", sectionSize, ImGuiChildFlags.Borders);

        foreach (var entry in MismatchedBtls)
        {
            if (DisplaySizeDiffsOnly)
            {
                if (entry.ByteDiff == 0)
                {
                    index++;
                    continue;
                }
            }

            if (ImGui.Selectable($"{entry.Name}##curMap{index}"))
            {
                SelectedMap = index;
            }

            index++;
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        ImGui.BeginChild("dataSection", sectionSize, ImGuiChildFlags.Borders);

        if (SelectedMap != -1)
        {
            var curMap = MismatchedBtls[SelectedMap];

            ImGui.Text($"Source Bytes: {curMap.SrcBytes}");
            ImGui.Text($"Write Bytes: {curMap.WriteBytes}");
            ImGui.Text($"Byte Difference: {curMap.ByteDiff}");
        }

        ImGui.EndChild();
    }

    public static bool Run(Smithbox baseEditor)
    {
        var curProject = baseEditor.ProjectManager.SelectedProject;

        MismatchedBtls = Test_MSB_Util.GetBtlMismatches(curProject);

        return true;
    }
}
