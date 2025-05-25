using SoulsFormats;
using System.Collections.Generic;
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
}
