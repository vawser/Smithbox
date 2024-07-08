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
    Dictionary<string, List<string>> MapTags = new Dictionary<string, List<string>>();

    public MapNameCache()
    {
    }

    public void BuildCache()
    {
        if (Smithbox.BankHandler.MapAliases.Aliases == null)
            return;

        if (Smithbox.BankHandler.MapAliases.Aliases.list == null)
            return;

        TaskManager.Run(new TaskManager.LiveTask($"Load Map Names", TaskManager.RequeueType.WaitThenRequeue, false, () =>
        {
            MapNames = new Dictionary<string, string>();
            MapTags = new Dictionary<string, List<string>>();

            foreach (var entry in Smithbox.BankHandler.MapAliases.Aliases.list)
            {
                if (!MapNames.ContainsKey(entry.id))
                {
                    MapNames.Add(entry.id, entry.name);
                }

                if (!MapTags.ContainsKey(entry.id))
                {
                    MapTags.Add(entry.id, entry.tags);
                }
            }
        }));
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

    public List<string> GetMapTags(string mapId)
    {
        if (MapTags == null)
            return new List<string>();

        if (MapTags.ContainsKey(mapId))
        {
            return MapTags[mapId];
        }

        return new List<string>();
    }
}

