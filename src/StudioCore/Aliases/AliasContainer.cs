using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using StudioCore.Aliases;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace StudioCore.Aliases;

public class AliasContainer
{
    private Dictionary<string, AliasResource> aliasMap = new Dictionary<string, AliasResource>();

    private AliasType aliasType;

    public AliasContainer()
    {
        aliasMap = null;
        aliasType = AliasType.None;
    }
    public AliasContainer(AliasType _aliasType, string gametype, string gameModDirectory)
    {
        aliasType = _aliasType;

        if (aliasType is AliasType.Model)
        {
            aliasMap.Add("Characters", LoadJSON(gametype, "Chr", gameModDirectory));
            aliasMap.Add("Objects", LoadJSON(gametype, "Obj", gameModDirectory));
            aliasMap.Add("Parts", LoadJSON(gametype, "Part", gameModDirectory));
            aliasMap.Add("MapPieces", LoadJSON(gametype, "MapPiece", gameModDirectory));
        }

        if (aliasType is AliasType.EventFlag)
        {
            aliasMap.Add("Flags", LoadJSON(gametype, "EventFlag", gameModDirectory));
        }

        if (aliasType is AliasType.Particle)
        {
            aliasMap.Add("Particles", LoadJSON(gametype, "Fxr", gameModDirectory));
        }
    }

    private AliasResource LoadJSON(string gametype, string filename, string gameModDirectory)
    {
        var baseResource = new AliasResource();
        var modResource = new AliasResource();

        if (aliasType is AliasType.None)
            return null;

        var baseResourcePath = "";

        if (aliasType is AliasType.Model)
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\Models\\{gametype}\\{filename}.json";

        if (aliasType is AliasType.EventFlag)
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\Flags\\{gametype}\\{filename}.json";

        if (aliasType is AliasType.Particle)
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\Particles\\{gametype}\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize<AliasResource>(File.OpenRead(baseResourcePath), options);
            }
        }

        var modResourcePath = "";

        if (aliasType is AliasType.Model)
            modResourcePath = gameModDirectory + $"\\.smithbox\\Assets\\Aliases\\Models\\{gametype}\\{filename}.json";

        if (aliasType is AliasType.EventFlag)
            modResourcePath = gameModDirectory + $"\\.smithbox\\Assets\\Aliases\\Flags\\{gametype}\\{filename}.json";

        if (aliasType is AliasType.Particle)
            modResourcePath = gameModDirectory + $"\\.smithbox\\Assets\\Aliases\\Particles\\{gametype}\\{filename}.json";

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
                modResource = JsonSerializer.Deserialize<AliasResource>(File.OpenRead(modResourcePath), options);
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

    public List<AliasReference> GetEntries(string name)
    {
        return aliasMap[name].list;
    }
}
