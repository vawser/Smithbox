using StudioCore.Banks.AliasBank;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace StudioCore.Banks.FormatBank;

public class FormatContainer
{
    public FormatResource Data;

    private FormatBankType FormatBankType;
    private bool IsGameSpecific;

    public FormatContainer()
    {
        Data = new FormatResource();
        FormatBankType = FormatBankType.None;
    }
    public FormatContainer(FormatBankType formatBankType, bool isGameSpecific)
    {
        IsGameSpecific = isGameSpecific;
        FormatBankType = formatBankType;

        Data = LoadJSON();
    }

    private FormatResource LoadJSON()
    {
        var baseResource = new FormatResource();

        if (FormatBankType is FormatBankType.None)
            return null;

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{GetFormatTypeDir()}\\Core.json";

        if (IsGameSpecific)
        {
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{GetFormatTypeDir()}\\{Project.GetGameIDForDir()}\\Core.json";
        }

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

        if (FormatBankType is FormatBankType.MSB)
            typDir = "MSB";

        if (FormatBankType is FormatBankType.FLVER)
            typDir = "FLVER";

        if (FormatBankType is FormatBankType.GPARAM)
            typDir = "GPARAM";

        return typDir;
    }
}
