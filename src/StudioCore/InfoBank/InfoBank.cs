using StudioCore.Editor;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text;

namespace StudioCore.FormatInfo;

public enum FormatType
{
    None,
    MSB
}

public class InfoBank
{
    private AssetLocator AssetLocator;

    public InfoContainer _loadedInfoBank { get; set; }

    public bool IsLoadingInfoBank { get; set; }
    public bool mayReloadInfoBank { get; set; }

    private string TemplateName = "Template.json";

    private string ProgramDirectory = ".smithbox";

    private string FormatInfoDirectory = "";

    private string FormatInfoName = "";

    private FormatType formatType;

    public InfoBank(AssetLocator locator, FormatType _formatType)
    {
        AssetLocator = locator;
        mayReloadInfoBank = false;

        formatType = _formatType;

        if (formatType is FormatType.MSB)
        {
            FormatInfoName = "MSB";
            FormatInfoDirectory = "MSB";
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

            if (AssetLocator.Type != GameType.Undefined)
            {
                try
                {
                    _loadedInfoBank = new InfoContainer(formatType, AssetLocator.GetGameIDForDir(), AssetLocator.GameModDirectory);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"FAILED LOAD: {e.Message}");
                }

                IsLoadingInfoBank = false;
            }
            else
            {
                IsLoadingInfoBank = false;
            }
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
            {
                newResource = JsonSerializer.Deserialize<InfoResource>(stream, options);
            }
        }

        return newResource;
    }

    public void WriteTargetInfoBank(InfoResource targetBank, string formatType)
    {
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\FormatInfo\\{FormatInfoDirectory}\\{AssetLocator.GetGameIDForDir()}\\";

        var resourceFilePath = $"{modResourcePath}\\{formatType}.json";

        if (File.Exists(resourceFilePath))
        {
            var options = new JsonSerializerOptions
            {
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            string jsonString = JsonSerializer.Serialize<InfoResource>(targetBank, options);

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

    public void AddToLocalInfoBank(string assetType, string refID, string refName, string refDesc)
    {
        var templateResource = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{TemplateName}";
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\FormatInfo\\{FormatInfoDirectory}\\{AssetLocator.GetGameIDForDir()}\\";

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

        if (File.Exists(resourceFilePath))
        {
            // Load up the target local bank.
            var targetResource = LoadTargetInfoBank(resourceFilePath);

            bool doesExist = false;

            // If it exists within the mod local file, update the contents
            foreach (var entry in targetResource.list)
            {
                if (entry.id == refID)
                {
                    doesExist = true;

                    entry.name = refName;
                    entry.desc = refDesc;
                }
            }

            // If it doesn't exist in the mod local file, add it in
            if (!doesExist)
            {
                InfoReference entry = new InfoReference();
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
        var modResourcePath = AssetLocator.GameModDirectory + $"\\{ProgramDirectory}\\Assets\\Aliases\\{FormatInfoDirectory}\\{AssetLocator.GetGameIDForDir()}\\";

        var resourceFilePath = $"{modResourcePath}\\{formatType}.json";

        if (File.Exists(resourceFilePath))
        {
            // Load up the target local model alias bank. 
            var targetResource = LoadTargetInfoBank(resourceFilePath);

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

            WriteTargetInfoBank(targetResource, formatType);
        }
    }
}
