using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace StudioCore.Banks.InfoBank;

public class InfoContainer
{
    private Dictionary<string, InfoResource> FormatInfo = new Dictionary<string, InfoResource>();

    private FormatType formatType;

    private string gametype;

    private string gameModDirectory;

    public InfoContainer()
    {
        FormatInfo = null;
        formatType = FormatType.None;
    }
    public InfoContainer(FormatType _formatType, string _gametype, string _gameModDirectory)
    {
        formatType = _formatType;
        gametype = _gametype;
        gameModDirectory = _gameModDirectory;

        if(formatType == FormatType.MSB)
        {
            FormatInfo.Add("Part", LoadJSON("Part"));
            FormatInfo.Add("Region", LoadJSON("Region"));
            FormatInfo.Add("Event", LoadJSON("Event"));
            FormatInfo.Add("Light", LoadJSON("Light"));
        }

        if (formatType == FormatType.FLVER)
        {
            FormatInfo.Add("All", LoadJSON("All"));
        }
    }

    private InfoResource LoadJSON(string filename)
    {
        var baseResource = new InfoResource();
        var modResource = new InfoResource();

        if (formatType is FormatType.None)
            return null;

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{GetFormatTypeDir()}\\{gametype}\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(baseResourcePath))
                baseResource = JsonSerializer.Deserialize<InfoResource>(File.OpenRead(baseResourcePath), options);
        }

        var modResourcePath = gameModDirectory + $"\\.smithbox\\Assets\\FormatInfo\\{GetFormatTypeDir()}\\{gametype}\\{filename}.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(modResourcePath))
                modResource = JsonSerializer.Deserialize<InfoResource>(File.OpenRead(modResourcePath), options);

            // Replace baseResource entries with those from modResource if there are ID matches
            foreach (var bEntry in baseResource.list)
            {
                var baseId = bEntry.id;
                var baseName = bEntry.name;
                var baseDesc = bEntry.desc;

                foreach (var mEntry in modResource.list)
                {
                    var modId = mEntry.id;
                    var modName = mEntry.name;
                    var modDesc = mEntry.desc;

                    // Mod override exists
                    if (baseId == modId)
                    {
                        bEntry.id = modId;
                        bEntry.name = modName;
                        bEntry.desc = modDesc;
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

    private string GetFormatTypeDir()
    {
        var typDir = "";

        if (formatType is FormatType.MSB)
            typDir = "MSB";

        if (formatType is FormatType.FLVER)
            typDir = "FLVER";

        return typDir;
    }

    public List<InfoReference> GetEntries(string name)
    {
        return FormatInfo[name].list;
    }
}
