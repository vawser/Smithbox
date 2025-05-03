using SoulsFormats;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Tests;

public static class Test_MSB_ER_BytePerfect
{
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
