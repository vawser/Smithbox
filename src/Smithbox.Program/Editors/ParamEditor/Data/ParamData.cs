using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Formats.JSON;
using StudioCore.Resource.Locators;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static StudioCore.Core.ProjectEntry;
using static StudioCore.Editors.ParamEditor.Data.ParamBank;

namespace StudioCore.Editors.ParamEditor.Data;

/// <summary>
/// Holds the data banks for params.
/// Data Flow: Full Load
/// </summary>
public class ParamData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public Dictionary<string, PARAMDEF> ParamDefs = new();
    public Dictionary<string, PARAMDEF> ParamDefsByFilename = new();

    public ParamBank PrimaryBank;
    public ParamBank VanillaBank;
    public Dictionary<string, ParamBank> AuxBanks = new();

    public Dictionary<PARAMDEF, ParamMeta> ParamMeta = new();

    public ParamTypeInfo ParamTypeInfo;

    public GraphLegends GraphLegends;

    public ParamData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Param Defs
        Task<bool> paramDefTask = SetupParamDefs();
        bool paramDefTaskResult = await paramDefTask;

        if (paramDefTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup PARAM definitions.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup PARAM definitions.");
        }

        // Param Meta
        Task<bool> paramMetaTask = SetupParamMeta();
        bool paramMetaTaskResult = await paramMetaTask;

        if (paramMetaTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup PARAM meta.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup PARAM meta.");
        }

        // Graph Legends
        Task<bool> graphLegendsTask = SetupGraphLegends();
        bool graphLegendsTaskResult = await graphLegendsTask;

        if (graphLegendsTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup graph legends.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup graph legends.");
        }


        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Load();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to fully setup Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Load();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to fully setup Vanilla Bank.");
        }

        if(!Project.ImportedParamRowNames)
        {
            PrimaryBank.ImportRowNames(ImportRowNameType.ID, ImportRowNameSourceType.Community);

            Project.ImportedParamRowNames = true;
            BaseEditor.ProjectManager.SaveProject(Project);
        }

        if (Project.EnableParamRowStrip)
        {
            PrimaryBank.RowNameRestore();
        }

        return true;
    }

    public async Task<bool> SetupAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Task.Yield();

        if (reloadProject)
        {
            await targetProject.Init(silent: true, InitType.ParamEditorOnly);
        }
        else
        {
            if (!targetProject.Initialized)
            {
                await targetProject.Init(silent: true, InitType.ParamEditorOnly);
            }
        }

        // Pass in the target project's filesystem,
        // so we fill it with the param data from that project
        var newAuxBank = new ParamBank(Project.ProjectName, BaseEditor, Project, targetProject.FS);

        Task<bool> auxBankTask = newAuxBank.Load();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup Aux PARAM Bank for {targetProject.ProjectName}.", LogLevel.Error, Tasks.LogPriority.High);
        }

        if (AuxBanks.ContainsKey(targetProject.ProjectName))
        {
            AuxBanks[targetProject.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.ProjectName, newAuxBank);
        }

        TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup Aux PARAM Bank for {targetProject.ProjectName}.");

        return true;
    }

    public async Task<bool> SetupParamDefs()
    {
        await Task.Yield();

        ParamDefs = new Dictionary<string, PARAMDEF>();
        ParamDefsByFilename = new Dictionary<string, PARAMDEF>();
        ParamTypeInfo = new();
        ParamTypeInfo.Mapping = new();
        ParamTypeInfo.Exceptions = new();

        var dir = ParamLocator.GetParamdefDir(Project);
        var files = Directory.GetFiles(dir, "*.xml");
        List<(string, PARAMDEF)> defPairs = new();

        foreach (var f in files)
        {
            try
            {
                var pdef = PARAMDEF.XmlDeserialize(f, true);
                ParamDefs.Add(pdef.ParamType, pdef);
                ParamDefsByFilename.Add(f, pdef);
                defPairs.Add((f, pdef));
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deseralize {f} as PARAMDEF", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        // Param Type Info
        var paramTypeInfoPath = @$"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(Project)}\Param Type Info.json";

        if (File.Exists(paramTypeInfoPath))
        {
            var paramTypeInfo = new ParamTypeInfo();
            paramTypeInfo.Mapping = new();
            paramTypeInfo.Exceptions = new();

            ParamTypeInfo = paramTypeInfo;

            try
            {
                var filestring = File.ReadAllText(paramTypeInfoPath);

                try
                {
                    var options = new JsonSerializerOptions();

                    paramTypeInfo = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ParamTypeInfo);
                    ParamTypeInfo = paramTypeInfo;
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deserialize Param Type Info: {paramTypeInfoPath}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to read Param Type Info: {paramTypeInfoPath}", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        return true;
    }

    // For the project meta switch
    public async void ReloadMeta()
    {
        Task<bool> paramMetaTask = SetupParamMeta();
        bool paramMetaTaskResult = await paramMetaTask;

        if (paramMetaTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Reloaded PARAM meta.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to reload PARAM meta.");
        }
    }

    public async Task<bool> SetupParamMeta()
    {
        await Task.Yield();

        var rootMetaDir = @$"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(Project)}\Meta";

        var projectMetaDir = @$"{Project.ProjectPath}\.smithbox\Assets\PARAM\{ProjectUtils.GetGameDirectory(Project)}\Meta";

        if (CFG.Current.UseProjectMeta)
        {
            if (Project.ProjectType != ProjectType.Undefined)
            {
                // Create the project meta copy if it doesn't already exist
                if (!Directory.Exists(projectMetaDir))
                {
                    Directory.CreateDirectory(projectMetaDir);
                    var files = Directory.GetFileSystemEntries(rootMetaDir);

                    foreach (var f in files)
                    {
                        var name = Path.GetFileName(f);
                        var tPath = Path.Combine(rootMetaDir, name);
                        var pPath = Path.Combine(projectMetaDir, name);

                        if (File.Exists(tPath) && !File.Exists(pPath))
                        {
                            File.Copy(tPath, pPath);
                        }
                    }
                }
            }
        }

        foreach ((var f, PARAMDEF pdef) in ParamDefsByFilename)
        {
            ParamMeta meta = new(this);

            var fName = f.Substring(f.LastIndexOf('\\') + 1);

            try
            {
                if (CFG.Current.UseProjectMeta && Project.ProjectType != ProjectType.Undefined)
                {
                    meta.XmlDeserialize($@"{projectMetaDir}\{fName}", pdef);
                }
                else
                {
                    meta.XmlDeserialize($@"{rootMetaDir}\{fName}", pdef);
                }

                ParamMeta.Add(pdef, meta);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deseralize {fName} as PARAMMETA", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        return true;
    }

    public async Task<bool> SetupGraphLegends()
    {
        await Task.Yield();

        var folder = @$"{AppContext.BaseDirectory}/Assets/PARAM/{ProjectUtils.GetGameDirectory(Project)}";
        var file = Path.Combine(folder, "Graph Legends.json");

        if (File.Exists(file))
        {
            try
            {
                var filestring = File.ReadAllText(file);

                try
                {
                    var options = new JsonSerializerOptions();
                    GraphLegends = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.GraphLegends);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deserialize Graph Legends: {file}", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to read Graph Legends: {file}", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }

        return true;
    }

    public void CreateProjectMeta()
    {
        var metaDir = ParamLocator.GetParammetaDir(Project);
        var rootDir = Path.Combine(AppContext.BaseDirectory, metaDir);
        var projectDir = $"{Project.ProjectPath}\\.smithbox\\{metaDir}";

        if (!Directory.Exists(projectDir))
        {
            Directory.CreateDirectory(projectDir);
            var files = Directory.GetFileSystemEntries(rootDir);

            foreach (var f in files)
            {
                var name = Path.GetFileName(f);
                var tPath = Path.Combine(rootDir, name);
                var pPath = Path.Combine(projectDir, name);
                if (File.Exists(tPath) && !File.Exists(pPath))
                {
                    File.Copy(tPath, pPath);
                }
            }
        }
    }

    public void RefreshParamDifferenceCacheTask(bool checkAuxVanillaDiff = false)
    {
        // Refresh diff cache
        TaskManager.LiveTask task = new(
            "paramEditor_refreshDifferenceCache",
            "Param Editor",
            "difference cache between param banks has been refreshed.",
            "difference cache refresh has failed.",
            TaskManager.RequeueType.Repeat,
            true,
            LogPriority.Low,
            () => RefreshAllParamDiffCaches(checkAuxVanillaDiff)
        );

        TaskManager.Run(task);
    }


    public void RefreshAllParamDiffCaches(bool checkAuxVanillaDiff)
    {
        PrimaryBank.RefreshPrimaryDiffCaches(true);
        VanillaBank.RefreshVanillaDiffCaches();

        foreach (KeyValuePair<string, ParamBank> bank in AuxBanks)
        {
            bank.Value.RefreshAuxDiffCaches(checkAuxVanillaDiff);
        }

        UICache.ClearCaches();
    }

    public ParamMeta GetParamMeta(PARAMDEF def)
    {
        if(ParamMeta.ContainsKey(def))
        {
            return ParamMeta[def];
        }
        else
        {
            return null;
        }
    }

    public ParamFieldMeta GetParamFieldMeta(ParamMeta curMeta, PARAMDEF.Field def)
    {
        if (curMeta.Fields.ContainsKey(def))
        {
            return curMeta.Fields[def];
        }
        else
        {
            return null;
        }
    }

}
