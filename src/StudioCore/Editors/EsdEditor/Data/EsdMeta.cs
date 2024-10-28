using StudioCore.Banks.AliasBank;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StudioCore.Editors.EsdEditor;

/// <summary>
/// Handles the Command/Function meta-data
/// </summary>
public static class EsdMeta
{
    private static EsdMetaData TalkEsdBank;

    public static void SetupMeta()
    {
        var metaDir = $"{Smithbox.SmithboxDataRoot}\\ESD\\{MiscLocator.GetGameIDForDir()}";

        // Only supporting Talk ESD currently
        var resourcePath = $"{metaDir}\\Talk.json";

        if (File.Exists(resourcePath))
        {
            string jsonString = JsonSerializer.Serialize(TalkEsdBank, typeof(AliasResource), AliasResourceSerializationContext.Default);

            try
            {
                var fs = new FileStream(resourcePath, System.IO.FileMode.Create);
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

    public static List<EsdCommandMetaData> GetAllCommandMeta()
    {
        return TalkEsdBank.commands;
    }

    public static EsdCommandMetaData GetCommandMeta(long passedBank, long passedId)
    {
        return TalkEsdBank.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
    }

    public static List<EsdFunctionMetaData> GetAllFunctionMeta()
    {
        return TalkEsdBank.functions;
    }

    public static EsdFunctionMetaData GetFunctionMeta(long passedId)
    {
        return TalkEsdBank.functions.Where(e => e.id == passedId).FirstOrDefault();
    }

    public static List<EsdArgMetaData> GetCommandArgMeta(long passedBank, long passedId)
    {
        if(HasCommandArgMeta(passedBank, passedId))
        {
            var command = TalkEsdBank.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
            return command.args;
        }
        else
        {
            return new List<EsdArgMetaData>();
        }
    }

    public static bool HasCommandArgMeta(long passedBank, long passedId)
    {
        var command = TalkEsdBank.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
        if(command != null)
        {
            if(command.args != null && command.args.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public static List<EsdArgMetaData> GetFunctionArgMeta(long passedId)
    {
        if (HasFunctionArgMeta(passedId))
        {
            var function = TalkEsdBank.commands.Where(e => e.id == passedId).FirstOrDefault();
            return function.args;
        }
        else
        {
            return new List<EsdArgMetaData>();
        }
    }

    public static bool HasFunctionArgMeta(long passedId)
    {
        var function = TalkEsdBank.commands.Where(e => e.id == passedId).FirstOrDefault();
        if (function != null)
        {
            if (function.args != null && function.args.Count > 0)
            {
                return true;
            }
        }

        return false;
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(EsdMetaData))]
[JsonSerializable(typeof(EsdCommandMetaData))]
[JsonSerializable(typeof(EsdFunctionMetaData))]
[JsonSerializable(typeof(EsdArgMetaData))]
[JsonSerializable(typeof(EsdEnumMetaData))]
[JsonSerializable(typeof(EsdEnumMemberMetaData))]
[JsonSerializable(typeof(EsdArgType))]
public partial class EsdMetaDataSerializationContext
    : JsonSerializerContext
{ }

public class EsdMetaData
{
    public List<EsdCommandMetaData> commands { get; set; }
    public List<EsdFunctionMetaData> functions { get; set; }
    public List<EsdEnumMetaData> enums { get; set; }
}

public class EsdCommandMetaData
{
    public long id { get; set; }
    public long bank { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }

    public List<EsdArgMetaData> args { get; set; }
}

public class EsdFunctionMetaData
{
    public long id { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }

    public List<EsdArgMetaData> args { get; set; }
}

public class EsdEnumMetaData
{
    public string referenceName { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }

    public List<EsdEnumMemberMetaData> members { get; set; }
}

public class EsdEnumMemberMetaData
{
    public string identifier { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }
}

public class EsdArgMetaData
{
    public string name { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }
    public EsdArgType type { get; set; }

    public EsdEnumMetaData argEnum { get; set; }
}

public enum EsdArgType
{
    [Display(Name = "s32")] s32 = 0,
    [Display(Name = "u32")] u32 = 1,
    [Display(Name = "f32")] f32 = 2,
    [Display(Name = "u8")] u8 = 3,
    [Display(Name = "s8")] s8 = 4,
    [Display(Name = "u16")] u16 = 5,
    [Display(Name = "s16")] s16 = 6
}