using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text;
using StudioCore.Core;
using StudioCore.Locators;

namespace StudioCore.Banks.AliasBank;

/// <summary>
/// An alias bank holds naming information, allowing for user-readable notes to be appended to raw identifiers (e.g. c0000 becomes c0000 <Player>)
/// An alias bank has 2 sources: Smithbox, and the local project. 
/// Entries in the local project version will supercede the Smithbox entries.
/// </summary>
public class AliasBank
{
    public AliasResource Aliases { get; set; }

    private string TemplateName = "Template.json";

    private string AliasDirectory = "";

    private string AliasFileName = "";

    private string AliasTitle = "";

    public AliasBank(string fileName, string path, string title)
    {
        AliasDirectory = path;
        AliasFileName = fileName;
        AliasTitle = title;
    }

    public void LoadBank()
    {
        try
        {
            Aliases = BankUtils.LoadAliasJSON(AliasFileName, AliasDirectory);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Failed to load: {AliasTitle} Bank: {e.Message}");
        }

        TaskLogs.AddLog($"Alias Bank: Loaded {AliasTitle} Bank");
    }

    public Dictionary<string, AliasReference> GetEntries()
    {
        Dictionary<string, AliasReference> Entries = new Dictionary<string, AliasReference>();

        if(Aliases.list != null)
        {
            foreach (var entry in Aliases.list)
            {
                if (!Entries.ContainsKey(entry.id))
                {
                    Entries.Add(entry.id, entry);
                }
            }
        }

        return Entries;
    }

    public AliasResource LoadAliasResource(string path)
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

    public void WriteAliasResource(AliasResource targetBank)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var resourcePath =  $"{Smithbox.SmithboxDataRoot}\\Assets\\Aliases\\{AliasDirectory}\\{MiscLocator.GetGameIDForDir()}\\";

        if(CFG.Current.AliasBank_EditorMode)
        {
            resourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{AliasDirectory}\\{MiscLocator.GetGameIDForDir()}\\";
        }

        var resourceFilePath = $"{resourcePath}\\{AliasFileName}.json";

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

    public void AddToLocalAliasBank(string refID, string refName, string refTags)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var templateResource = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{TemplateName}";

        var resourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\Aliases\\{AliasDirectory}\\{MiscLocator.GetGameIDForDir()}\\";

        if (CFG.Current.AliasBank_EditorMode)
        {
            resourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{AliasDirectory}\\{MiscLocator.GetGameIDForDir()}\\";
        }

        var resourceFilePath = $"{resourcePath}\\{AliasFileName}.json";

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
            var targetResource = LoadAliasResource(resourceFilePath);

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

            WriteAliasResource(targetResource);
        }

        LoadBank();
    }

    /// <summary>
    /// Removes specified reference from local model alias bank.
    /// </summary>
    public void RemoveFromLocalAliasBank(string refID)
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        var resourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\Aliases\\{AliasDirectory}\\{MiscLocator.GetGameIDForDir()}\\";

        if (CFG.Current.AliasBank_EditorMode)
        {
            resourcePath = AppContext.BaseDirectory + $"\\Assets\\Aliases\\{AliasDirectory}\\{MiscLocator.GetGameIDForDir()}\\";
        }

        var resourceFilePath = $"{resourcePath}\\{AliasFileName}.json";


        if (File.Exists(resourceFilePath))
        {
            // Load up the target local model alias bank. 
            var targetResource = LoadAliasResource(resourceFilePath);

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

            WriteAliasResource(targetResource);
        }

        LoadBank();
    }

    private Dictionary<string, string> enumDict;

    public Dictionary<string, string> GetEnumDictionary()
    {
        if (enumDict == null)
        {
            enumDict = new Dictionary<string, string>();
            foreach (var entry in Aliases.list)
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
