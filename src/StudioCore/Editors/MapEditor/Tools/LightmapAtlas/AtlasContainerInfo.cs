using SoulsFormats;
using System.IO;

namespace StudioCore.Editors.MapEditor.Tools.LightmapAtlasEditor;

public class AtlasContainerInfo
{
    public string Filename { get; set; }
    public string AssetPath { get; set; }
    public BTAB LightmapAtlas { get; set; }

    public bool IsModified { get; set; }

    public AtlasContainerInfo(string path)
    {
        IsModified = false;
        AssetPath = path;
        Filename = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(path));
    }
}