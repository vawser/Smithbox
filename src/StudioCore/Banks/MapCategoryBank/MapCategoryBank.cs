using Microsoft.Extensions.Logging;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.TextureAdditionBank;
using StudioCore.Core.Project;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.MapCategoryBank;

public class MapCategoryBank
{
    public MapCategoryResource Categories { get; set; }

    public MapCategoryBank() { }

    public void LoadBank(bool loadBase = false)
    {
        try
        {
            Categories = LoadParamCategoriesJSON(loadBase);
            TaskLogs.AddLog($"Banks: setup map categories bank.");
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Banks: failed to setup map categories bank:\n{e}", LogLevel.Error);
        }
    }

    public static MapCategoryResource LoadParamCategoriesJSON(bool loadBase = false)
    {
        var resourcePath = $"{AppContext.BaseDirectory}\\Assets\\MSB\\{MiscLocator.GetGameIDForDir()}\\Categories.json";
        var modResourcePath = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\MSB\\{MiscLocator.GetGameIDForDir()}\\Categories.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath) && !loadBase)
        {
            // Otherwise, replace with project-local version
            using (var stream = File.OpenRead(modResourcePath))
            {
                return JsonSerializer.Deserialize(stream, MapCategorySerializationContext.Default.MapCategoryResource);
            }
        }

        if (File.Exists(resourcePath))
        {
            using (var stream = File.OpenRead(resourcePath))
            {
                return JsonSerializer.Deserialize(stream, MapCategorySerializationContext.Default.MapCategoryResource);
            }
        }
        else
        {
            var filename = Path.GetFileNameWithoutExtension(resourcePath);
            TaskLogs.AddLog($"Failed to load map categories: {filename} at {resourcePath}", LogLevel.Error);
        }

        return new MapCategoryResource();
    }

    public void WriteProjectMapCategories()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var modResourceDir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\MSB\\{MiscLocator.GetGameIDForDir()}\\";

        var modResourcePath = $"{modResourceDir}\\Categories.json";

        if (!Directory.Exists(modResourceDir))
        {
            Directory.CreateDirectory(modResourceDir);
        }

        if (Directory.Exists(modResourceDir))
        {
            string jsonString = JsonSerializer.Serialize(Categories, typeof(MapCategoryResource), MapCategorySerializationContext.Default);

            try
            {
                var fs = new FileStream(modResourcePath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to write project map categories:\n{ex}", LogLevel.Error);
            }
        }
    }
}
