using StudioCore.Editor;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using static SoulsFormats.ACB;
using System.Threading;
using System.Text;

namespace StudioCore.Aliases;

public class ModelAliasBank
{
    private static AssetLocator AssetLocator;

    public static ModelAliasContainer _loadedModelAliasBank { get; set; }

    public static bool IsLoadingModelAliases { get; set; }

    private string modResourcePath;
    private string templateResource;
    private string resourceFilePath;

    public static ModelAliasContainer ModelNames
    {
        get
        {
            if (IsLoadingModelAliases)
                return null;

            return _loadedModelAliasBank;
        }
    }

    private static void LoadModelNames()
    {
        try
        {
            _loadedModelAliasBank = new ModelAliasContainer(AssetLocator.GetGameIDForDir(), AssetLocator.GameModDirectory);
        }
        catch (Exception e)
        {
            
        }
    }

    public static void ReloadModelAliases()
    {
        TaskManager.Run(new TaskManager.LiveTask("Models - Load Names", TaskManager.RequeueType.None, false,
        () =>
        {
            _loadedModelAliasBank = new ModelAliasContainer();
            IsLoadingModelAliases = true;

            if (AssetLocator.Type != GameType.Undefined)
                LoadModelNames();

            IsLoadingModelAliases = false;
        }));
    }

    public static void SetAssetLocator(AssetLocator l)
    {
        AssetLocator = l;
    }

    public static ModelAliasResource LoadTargetModelAliasBank(string path)
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

    public static void WriteTargetModelAliasBank(ModelAliasResource targetBank, string assetType)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\Template.json";
        var modResourcePath = AssetLocator.GameModDirectory + $"\\.smithbox\\Assets\\ModelAliases\\{AssetLocator.GetGameIDForDir()}\\";
        var resourceFilePath = $"{modResourcePath}\\{assetType}.json";

        if (File.Exists(resourceFilePath))
        {
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            string jsonString = JsonSerializer.Serialize<ModelAliasResource>(targetBank, options);

            FileStream fs = new FileStream(resourceFilePath, FileMode.Create);
            byte[] data = Encoding.ASCII.GetBytes(jsonString);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Close();
        }
    }

    public static void AddToLocalModelAliasBank(string assetType, string refID, string refName, string refTags)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\Template.json";
        var modResourcePath = AssetLocator.GameModDirectory + $"\\.smithbox\\Assets\\ModelAliases\\{AssetLocator.GetGameIDForDir()}\\";
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
        var targetResource = LoadTargetModelAliasBank(resourceFilePath);

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

        ModelAliasBank.WriteTargetModelAliasBank(targetResource, assetType);
    }

    /// <summary>
    /// Removes specified reference from local model alias bank.
    /// </summary>
    public static void RemoveFromLocalModelAliasBank(string assetType, string refID)
    {
        var modResourcePath = AssetLocator.GameModDirectory + $"\\.smithbox\\Assets\\ModelAliases\\{AssetLocator.GetGameIDForDir()}\\";
        var resourceFilePath = $"{modResourcePath}\\{assetType}.json";

        // Load up the target local model alias bank. 
        var targetResource = LoadTargetModelAliasBank(resourceFilePath);

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

        ModelAliasBank.WriteTargetModelAliasBank(targetResource, assetType);
    }
}
