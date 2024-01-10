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
using static SoulsFormats.HKXPWV;

namespace StudioCore.Aliases;

public class ModelAliasContainer
{
    private ModelAliasResource chrEntries = new ModelAliasResource();
    private ModelAliasResource objEntries = new ModelAliasResource();
    private ModelAliasResource partEntries = new ModelAliasResource();
    private ModelAliasResource mapPieceEntries = new ModelAliasResource();

    public ModelAliasContainer()
    {
        chrEntries = null;
        objEntries = null;
        partEntries = null;
        mapPieceEntries = null;
    }
    public ModelAliasContainer(string gametype, string gameModDirectory)
    {
        chrEntries = LoadJSON(gametype, "Chr", gameModDirectory);
        objEntries = LoadJSON(gametype, "Obj", gameModDirectory);
        partEntries = LoadJSON(gametype, "Part", gameModDirectory);
        mapPieceEntries = LoadJSON(gametype, "MapPiece", gameModDirectory);
    }

    private ModelAliasResource LoadJSON(string gametype, string type, string gameModDirectory)
    {
        var baseResource = new ModelAliasResource();
        var modResource = new ModelAliasResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\ModelAliases\\{gametype}\\{type}.json";

        if (File.Exists(baseResourcePath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            baseResource = JsonSerializer.Deserialize<ModelAliasResource>(File.OpenRead(baseResourcePath), options);
        }

        var modResourcePath = gameModDirectory + $"\\.smithbox\\Assets\\ModelAliases\\{gametype}\\{type}.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            modResource = JsonSerializer.Deserialize<ModelAliasResource>(File.OpenRead(modResourcePath), options);

            // Replace baseResource entries with those from modResource if there are ID matches
            foreach(var bEntry in baseResource.list)
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
        }

        return baseResource;
    }

    public List<ModelAliasReference> GetChrEntries()
    {
        return chrEntries.list;
    }
    public List<ModelAliasReference> GetObjEntries()
    {
        return objEntries.list;
    }
    public List<ModelAliasReference> GetPartEntries()
    {
        return partEntries.list;
    }
    public List<ModelAliasReference> GetMapPieceEntries()
    {
        return mapPieceEntries.list;
    }
}
