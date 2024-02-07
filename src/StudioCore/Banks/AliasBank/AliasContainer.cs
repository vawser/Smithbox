using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace StudioCore.Banks.AliasBank;

public class AliasContainer
{
    private Dictionary<string, AliasResource> aliasMap = new Dictionary<string, AliasResource>();

    private AliasType aliasType;

    private string gametype;

    private string gameModDirectory;

    public AliasContainer()
    {
        aliasMap = null;
        aliasType = AliasType.None;
    }
    public AliasContainer(AliasType _aliasType, string _gametype, string _gameModDirectory)
    {
        aliasType = _aliasType;
        gametype = _gametype;
        gameModDirectory = _gameModDirectory;

        if (aliasType is AliasType.Model)
        {
            aliasMap.Add("Characters", LoadJSON("Chr"));
            aliasMap.Add("Objects", LoadJSON("Obj"));
            aliasMap.Add("Parts", LoadJSON("Part"));
            aliasMap.Add("MapPieces", LoadJSON("MapPiece"));
        }

        if (aliasType is AliasType.EventFlag)
            aliasMap.Add("Flags", LoadJSON("EventFlag"));

        if (aliasType is AliasType.Particle)
            aliasMap.Add("Particles", LoadJSON("Fxr"));

        if (aliasType is AliasType.Map)
            aliasMap.Add("Maps", LoadJSON("Maps"));
    }

    private AliasResource LoadJSON(string filename)
    {
        var baseResource = new AliasResource();
        var modResource = new AliasResource();

        if (aliasType is AliasType.None)
            return null;

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{GetAliasTypeDir()}\\{gametype}\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(baseResourcePath))
                baseResource = JsonSerializer.Deserialize<AliasResource>(File.OpenRead(baseResourcePath), options);
        }

        var modResourcePath = gameModDirectory + $"\\.smithbox\\Assets\\Aliases\\{GetAliasTypeDir()}\\{gametype}\\{filename}.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(modResourcePath))
                modResource = JsonSerializer.Deserialize<AliasResource>(File.OpenRead(modResourcePath), options);

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

                var isUnique = true;

                foreach (var bEntry in baseResource.list)
                {
                    var baseId = bEntry.id;

                    // Mod override exists
                    if (baseId == modId)
                        isUnique = false;
                }

                if (isUnique)
                    baseResource.list.Add(mEntry);
            }
        }

        return baseResource;
    }

    private string GetAliasTypeDir()
    {
        var typDir = "";

        if (aliasType is AliasType.Model)
            typDir = "Models";

        if (aliasType is AliasType.EventFlag)
            typDir = "Flags";

        if (aliasType is AliasType.Particle)
            typDir = "Particles";

        if (aliasType is AliasType.Map)
            typDir = "Maps";

        return typDir;
    }

    public List<AliasReference> GetEntries(string name)
    {
        return aliasMap[name].list;
    }
}
