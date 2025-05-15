using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
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
        var buttonSize = new Vector2(400, 32);

        if (ImGui.Button("Check all Maps for Byte-Perfect Match", buttonSize))
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
                if (!Directory.Exists($@"{basepath}\mismatches"))
                {
                    Directory.CreateDirectory($@"{basepath}\mismatches");
                }

                Console.WriteLine($@"Mismatch: {msb}");
                File.WriteAllBytes($@"{basepath}\mismatches\{Path.GetFileNameWithoutExtension(path.AssetPath)}",
                    written);
            }
        }

        return true;
    }
}
