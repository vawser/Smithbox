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
    public FxrAliasContainer(string gametype, string gameModDirectory)
    {
        entries = LoadJSON(gametype, gameModDirectory);
    }

    private FxrAliasResource LoadJSON(string gametype, string gameModDirectory)
    {
        var baseResource = new FxrAliasResource();
        var modResource = new FxrAliasResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FxrAliases\\{gametype}\\Fxr.json";

        if (File.Exists(baseResourcePath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize<FxrAliasResource>(File.OpenRead(baseResourcePath), options);
            }
        }

        var modResourcePath = gameModDirectory + $"\\.smithbox\\Assets\\FxrAliases\\{gametype}\\Fxr.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(modResourcePath))
            {
                modResource = JsonSerializer.Deserialize<FxrAliasResource>(File.OpenRead(modResourcePath), options);
            }

            // Replace baseResource entries with those from modResource if there are ID matches
            foreach (var bEntry in baseResource.list)
            {
                var baseId = bEntry.id;
                var baseName = bEntry.name;
                var baseTags = bEntry.tags;

                foreach (var mEntry in modResource.list)
                {
                    var modId = mEntry.id;
                    var modName = mEntry.name;
                    var modTags = mEntry.tags;

                    // Mod override exists
                    if (baseId == modId)
                    {
                        bEntry.id = modId;
                        bEntry.name = modName;
                        bEntry.tags = modTags;
                    }
                }
            }

            // Add mod local unique rentries
            foreach (var mEntry in modResource.list)
            {
                var modId = mEntry.id;

                bool isUnique = true;

                foreach (var bEntry in baseResource.list)
                {
                    var baseId = bEntry.id;

                    // Mod override exists
                    if (baseId == modId)
                    {
                        isUnique = false;
                    }
                }

                if (isUnique)
                {
                    baseResource.list.Add(mEntry);
                }
            }
        }

        return baseResource;
    }

    public List<FxrAliasReference> GetEntries()
    {
        return entries.list;
    }
}
