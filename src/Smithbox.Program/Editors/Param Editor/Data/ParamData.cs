using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.FileBrowser;
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
public class ParamData : IDisposable
{
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

    public GameOffsetResource ParamMemoryOffsets;
    public ParamCategoryResource ParamCategories;
    public ParamCommutativeResource CommutativeParamGroups;

    public ParamData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // Param Defs
        Task<bool> paramDefTask = SetupParamDefs();
        bool paramDefTaskResult = await paramDefTask;

        if (paramDefTaskResult)
        {
            TaskLogs.AddLog($"[Param Editor] Setup PARAM definitions.");
        }
        else
        {
            TaskLogs.AddError($"[Param Editor] Failed to setup PARAM definitions.");
        }

        // Param Meta
        Task<bool> paramMetaTask = SetupParamMeta();
        bool paramMetaTaskResult = await paramMetaTask;

        if (paramMetaTaskResult)
        {
            TaskLogs.AddLog($"[Param Editor] Setup PARAM meta.");
        }
        else
        {
            TaskLogs.AddError($"[Param Editor] Failed to setup PARAM meta.");
        }

        // Graph Legends
        Task<bool> graphLegendsTask = SetupGraphLegends();
        bool graphLegendsTaskResult = await graphLegendsTask;

        if (graphLegendsTaskResult)
        {
            TaskLogs.AddLog($"[Param Editor] Setup graph legends.");
        }
        else
        {
            TaskLogs.AddError($"[Param Editor] Failed to setup graph legends.");
        }

        // Icon Configurations
        Task<bool> iconConfigTask = SetupIconConfigurations();
        bool iconConfigTaskResult = await iconConfigTask;

        if (iconConfigTaskResult)
        {
            TaskLogs.AddLog($"[Param Editor] Setup icon configurations.");
        }
        else
        {
            TaskLogs.AddError($"[Param Editor] Failed to setup icon configurations.");
        }

        // Table Param List
        Task<bool> tableParamTask = SetupTableParamList();
        bool tableParamTaskResult = await tableParamTask;

        if (tableParamTaskResult)
        {
            TaskLogs.AddLog($"[Param Editor] Setup table param list.");
        }
        else
        {
            //TaskLogs.AddError($"[Param Editor] Failed to setup table param list.");
        }

        // Table Group Names
        Task<bool> tableGroupNameTask = SetupTableGroupNames();
        bool tableGroupNameTaskResult = await tableGroupNameTask;

        if (tableGroupNameTaskResult)
        {
            TaskLogs.AddLog($"[Param Editor] Setup table group name bank.");
        }
        else
        {
            // TaskLogs.AddError($"[Param Editor] Failed to setup table group name bank.");
        }

        // Game Offsets (per project)
        Task<bool> gameOffsetTask = SetupParamMemoryOffsets();
        bool gameOffsetResult = await gameOffsetTask;

        if (gameOffsetResult)
        {
            TaskLogs.AddLog($"[Param Editor] Setup Param Memory Offsets.");
        }
        else
        {
            TaskLogs.AddError($"[Param Editor] Failed to setup Param Memory Offsets.");
        }

        // Param Categories (per project)
        Task<bool> paramCategoryTask = SetupParamCategories();
        bool paramCategoryResult = await paramCategoryTask;

        if (paramCategoryResult)
        {
            TaskLogs.AddLog($"[Param Editor] Setup Param Categories.");
        }
        else
        {
            TaskLogs.AddError($"[Param Editor] Failed to setup Param Categories.");
        }

        // Commutative Param Groups (per project)
        Task<bool> commutativeParamGroupTask = SetupCommutativeParamGroups();
        bool commutativeParamGroupResult = await commutativeParamGroupTask;

        if (commutativeParamGroupResult)
        {
            TaskLogs.AddLog($"[Param Editor] Setup Commutative Param Groups.");
        }
        else
        {
            TaskLogs.AddError($"[Param Editor] Failed to setup Commutative Param Groups.");
        }

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Load();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddError($"[Param Editor] Failed to fully setup Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Load();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddError($"[Param Editor] Failed to fully setup Vanilla Bank.");
        }

        if(!Project.Descriptor.ImportedParamRowNames)
        {
            var dialog = PlatformUtils.Instance.MessageBox("Do you wish to import row names?", "Automatic Row Naming", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            if (dialog is DialogResult.OK)
            {
                PrimaryBank.ImportRowNames(ParamImportRowNameSourceType.Community);
            }

            Project.Descriptor.ImportedParamRowNames = true;

            Smithbox.Orchestrator.SaveProject(Project);
        }

        switch(Project.Descriptor.ProjectType)
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
        await Smithbox.Orchestrator.LoadAuxiliaryProject(targetProject, ProjectInitType.ParamEditorOnly, reloadProject);

        // Pass in the target project's filesystem,
        // so we fill it with the param data from that project
        var newAuxBank = new ParamBank(Project.Descriptor.ProjectName, Project, targetProject.VFS.FS);

        Task<bool> auxBankTask = newAuxBank.Load();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            TaskLogs.AddError($"[Param Editor] Failed to setup Aux PARAM Bank for {targetProject.Descriptor.ProjectName}.");
        }

        if (AuxBanks.ContainsKey(targetProject.Descriptor.ProjectName))
        {
            AuxBanks[targetProject.Descriptor.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.Descriptor.ProjectName, newAuxBank);
        }

        TaskLogs.AddLog($"[Param Editor] Setup Aux PARAM Bank for {targetProject.Descriptor.ProjectName}.");

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
                TaskLogs.AddError($"[Param Editor] Failed to deseralize {f} as PARAMDEF", e);
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
                var filestring = await File.ReadAllTextAsync(paramTypeInfoPath);

                try
                {
                    paramTypeInfo = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.ParamTypeInfo);
                    ParamTypeInfo = paramTypeInfo;
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"[Param Editor] Failed to deserialize Param Type Info: {paramTypeInfoPath}", e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError($"[Param Editor] Failed to read Param Type Info: {paramTypeInfoPath}", e);
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
            TaskLogs.AddLog($"[Param Editor] Reloaded PARAM meta.");
        }
        else
        {
            TaskLogs.AddError($"[Param Editor] Failed to reload PARAM meta.");
        }
    }

    public async Task<bool> SetupParamMeta()
    {
        await Task.Yield();

        var rootMetaDir = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Meta");

        var projectMetaDir = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Meta");

        if (CFG.Current.Param_UseProjectMeta)
        {
            if (Project.Descriptor.ProjectType != ProjectType.Undefined)
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
                if (CFG.Current.Param_UseProjectMeta && Project.Descriptor.ProjectType != ProjectType.Undefined)
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
                TaskLogs.AddError($"[Param Editor] Failed to deseralize {fName} as PARAMMETA",  e);
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
            var projFolder = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project");
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
                var filestring = await File.ReadAllTextAsync(file);

                try
                {
                    GraphLegends = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.GraphLegends);
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"[Param Editor] Failed to deserialize Graph Legends: {file}",  e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError($"[Param Editor] Failed to read Graph Legends: {file}", e);
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
            var projFolder = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project");
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
                var filestring = await File.ReadAllTextAsync(file);

                try
                {
                    IconConfigurations = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.IconConfigurations);
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"[Param Editor] Failed to deserialize Icon Configurations: {file}", e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError($"[:Param Editor] Failed to read Icon Configurations: {file}", e);
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
                var filestring = await File.ReadAllTextAsync(file);

                var item = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.TableGroupParamEntry);

                if (item != null)
                {
                    baseStore.Groups.Add(item);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError($"[Param Editor] Failed to load {file} for table group name import during Base Store step.", e);
            }
        }

        // Project Store
        var projDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Community Table Names");

        var projStore = new TableGroupNameStore();
        projStore.Groups = new();

        if (Directory.Exists(projDir))
        {
            foreach (var file in Directory.EnumerateFiles(projDir))
            {
                try
                {
                    var filestring = await File.ReadAllTextAsync(file);

                    var item = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.TableGroupParamEntry);

                    if (item != null)
                    {
                        projStore.Groups.Add(item);
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddError($"[Param Editor] Failed to load {file} for table group name import during Project Store step.", e);
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
        var projFile = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Table Params.json");

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
            var filestring = await File.ReadAllTextAsync(srcFile);

            var item = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.TableParams);

            if (item != null)
            {
                TableParamList = item;
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddError($"[Param Editor] Failed to load table param list.", e);
        }

        return true;
    }

    public void CreateProjectMetadata()
    {
        // META
        var metaDir = ParamLocator.GetParammetaDir(Project);
        var rootDir = Path.Combine(AppContext.BaseDirectory, metaDir);
        var projectDir = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", metaDir);

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

        var targetFolder = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project");
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

    /// <summary>
    /// Setup the PARAM memory offsets for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupParamMemoryOffsets()
    {
        await Task.Yield();

        ParamMemoryOffsets = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Param Reload Offsets.json");

        var targetFile = sourceFile;

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(targetFile);

                try
                {
                    ParamMemoryOffsets = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.GameOffsetResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Param Reload offsets: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Param Reload offsets: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the param categories for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupParamCategories()
    {
        await Task.Yield();

        ParamCategories = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Param Categories.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var projectFile = Path.Combine(projectFolder, "Param Categories.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(targetFile);

                try
                {
                    ParamCategories = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.ParamCategoryResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Param Categories: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Param Categories: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }

    public async Task<bool> SetupCommutativeParamGroups()
    {
        await Task.Yield();

        CommutativeParamGroups = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Commutative Params.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var projectFile = Path.Combine(projectFolder, "Commutative Params.json");

        var targetFile = sourceFile;

        if (File.Exists(projectFile))
        {
            targetFile = projectFile;
        }

        if (File.Exists(targetFile))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(targetFile);

                try
                {
                    CommutativeParamGroups = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.ParamCommutativeResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Commutative Param Groups: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Commutative Param Groups: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        VanillaBank?.Dispose();

        foreach (var entry in AuxBanks)
        {
            entry.Value?.Dispose();
        }

        PrimaryBank = null;
        VanillaBank = null;
        AuxBanks = null;

        ParamDefs = null;
        ParamDefsByFilename = null;
        ParamMeta = null;
        ParamTypeInfo = null;
        GraphLegends = null;
        IconConfigurations = null;
        TableParamList = null;
        TableGroupNames = null;
        ParamMemoryOffsets = null;
        ParamCategories = null;
        CommutativeParamGroups = null;
    }
    #endregion
}
