
using System.Text.Json;
using StudioCore.UserProject;
using System.IO;
using System;
using StudioCore.Banks.AliasBank;

namespace StudioCore.Editors.MapEditor.MapGroup;


public class MapGroupContainer
{
    public MapGroupResource Data;

    private string ProgramDirectory = ".smithbox";

    public MapGroupContainer()
    {
        Data = LoadJSON();
    }

    private MapGroupResource LoadJSON()
    {
        var baseResource = new MapGroupResource();
        var modResource = new MapGroupResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\MapGroups\\{Project.GetGameIDForDir()}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, MapGroupResourceSerializationContext.Default.MapGroupResource);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        var modResourcePath = Project.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\MapGroups\\{Project.GetGameIDForDir()}.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath))
        {
            using (var stream = File.OpenRead(modResourcePath))
            {
                modResource = JsonSerializer.Deserialize(stream, MapGroupResourceSerializationContext.Default.MapGroupResource);
            }

            // Replace baseResource entries with those from modResource if there are ID matches
            foreach (var bEntry in baseResource.list)
            {
                var baseId = bEntry.id;
                var baseName = bEntry.name;
                var baseDesc = bEntry.description;
                var baseCategory = bEntry.category;
                var baseMembers = bEntry.members;

                foreach (var mEntry in modResource.list)
                {
                    var modId = mEntry.id;
                    var modName = mEntry.name;
                    var modDesc = mEntry.description;
                    var modCategory = mEntry.category;
                    var modMembers = mEntry.members;

                    // Mod override exists
                    if (baseId == modId)
                    {
                        bEntry.id = modId;
                        bEntry.name = modName;
                        bEntry.description = modDesc;
                        bEntry.category = modCategory;
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
}
