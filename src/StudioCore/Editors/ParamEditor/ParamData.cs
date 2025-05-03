using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Resource.Locators;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.ParamEditor.ParamBank;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Holder for the discrete param banks (primary, vanilla, auxs)
/// </summary>
public class ParamData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public Dictionary<string, PARAMDEF> ParamDefs = new();
    public Dictionary<string, string> FakeParamTypes = new();

    public ParamBank PrimaryBank;
    public ParamBank VanillaBank;
    public Dictionary<string, ParamBank> AuxBanks = new();

    public ParamMetaData Meta;


    public ParamData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1000);

        PrimaryBank = new(BaseEditor, Project, Project.ProjectPath, Project.DataPath);
        VanillaBank = new(BaseEditor, Project, Project.DataPath, Project.DataPath);

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

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup Primary PARAM Bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup Primary PARAM Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup Vanilla PARAM Bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup Vanilla PARAM Bank.");
        }

        return true;
    }

    public async Task<bool> SetupAuxBank(string sourcePath, string fallbackPath)
    {
        await Task.Delay(1000);

        var newAuxBank = new ParamBank(BaseEditor, Project, sourcePath, fallbackPath);

        Task<bool> auxBankTask = newAuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (auxBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup Aux PARAM Bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup Aux PARAM Bank.");
        }

        return true;
    }

    public async Task<bool> SetupParamDefs()
    {
        await Task.Delay(1000);

        ParamDefs = new Dictionary<string, PARAMDEF>();
        FakeParamTypes = new Dictionary<string, string>();

        var dir = ParamLocator.GetParamdefDir();
        var files = Directory.GetFiles(dir, "*.xml");
        List<(string, PARAMDEF)> defPairs = new();

        foreach (var f in files)
        {
            var pdef = PARAMDEF.XmlDeserialize(f, true);
            ParamDefs.Add(pdef.ParamType, pdef);
            defPairs.Add((f, pdef));
        }

        var tentativeMappingPath = ParamLocator.GetTentativeParamTypePath();

        if (File.Exists(tentativeMappingPath))
        {
            foreach (var line in File.ReadAllLines(tentativeMappingPath).Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
                    throw new FormatException($"Malformed line in {tentativeMappingPath}: {line}");

                FakeParamTypes[parts[0]] = parts[1];
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
        await Task.Delay(1000);

        var mdir = ParamLocator.GetParammetaDir();

        if (CFG.Current.Param_UseProjectMeta)
        {
            CreateProjectMeta();
        }

        foreach ((var f, PARAMDEF pdef) in ParamDefs)
        {
            var fName = f.Substring(f.LastIndexOf('\\') + 1);

            //TaskLogs.AddLog(fName);

            if (CFG.Current.Param_UseProjectMeta && Project.ProjectType != ProjectType.Undefined)
            {
                var metaDir = ParamLocator.GetParammetaDir();
                var projectDir = $"{Project.ProjectPath}\\.smithbox\\{metaDir}";
                ParamMetaData.XmlDeserialize($@"{projectDir}\{fName}", pdef);
            }
            else
            {
                ParamMetaData.XmlDeserialize($@"{mdir}\{fName}", pdef);
            }
        }

        return true;
    }

    public void CreateProjectMeta()
    {
        var metaDir = ParamLocator.GetParammetaDir();
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
}
