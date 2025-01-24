using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StudioCore.Banks;
using StudioCore.Banks.AliasBank;
using System.IO;

namespace StudioCore.Banks.MapTransformBank;

public class MapTransformBank
{
    public MapTransformList Transforms = new MapTransformList();

    public MapTransformBank() { }

    public void LoadBank()
    {
        var path = $"{Smithbox.ProjectRoot}\\.smithbox\\Workflow\\MapTransforms.json";

        try
        {
            Transforms = BankUtils.LoadMapTransformJson(path);
            TaskLogs.AddLog($"Banks: setup Map Transform bank.");
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Banks: failed to setup Map Transform bank:\n{e}", LogLevel.Error);
        }
    }

    public void SaveBank()
    {
        var pathDir = $"{Smithbox.ProjectRoot}\\.smithbox\\Workflow\\";

        var path = $"{Smithbox.ProjectRoot}\\.smithbox\\Workflow\\MapTransforms.json";

        string jsonString = JsonSerializer.Serialize(Transforms, typeof(MapTransformList), MapTransformListSerializationContext.Default);

        if(!Directory.Exists(pathDir))
            Directory.CreateDirectory(pathDir); 

        try
        {
            var fs = new FileStream(path, System.IO.FileMode.Create);
            var data = Encoding.ASCII.GetBytes(jsonString);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Dispose();
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Failed to write map transform bank:\n{ex}", LogLevel.Error);
        }
    }
}

