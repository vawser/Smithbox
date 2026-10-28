using Microsoft.Extensions.Logging;
using Silk.NET.SDL;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Renderer;

public static class LocatorUtils
{
    public static bool IsTPF(string path)
    {
        bool success = false;

        if (path.EndsWith(".tpf", StringComparison.OrdinalIgnoreCase) || 
            path.EndsWith(".tpf.dcx", StringComparison.OrdinalIgnoreCase))
        {
            success = true;
        }

        return success;
    }
}
