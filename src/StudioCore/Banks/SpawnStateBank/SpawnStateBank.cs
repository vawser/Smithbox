using Microsoft.Extensions.Logging;
using StudioCore.Banks.AliasBank;
using StudioCore.Banks.TextureAdditionBank;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.SpawnStateBank;


public class SpawnStateBank
{
    public SpawnStateResource List { get; set; }

    public SpawnStateBank() { }

    public void LoadBank()
    {
        if (Smithbox.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {

            try
            {
                List = LoadSpawnStateJSON();
                TaskLogs.AddLog($"Banks: setup spawn state bank.");
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"Banks: failed to setup spawn state bank:\n{e}", LogLevel.Error);
            }
        }
        else
        {
            List = new SpawnStateResource();
        }
    }

    public static SpawnStateResource LoadSpawnStateJSON()
    {
        var resourcePath = AppContext.BaseDirectory + $"\\Assets\\MSB\\{MiscLocator.GetGameIDForDir()}\\SpawnStates.json";

        var baseResource = new SpawnStateResource();
        var modResource = new SpawnStateResource();

        if (File.Exists(resourcePath))
        {
            using (var stream = File.OpenRead(resourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, SpawnStateSerializationContext.Default.SpawnStateResource);
            }
        }
        else
        {
            var filename = Path.GetFileNameWithoutExtension(resourcePath);
            TaskLogs.AddLog($"Failed to load spawn state bank: {filename} at {resourcePath}", LogLevel.Error);
        }

        return baseResource;
    }
}
