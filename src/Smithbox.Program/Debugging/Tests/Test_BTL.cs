using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.DebugNS;

public static class Test_BTL
{
    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        if (ImGui.Button("Run Test"))
        {
            Run(baseEditor, project);
        }
    }

    public static bool Run(Smithbox baseEditor, ProjectEntry project)
    {
        var curProject = baseEditor.ProjectManager.SelectedProject;

        List<string> msbs = MapLocator.GetFullMapList(curProject);
        List<string> floats = new();
        List<string> noWrite = new();
        List<string> ver = new();
        foreach (var msb in msbs)
        {
            List<ResourceDescriptor> btls = MapLocator.GetMapBTLs(curProject, msb);

            foreach (ResourceDescriptor file in btls)
            {
                BTL btl;
                /*
                if (locator.Type == GameType.DarkSoulsIISOTFS)
                {
                    var bdt = BXF4.Read(file.AssetPath, file.AssetPath[..^3] + "bdt");
                    var bdtFile = bdt.Files.Find(f => f.Name.EndsWith("light.btl.dcx"));
                    if (bdtFile == null)
                    {
                        continue;
                    }
                    btl = BTL.Read(bdtFile.Bytes);
                }
                else
                {
                    btl = BTL.Read(file.AssetPath);
                }

                foreach (var l in btl.Lights)
                {
                    floats.Add(l.Rotation.Z.ToString());
                }
                ver.Add(btl.Version.ToString());
                */

                var bytes = File.ReadAllBytes(file.AssetPath);
                Memory<byte> decompressed = DCX.Decompress(bytes);

                btl = BTL.Read(decompressed);

                var written = btl.Write(DCX.Type.None);
                if (!decompressed.Span.SequenceEqual(written))
                {
                    noWrite.Add(file.AssetName);

                    var basepath = "Tests";
                    if (!Directory.Exists($@"{basepath}\mismatches"))
                    {
                        Directory.CreateDirectory($@"{basepath}\mismatches");
                    }

                    TaskLogs.AddLog($"Mismatch: {file.AssetName}");
                    File.WriteAllBytes($@"Tests\\mismatches\{Path.GetFileNameWithoutExtension(file.AssetName)}",
                        written);
                }
            }
        }

        IEnumerable<string> floatsD = floats.Distinct();
        IEnumerable<string> noWriteD = noWrite.Distinct();
        IEnumerable<string> verD = ver.Distinct();

        File.WriteAllLines("Tests\\BTL Zrot.txt", floatsD);
        File.WriteAllLines("Tests\\BTL Write Failure.txt", noWriteD);
        File.WriteAllLines("Tests\\BTL versions.txt", verD);

        return true;
    }
}
