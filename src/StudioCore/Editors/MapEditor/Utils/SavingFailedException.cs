using System;

namespace StudioCore.Editors.MapEditor;

public class SavingFailedException : Exception
{
    public string Filename;
    public Exception Wrapped;

    public SavingFailedException(string fname, Exception wrapped)
        : base($@"Failed to save file {fname}")
    {
        Filename = fname;
        Wrapped = wrapped;
    }
}
