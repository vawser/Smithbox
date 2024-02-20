using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StudioCore.Banks.AliasBank;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace StudioCore.Banks.GparamBank;

public class GparamInfoContainer
{
    private Dictionary<string, GparamInfoResource> FormatInfo = new Dictionary<string, GparamInfoResource>();

    private string gametype;

    private string gameModDirectory;

    private string ProgramDirectory = ".smithbox";

    public GparamInfoContainer()
    {
        FormatInfo = null;
    }
    public GparamInfoContainer(string _gameModDirectory)
    {
        gameModDirectory = _gameModDirectory;

        FormatInfo.Add("Core", LoadJSON("Core"));
    }

    private GparamInfoResource LoadJSON(string filename)
    {
        var baseResource = new GparamInfoResource();
        var modResource = new GparamInfoResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\GPARAM\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, GparamInfoResourceSerializationContext.Default.GparamInfoResource);
            }
        }

        var modResourcePath = gameModDirectory + $"\\{ProgramDirectory}\\Assets\\FormatInfo\\GPARAM\\{filename}.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath))
        {
            using (var stream = File.OpenRead(modResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, GparamInfoResourceSerializationContext.Default.GparamInfoResource);
            }

            // Replace baseResource entries with those from modResource if there are ID matches
            foreach (var bEntry in baseResource.list)
            {
                var baseId = bEntry.id;
                var baseName = bEntry.name;
                var baseMembers = bEntry.members;

                foreach (var mEntry in modResource.list)
                {
                    var modId = mEntry.id;
                    var modName = mEntry.name;
                    var modMembers = mEntry.members;

                    // Mod override exists
                    if (baseId == modId)
                    {
                        bEntry.id = modId;
                        bEntry.name = modName;
                        bEntry.members = modMembers;
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

    public List<GparamInfoReference> GetEntries(string name)
    {
        return FormatInfo[name].list;
    }
}
