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

public class FxrAliasBank
{
    private AssetLocator AssetLocator;

    public FxrAliasContainer _loadedAliasBank { get; set; }

    public bool IsLoadingAliases { get; private set; }

    public string ProgramDirectory = ".smithbox";

    public string AliasDirectory = "FxrAliases";

    public string FileName = "Fxr.json";

    public string TemplateName = "Template.json";

    public bool mayReloadAliasBank { get; set; }

    public FxrAliasBank(AssetLocator locator)
    {
        AssetLocator = locator;
        mayReloadAliasBank = false;
    }

    public FxrAliasContainer AliasNames
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
            _loadedAliasBank = new FxrAliasContainer(AssetLocator.GetGameIDForDir(), AssetLocator.GameModDirectory);
        }
        catch (Exception e)
        {

        }
    }

    public void ReloadAliasBank()
    {
        _loadedAliasBank = new FxrAliasContainer();
        IsLoadingAliases = true;

        if (AssetLocator.Type != GameType.Undefined)
            LoadAliasNames();

        IsLoadingAliases = false;
    }

    public FxrAliasResource LoadTargetAliasBank(string path)
    {
        var newResource = new FxrAliasResource();

        if (File.Exists(path))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(path))
            {
                newResource = JsonSerializer.Deserialize<FxrAliasResource>(stream, options);
            }
        }

        return newResource;
    }

    public void WriteTargetAliasBank(FxrAliasResource targetBank)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\{AliasDirectory}\\{TemplateName}";
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\{AliasDirectory}\\{AssetLocator.GetGameIDForDir()}\\";
        var resourceFilePath = $"{modResourcePath}\\{FileName}";

        if (File.Exists(resourceFilePath))
        {
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            string jsonString = JsonSerializer.Serialize<FxrAliasResource>(targetBank, options);

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

    public void AddToLocalAliasBank(string refID, string refName, string refTags)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\{AliasDirectory}\\{TemplateName}";
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\{AliasDirectory}\\{AssetLocator.GetGameIDForDir()}\\";
        var resourceFilePath = $"{modResourcePath}\\{FileName}";

        // Create directory/file if they don't exist
        if (!Directory.Exists(modResourcePath))
        {
            Directory.CreateDirectory(modResourcePath);
        }
        if (!File.Exists(resourceFilePath))
        {
            File.Copy(templateResource, resourceFilePath);
        }

        // Load up the target local alias bank.
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
            FxrAliasReference entry = new FxrAliasReference();
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

        WriteTargetAliasBank(targetResource);
    }

    /// <summary>
    /// Removes specified reference from local model alias bank.
    /// </summary>
    public void RemoveFromLocalAliasBank(string refID)
    {
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\{AliasDirectory}\\{AssetLocator.GetGameIDForDir()}\\";
        var resourceFilePath = $"{modResourcePath}\\{FileName}";

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

        WriteTargetAliasBank(targetResource);
    }
}
