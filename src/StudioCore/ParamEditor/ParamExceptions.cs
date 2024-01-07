using System;

namespace StudioCore.ParamEditor;

public class ParamVersionMismatchException : Exception
{
    public ParamVersionMismatchException(ulong vanillaVersion, ulong modVersion)
    {
        VanillaVersion = vanillaVersion;
        ModVersion = modVersion;
    }

    public ulong VanillaVersion { get; }
    public ulong ModVersion { get; }
}
