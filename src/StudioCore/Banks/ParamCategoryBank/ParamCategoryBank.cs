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

namespace StudioCore.Banks.ParamCategoryBank;

public class ParamCategoryBank
{
    public ParamCategoryResource Categories { get; set; }

    public ParamCategoryBank() { }

    public void LoadBank()
    {
        try
        {
            Categories = LoadParamCategoriesJSON();
            TaskLogs.AddLog($"Banks: setup param categories bank.");
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Banks: failed to setup param categories bank:\n{e}", LogLevel.Error);
        }
    }

    public static ParamCategoryResource LoadParamCategoriesJSON()
    {
        var resourcePath = AppContext.BaseDirectory + $"\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Categories.json";

        var baseResource = new ParamCategoryResource();
        var modResource = new ParamCategoryResource();

        if (File.Exists(resourcePath))
        {
            using (var stream = File.OpenRead(resourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, ParamCategorySerializationContext.Default.ParamCategoryResource);
            }
        }
        else
        {
            var filename = Path.GetFileNameWithoutExtension(resourcePath);
            TaskLogs.AddLog($"Failed to load param categories: {filename} at {resourcePath}", LogLevel.Error);
        }

        var modResourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Categories.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath))
        {
            using (var stream = File.OpenRead(modResourcePath))
            {
                modResource = JsonSerializer.Deserialize(stream, ParamCategorySerializationContext.Default.ParamCategoryResource);
            }

            // Add mod local unique rentries
            foreach (var mEntry in modResource.Categories)
            {
                var modDisplayName = mEntry.DisplayName;

                var isUnique = true;

                foreach (var bEntry in baseResource.Categories)
                {
                    var baseDisplayName = mEntry.DisplayName;
                    var baseParams = mEntry.Params;

                    // Mod override exists
                    if (baseDisplayName == modDisplayName)
                        isUnique = false;
                }

                if (isUnique)
                {
                    mEntry.IsProjectSpecific = true;

                    baseResource.Categories.Add(mEntry);
                }
            }
        }

        return baseResource;
    }

    public void WriteProjectParamCategories()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var modResourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\Categories.json";

        if (File.Exists(modResourcePath))
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
