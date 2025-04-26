using Microsoft.Extensions.Logging;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.TextureAdditionBank;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.ParamCategoryBank;

public class ParamCategoryBank
{
    public ParamCategoryResource Categories { get; set; }

    public ParamCategoryBank() { }

    public void LoadBank(bool loadBase = false)
    {
        try
        {
            Categories = LoadParamCategoriesJSON(loadBase);
            TaskLogs.AddLog($"Banks: setup param categories bank.");
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Banks: failed to setup param categories bank:\n{e}", LogLevel.Error);
        }
    }

    public static ParamCategoryResource LoadParamCategoriesJSON(bool loadBase = false)
    {
        var resourcePath = $"{AppContext.BaseDirectory}\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Categories.json";
        var modResourcePath = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Categories.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath) && !loadBase)
        {
            // Otherwise, replace with project-local version
            using (var stream = File.OpenRead(modResourcePath))
            {
                return JsonSerializer.Deserialize(stream, ParamCategorySerializationContext.Default.ParamCategoryResource);
            }
        }

        if (File.Exists(resourcePath))
        {
            using (var stream = File.OpenRead(resourcePath))
            {
                return JsonSerializer.Deserialize(stream, ParamCategorySerializationContext.Default.ParamCategoryResource);
            }
        }
        else
        {
            var filename = Path.GetFileNameWithoutExtension(resourcePath);
            TaskLogs.AddLog($"Failed to load param categories: {filename} at {resourcePath}", LogLevel.Error);
        }

        return new ParamCategoryResource();
    }

    public void WriteProjectParamCategories()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var modResourceDir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\";

        var modResourcePath = $"{modResourceDir}\\Categories.json";

        if(!Directory.Exists(modResourceDir))
        {
            Directory.CreateDirectory(modResourceDir);
        }

        if (Directory.Exists(modResourceDir))
        {
            string jsonString = JsonSerializer.Serialize(Categories, typeof(ParamCategoryResource), ParamCategorySerializationContext.Default);

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
                TaskLogs.AddLog($"Failed to write project param categories:\n{ex}", LogLevel.Error);
            }
        }
    }
}
