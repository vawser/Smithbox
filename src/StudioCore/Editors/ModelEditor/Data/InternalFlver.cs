using SoulsFormats;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// This represents the internal FLVER file, attached to a FLVER Container.
/// </summary>
public class InternalFlver
{
    public string Name;
    public string ModelID;
    public FLVER2 CurrentFLVER;
    public string VirtualResourcePath = "";

    // Hold the bytes of the FLVER before saving (for byte perfect test)
    public byte[] InitialFlverBytes { get; set; }
}