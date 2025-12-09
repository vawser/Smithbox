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

public static class Test_MSB_Util
{
    public static List<MismatchData> GetMsbMismatches(ProjectEntry curProject)
    {
        var mismatches = new List<MismatchData>();

        var maps = curProject.MapData.MapFiles.Entries;

        var ouputDir = Path.Combine(curProject.ProjectPath, "_msbMismatches");

        if(!Directory.Exists(ouputDir))
        {
            Directory.CreateDirectory(ouputDir);
        }

        foreach (var entry in maps)
        {
            // Read the root version of the MSB
            var bytes = curProject.VanillaFS.ReadFile(entry.Path);
            var byteArray = bytes.Value.ToArray();
            var decompressed = DCX.Decompress(byteArray);

            // Write vanilla version
            if (!Directory.Exists(Path.Join(ouputDir, "decompressed")))
            {
                Directory.CreateDirectory(Path.Join(ouputDir, "decompressed"));
            }
            File.WriteAllBytes(
                Path.Join(ouputDir, "decompressed", Path.GetFileNameWithoutExtension(entry.Path)),
                decompressed.ToArray());

            byte[] written = new byte[0];

            switch (curProject.ProjectType)
            {
                case ProjectType.NR:
                    MSB_NR msb_nr = MSB_NR.Read(decompressed);
                    written = msb_nr.Write(DCX.Type.None);
                break;
                case ProjectType.AC6:
                    MSB_AC6 msb_ac6 = MSB_AC6.Read(decompressed);
                    written = msb_ac6.Write(DCX.Type.None);
                    break;
                case ProjectType.ER:
                    MSBE msb_er = MSBE.Read(decompressed);
                    written = msb_er.Write(DCX.Type.None);
                    break;
                case ProjectType.SDT:
                    MSBS msb_sdt = MSBS.Read(decompressed);
                    written = msb_sdt.Write(DCX.Type.None);
                    break;
                case ProjectType.DS3:
                    MSB3 msb_ds3 = MSB3.Read(decompressed);
                    written = msb_ds3.Write(DCX.Type.None);
                    break;
                case ProjectType.BB:
                    MSBB msb_bb = MSBB.Read(decompressed);
                    written = msb_bb.Write(DCX.Type.None);
                    break;
                case ProjectType.DS2S:
                case ProjectType.DS2:
                    MSB2 msb_ds2 = MSB2.Read(decompressed);
                    written = msb_ds2.Write(DCX.Type.None);
                    break;
                case ProjectType.DS1R:
                case ProjectType.DS1:
                    MSB1 msb_ds1 = MSB1.Read(decompressed);
                    written = msb_ds1.Write(DCX.Type.None);
                    break;
            }

            if (!Directory.Exists(Path.Join(ouputDir, "mismatches")))
            {
                Directory.CreateDirectory(Path.Join(ouputDir, "mismatches"));
            }

            File.WriteAllBytes(Path.Join(ouputDir, "mismatches", Path.GetFileNameWithoutExtension(entry.Path)),
                written);

            var isMismatch = false;

            if (!BytePerfectHelper.Md5Equal(decompressed.Span, written))
            {
                isMismatch = true;
            }

            if (isMismatch)
            {
                var mismatch = new MismatchData(entry.Filename, decompressed.Length, written.Length);
                mismatches.Add(mismatch);
            }
        }

        return mismatches;
    }
    public static List<MismatchData> GetBtlMismatches(ProjectEntry curProject)
    {
        var mismatches = new List<MismatchData>();

        var lights = curProject.MapData.LightFiles.Entries;

        var ouputDir = Path.Combine(curProject.ProjectPath, "_btlMismatches");

        if (!Directory.Exists(ouputDir))
        {
            Directory.CreateDirectory(ouputDir);
        }

        foreach (var entry in lights)
        {
            // Read the root version of the MSB
            var bytes = curProject.VanillaFS.ReadFile(entry.Path);
            var byteArray = bytes.Value.ToArray();
            var decompressed = DCX.Decompress(byteArray);

            // Write vanilla version
            if (!Directory.Exists(Path.Join(ouputDir, "decompressed")))
            {
                Directory.CreateDirectory(Path.Join(ouputDir, "decompressed"));
            }
            File.WriteAllBytes(
                Path.Join(ouputDir, "decompressed", Path.GetFileNameWithoutExtension(entry.Path)),
                decompressed.ToArray());

            byte[] written = new byte[0];

            BTL btl = BTL.Read(decompressed);
            written = btl.Write(DCX.Type.None);

            if (!Directory.Exists(Path.Join(ouputDir, "mismatches")))
            {
                Directory.CreateDirectory(Path.Join(ouputDir, "mismatches"));
            }

            File.WriteAllBytes(Path.Join(ouputDir, "mismatches", Path.GetFileNameWithoutExtension(entry.Path)),
                written);

            var isMismatch = false;

            if (!BytePerfectHelper.Md5Equal(decompressed.Span, written))
            {
                isMismatch = true;
            }

            if (isMismatch)
            {
                var mismatch = new MismatchData(entry.Filename, decompressed.Length, written.Length);
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
