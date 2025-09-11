using SoulsFormats;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MSB_NR;

namespace StudioCore.DebugNS;

public static class MapWriteValidator
{
    public static List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();

    public static void ValidatorMapWrite(Smithbox baseEditor, ProjectEntry curProject)
    {
        var mismatches = new List<MismatchData>();
        var regionTypes = new List<RegionType>();

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

            // MSB Type
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

        if (mismatches.Count > 0)
        {
            TaskLogs.AddLog("Mismatches:");

            foreach (var entry in mismatches)
            {
                var diffSize = entry.OriginalBytes - entry.WrittenBytes;

                TaskLogs.AddLog($" {entry.MSB} - Original Size: {entry.OriginalBytes}, Written Size: {entry.WrittenBytes}, Diff Size: {diffSize}");
            }
        }
        else
        {
            TaskLogs.AddLog("No mismatches!");
        }
    }
}
