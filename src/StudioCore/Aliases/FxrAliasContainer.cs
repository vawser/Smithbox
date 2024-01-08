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

public class FxrAliasContainer
{
    private FxrAliasResource entries = new FxrAliasResource();

    public FxrAliasContainer()
    {
        entries = null;
    }
    public FxrAliasContainer(string gametype)
    {
        entries = LoadJSON(gametype);
    }

    private FxrAliasResource LoadJSON(string gametype)
    {
        var resource = new FxrAliasResource();

        var json_filepath = AppContext.BaseDirectory + $"\\Assets\\FxrAliases\\{gametype}\\Fxr.json";

        if (File.Exists(json_filepath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            resource = JsonSerializer.Deserialize<FxrAliasResource>(File.OpenRead(json_filepath), options);
        }

        return resource;
    }

    public List<FxrAliasReference> GetEntries()
    {
        return entries.list;
    }
}
