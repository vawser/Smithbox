using SoulsFormats;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Application;

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
