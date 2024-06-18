using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Caches;

public class MapNameCache
{
    Dictionary<string, string> MapNames = new Dictionary<string, string>();

    public MapNameCache()
    {
    }

    public void BuildCache()
    {
        if (Smithbox.BankHandler.MapAliases.Aliases != null)
        {
            MapNames = new Dictionary<string, string>();

            foreach (var entry in Smithbox.BankHandler.MapAliases.Aliases.list)
            {
                if (!CFG.Current.MapNameAtlas_ShowUnused)
                {
                    if (entry.tags[0] != "unused")
                    {
                        MapNames.Add(entry.id, entry.name);
                    }
                    else
                    {
                        MapNames.Add(entry.id, entry.name);
                    }
                }
            }
        }

        TaskLogs.AddLog($"Name Cache: Loaded Map Names");
    }

    public string GetMapName(string mapId)
    {
        if (MapNames == null)
            return "";

        if (MapNames.ContainsKey(mapId))
        {
            return $"{MapNames[mapId]}";
        }

        return $"";
    }
}

