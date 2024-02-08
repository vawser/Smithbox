using System;

namespace StudioCore.Editors.ParamEditor;

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
