using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.MapGroupBank;

public class MapGroupBank
{
    public MapGroupResource Groups { get; set; }

    private string TemplateName = "Template.json";

    private string MapGroupDirectory = "";

    private string MapGroupFileName = "";

    public MapGroupBank()
    {
        MapGroupDirectory = "MapGroups";
        MapGroupFileName = "Groups";
    }

    public void LoadBank()
    {
        try
        {
            Groups = BankUtils.LoadMapGroupJSON(MapGroupFileName, MapGroupDirectory);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Failed to load Map Group Bank {MapGroupFileName}: {e.Message}");
        }

        TaskLogs.AddLog($"Map Group Bank: Loaded Map Groups");
    }

    public List<MapGroupReference> GetEntries()
    {
        var entries = new List<MapGroupReference>();

        if(Groups == null)
        {
            return entries;
        }
        else if (Groups.list == null)
        {
            return entries;
        }
        else
        {
            return Groups.list;
        }
    }

    public MapGroupResource LoadMapGroupResource(string path)
    {
        var newResource = new MapGroupResource();

        if (File.Exists(path))
        {
            using (var stream = File.OpenRead(path))
            {
                newResource = JsonSerializer.Deserialize(stream, MapGroupResourceSerializationContext.Default.MapGroupResource);
            }
        }

        return newResource;
    }

    public void WriteMapGroupResource(MapGroupResource targetBank)
    {
        var resourceFilePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\MapGroups\\{ResourceMiscLocator.GetGameIDForDir()}\\{MapGroupFileName}.json";

        if (File.Exists(resourceFilePath))
        {
            string jsonString = JsonSerializer.Serialize(targetBank, typeof(MapGroupResource), MapGroupResourceSerializationContext.Default);

            try
            {
                var fs = new FileStream(resourceFilePath, FileMode.Create);
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

    public void AddToLocalBank(string refID, string refName, string refDesc, string refCategory, List<MapGroupMember> refMembers)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\MapGroups\\{TemplateName}";

        var resourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\MapGroups\\";

        var resourceFilePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\MapGroups\\{ResourceMiscLocator.GetGameIDForDir()}\\{MapGroupFileName}.json";

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
            var targetResource = LoadMapGroupResource(resourceFilePath);

            var doesExist = false;

            // If it exists within the mod local file, update the contents
            foreach (var entry in targetResource.list)
            {
                if (entry.id == refID)
                {
                    doesExist = true;

                    entry.name = refName;
                    entry.description = refDesc;
                    entry.category = refCategory;
                    entry.members = refMembers;
                }
            }

            // If it doesn't exist in the mod local file, add it in
            if (!doesExist)
            {
                var entry = new MapGroupReference();
                entry.id = refID;
                entry.name = refName;
                entry.description = refDesc;
                entry.category = refCategory;
                entry.members = refMembers;

                targetResource.list.Add(entry);
            }

            WriteMapGroupResource(targetResource);
        }

        LoadBank();
    }

    /// <summary>
    /// Removes specified reference from local model alias bank.
    /// </summary>
    public void RemoveFromLocalBank(string refID)
    {
        var resourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\MapGroups\\{ResourceMiscLocator.GetGameIDForDir()}\\{MapGroupFileName}.json";

        if (File.Exists(resourcePath))
        {
            var targetResource = LoadMapGroupResource(resourcePath);

            for (var i = 0; i <= targetResource.list.Count - 1; i++)
            {
                var entry = targetResource.list[i];
                if (entry.id == refID)
                {
                    targetResource.list.Remove(entry);
                    break;
                }
            }

            WriteMapGroupResource(targetResource);
        }

        LoadBank();
    }

    public bool IsLocalMapGroup(string refID)
    {
        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\MapGroups\\{ResourceMiscLocator.GetGameIDForDir()}\\{MapGroupFileName}.json";

        var resourcePath = $"{Smithbox.SmithboxDataRoot}\\Assets\\MapGroups\\{ResourceMiscLocator.GetGameIDForDir()}\\{MapGroupFileName}.json";

        var isInBase = false;

        if (File.Exists(baseResourcePath))
        {
            var targetResource = LoadMapGroupResource(baseResourcePath);

            for (var i = 0; i <= targetResource.list.Count - 1; i++)
            {
                var entry = targetResource.list[i];
                if (entry.id == refID)
                {
                    isInBase = true;
                }
            }
        }

        var isInProject = false;

        if (File.Exists(resourcePath))
        {
            var targetResource = LoadMapGroupResource(resourcePath);

            for (var i = 0; i <= targetResource.list.Count - 1; i++)
            {
                var entry = targetResource.list[i];
                if (entry.id == refID)
                {
                    isInProject = true;
                }
            }
        }

        if (isInProject && !isInBase)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
