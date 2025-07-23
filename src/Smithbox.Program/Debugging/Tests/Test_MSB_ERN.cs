using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static SoulsFormats.MSB_NR;

namespace StudioCore.DebugNS;
public static class Test_MSB_NR
{
    public static List<MismatchData> mismatches = new List<MismatchData>();

    public static List<RegionType> regionTypes = new List<RegionType>();

    public static bool IncludeDisambiguation = false;

    public static bool RunOnce = false;

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        var buttonSize = new Vector2(400, 32);

        if (ImGui.Button("Check all Maps for Byte-Perfect Match", buttonSize))
        {
            Run(baseEditor);
        }

        ImGui.Separator();

        if (mismatches.Count > 0)
        {
            ImGui.Text("Mismatches:");

            foreach (var entry in mismatches)
            {
                ImGui.Text($" {entry.MSB} - {entry.OriginalBytes} - {entry.WrittenBytes}");
            }
        }
        else
        {
            if (RunOnce)
            {
                ImGui.Text("No mismatches!");
            }
        }
    }

    public static List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();

    public static bool Run(Smithbox baseEditor)
    {
        var curProject = baseEditor.ProjectManager.SelectedProject;

        mismatches = new List<MismatchData>();
        regionTypes = new List<RegionType>();

        var mapDir = $"{curProject.DataPath}/map/mapstudio/";

        foreach (var entry in Directory.EnumerateFiles(mapDir))
        {
            if (entry.Contains(".msb.dcx"))
            {
                var name = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(entry));
                ResourceDescriptor ad = MapLocator.GetMapMSB(curProject, name);
                if (ad.AssetPath != null)
                {
                    resMaps.Add(ad);
                }
            }
        }

        foreach (var res in resMaps)
        {
            var basepath = Path.GetDirectoryName(res.AssetPath);
            var filename = Path.GetFileName(res.AssetPath);

            var bytes = File.ReadAllBytes(res.AssetPath);
            Memory<byte> decompressed = DCX.Decompress(bytes);

            // Write vanilla version
            if (!Directory.Exists(Path.Join(basepath, "decompressed")))
            {
                Directory.CreateDirectory(Path.Join(basepath, "decompressed"));
            }
            File.WriteAllBytes(Path.Join(basepath, "decompressed", Path.GetFileNameWithoutExtension(res.AssetPath)),
                decompressed.ToArray());

            MSB_NR m = MSB_NR.Read(decompressed);

            // Write test version
            var written = m.Write(DCX.Type.None);

            if (!Directory.Exists(Path.Join(basepath, "mismatches")))
            {
                Directory.CreateDirectory(Path.Join(basepath, "mismatches"));
            }

            File.WriteAllBytes(Path.Join(basepath, "mismatches", Path.GetFileNameWithoutExtension(res.AssetPath)),
                written);

            var isMismatch = false;

            if (!decompressed.Span.SequenceEqual(written))
            {
                isMismatch = true;
            }

            if (isMismatch)
            {
                var mismatch = new MismatchData(filename, decompressed.Length, written.Length);
                mismatches.Add(mismatch);
            }
        }

        RunOnce = true;

        return true;
    }
}

