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
    public ModelAliasContainer(string gametype)
    {
        chrEntries = LoadJSON(gametype, "Chr");
        objEntries = LoadJSON(gametype, "Obj");
        partEntries = LoadJSON(gametype, "Part");
        mapPieceEntries = LoadJSON(gametype, "MapPiece");
    }

    private ModelAliasResource LoadJSON(string gametype, string type)
    {
        var resource = new ModelAliasResource();

        var json_filepath = AppContext.BaseDirectory + $"\\Assets\\ModelAliases\\{gametype}\\{type}.json";

        if (File.Exists(json_filepath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };
            resource = JsonSerializer.Deserialize<ModelAliasResource>(File.OpenRead(json_filepath), options);
        }

        return resource;
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
