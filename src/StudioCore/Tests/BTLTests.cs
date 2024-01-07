using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace StudioCore.Tests;

public static class BTLReadWrite
{
    public static bool Run(AssetLocator locator)
    {
        List<string> msbs = locator.GetFullMapList();
        List<string> floats = new();
        List<string> noWrite = new();
        List<string> ver = new();
        foreach (var msb in msbs)
        {
            List<AssetDescription> btls = locator.GetMapBTLs(msb);

            foreach (AssetDescription file in btls)
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

                    Debug.WriteLine($"Mismatch: {file.AssetName}");
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
