using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Holds the data banks for params.
/// Data Flow: Full Load
/// </summary>
public class ParamData : IDisposable
{
    public ProjectEntry Project;

    public ParamBank PrimaryBank;
    public ParamBank VanillaBank;
    public Dictionary<string, ParamBank> AuxBanks = new();

    public Dictionary<string, PARAMDEF> ParamDefs = new();
    public Dictionary<string, PARAMDEF> ParamDefsByFilename = new();
    public Dictionary<PARAMDEF, ParamMeta> ParamMeta = new();

    // Additional Data
    public ParamTypeInfo ParamTypeInfo;
    public GraphLegends GraphLegends;
    public IconConfigurations IconConfigurations;
    public TableParams TableParamList;
    public TableGroupNameStore TableGroupNames;
    public GameOffsetResource ParamMemoryOffsets;
    public ParamCategoryResource ParamCategories;
    public ParamCommutativeResource CommutativeParamGroups;

    public GroupReferences GroupReferences;
    public FieldLayouts FieldLayouts;

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

        if (!paramDefTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the PARAM definitions.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the PARAM definitions.");
        }

        // Param Meta
        Task<bool> paramMetaTask = SetupParamMeta();
        bool paramMetaTaskResult = await paramMetaTask;

        if (!paramMetaTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the PARAM meta.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the PARAM meta.");
        }

        // Graph Legends
        Task<bool> graphLegendsTask = SetupGraphLegends();
        bool graphLegendsTaskResult = await graphLegendsTask;

        if (!graphLegendsTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Graph annotations.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Graph annotations.");
        }

        // Icon Configurations
        Task<bool> iconConfigTask = SetupIconConfigurations();
        bool iconConfigTaskResult = await iconConfigTask;

        if (!iconConfigTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Icon Configuration data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Icon Configuration data.");
        }

        // Table Param List
        Task<bool> tableParamTask = SetupTableParamList();
        bool tableParamTaskResult = await tableParamTask;

        if (!tableParamTaskResult)
        {
            //Smithbox.LogError(this, $"[Param Editor] Failed to setup table param list.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Table Param list.");
        }

        // Table Group Names
        Task<bool> tableGroupNameTask = SetupTableGroupNames();
        bool tableGroupNameTaskResult = await tableGroupNameTask;

        if (!tableGroupNameTaskResult)
        {
            // Smithbox.LogError(this, $"[Param Editor] Failed to setup table group name bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Table Group Name data.");
        }

        // Game Offsets (per project)
        Task<bool> gameOffsetTask = SetupParamMemoryOffsets();
        bool gameOffsetResult = await gameOffsetTask;

        if (!gameOffsetResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Param Memory Offset data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Param Memory Offset data.");
        }

        // Param Categories (per project)
        Task<bool> paramCategoryTask = SetupParamCategories();
        bool paramCategoryResult = await paramCategoryTask;

        if (!paramCategoryResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Param Categories data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Param Categories data.");
        }

        // Commutative Param Groups (per project)
        Task<bool> commutativeParamGroupTask = SetupCommutativeParamGroups();
        bool commutativeParamGroupResult = await commutativeParamGroupTask;

        if (!commutativeParamGroupResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Commutative Param Groups data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Commutative Param Groups data.");
        }

        // Group References
        Task<bool> groupRefTask = SetupGroupReferences();
        bool groupRefTaskResult = await groupRefTask;

        if (!groupRefTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Group Reference data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Group Reference data.");
        }

        // Field Layouts
        Task<bool> fieldLayoutsTask = SetupFieldLayouts();
        bool fieldLayoutsTaskResult = await fieldLayoutsTask;

        if (!fieldLayoutsTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Field Layout data.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Field Layout data.");
        }


        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Load();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup the Primary Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Load();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to setup Vanilla Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Param Editor] Setup the Vanilla Bank.");
        }

        switch (Project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_DES)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.DS1:
            case ProjectType.DS1R:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_DS1)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.DS2:
            case ProjectType.DS2S:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_DS2)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.BB:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_BB)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.DS3:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_DS3)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.ER:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_ER)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.AC6:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_AC6)
                {
                    RowNameHelper.RowNameRestore(Project);
                }
                break;

            case ProjectType.NR:
                if (CFG.Current.ParamEditor_Stripped_Row_Name_Load_NR)
                {
                    RowNameHelper.RowNameRestore(Project);
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
            Smithbox.LogError(this, $"[Param Editor] Failed to setup Aux PARAM Bank for {targetProject.Descriptor.ProjectName}.");
        }

        if (AuxBanks.ContainsKey(targetProject.Descriptor.ProjectName))
        {
            AuxBanks[targetProject.Descriptor.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.Descriptor.ProjectName, newAuxBank);
        }

        Smithbox.Log(this, $"[Param Editor] Setup Aux PARAM Bank for {targetProject.Descriptor.ProjectName}.");

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
                Smithbox.LogError(this, $"[Param Editor] Failed to deseralize {f} as PARAMDEF", e);
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize Param Type Info: {paramTypeInfoPath}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Failed to read Param Type Info: {paramTypeInfoPath}", e);
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
            Smithbox.Log(this, $"[Param Editor] Reloaded PARAM meta.");
        }
        else
        {
            Smithbox.LogError(this, $"[Param Editor] Failed to reload PARAM meta.");
        }
    }

    public async Task<bool> SetupParamMeta()
    {
        await Task.Yield();

        var rootMetaDir = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Meta");

        var projectMetaDir = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project), "Meta");

        if (CFG.Current.Project_Enable_Project_Metadata)
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
                if (CFG.Current.Project_Enable_Project_Metadata && Project.Descriptor.ProjectType != ProjectType.Undefined)
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
                Smithbox.LogError(this, $"[Param Editor] Failed to deseralize {fName} as PARAMMETA", e);
            }
        }

        return true;
    }

    public async Task<bool> SetupGraphLegends()
    {
        await Task.Yield();

        var folder = @$"{AppContext.BaseDirectory}/Assets/PARAM/{ProjectUtils.GetGameDirectory(Project)}";
        var file = Path.Combine(folder, "Graph Legends.json");

        if(CFG.Current.Project_Enable_Project_Metadata)
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize Graph Legends: {file}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Failed to read Graph Legends: {file}", e);
            }
        }

        return true;
    }

    public async Task<bool> SetupIconConfigurations()
    {
        await Task.Yield();

        var folder = @$"{AppContext.BaseDirectory}/Assets/PARAM/{ProjectUtils.GetGameDirectory(Project)}";
        var file = Path.Combine(folder, "Icon Configurations.json");

        if (CFG.Current.Project_Enable_Project_Metadata)
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize Icon Configurations: {file}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[:Param Editor] Failed to read Icon Configurations: {file}", e);
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
                Smithbox.LogError(this, $"[Param Editor] Failed to load {file} for table group name import during Base Store step.", e);
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to load {file} for table group name import during Project Store step.", e);
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
            Smithbox.LogError(this, $"[Param Editor] Failed to load table param list.", e);
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
            "[Param Editor]",
            "Difference cache between param banks has been refreshed.",
            "Difference cache refresh has failed.",
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

        CacheBank.ClearCaches();
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize the Param Reload offsets: {targetFile}", LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Failed to read the Param Reload offsets: {targetFile}", LogPriority.High, e);
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize the Param Categories: {targetFile}", LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Failed to read the Param Categories: {targetFile}", LogPriority.High, e);
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
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize the Commutative Param Groups: {targetFile}", LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Failed to read the Commutative Param Groups: {targetFile}", LogPriority.High, e);
            }
        }

        return true;
    }

    public async Task<bool> SetupGroupReferences()
    {
        await Task.Yield();

        GroupReferences = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Group Reference List.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var projectFile = Path.Combine(projectFolder, "Group Reference List.json");

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
                    GroupReferences = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.GroupReferences);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize the Group References: {targetFile}", LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Param Editor] Failed to read the Group References: {targetFile}", LogPriority.High, e);
            }
        }

        return true;
    }

    public async Task<bool> SetupFieldLayouts()
    {
        await Task.Yield();

        FieldLayouts = new();

        // Build project-local first, so it takes precedence over the base versions
        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Field Layouts");

        if (Path.Exists(projectFolder))
        {
            foreach (var entry in Directory.EnumerateFiles(projectFolder))
            {
                var file = File.ReadAllText(entry);
                try
                {
                    var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.FieldLayout);

                    if (!FieldLayouts.Entries.Any(e => e.Name == layout.Name))
                    {
                        FieldLayouts.Entries.Add(layout);
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize field layout: {file}", LogPriority.High, e);
                }
            }
        }

        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Field Layouts");

        if (Path.Exists(sourceFolder))
        {
            foreach (var entry in Directory.EnumerateFiles(sourceFolder))
            {
                var file = File.ReadAllText(entry);
                try
                {
                    var layout = JsonSerializer.Deserialize(file, ParamEditorJsonSerializerContext.Default.FieldLayout);

                    if (!FieldLayouts.Entries.Any(e => e.Name == layout.Name))
                    {
                        FieldLayouts.Entries.Add(layout);
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Param Editor] Failed to deserialize field layout: {file}", LogPriority.High, e);
                }
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
