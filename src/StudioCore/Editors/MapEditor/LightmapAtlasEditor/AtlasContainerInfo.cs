using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.LightmapAtlasEditor;

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