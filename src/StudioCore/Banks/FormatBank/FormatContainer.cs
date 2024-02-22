using StudioCore.Banks.AliasBank;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace StudioCore.Banks.FormatBank;

public class FormatContainer
{
    public FormatResource Data;

    private FormatBankType FormatType;

    public FormatContainer()
    {
        Data = new FormatResource();
        FormatType = FormatBankType.None;
    }
    public FormatContainer(FormatBankType _formatType)
    {
        FormatType = _formatType;

        Data = LoadJSON();
    }

    private FormatResource LoadJSON()
    {
        var baseResource = new FormatResource();

        if (FormatType is FormatBankType.None)
            return null;

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{GetFormatTypeDir()}\\Core.json";

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, FormatResourceSerializationContext.Default.FormatResource);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }

    private string GetFormatTypeDir()
    {
        var typDir = "";

        if (FormatType is FormatBankType.MSB)
            typDir = "MSB";

        if (FormatType is FormatBankType.FLVER)
            typDir = "FLVER";

        if (FormatType is FormatBankType.GPARAM)
            typDir = "GPARAM";

        return typDir;
    }
}
