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
    public FormatEnum Enums;
    public FormatMask Masks;

    private FormatBankType FormatBankType;
    private bool IsGameSpecific;

    public FormatContainer()
    {
        Data = new FormatResource();
        Enums = new FormatEnum();
        Masks = new FormatMask();

        FormatBankType = FormatBankType.None;
    }
    public FormatContainer(FormatBankType formatBankType, bool isGameSpecific)
    {
        IsGameSpecific = isGameSpecific;
        FormatBankType = formatBankType;

        Data = LoadFormatJSON();
        Enums = LoadEnumJSON();
        Masks = LoadMaskJSON();
    }

    private FormatResource LoadFormatJSON()
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

    private FormatEnum LoadEnumJSON()
    {
        var baseResource = new FormatEnum();

        if (FormatBankType is FormatBankType.None)
            return null;

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{GetFormatTypeDir()}\\Enums.json";

        if (IsGameSpecific)
        {
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{GetFormatTypeDir()}\\{Project.GetGameIDForDir()}\\Enums.json";
        }

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, FormatEnumSerializationContext.Default.FormatEnum);
            }
        }
        else
        {
            TaskLogs.AddLog($"{baseResourcePath} does not exist!");
        }

        return baseResource;
    }

    private FormatMask LoadMaskJSON()
    {
        var baseResource = new FormatMask();

        if (FormatBankType is FormatBankType.None)
            return null;

        var baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{GetFormatTypeDir()}\\Masks.json";

        if (IsGameSpecific)
        {
            baseResourcePath = AppContext.BaseDirectory + $"\\Assets\\FormatInfo\\{GetFormatTypeDir()}\\{Project.GetGameIDForDir()}\\Masks.json";
        }

        if (File.Exists(baseResourcePath))
        {
            using (var stream = File.OpenRead(baseResourcePath))
            {
                baseResource = JsonSerializer.Deserialize(stream, FormatMaskSerializationContext.Default.FormatMask);
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
