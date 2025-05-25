using Microsoft.Extensions.Logging;
using Octokit;
using StudioCore.Core;
using StudioCore.Resource.Locators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace StudioCore.EzStateEditorNS;

/// <summary>
/// Handles the Command/Function meta-data
/// </summary>
public class EsdMeta
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public EsdMeta(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    private EsdMeta_Root TalkEsdBank;

    public async Task<bool> Setup()
    {
        await Task.Yield();

        TalkEsdBank = new EsdMeta_Root();
        TalkEsdBank.commands = new List<EsdMeta_Command>();
        TalkEsdBank.functions = new List<EsdMeta_Function>();
        TalkEsdBank.enums = new List<EsdMeta_Enum>();

        var metaDir = $"{AppContext.BaseDirectory}\\Assets\\ESD\\{ProjectUtils.GetGameDirectory(Project)}";

        // Only supporting Talk ESD currently (change this based on project type)
        var resourcePath = $"{metaDir}\\Talk.json";

        if (File.Exists(resourcePath))
        {
            try
            {
                using (var stream = File.OpenRead(resourcePath))
                {
                    TalkEsdBank = JsonSerializer.Deserialize(stream, EsdMetaDataSerializationContext.Default.EsdMeta_Root);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to read ESD Meta JSON file.", LogLevel.Error, Tasks.LogPriority.High, e);
                return false;
            }
        }

        return true;
    }

    public List<EsdMeta_Command> GetAllCommandMeta()
    {
        return TalkEsdBank.commands;
    }

    public EsdMeta_Command GetCommandMeta(long passedBank, long passedId)
    {
        return TalkEsdBank.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
    }

    public List<EsdMeta_Function> GetAllFunctionMeta()
    {
        return TalkEsdBank.functions;
    }

    public EsdMeta_Function GetFunctionMeta(long passedId)
    {
        return TalkEsdBank.functions.Where(e => e.id == passedId).FirstOrDefault();
    }

    public List<EsdMeta_Arg> GetCommandArgMeta(long passedBank, long passedId)
    {
        if(HasCommandArgMeta(passedBank, passedId))
        {
            var command = TalkEsdBank.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
            return command.args;
        }
        else
        {
            return new List<EsdMeta_Arg>();
        }
    }

    public bool HasCommandArgMeta(long passedBank, long passedId)
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

    public List<EsdMeta_Arg> GetFunctionArgMeta(long passedId)
    {
        if (HasFunctionArgMeta(passedId))
        {
            var function = TalkEsdBank.commands.Where(e => e.id == passedId).FirstOrDefault();
            return function.args;
        }
        else
        {
            return new List<EsdMeta_Arg>();
        }
    }

    public bool HasFunctionArgMeta(long passedId)
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
    public EsdMeta_Enum GetArgEnum(long passedBank, long passedId, int argIndex, string argEnumName)
    {
        var command = TalkEsdBank.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
        if (command != null)
        {
            if (command.args != null && command.args.Count > 0)
            {
                for (int i = 0; i < command.args.Count; i++)
                {
                    var arg = command.args[i];
                    if (i == argIndex)
                    {
                        var argEnumStr = arg.argEnum;

                        var argEnum = TalkEsdBank.enums.Where(e => e.referenceName == argEnumName).FirstOrDefault();
                        if (argEnum != null)
                        {
                            return argEnum;
                        }
                    }
                }
            }
        }

        return null;
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(EsdMeta_Root))]
[JsonSerializable(typeof(EsdMeta_Command))]
[JsonSerializable(typeof(EsdMeta_Function))]
[JsonSerializable(typeof(EsdMeta_Arg))]
[JsonSerializable(typeof(EsdMeta_Enum))]
[JsonSerializable(typeof(EsdMeta_EnumMember))]
public partial class EsdMetaDataSerializationContext
    : JsonSerializerContext
{ }

public class EsdMeta_Root
{
    public List<EsdMeta_Command> commands { get; set; }
    public List<EsdMeta_Function> functions { get; set; }
    public List<EsdMeta_Enum> enums { get; set; }
}

public class EsdMeta_Command
{
    public long id { get; set; }
    public long bank { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }

    public List<EsdMeta_Arg> args { get; set; }
}

public class EsdMeta_Function
{
    public long id { get; set; }
    public string name { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }

    public List<EsdMeta_Arg> args { get; set; }
}

public class EsdMeta_Arg
{
    public string name { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }
    public string argLink { get; set; }
    public string argEnum { get; set; }
}

public class EsdMeta_Enum
{
    public string referenceName { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }

    public List<EsdMeta_EnumMember> members { get; set; }
}

public class EsdMeta_EnumMember
{
    public string identifier { get; set; }
    public string displayName { get; set; }
    public string description { get; set; }
}
