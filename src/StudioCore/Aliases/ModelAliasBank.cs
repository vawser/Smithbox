using StudioCore.Editor;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text;

namespace StudioCore.Aliases;

public class ModelAliasBank
{
    private static AssetLocator AssetLocator;

    public static ModelAliasContainer _loadedAliasBank { get; set; }

    public static bool IsLoadingAliases { get; set; }

    public static string ProgramDirectory = ".smithbox";

    public static string AliasDirectory = "ModelAliases";

    public static string TemplateName = "Template.json";

    public static ModelAliasContainer AliasNames
    {
        get
        {
            if (IsLoadingAliases)
                return null;

            return _loadedAliasBank;
        }
    }

    private static void LoadAliasNames()
    {
        try
        {
            _loadedAliasBank = new ModelAliasContainer(AssetLocator.GetGameIDForDir(), AssetLocator.GameModDirectory);
        }
        catch (Exception e)
        {
            
        }
    }

    public static void ReloadAliasBank()
    {
        TaskManager.Run(new TaskManager.LiveTask("Models - Load Names", TaskManager.RequeueType.None, false,
        () =>
        {
            _loadedAliasBank = new ModelAliasContainer();
            IsLoadingAliases = true;

            if (AssetLocator.Type != GameType.Undefined)
                LoadAliasNames();

            IsLoadingAliases = false;
        }));
    }

    public static void SetAssetLocator(AssetLocator l)
    {
        AssetLocator = l;
    }

    public static ModelAliasResource LoadTargetAliasBank(string path)
    {
        var newResource = new ModelAliasResource();

        if (File.Exists(path))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(path))
            {
                newResource = JsonSerializer.Deserialize<ModelAliasResource>(stream, options);
            }
        }

        return newResource;
    }

    public static void WriteTargetAliasBank(ModelAliasResource targetBank, string assetType)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\{AliasDirectory}\\{TemplateName}";
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\{AliasDirectory}\\{AssetLocator.GetGameIDForDir()}\\";
        var resourceFilePath = $"{modResourcePath}\\{assetType}.json";

        if (File.Exists(resourceFilePath))
        {
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            string jsonString = JsonSerializer.Serialize<ModelAliasResource>(targetBank, options);

            try
            {
                FileStream fs = new FileStream(resourceFilePath, FileMode.Create);
                byte[] data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }

    public static void AddToLocalAliasBank(string assetType, string refID, string refName, string refTags)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\{AliasDirectory}\\{TemplateName}";
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\{AliasDirectory}\\{AssetLocator.GetGameIDForDir()}\\";
        var resourceFilePath = $"{modResourcePath}\\{assetType}.json";

        // Create directory/file if they don't exist
        if (!Directory.Exists(modResourcePath))
        {
            Directory.CreateDirectory(modResourcePath);
        }
        if (!File.Exists(resourceFilePath))
        {
            File.Copy(templateResource, resourceFilePath);
        }

        // Load up the target local model alias bank.
        var targetResource = LoadTargetAliasBank(resourceFilePath);

        bool doesExist = false;

        // If it exists within the mod local file, update the contents
        foreach (var entry in targetResource.list)
        {
            if (entry.id == refID)
            {
                doesExist = true;

                entry.name = refName;

                if (refTags.Contains(","))
                {
                    List<string> newTags = new List<string>();
                    var tagList = refTags.Split(",");
                    foreach (var tag in tagList)
                    {
                        newTags.Add(tag);
                    }
                    entry.tags = newTags;
                }
                else
                {
                    entry.tags = new List<string> { refTags };
                }
            }
        }

        // If it doesn't exist in the mod local file, add it in
        if (!doesExist)
        {
            ModelAliasReference entry = new ModelAliasReference();
            entry.id = refID;
            entry.name = refName;
            entry.tags = new List<string>();

            if (refTags.Contains(","))
            {
                List<string> newTags = new List<string>();
                var tagList = refTags.Split(",");
                foreach (var tag in tagList)
                {
                    newTags.Add(tag);
                }
                entry.tags = newTags;
            }
            else
            {
                entry.tags.Add(refTags);
            }

            targetResource.list.Add(entry);
        }

        ModelAliasBank.WriteTargetAliasBank(targetResource, assetType);
    }

    /// <summary>
    /// Removes specified reference from local model alias bank.
    /// </summary>
    public static void RemoveFromLocalAliasBank(string assetType, string refID)
    {
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\{AliasDirectory}\\{AssetLocator.GetGameIDForDir()}\\";
        var resourceFilePath = $"{modResourcePath}\\{assetType}.json";

        // Load up the target local model alias bank. 
        var targetResource = LoadTargetAliasBank(resourceFilePath);

        // Remove the specified reference from the local model alias bank.
        for (int i = 0; i <= targetResource.list.Count - 1; i++)
        {
            var entry = targetResource.list[i];
            if (entry.id == refID)
            {
                targetResource.list.Remove(entry);
                break;
            }
        }

        ModelAliasBank.WriteTargetAliasBank(targetResource, assetType);
    }
}
