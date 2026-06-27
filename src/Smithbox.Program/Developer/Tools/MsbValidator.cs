using Hexa.NET.ImGui;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.Application;

public class MsbValidator
{
    public List<MismatchData> MismatchedMaps = new List<MismatchData>();

    public int SelectedMap = -1;

    public bool DisplaySizeDiffsOnly = false;

    public void Display()
    {
        UIHelper.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Actions"),
            LOC.Get("DEV_Tool_Header_Actions_TT"));

        UIHelper.MultiButtonInput("validateActions",
            "validate",
            LOC.Get("DEV_Tool_Validate_For_BP"),
            LOC.Get("DEV_Tool_Validate_For_BP_TT"),
            Run);

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Options"),
            LOC.Get("DEV_Tool_Header_Options_TT"));

        ImGui.Checkbox($"{LOC.Get("DEV_Tool_Display_Size_Diffs_Only")}##sizeDiffsOnly", ref DisplaySizeDiffsOnly);
        ImGui.Separator();

        ImGui.Columns(2);

        int index = 0;

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Map"),
            LOC.Get("DEV_Tool_Header_Map_TT"));

        ImGui.BeginChild("mapSection", new Vector2(0, 200), ImGuiChildFlags.Borders);

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

        UIHelper.Spacer();
        UIHelper.SimpleHeader(
            LOC.Get("DEV_Tool_Header_Data"),
            LOC.Get("DEV_Tool_Header_Data_TT"));

        ImGui.BeginChild("dataSection", new Vector2(0, 200), ImGuiChildFlags.Borders);

        if (SelectedMap != -1)
        {
            var curMap = MismatchedMaps[SelectedMap];

            ImGui.Text(LOC.Get("DEV_Tool_Source_Bytes", curMap.SrcBytes));
            ImGui.Text(LOC.Get("DEV_Tool_Write_Bytes", curMap.WriteBytes));
            ImGui.Text(LOC.Get("DEV_Tool_Byte_Difference", curMap.ByteDiff));
        }

        ImGui.EndChild();
    }

    public void Run()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        MismatchedMaps = GetMsbMismatches(curProject);
    }

    public List<MismatchData> GetMsbMismatches(ProjectEntry curProject)
    {
        var mismatches = new List<MismatchData>();

        var maps = curProject.Locator.MapFiles.Entries;

        var ouputDir = Path.Combine(curProject.Descriptor.ProjectPath, ".tests", "msb-mismatches");

        if (!Directory.Exists(ouputDir))
        {
            Directory.CreateDirectory(ouputDir);
        }

        var isCompressedMap = true;

        if (curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            isCompressedMap = false;
        }

        foreach (var entry in maps)
        {
            // Read the root version of the MSB
            var bytes = curProject.VFS.VanillaFS.ReadFile(entry.Path);
            var data = (Memory<byte>)bytes;

            if (isCompressedMap)
            {
                var decompressed = DCX.Decompress(data);

                // Write vanilla version
                if (!Directory.Exists(Path.Join(ouputDir, "decompressed")))
                {
                    Directory.CreateDirectory(Path.Join(ouputDir, "decompressed"));
                }
                File.WriteAllBytes(
                    Path.Join(ouputDir, "decompressed", Path.GetFileNameWithoutExtension(entry.Path)),
                    decompressed.ToArray());

                data = decompressed.ToArray();
            }

            byte[] written = new byte[0];

            switch (curProject.Descriptor.ProjectType)
            {
                case ProjectType.NR:
                    MSB_NR msb_nr = MSB_NR.Read(data);
                    written = msb_nr.Write(DCX.Type.None);
                    break;
                case ProjectType.AC6:
                    MSB_AC6 msb_ac6 = MSB_AC6.Read(data);
                    written = msb_ac6.Write(DCX.Type.None);
                    break;
                case ProjectType.ER:
                    MSBE msb_er = MSBE.Read(data);
                    written = msb_er.Write(DCX.Type.None);
                    break;
                case ProjectType.SDT:
                    MSBS msb_sdt = MSBS.Read(data);
                    written = msb_sdt.Write(DCX.Type.None);
                    break;
                case ProjectType.DS3:
                    MSB3 msb_ds3 = MSB3.Read(data);
                    written = msb_ds3.Write(DCX.Type.None);
                    break;
                case ProjectType.BB:
                    MSBB msb_bb = MSBB.Read(data);
                    written = msb_bb.Write(DCX.Type.None);
                    break;
                case ProjectType.DS2S:
                case ProjectType.DS2:
                    MSB2 msb_ds2 = MSB2.Read(data);
                    written = msb_ds2.Write(DCX.Type.None);
                    break;
                case ProjectType.DS1R:
                case ProjectType.DS1:
                    MSB1 msb_ds1 = MSB1.Read(data);
                    written = msb_ds1.Write(DCX.Type.None);
                    break;
            }

            if (!Directory.Exists(Path.Join(ouputDir, "mismatches")))
            {
                Directory.CreateDirectory(Path.Join(ouputDir, "mismatches"));
            }

            File.WriteAllBytes(Path.Join(ouputDir, "mismatches", Path.GetFileNameWithoutExtension(entry.Path)),
                written);

            var isMismatch = false;

            if (!BytePerfectHelper.Md5Equal(data.Span, written))
            {
                isMismatch = true;
            }

            if (isMismatch)
            {
                var mismatch = new MismatchData(entry.Filename, data.Length, written.Length);
                mismatches.Add(mismatch);
            }
        }

        return mismatches;
    }
}

