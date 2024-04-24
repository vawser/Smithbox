using StudioCore.Banks.FormatBank;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Memory;

public class GameOffsetContainer
{
    public GameOffsetResource Data;

    public GameOffsetContainer()
    {
        Data = LoadJSON();
    } 

    private GameOffsetResource LoadJSON()
    {
        var baseResource = new GameOffsetResource();

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\GameOffsets\\{Project.GetGameIDForDir()}\\Data.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, GameOffsetResourceSerializationContext.Default.GameOffsetResource);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }
}
