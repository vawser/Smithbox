using Hexa.NET.ImGui;
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.Application;

public class NvaValidator
{
    public List<MismatchData> MismatchedNvas = new List<MismatchData>();

    public int SelectedMap = -1;

    public bool DisplaySizeDiffsOnly = false;

    public NvaValidator() { }

    public void Display()
    {
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("validateActions",
            "validate", "Validate for Byte-Perfectness", "", Run);

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Options", "");
        ImGui.Checkbox("Display Size Diffs Only", ref DisplaySizeDiffsOnly);

        ImGui.Columns(2);

        int index = 0;

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Map", "");
        ImGui.BeginChild("mapSection", new Vector2(0, 200), ImGuiChildFlags.Borders);

        foreach (var entry in MismatchedNvas)
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

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Data", "");
        ImGui.BeginChild("dataSection", new Vector2(0, 200), ImGuiChildFlags.Borders);

        if (SelectedMap != -1)
        {
            var curMap = MismatchedNvas[SelectedMap];

            ImGui.Text($"Source Bytes: {curMap.SrcBytes}");
            ImGui.Text($"Write Bytes: {curMap.WriteBytes}");
            ImGui.Text($"Byte Difference: {curMap.ByteDiff}");
        }

        ImGui.EndChild();
    }

    public void Run()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        MismatchedNvas = GetNvaMismatches(curProject);
    }

    public List<MismatchData> GetNvaMismatches(ProjectEntry curProject)
    {
        var mismatches = new List<MismatchData>();

        var nvaFiles = curProject.Locator.NavmeshFiles.Entries;

        var ouputDir = Path.Combine(curProject.Descriptor.ProjectPath, ".tests", "nva-mismatches");

        if (!Directory.Exists(ouputDir))
        {
            Directory.CreateDirectory(ouputDir);
        }

        foreach (var entry in nvaFiles)
        {
            // Read the root version of the MSB
            var bytes = curProject.VFS.VanillaFS.ReadFile(entry.Path);

            if (bytes == null)
                continue;

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
            NVA nva = null;

            try
            {
                nva = NVA.Read(decompressed);
            }
            catch (Exception e)
            {
                Smithbox.Log(typeof(NvaValidator), $"NVA Mismatch: {entry.Filename} {e}");
            }

            if (nva == null)
                continue;

            written = nva.Write(DCX.Type.None);

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
