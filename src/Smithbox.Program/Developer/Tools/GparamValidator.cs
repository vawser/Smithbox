using Hexa.NET.ImGui;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ZstdNet;

namespace StudioCore.Application;

public class GparamValidator
{
    public List<MismatchData> MismatchedEntries = new List<MismatchData>();

    public int SelectedMap = -1;

    public bool DisplaySizeDiffsOnly = false;

    public void Display()
    {
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("validateActions",
            "validate", "Validate for Byte-Perfectness", "", Run);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Options", "");
        ImGui.Checkbox("Display Size Diffs Only", ref DisplaySizeDiffsOnly);
        ImGui.Separator();

        ImGui.Columns(2);

        int index = 0;

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Entries", "");
        ImGui.BeginChild("entrySection", new Vector2(0, 200), ImGuiChildFlags.Borders);

        foreach (var entry in MismatchedEntries)
        {
            if (DisplaySizeDiffsOnly)
            {
                if (entry.ByteDiff == 0)
                {
                    index++;
                    continue;
                }
            }

            if (ImGui.Selectable($"{entry.Name}##curEntry{index}"))
            {
                SelectedMap = index;
            }

            index++;
        }

        ImGui.EndChild();

        ImGui.NextColumn();

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Data", "");
        ImGui.BeginChild("dataSection", new Vector2(0, 200), ImGuiChildFlags.Borders);

        if (SelectedMap != -1)
        {
            var curMap = MismatchedEntries[SelectedMap];

            ImGui.Text($"Source Bytes: {curMap.SrcBytes}");
            ImGui.Text($"Write Bytes: {curMap.WriteBytes}");
            ImGui.Text($"Byte Difference: {curMap.ByteDiff}");
        }

        ImGui.EndChild();
    }

    public void Run()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        MismatchedEntries = GetMsbMismatches(curProject);
    }

    public List<MismatchData> GetMsbMismatches(ProjectEntry curProject)
    {
        var mismatches = new List<MismatchData>();

        var gparams = curProject.Locator.GparamFiles.Entries;

        var ouputDir = Path.Combine(curProject.Descriptor.ProjectPath, ".tests", "gparam-mismatches");

        if (!Directory.Exists(ouputDir))
        {
            Directory.CreateDirectory(ouputDir);
        }

        var isCompressed = true;

        if (curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            isCompressed = false;
        }

        foreach (var entry in gparams)
        {
            // Read the root version of the MSB
            var bytes = curProject.VFS.VanillaFS.ReadFile(entry.Path);
            var data = (Memory<byte>)bytes;

            var finalData = data;

            if (isCompressed)
            {
                finalData = DCX.Decompress(data);
            }

            // Write vanilla version
            if (!Directory.Exists(Path.Join(ouputDir, "decompressed")))
            {
                Directory.CreateDirectory(Path.Join(ouputDir, "decompressed"));
            }
            File.WriteAllBytes(
                Path.Join(ouputDir, "decompressed", $"{Path.GetFileNameWithoutExtension(entry.Path)}.gparam"),
                finalData.ToArray());

            data = finalData.ToArray();

            byte[] written = new byte[0];

            var gparam = GPARAM.Read(data);
            written = gparam.Write(DCX.Type.None);

            if (!Directory.Exists(Path.Join(ouputDir, "mismatches")))
            {
                Directory.CreateDirectory(Path.Join(ouputDir, "mismatches"));
            }

            File.WriteAllBytes(Path.Join(ouputDir, "mismatches", $"{Path.GetFileNameWithoutExtension(entry.Path)}.gparam"), written);

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

