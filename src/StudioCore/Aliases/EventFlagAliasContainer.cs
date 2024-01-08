using Org.BouncyCastle.Pqc.Crypto.Lms;
using StudioCore.Aliases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace StudioCore.Aliases;

public class EventFlagAliasContainer
{
    private EventFlagAliasResource entries = new EventFlagAliasResource();

    public EventFlagAliasContainer()
    {
        entries = null;
    }
    public EventFlagAliasContainer(string gametype)
    {
        entries = LoadJSON(gametype);
    }

    private EventFlagAliasResource LoadJSON(string gametype)
    {
        var resource = new EventFlagAliasResource();

        var json_filepath = AppContext.BaseDirectory + $"\\Assets\\EventFlagAliases\\{gametype}\\EventFlag.json";

        if (File.Exists(json_filepath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            resource = JsonSerializer.Deserialize<EventFlagAliasResource>(File.OpenRead(json_filepath), options);
        }

        return resource;
    }

    public List<EventFlagAliasReference> GetEntries()
    {
        return entries.list;
    }
}
