using StudioCore.Banks.AliasBank;
using StudioCore.Banks.FormatBank;
using StudioCore.Banks.GameOffsetBank;
using StudioCore.Banks.MapGroupBank;
using StudioCore.Banks.ProjectEnumBank;
using StudioCore.Banks.SelectionGroupBank;
using StudioCore.Banks.TextureAdditionBank;
using StudioCore.Banks.TextureBlockBank;
using StudioCore.Banks.TextureCorrectionBank;
using StudioCore.Core;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks;

public static class BankUtils
{
    public static AliasResource LoadAliasJSON(string filename, string directory)
    {
        var baseResource = new AliasResource();
        var modResource = new AliasResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, AliasResourceSerializationContext.Default.AliasResource);
            }
        }

        var modResourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\Aliases\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\{filename}.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath))
        {
            using (var stream = File.OpenRead(modResourcePath))
            {
                modResource = JsonSerializer.Deserialize(stream, AliasResourceSerializationContext.Default.AliasResource);
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

    public static ProjectEnumResource LoadProjectEnumJSON()
    {
        var baseResource = new ProjectEnumResource();
        var modResource = new ProjectEnumResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Enums.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, ProjectEnumResourceSerializationContext.Default.ProjectEnumResource);
            }
        }

        var modResourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\Paramdex\\{ResourceMiscLocator.GetGameIDForDir()}\\Enums.json";

        // If project version exists, use it instead
        if (File.Exists(modResourcePath))
        {
            using (var stream = File.OpenRead(modResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, ProjectEnumResourceSerializationContext.Default.ProjectEnumResource);
            }
        }

        return baseResource;
    }

    public static FormatResource LoadFormatResourceJSON(string directory, bool isGameSpecific)
    {
        var baseResource = new FormatResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\Core.json";

        if (isGameSpecific)
        {
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\Core.json";
        }

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, FormatResourceSerializationContext.Default.FormatResource);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }

    public static FormatEnum LoadFormatEnumJSON(string directory, bool isGameSpecific)
    {
        var baseResource = new FormatEnum();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\Enums.json";

        if (isGameSpecific)
        {
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\Enums.json";
        }

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, FormatEnumSerializationContext.Default.FormatEnum);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }

    public static FormatMask LoadFormatMaskJSON(string directory, bool isGameSpecific)
    {
        var baseResource = new FormatMask();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\Masks.json";

        if (isGameSpecific)
        {
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\Masks.json";
        }

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, FormatMaskSerializationContext.Default.FormatMask);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }

    public static TextureAdditionResource LoadTextureAdditionJSON(string directory, string name)
    {
        var baseResource = new TextureAdditionResource();

        var baseResourcePath = "";

        baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\{name}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, TextureAdditionSerializationContext.Default.TextureAdditionResource);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }

    public static TextureBlockResource LoadTextureBlockJSON(string directory, string name)
    {
        var baseResource = new TextureBlockResource();

        var baseResourcePath = "";

        baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\{name}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, TextureBlockSerializationContext.Default.TextureBlockResource);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }

    public static TextureCorrectionResource LoadTextureCorrectionJSON(string directory, string name)
    {
        var baseResource = new TextureCorrectionResource();

        var baseResourcePath = "";

        baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\{name}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, TextureCorrectionSerializationContext.Default.TextureCorrectionResource);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }

    public static MapGroupResource LoadMapGroupJSON(string filename, string directory)
    {
        var baseResource = new MapGroupResource();
        var modResource = new MapGroupResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, MapGroupResourceSerializationContext.Default.MapGroupResource);
            }
        }

        var modResourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\{filename}.json";

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

    public static GameOffsetResource LoadGameOffsetJSON(string directory, string filename)
    {
        var baseResource = new GameOffsetResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\{directory}\\{ResourceMiscLocator.GetGameIDForDir()}\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, GameOffsetResourceSerializationContext.Default.GameOffsetResource);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }

    public static SelectionGroupList LoadSelectionGroupJSON(string directory, string filename)
    {
        var smithboxResource = new SelectionGroupList();

        var smithboxResourcePath = $"{Smithbox.SmithboxDataRoot}\\{ResourceMiscLocator.GetGameIDForDir()}\\{directory}\\{filename}.json";

        if (File.Exists(smithboxResourcePath))
        {
            using (var stream = File.OpenRead(smithboxResourcePath))
            {
                smithboxResource = JsonSerializer.Deserialize(stream, SelectionGroupListSerializationContext.Default.SelectionGroupList);
            }
        }
        else
        {
            TaskLogs.AddLog($"{smithboxResource} does not exist!");
        }

        return smithboxResource;
    }
}
