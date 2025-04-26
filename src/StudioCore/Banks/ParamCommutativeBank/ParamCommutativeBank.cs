using Microsoft.Extensions.Logging;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.ParamCommutativeBank;
using StudioCore.Banks.TextureAdditionBank;
using StudioCore.Core.ProjectNS;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.ParamCommutativeBank;

public class ParamCommutativeBank
{
    public ParamCommutativeResource CommutativeGroups { get; set; }

    public ParamCommutativeBank() { }

    public void LoadBank(bool loadBase = false)
    {
        try
        {
            CommutativeGroups = LoadCommutativeGroupJSON(loadBase);
            TaskLogs.AddLog($"Banks: setup param commutative group bank.");
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Banks: failed to setup param commutative group bank:\n{e}", LogLevel.Error);
        }
    }

    public static ParamCommutativeResource LoadCommutativeGroupJSON(bool loadBase = false)
    {
        var resourcePath = $"{AppContext.BaseDirectory}\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\CommutativeGroups.json";
        var modResourcePath = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\PARAM\\{MiscLocator.GetGameIDForDir()}\\CommutativeGroups.json";

        // If path does not exist, use baseResource only
        if (File.Exists(modResourcePath) && !loadBase)
        {
            // Otherwise, replace with project-local version
            using (var stream = File.OpenRead(modResourcePath))
            {
                return JsonSerializer.Deserialize(stream, ParamCommutativeGroupSerializationContext.Default.ParamCommutativeResource);
            }
        }

        if (File.Exists(resourcePath))
        {
            using (var stream = File.OpenRead(resourcePath))
            {
                return JsonSerializer.Deserialize(stream, ParamCommutativeGroupSerializationContext.Default.ParamCommutativeResource);
            }
        }
        else
        {
            var filename = Path.GetFileNameWithoutExtension(resourcePath);
            TaskLogs.AddLog($"Failed to load param commutative groups: {filename} at {resourcePath}", LogLevel.Error);
        }

        return new ParamCommutativeResource();
    }
}
