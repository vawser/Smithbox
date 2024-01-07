using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Tests;

public static class MSBReadWrite
{
    public static bool Run(AssetLocator locator)
    {
        List<string> msbs = locator.GetFullMapList();
        foreach (var msb in msbs)
        {
            AssetDescription path = locator.GetMapMSB(msb);
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
