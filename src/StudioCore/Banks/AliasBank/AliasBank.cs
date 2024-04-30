using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text;
using StudioCore.UserProject;
using StudioCore.BanksMain;

namespace StudioCore.Banks.AliasBank;

/// <summary>
/// An alias bank holds naming information, allowing for user-readable notes to be appended to raw identifiers (e.g. c0000 becomes c0000 <Player>)
/// An alias bank has 2 sources: Smithbox, and the local project. 
/// Entries in the local project version will supercede the Smithbox entries.
/// </summary>
public class AliasBank
{
    public AliasContainer _loadedAliasBank { get; set; }

    public bool IsLoadingAliases { get; set; }
    public bool CanReloadBank { get; set; }

    private string TemplateName = "Template.json";

    private string AliasDirectory = "";

    private string FileName = "";

    private bool IsAssetFileType = false;

    private string AliasName = "";

    public AliasBankType aliasType;

    public Dictionary<string, string> enumDict;

    public AliasBank(AliasBankType _aliasType)
    {
        CanReloadBank = false;

        aliasType = _aliasType;

        if (aliasType is AliasBankType.Model)
        {
            AliasName = "Models";
            AliasDirectory = "Models";
            FileName = "";
            IsAssetFileType = true;
        }

        if (aliasType is AliasBankType.EventFlag)
        {
            AliasName = "Flags";
            AliasDirectory = "Flags";
            FileName = "EventFlag";
            IsAssetFileType = false;
        }

        if (aliasType is AliasBankType.Particle)
        {
            AliasName = "Particles";
            AliasDirectory = "Particles";
            FileName = "Fxr";
            IsAssetFileType = false;
        }

        if (aliasType is AliasBankType.Map)
        {
            AliasName = "Maps";
            AliasDirectory = "Maps";
            FileName = "Maps";
            IsAssetFileType = false;
        }

        if (aliasType is AliasBankType.Gparam)
        {
            AliasName = "Gparams";
            AliasDirectory = "Gparams";
            FileName = "Gparams";
            IsAssetFileType = false;
        }

        if (aliasType is AliasBankType.Sound)
        {
            AliasName = "Sounds";
            AliasDirectory = "Sounds";
            FileName = "Sound";
            IsAssetFileType = false;
        }

        if (aliasType is AliasBankType.Cutscene)
        {
            AliasName = "Cutscenes";
            AliasDirectory = "Cutscenes";
            FileName = "Cutscene";
            IsAssetFileType = false;
        }

        if (aliasType is AliasBankType.Movie)
        {
            AliasName = "Movies";
            AliasDirectory = "Movies";
            FileName = "Movie";
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

    public void ReloadAliasBank()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Alias Bank - Load {AliasName}", TaskManager.RequeueType.None, false,
        () =>
        {
            _loadedAliasBank = new AliasContainer();
            IsLoadingAliases = true;

            if (Project.Type != ProjectType.Undefined)
            {
                try
                {
                    _loadedAliasBank = new AliasContainer(aliasType);

                    if (aliasType == AliasBankType.Map)
                    {
                        MapAliasBank.ReloadMapNames();
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"FAILED LOAD: {e.Message}");
                }

                IsLoadingAliases = false;
            }
            else
            {
                IsLoadingAliases = false;
            }
        }));
    }

    public AliasResource LoadTargetAliasBank(string path)
    {
        var newResource = new AliasResource();

        if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                newResource = JsonSerializer.Deserialize(stream, AliasResourceSerializationContext.Default.AliasResource);
            }
        }

        return newResource;
    }

    public void WriteTargetAliasBank(AliasResource targetBank, string assetType)
    {
        var resourcePath =  $"{Project.ProjectDataDir}\\Assets\\Aliases\\{AliasDirectory}\\{Project.GetGameIDForDir()}\\";

        if(CFG.Current.AliasBank_EditorMode)
        {
            resourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{AliasDirectory}\\{Project.GetGameIDForDir()}\\";
        }

        var resourceFilePath = $"{resourcePath}\\{FileName}.json";

        if (IsAssetFileType)
        {
            resourceFilePath = $"{resourcePath}\\{assetType}.json";
        }

        if (File.Exists(resourceFilePath))
        {
            string jsonString = JsonSerializer.Serialize(targetBank, typeof(AliasResource), AliasResourceSerializationContext.Default);

            try
            {
                var fs = new FileStream(resourceFilePath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
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

        var resourcePath = $"{Project.ProjectDataDir}\\Assets\\Aliases\\{AliasDirectory}\\{Project.GetGameIDForDir()}\\";

        if (CFG.Current.AliasBank_EditorMode)
        {
            resourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{AliasDirectory}\\{Project.GetGameIDForDir()}\\";
        }

        var resourceFilePath = $"{resourcePath}\\{FileName}.json";

        if (IsAssetFileType)
        {
            resourceFilePath = $"{resourcePath}\\{assetType}.json";
        }

        // Create directory/file if they don't exist
        if (!Directory.Exists(resourcePath))
        {
            Directory.CreateDirectory(resourcePath);
        }

        if (!File.Exists(resourceFilePath))
        {
            File.Copy(templateResource, resourceFilePath);
        }

        if (File.Exists(resourceFilePath))
        {
            // Load up the target local model alias bank.
            var targetResource = LoadTargetAliasBank(resourceFilePath);

            var doesExist = false;

            // If it exists within the mod local file, update the contents
            foreach (var entry in targetResource.list)
            {
                if (entry.id == refID)
                {
                    doesExist = true;

                    entry.name = refName;

                    if (refTags.Contains(","))
                    {
                        var newTags = new List<string>();
                        var tagList = refTags.Split(",");
                        foreach (var tag in tagList)
                            newTags.Add(tag);
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
                var entry = new AliasReference();
                entry.id = refID;
                entry.name = refName;
                entry.tags = new List<string>();

                if (refTags.Contains(","))
                {
                    var newTags = new List<string>();
                    var tagList = refTags.Split(",");
                    foreach (var tag in tagList)
                        newTags.Add(tag);
                    entry.tags = newTags;
                }
                else
                    entry.tags.Add(refTags);

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
        var resourcePath = $"{Project.ProjectDataDir}\\Assets\\Aliases\\{AliasDirectory}\\{Project.GetGameIDForDir()}\\";

        if (CFG.Current.AliasBank_EditorMode)
        {
            resourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{AliasDirectory}\\{Project.GetGameIDForDir()}\\";
        }

        var resourceFilePath = $"{resourcePath}\\{FileName}.json";

        if (IsAssetFileType)
        {
            resourceFilePath = $"{resourcePath}\\{assetType}.json";
        }

        if (File.Exists(resourceFilePath))
        {
            // Load up the target local model alias bank. 
            var targetResource = LoadTargetAliasBank(resourceFilePath);

            // Remove the specified reference from the local model alias bank.
            for (var i = 0; i <= targetResource.list.Count - 1; i++)
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

    public Dictionary<string, string> GetEnumDictionary()
    {
        if (enumDict == null)
        {
            enumDict = new Dictionary<string, string>();
            var entries = AliasNames.GetEntries(AliasName);
            foreach (var entry in entries)
            {
                var name = entry.name;
                if(name == "")
                {
                    name = "Not named";
                }

                enumDict[entry.id] = name;
            }
        }

        return enumDict;
    }
}
