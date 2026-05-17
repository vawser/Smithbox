using SoulsFormats;
using StudioCore.Application;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public static class PrefabUtils
{
    public static IEnumerable<IMsbEntry> GetMapMsbEntries(IMsb map)
    {
        if(map == null)
            return Enumerable.Empty<IMsbEntry>();

        return new IEnumerable<IMsbEntry>[] {
                map.Parts.GetEntries(),
                map.Events.GetEntries(),
                map.Regions.GetEntries(),
            }
            .SelectMany(e => e);
    }

    public static string GetPrefabStorageDirectory(ProjectEntry curProject)
    {
        var prefabStorageDir = @$"{curProject.Descriptor.ProjectPath}/.smithbox/MSB/Prefabs";

        if(!Directory.Exists(prefabStorageDir))
        {
            Directory.CreateDirectory(prefabStorageDir);
        }

        return prefabStorageDir;
    }
}
