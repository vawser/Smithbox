using Microsoft.Extensions.Logging;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.FormatBank;
using StudioCore.Banks.GameOffsetBank;
using StudioCore.Banks.HavokAliasBank;
using StudioCore.Banks.ProjectEnumBank;
using StudioCore.Banks.TextureAdditionBank;
using StudioCore.Banks.TextureBlockBank;
using StudioCore.Banks.TextureCorrectionBank;
using StudioCore.Core;
using StudioCore.Resource.Locators;
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

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{directory}\\{MiscLocator.GetGameIDForDir()}\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, AliasResourceSerializationContext.Default.AliasResource);
            }
        }

        var modResourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\Aliases\\{directory}\\{MiscLocator.GetGameIDForDir()}\\{filename}.json";

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

    public static HavokGeneratorAliasResource LoadHavokAliasJSON(string filename)
    {
        var baseResource = new HavokGeneratorAliasResource();
        var modResource = new HavokGeneratorAliasResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Havok Aliases\\{MiscLocator.GetGameIDForDir()}\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, HavokAliasResourceSerializationContext.Default.HavokGeneratorAliasResource);
            }
        }

        return baseResource;
    }

    public static ProjectEnumResource LoadProjectEnumJSON()
    {
        var baseResource = new ProjectEnumResource();
        var modResource = new ProjectEnumResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Enums.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, ProjectEnumResourceSerializationContext.Default.ProjectEnumResource);
            }
        }

        var modResourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\Paramdex\\{MiscLocator.GetGameIDForDir()}\\Enums.json";

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
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\{MiscLocator.GetGameIDForDir()}\\Core.json";
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
            var filename = Path.GetFileNameWithoutExtension(baseResourcePath);
            TaskLogs.AddLog($"Failed to load format information resource: {filename} at {baseResourcePath}", LogLevel.Error);
        }

        return baseResource;
    }

    public static FormatEnum LoadFormatEnumJSON(string directory, bool isGameSpecific)
    {
        var baseResource = new FormatEnum();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\Enums.json";

        if (isGameSpecific)
        {
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\{MiscLocator.GetGameIDForDir()}\\Enums.json";
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
            var filename = Path.GetFileNameWithoutExtension(baseResourcePath);
            TaskLogs.AddLog($"Failed to load format enum resource: {filename} at {baseResourcePath}", LogLevel.Error);
        }

        return baseResource;
    }

    public static FormatMask LoadFormatMaskJSON(string directory, bool isGameSpecific)
    {
        var baseResource = new FormatMask();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\Masks.json";

        if (isGameSpecific)
        {
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{directory}\\{MiscLocator.GetGameIDForDir()}\\Masks.json";
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
            var filename = Path.GetFileNameWithoutExtension(baseResourcePath);
            TaskLogs.AddLog($"Failed to load format mask resource: {filename} at {baseResourcePath}", LogLevel.Error);
        }

        return baseResource;
    }

    public static TextureAdditionResource LoadTextureAdditionJSON(string directory, string name)
    {
        var baseResource = new TextureAdditionResource();

        var baseResourcePath = "";

        baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\{directory}\\{MiscLocator.GetGameIDForDir()}\\{name}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, TextureAdditionSerializationContext.Default.TextureAdditionResource);
            }
        }
        else
        {
            var filename = Path.GetFileNameWithoutExtension(baseResourcePath);
            TaskLogs.AddLog($"Failed to load texture addition resource: {filename} at {baseResourcePath}", LogLevel.Error);
        }

        return baseResource;
    }

    public static TextureBlockResource LoadTextureBlockJSON(string directory, string name)
    {
        var baseResource = new TextureBlockResource();

        var baseResourcePath = "";

        baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\{directory}\\{MiscLocator.GetGameIDForDir()}\\{name}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, TextureBlockSerializationContext.Default.TextureBlockResource);
            }
        }
        else
        {
            var filename = Path.GetFileNameWithoutExtension(baseResourcePath);
            TaskLogs.AddLog($"Failed to load texture block resource: {filename} at {baseResourcePath}", LogLevel.Error);
        }

        return baseResource;
    }

    public static TextureCorrectionResource LoadTextureCorrectionJSON(string directory, string name)
    {
        var baseResource = new TextureCorrectionResource();

        var baseResourcePath = "";

        baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\{directory}\\{MiscLocator.GetGameIDForDir()}\\{name}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, TextureCorrectionSerializationContext.Default.TextureCorrectionResource);
            }
        }
        else
        {
            var filename = Path.GetFileNameWithoutExtension(baseResourcePath);
            TaskLogs.AddLog($"Failed to load texture correction resource: {filename} at {baseResourcePath}", LogLevel.Error);
        }

        return baseResource;
    }

    public static GameOffsetResource LoadGameOffsetJSON(string directory, string filename)
    {
        var baseResource = new GameOffsetResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\{directory}\\{MiscLocator.GetGameIDForDir()}\\{filename}.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, GameOffsetResourceSerializationContext.Default.GameOffsetResource);
            }
        }
        else
        {
            TaskLogs.AddLog($"Failed to load {baseResourcePath} game offset resource for Param Reloader.", LogLevel.Error);
        }

        return baseResource;
    }
}
