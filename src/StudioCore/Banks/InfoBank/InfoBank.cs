using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text;
using StudioCore.UserProject;

namespace StudioCore.Banks.InfoBank;

public enum FormatType
{
    None,
    MSB,
    FLVER
}
/// <summary>
/// An info bank holds information for annotating formats, such as MSB.
/// An info bank has 1 source: Smithbox.
/// </summary>
public class InfoBank
{
    public InfoContainer _loadedInfoBank { get; set; }

    public bool IsLoadingInfoBank { get; set; }
    public bool mayReloadInfoBank { get; set; }

    private string TemplateName = "Template.json";

    private string ProgramDirectory = ".smithbox";

    private string FormatInfoDirectory = "";

    private string FormatInfoName = "";

    private FormatType formatType;

    public InfoBank(FormatType _formatType)
    {
        mayReloadInfoBank = false;

        formatType = _formatType;

        if (formatType is FormatType.MSB)
        {
            FormatInfoName = "MSB";
            FormatInfoDirectory = "MSB";
        }

        if (formatType is FormatType.FLVER)
        {
            FormatInfoName = "FLVER";
            FormatInfoDirectory = "FLVER";
        }
    }

    public InfoContainer FormatInformation
    {
        get
        {
            if (IsLoadingInfoBank)
                return null;

            return _loadedInfoBank;
        }
    }

    public void ReloadInfoBank()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Format Info - Load {FormatInfoName}", TaskManager.RequeueType.None, false,
        () =>
        {
            _loadedInfoBank = new InfoContainer();
            IsLoadingInfoBank = true;

            if (Project.Type != ProjectType.Undefined)
            {
                try
                {
                    _loadedInfoBank = new InfoContainer(formatType, Project.GetGameIDForDir(), Project.GameModDirectory);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"FAILED LOAD: {e.Message}");
                }

                IsLoadingInfoBank = false;
            }
            else
                IsLoadingInfoBank = false;
        }));
    }

    public InfoResource LoadTargetInfoBank(string path)
    {
        var newResource = new InfoResource();

        if (File.Exists(path))
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            using (var stream = File.OpenRead(path))
                newResource = JsonSerializer.Deserialize<InfoResource>(stream, options);
        }

        return newResource;
    }

    public void WriteTargetInfoBank(InfoResource targetBank, string formatType)
    {
        var modResourcePath = Project.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\FormatInfo\\{FormatInfoDirectory}\\{Project.GetGameIDForDir()}\\";

        var resourceFilePath = $"{modResourcePath}\\{formatType}.json";

        if (File.Exists(resourceFilePath))
        {
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            var jsonString = JsonSerializer.Serialize(targetBank, options);

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

    public void AddToLocalInfoBank(string assetType, string refID, string refName, string refDesc)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{TemplateName}";
        var modResourcePath = Project.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\FormatInfo\\{FormatInfoDirectory}\\{Project.GetGameIDForDir()}\\";

        var resourceFilePath = $"{modResourcePath}\\{assetType}.json";

        // Create directory/file if they don't exist
        if (!Directory.Exists(modResourcePath))
            Directory.CreateDirectory(modResourcePath);
        if (!File.Exists(resourceFilePath))
            File.Copy(templateResource, resourceFilePath);

        if (File.Exists(resourceFilePath))
        {
            // Load up the target local bank.
            var targetResource = LoadTargetInfoBank(resourceFilePath);

            var doesExist = false;

            // If it exists within the mod local file, update the contents
            foreach (var entry in targetResource.list)
                if (entry.id == refID)
                {
                    doesExist = true;

                    entry.name = refName;
                    entry.desc = refDesc;
                }

            // If it doesn't exist in the mod local file, add it in
            if (!doesExist)
            {
                var entry = new InfoReference();
                entry.id = refID;
                entry.name = refName;
                entry.desc = refDesc;

                targetResource.list.Add(entry);
            }

            WriteTargetInfoBank(targetResource, assetType);
        }
    }

    /// <summary>
    /// Removes specified reference from local model alias bank.
    /// </summary>
    public void RemoveFromLocalAliasBank(string formatType, string refID)
    {
        var modResourcePath = Project.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\Aliases\\{FormatInfoDirectory}\\{Project.GetGameIDForDir()}\\";

        var resourceFilePath = $"{modResourcePath}\\{formatType}.json";

        if (File.Exists(resourceFilePath))
        {
            // Load up the target local model alias bank. 
            var targetResource = LoadTargetInfoBank(resourceFilePath);

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

            WriteTargetInfoBank(targetResource, formatType);
        }
    }
}
