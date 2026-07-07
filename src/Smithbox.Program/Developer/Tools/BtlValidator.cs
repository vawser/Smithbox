using Hexa.NET.ImGui;
using SoulsFormats;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.Application;

public class BtlValidator
{
    public List<MismatchData> MismatchedBtls = new List<MismatchData>();

    public int SelectedMap = -1;

    public bool DisplaySizeDiffsOnly = false;

    public void Display()
    {
        GUI.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Actions"),
            LOC.Get("DEV_Tool_Header_Actions_TT"));

        GUI.MultiButtonInput("validateActions",
            "validate", 
            LOC.Get("DEV_Tool_Validate_For_BP"),
            LOC.Get("DEV_Tool_Validate_For_BP_TT"),
            Run);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Options"),
            LOC.Get("DEV_Tool_Header_Options_TT"));

        ImGui.Checkbox($"{LOC.Get("DEV_Tool_Display_Size_Diffs_Only")}##sizeDiffsOnly", ref DisplaySizeDiffsOnly);
        ImGui.Separator();

        ImGui.Columns(2);

        int index = 0;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Map"),
            LOC.Get("DEV_Tool_Header_Map_TT"));

        ImGui.BeginChild("mapSection", new Vector2(0, 200), ImGuiChildFlags.Borders);

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

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Data"),
            LOC.Get("DEV_Tool_Header_Data_TT"));

        ImGui.BeginChild("dataSection", new Vector2(0, 200), ImGuiChildFlags.Borders);

        if (SelectedMap != -1)
        {
            var curMap = MismatchedBtls[SelectedMap];

            ImGui.Text(LOC.Get("DEV_Tool_Source_Bytes", curMap.SrcBytes));
            ImGui.Text(LOC.Get("DEV_Tool_Write_Bytes", curMap.WriteBytes));
            ImGui.Text(LOC.Get("DEV_Tool_Byte_Difference", curMap.ByteDiff));
        }

        ImGui.EndChild();
    }

    public void Run()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        MismatchedBtls = GetBtlMismatches(curProject);
    }

    public List<MismatchData> GetBtlMismatches(ProjectEntry curProject)
    {
        var mismatches = new List<MismatchData>();

        var lights = curProject.Locator.LightFiles.Entries;

        var ouputDir = Path.Combine(curProject.Descriptor.ProjectPath, ".tests", "btl-mismatches");

        if (!Directory.Exists(ouputDir))
        {
            Directory.CreateDirectory(ouputDir);
        }

        foreach (var entry in lights)
        {
            // Read the root version of the MSB
            var bytes = curProject.VFS.VanillaFS.ReadFile(entry.Path);
            var byteArray = bytes.Value.ToArray();
            var decompressed = DCX.Decompress(byteArray);

            // Write vanilla version
            if (!Directory.Exists(Path.Join(ouputDir, "decompressed")))
            {
                Directory.CreateDirectory(Path.Join(ouputDir, "decompressed"));
            }
            File.WriteAllBytes(
                Path.Join(ouputDir, "decompressed", Path.GetFileNameWithoutExtension(entry.Path)),
                decompressed.ToArray());

            byte[] written = new byte[0];

            BTL btl = BTL.Read(decompressed);
            written = btl.Write(DCX.Type.None);

            if (!Directory.Exists(Path.Join(ouputDir, "mismatches")))
            {
                Directory.CreateDirectory(Path.Join(ouputDir, "mismatches"));
            }

            File.WriteAllBytes(Path.Join(ouputDir, "mismatches", Path.GetFileNameWithoutExtension(entry.Path)),
                written);

            var isMismatch = false;

            if (!BytePerfectHelper.Md5Equal(decompressed.Span, written))
            {
                isMismatch = true;
            }

            if (isMismatch)
            {
                var mismatch = new MismatchData(entry.Filename, decompressed.Length, written.Length);
                mismatches.Add(mismatch);
            }
        }

        return mismatches;
    }
}
