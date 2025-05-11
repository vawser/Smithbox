using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static SoulsFormats.MSB_AC6;

namespace StudioCore.DebugNS;
public static class Test_MSB_AC6
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
            if(RunOnce)
            {
                ImGui.Text("No mismatches!");
            }
        }
    }

    public static bool Run(Smithbox baseEditor)
    {
        var curProject = baseEditor.ProjectManager.SelectedProject;

        mismatches = new List<MismatchData>();
        regionTypes = new List<RegionType>();

        List<string> msbs = MapLocator.GetFullMapList(curProject);

        foreach (var msb in msbs)
        {
            ResourceDescriptor path = MapLocator.GetMapMSB(curProject, msb, false, true);
            var basepath = Path.GetDirectoryName(path.AssetPath);

            var bytes = File.ReadAllBytes(path.AssetPath);
            Memory<byte> decompressed = DCX.Decompress(bytes);

            // Write vanilla version
            if (!Directory.Exists($@"{basepath}\decompressed"))
            {
                Directory.CreateDirectory($@"{basepath}\decompressed");
            }
            File.WriteAllBytes($@"{basepath}\decompressed\{Path.GetFileNameWithoutExtension(path.AssetPath)}",
                decompressed.ToArray());

            MSB_AC6 m = MSB_AC6.Read(decompressed);

            // Write test version
            var written = m.Write(DCX.Type.None);

            File.WriteAllBytes($@"{basepath}\mismatches\{Path.GetFileNameWithoutExtension(path.AssetPath)}",
                written);

            var isMismatch = false;

            if (!decompressed.Span.SequenceEqual(written))
            {
                isMismatch = true;
            }

            if (isMismatch)
            {
                if (!Directory.Exists($@"{basepath}\mismatches"))
                {
                    Directory.CreateDirectory($@"{basepath}\mismatches");
                }

                var mismatch = new MismatchData(msb, decompressed.Length, written.Length);
                mismatches.Add(mismatch);
            }
        }

        RunOnce = true;

        return true;
    }
}

public class MismatchData
{
    public string MSB { get; set; }

    public long OriginalBytes { get; set; }
    public long WrittenBytes { get; set; }

    public MismatchData(string msb, long originalBytes, long writtenBytes)
    {
        MSB = msb;
        OriginalBytes = originalBytes;
        WrittenBytes = writtenBytes;
    }
}
