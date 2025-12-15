using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using static StudioCore.Editors.ParamEditor.ParamBank;

namespace StudioCore.Editors.ParamEditor;

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

    public IconConfigurations IconConfigurations;

    public TableParams TableParamList;
    public TableGroupNameStore TableGroupNames;

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

        // Icon Configurations
        Task<bool> iconConfigTask = SetupIconConfigurations();
        bool iconConfigTaskResult = await iconConfigTask;

        if (iconConfigTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup icon configurations.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup icon configurations.");
        }

        // Table Param List
        Task<bool> tableParamTask = SetupTableParamList();
        bool tableParamTaskResult = await tableParamTask;

        if (tableParamTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup table param list.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup table param list.");
        }

        // Table Group Names
        Task<bool> tableGroupNameTask = SetupTableGroupNames();
        bool tableGroupNameTaskResult = await tableGroupNameTask;

        if (tableGroupNameTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Setup table group name bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup table group name bank.");
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
            var dialog = PlatformUtils.Instance.MessageBox("Do you wish to import row names?", "Automatic Row Naming", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (dialog is DialogResult.OK)
            {
                PrimaryBank.ImportRowNames(ImportRowNameSourceType.Community);
            }

            Project.ImportedParamRowNames = true;
            BaseEditor.ProjectManager.SaveProject(Project);
        }

        switch(Project.ProjectType)
        {
            case ProjectType.DES:
                if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DES)
                {
                    PrimaryBank.RowNameRestore();
                }
                break;

            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS1)
                {
                    PrimaryBank.RowNameRestore();
                }
                break;

            case ProjectType.DS2:
            case ProjectType.DS2S:
                if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS2)
                {
                    PrimaryBank.RowNameRestore();
                }
                break;

            case ProjectType.BB:
                if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_BB)
                {
                    PrimaryBank.RowNameRestore();
                }
                break;

            case ProjectType.DS3:
                if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_DS3)
                {
                    PrimaryBank.RowNameRestore();
                }
                break;

            case ProjectType.ER:
                if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_ER)
                {
                    PrimaryBank.RowNameRestore();
                }
                break;

            case ProjectType.AC6:
                if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_AC6)
                {
                    PrimaryBank.RowNameRestore();
                }
                break;

            case ProjectType.NR:
                if (CFG.Current.Param_RestoreStrippedRowNamesOnLoad_NR)
                {
                    PrimaryBank.RowNameRestore();
                }
                break;

            default: break;
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
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to setup Aux PARAM Bank for {targetProject.ProjectName}.", LogLevel.Error, LogPriority.High);
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
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deseralize {f} as PARAMDEF", LogLevel.Error, LogPriority.High, e);
            }
        }

        // Param Type Info
        var paramTypeInfoPath = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Param Type Info.json");

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
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deserialize Param Type Info: {paramTypeInfoPath}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to read Param Type Info: {paramTypeInfoPath}", LogLevel.Error, LogPriority.High, e);
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

        var rootMetaDir = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Meta");

        var projectMetaDir = Path.Join(Project.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Meta");

        if (CFG.Current.Param_UseProjectMeta)
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
                if (CFG.Current.Param_UseProjectMeta && Project.ProjectType != ProjectType.Undefined)
                {
                    meta.XmlDeserialize(Path.Join(projectMetaDir, fName), pdef);
                }
                else
                {
                    meta.XmlDeserialize(Path.Join(rootMetaDir, fName), pdef);
                }

                ParamMeta.Add(pdef, meta);
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deseralize {fName} as PARAMMETA", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }

    public async Task<bool> SetupGraphLegends()
    {
        await Task.Yield();

        var folder = @$"{AppContext.BaseDirectory}/Assets/PARAM/{ProjectUtils.GetGameDirectory(Project)}";
        var file = Path.Combine(folder, "Graph Legends.json");

        if(CFG.Current.Param_UseProjectMeta)
        {
            var projFolder = Path.Combine(Project.ProjectPath, ".smithbox", "Project");
            var projFile = Path.Combine(projFolder, "Graph Legends.json");

            if(File.Exists(projFile))
            {
                folder = projFolder;
                file = projFile;
            }
        }

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
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deserialize Graph Legends: {file}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to read Graph Legends: {file}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }

    public async Task<bool> SetupIconConfigurations()
    {
        await Task.Yield();

        var folder = @$"{AppContext.BaseDirectory}/Assets/PARAM/{ProjectUtils.GetGameDirectory(Project)}";
        var file = Path.Combine(folder, "Icon Configurations.json");

        if (CFG.Current.Param_UseProjectMeta)
        {
            var projFolder = Path.Combine(Project.ProjectPath, ".smithbox", "Project");
            var projFile = Path.Combine(projFolder, "Icon Configurations.json");

            if (File.Exists(projFile))
            {
                folder = projFolder;
                file = projFile;
            }
        }

        if (File.Exists(file))
        {
            try
            {
                var filestring = File.ReadAllText(file);

                try
                {
                    var options = new JsonSerializerOptions();
                    IconConfigurations = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.IconConfigurations);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to deserialize Icon Configurations: {file}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to read Icon Configurations: {file}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }

    public async Task<bool> SetupTableGroupNames()
    {
        await Task.Yield();

        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Community Table Names");

        if (!Directory.Exists(srcDir))
        {
            // Create blank version so user can add names
            TableGroupNames = new();
            TableGroupNames.Groups = new();

            return false;
        }

        // Base Store
        var baseStore = new TableGroupNameStore();
        baseStore.Groups = new();

        foreach (var file in Directory.EnumerateFiles(srcDir))
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                var item = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.TableGroupParamEntry);

                if (item == null)
                {
                    throw new Exception($"[{Project.ProjectName}:Param Editor] JsonConvert returned null.");
                }
                else
                {
                    baseStore.Groups.Add(item);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to load {file} for table group name import during Base Store step.", LogLevel.Error, LogPriority.High, e);
            }
        }

        // Project Store
        var projDir = Path.Combine(Project.ProjectPath, ".smithbox", "Project", "Community Table Names");

        var projStore = new TableGroupNameStore();
        projStore.Groups = new();

        if (Directory.Exists(projDir))
        {
            foreach (var file in Directory.EnumerateFiles(projDir))
            {
                try
                {
                    var filestring = File.ReadAllText(file);
                    var options = new JsonSerializerOptions();
                    var item = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.TableGroupParamEntry);

                    if (item == null)
                    {
                        throw new Exception($"[{Project.ProjectName}:Param Editor] JsonConvert returned null.");
                    }
                    else
                    {
                        projStore.Groups.Add(item);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to load {file} for table group name import during Project Store step.", LogLevel.Error, LogPriority.High, e);
                }
            }
        }

        // Final Store
        TableGroupNames = baseStore;

        // Add unique groups from project store
        foreach (var entry in projStore.Groups)
        {
            if(!TableGroupNames.Groups.Any(e => e.Param == entry.Param))
            {
                TableGroupNames.Groups.Add(entry);
            }
        }

        // Merge in unique table group names from the project store,
        // and replace base group names if the project store contains entries that match
        foreach(var group in TableGroupNames.Groups)
        {
            var projGroup = projStore.Groups.FirstOrDefault(e => e.Param == group.Param);

            if (projGroup != null)
            {
                // Update the names if any project entries should replace the base entries
                foreach (var entry in group.Entries)
                {
                    if(projGroup.Entries.Any(e => e.ID == entry.ID))
                    {
                        entry.Name = projGroup.Entries.FirstOrDefault(e => e.ID == entry.ID).Name;
                    }
                }

                foreach(var entry in projGroup.Entries)
                {
                    if(!group.Entries.Any(e => e.ID == entry.ID))
                    {
                        var newEntry = new TableGroupEntry();
                        newEntry.ID = entry.ID;
                        newEntry.Name = entry.Name;

                        group.Entries.Add(newEntry);
                    }
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupTableParamList()
    {
        await Task.Yield();

        var srcFile = Path.Combine(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Table Params.json");
        var projFile = Path.Combine(Project.ProjectPath, ".smithbox", "Project", "Table Params.json");

        if (Directory.Exists(projFile))
        {
            srcFile = projFile;
        }

        TableParamList = new();
        TableParamList.Params = new();

        if (!File.Exists(srcFile))
        {
            return false;
        }

        try
        {
            var filestring = File.ReadAllText(srcFile);
            var options = new JsonSerializerOptions();
            var item = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.TableParams);

            if (item == null)
            {
                throw new Exception($"[{Project.ProjectName}:Param Editor] JsonConvert returned null.");
            }
            else
            {
                TableParamList = item;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Param Editor] Failed to load table param list.", LogLevel.Error, LogPriority.High, e);
        }

        return true;
    }

    public void CreateProjectMetadata()
    {
        // META
        var metaDir = ParamLocator.GetParammetaDir(Project);
        var rootDir = Path.Combine(AppContext.BaseDirectory, metaDir);
        var projectDir = Path.Join(Project.ProjectPath, ".smithbox", metaDir);

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

        CopyMetadataFile("Shared Param Enums.json");
        CopyMetadataFile("Graph Legends.json");
        CopyMetadataFile("Icon Configurations.json");
    }

    public void CopyMetadataFile(string name)
    {
        var srcFolder = @$"{AppContext.BaseDirectory}/Assets/PARAM/{ProjectUtils.GetGameDirectory(Project)}";
        var srcFile = Path.Combine(srcFolder, name);

        var targetFolder = Path.Combine(Project.ProjectPath, ".smithbox", "Project");
        var targetFile = Path.Combine(targetFolder, name);

        if(!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        if (File.Exists(srcFile))
        {
            File.Copy(srcFile, targetFile, true);
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
        if (curMeta != null && curMeta.Fields != null && curMeta.Fields.ContainsKey(def))
        {
            return curMeta.Fields[def];
        }
        else
        {
            return null;
        }
    }

}
