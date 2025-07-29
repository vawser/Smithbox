using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace StudioCore.DebugNS;
public static class Test_MSB_ER
{
    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        if (ImGui.Button("Check all Maps for Byte-Perfect Match", DPI.StandardButtonSize))
        {
            Run(baseEditor);
        }
    }
    public static bool Run(Smithbox baseEditor)
    {
        var curProject = baseEditor.ProjectManager.SelectedProject;

        List<string> msbs = MapLocator.GetFullMapList(curProject);
        foreach (var msb in msbs)
        {
            ResourceDescriptor path = MapLocator.GetMapMSB(curProject, msb);
            var bytes = File.ReadAllBytes(path.AssetPath);
            Memory<byte> decompressed = DCX.Decompress(bytes);
            MSBE m = MSBE.Read(decompressed);
            var written = m.Write(DCX.Type.None);
            if (!decompressed.Span.SequenceEqual(written))
            {
                var basepath = Path.GetDirectoryName(path.AssetPath);
                if (!Directory.Exists(Path.Join(basepath, "mismatches")))
                {
                    Directory.CreateDirectory(Path.Join(basepath, "mismatches"));
                }

                Console.WriteLine($@"Mismatch: {msb}");
                File.WriteAllBytes(Path.Join(basepath, "mismatches", Path.GetFileNameWithoutExtension(path.AssetPath)),
                    written);
            }
        }

        return true;
    }
}
