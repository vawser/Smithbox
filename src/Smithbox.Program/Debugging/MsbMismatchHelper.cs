using Octokit;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Program.Debugging;

public static class MsbMismatchHelper
{
    public static List<MismatchData> GetMsbMismatches(ProjectEntry curProject)
    {
        var mismatches = new List<MismatchData>();
        var resMaps = new List<ResourceDescriptor>();

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

            if (!BytePerfectHelper.Md5Equal(decompressed.Span, written))
            {
                isMismatch = true;
            }

            if (isMismatch)
            {
                var mismatch = new MismatchData(filename, decompressed.Length, written.Length);
                mismatches.Add(mismatch);
            }
        }

        return mismatches;
    }
}

public class MismatchData
{
    public string Name { get; set; }

    public long SrcBytes { get; set; } = 0;
    public long WriteBytes { get; set; } = 0;
    public long ByteDiff { get; set; } = 0;

    public MismatchData(string msb, long srcBytes, long writeBytes)
    {
        Name = msb;
        SrcBytes = srcBytes;
        WriteBytes = writeBytes;

        ByteDiff = srcBytes - writeBytes;
    }
}
