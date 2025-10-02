using SoulsFormats;
using StudioCore.Core;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.MapEditor.Tools.Prefabs;

public static class PrefabUtils
{
    public static IEnumerable<IMsbEntry> GetMapMsbEntries(IMsb map)
    {
        return new IEnumerable<IMsbEntry>[] {
                map.Parts.GetEntries(),
                map.Events.GetEntries(),
                map.Regions.GetEntries(),
            }
            .SelectMany(e => e);
    }

    public static string GetPrefabStorageDirectory(ProjectEntry curProject)
    {
        var prefabStorageDir = @$"{curProject.ProjectPath}/.smithbox/MSB/Prefabs";

        if(!Directory.Exists(prefabStorageDir))
        {
            Directory.CreateDirectory(prefabStorageDir);
        }

        return prefabStorageDir;
    }
}
