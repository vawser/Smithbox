using SoulsFormats;
using StudioCore.Locators;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using static SoulsFormats.MSB_AC6;

namespace StudioCore.Tests;
public static class Test_MSB_AC6_BytePerfect
{
    public static List<MismatchData> mismatches = new List<MismatchData>();

    public static List<RegionType> regionTypes = new List<RegionType>();

    public static bool Run()
    {
        mismatches = new List<MismatchData>();
        regionTypes = new List<RegionType>();

        List<string> msbs = MapLocator.GetFullMapList();

        foreach (var msb in msbs)
        {
            ResourceDescriptor path = MapLocator.GetMapMSB(msb);
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

            if (decompressed.Length != written.Length)
            {
                if (!Directory.Exists($@"{basepath}\mismatches"))
                {
                    Directory.CreateDirectory($@"{basepath}\mismatches");
                }

                var mismatch = new MismatchData(msb, decompressed.Length, written.Length);
                mismatches.Add(mismatch);
            }
        }

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
