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

public enum AliasType
{
    None,
    Model,
    EventFlag,
    Particle,
    Map
}

public class AliasBank
{
    private AssetLocator AssetLocator;

    public AliasContainer _loadedAliasBank { get; set; }

    public bool IsLoadingAliases { get; set; }
    public bool mayReloadAliasBank { get; set; }

    private string TemplateName = "Template.json";

    private string ProgramDirectory = ".smithbox";

    private string AliasDirectory = "";

    private string FileName = "";

    private bool IsAssetFileType = false;

    private string AliasName = "";

    private AliasType aliasType;

    public Dictionary<string, string> MapNames;

    public AliasBank(AssetLocator locator, AliasType _aliasType)
    {
        AssetLocator = locator;
        mayReloadAliasBank = false;

        aliasType = _aliasType;

        if (aliasType is AliasType.Model)
        {
            AliasName = "Models";
            AliasDirectory = "Models";
            FileName = "";
            IsAssetFileType = true;
        }

        if (aliasType is AliasType.EventFlag)
        {
            AliasName = "Flags";
            AliasDirectory = "Flags";
            FileName = "EventFlag";
            IsAssetFileType = false;
        }

        if (aliasType is AliasType.Particle)
        {
            AliasName = "Particles";
            AliasDirectory = "Particles";
            FileName = "Fxr";
            IsAssetFileType = false;
        }

        if (aliasType is AliasType.Map)
        {
            AliasName = "Maps";
            AliasDirectory = "Maps";
            FileName = "Maps";
            IsAssetFileType = false;
        }
    }

    public AliasContainer AliasNames
    {
        get
        {
            if (IsLoadingAliases)
                return null;

            return _loadedAliasBank;
        }
    }

    private void LoadAliasNames()
    {
        try
        {
            _loadedAliasBank = new AliasContainer(aliasType, AssetLocator.GetGameIDForDir(), AssetLocator.GameModDirectory);
        }
        catch (Exception e)
        {

        }
    }

    public void ReloadAliasBank()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Alias Bank - Load {AliasName}", TaskManager.RequeueType.None, false,
        () =>
        {
            _loadedAliasBank = new AliasContainer();
            IsLoadingAliases = true;

            if (AssetLocator.Type != GameType.Undefined)
                LoadAliasNames();

            IsLoadingAliases = false;

            if (AssetLocator.Type != GameType.Undefined)
                UpdateMapNames();
        }));
    }

    public AliasResource LoadTargetAliasBank(string path)
    {
        var newResource = new AliasResource();

        if (File.Exists(path))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(path))
            {
                newResource = JsonSerializer.Deserialize<AliasResource>(stream, options);
            }
        }

        return newResource;
    }

    public void WriteTargetAliasBank(AliasResource targetBank, string assetType)
    {
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\Aliases\\{AliasDirectory}\\{AssetLocator.GetGameIDForDir()}\\";

        var resourceFilePath = $"{modResourcePath}\\{FileName}.json";

        if (IsAssetFileType)
            resourceFilePath = $"{modResourcePath}\\{assetType}.json";
            

        if (File.Exists(resourceFilePath))
        {
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            string jsonString = JsonSerializer.Serialize<AliasResource>(targetBank, options);

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

    public void AddToLocalAliasBank(string assetType, string refID, string refName, string refTags)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{TemplateName}";
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\Aliases\\{AliasDirectory}\\{AssetLocator.GetGameIDForDir()}\\";

        var resourceFilePath = $"{modResourcePath}\\{FileName}.json";

        if (IsAssetFileType)
            resourceFilePath = $"{modResourcePath}\\{assetType}.json";
        

        // Create directory/file if they don't exist
        if (!Directory.Exists(modResourcePath))
        {
            Directory.CreateDirectory(modResourcePath);
        }
        if (!File.Exists(resourceFilePath))
        {
            File.Copy(templateResource, resourceFilePath);
        }

        if (File.Exists(resourceFilePath))
        {
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
                AliasReference entry = new AliasReference();
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

            WriteTargetAliasBank(targetResource, assetType);
        }
    }

    /// <summary>
    /// Removes specified reference from local model alias bank.
    /// </summary>
    public void RemoveFromLocalAliasBank(string assetType, string refID)
    {
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\Aliases\\{AliasDirectory}\\{AssetLocator.GetGameIDForDir()}\\";

        var resourceFilePath = $"{modResourcePath}\\{FileName}.json";

        if (IsAssetFileType)
            resourceFilePath = $"{modResourcePath}\\{assetType}.json";

        if (File.Exists(resourceFilePath))
        {
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

            WriteTargetAliasBank(targetResource, assetType);
        }
    }

    public void UpdateMapNames()
    {
        if (aliasType is AliasType.Map)
        {
            Dictionary<string, string> _mapNames = new Dictionary<string, string>();

            foreach (var entry in AliasNames.GetEntries("Maps"))
            {
                if (!CFG.Current.MapAliases_ShowUnusedNames)
                {
                    if (entry.tags[0] != "unused")
                    {
                        _mapNames.Add(entry.id, entry.name);
                    }
                }
                else
                {
                    _mapNames.Add(entry.id, entry.name);
                }
            }

            MapNames = _mapNames;
        }
    }
}
