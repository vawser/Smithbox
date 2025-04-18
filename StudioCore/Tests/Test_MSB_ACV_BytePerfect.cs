using SoulsFormats;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Tests;

public static class Test_MSB_ACV_BytePerfect
{
    public static bool Run()
    {
        List<string> msbs = MapLocator.GetFullMapList();
        foreach (var msb in msbs)
        {
            ResourceDescriptor path = MapLocator.GetMapMSB(msb);
#if MSB_READ_WRITE_TEST_LOG_ON_CRASH
            try
            {
#endif
            var bytes = File.ReadAllBytes(path.AssetPath);
            MSBV m = MSBV.Read(bytes);
            var written = m.Write(DCX.Type.None);
            MSBV wm = MSBV.Read(written);
            var basepath = Path.GetDirectoryName(path.AssetPath).Replace("map", "map_test_mismatches");
            if (!bytes.AsMemory().Span.SequenceEqual(written))
            {
                Directory.CreateDirectory(basepath);
                File.WriteAllBytes($@"{basepath}\{Path.GetFileNameWithoutExtension(path.AssetPath)}",
                    written);
            }
            else
            {
                if (Directory.Exists(basepath))
                {
                    if (File.Exists($@"{basepath}\{Path.GetFileNameWithoutExtension(path.AssetPath)}"))
                    {
                        File.Delete($@"{basepath}\{Path.GetFileNameWithoutExtension(path.AssetPath)}");
                    }

                    if (Directory.GetFiles(basepath).Length == 0)
                    {
                        Directory.Delete(basepath);
                    }
                }
            }
#if MSB_READ_WRITE_TEST_LOG_ON_CRASH
        }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Failed testing ACFA MSB: {path.AssetPath}\n{e}", Microsoft.Extensions.Logging.LogLevel.Debug);
            }
#endif
        }

        return true;
    }
}
