using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.FileBrowser;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Holds the data banks for Maps.
/// Data Flow: Lazy Load
/// </summary>
public class MapData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public MapBank PrimaryBank;
    public MapBank VanillaBank;
    public Dictionary<string, MapBank> AuxBanks = new();

    public MsbMeta Meta;
    public EntitySelectionGroupList MapObjectSelections;

    public FileDictionary MapFiles = new();
    public FileDictionary LightFiles = new();
    public FileDictionary DS2_LightFiles = new();
    public FileDictionary NavmeshFiles = new();
    public FileDictionary LightAtlasFiles = new();
    public FileDictionary CollisionFiles = new();

    public FileDictionary AutoInvadeBinders = new();

    public Dictionary<string, MapObjectNameMapEntry> MapObjectNameLists = new();

    public FormatResource MsbInformation;
    public FormatEnum MsbEnums;
    public FormatMask MsbMasks;
    public SpawnStateResource MapSpawnStates;

    public MapData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        SetupFileDictionaries();

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to fully setup Primary Bank.", LogLevel.Error, LogPriority.High);
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to fully setup Vanilla Bank.", LogLevel.Error, LogPriority.High);
        }

        // META
        Meta = new MsbMeta(BaseEditor, Project);

        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        if (!metaTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to setup MSB Meta.", LogLevel.Error, LogPriority.High);
        }

        // MSB Information
        Task<bool> msbInfoTask = SetupMsbInfo();
        bool msbInfoResult = await msbInfoTask;

        if (msbInfoResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Setup MSB information.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to setup MSB information.");
        }

        // Map Object Names
        Task<bool> mapObjNamesTask = SetupMapObjectNames();
        bool mapObjNamesTaskResult = await mapObjNamesTask;

        if (!mapObjNamesTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to setup Map Object Name lists.", LogLevel.Error, LogPriority.High);
        }

        // Map Object Selections
        Task<bool> mapObjSelectionTask = SetupMapObjectSelections();
        bool mapObjSelectionTaskResult = await mapObjSelectionTask;

        if (!mapObjSelectionTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to setup Map Object Selections.", LogLevel.Error, LogPriority.High);
        }

        // Spawn States (per project) -- DS2 specific
        Task<bool> mapSpawnStatesTask = SetupMapSpawnStates();
        bool mapSpawnStatesResult = await mapSpawnStatesTask;

        if (mapSpawnStatesResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Setup Spawn States information.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to setup Spawn States information.");
        }

        return primaryBankTaskResult && vanillaBankTaskResult;
    }

    public void SetupFileDictionaries()
    {
        // MSB
        MapFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map") && !e.Folder.Contains("autoroute"))
            .Where(e => e.Extension == "msb")
            .ToList();

        // BTL
        LightFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "btl")
            .ToList();

        DS2_LightFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "gibhd")
            .ToList();

        // NVA
        NavmeshFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "nva")
            .ToList();

        // BTAB
        LightAtlasFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "btab")
            .ToList();

        // Collision
        CollisionFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/map"))
            .Where(e => e.Extension == "hkxbhd")
            .ToList();

        // AutoInvade
        AutoInvadeBinders.Entries = Project.FileDictionary.Entries
            .Where(e => e.Folder.StartsWith("/other"))
            .Where(e => e.Extension == "aipbnd")
            .ToList();
    }

    public async Task<bool> SetupAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Task.Yield();

        if (reloadProject)
        {
            await targetProject.Init(silent: true, ProjectInitType.MapEditorOnly);
        }
        else
        {
            if (!targetProject.Initialized)
            {
                await targetProject.Init(silent: true, ProjectInitType.MapEditorOnly);
            }
        }

        var newAuxBank = new MapBank(targetProject.ProjectName, BaseEditor, Project, targetProject.FS);

        Task<bool> auxBankTask = newAuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to setup Aux MSB Bank for {targetProject.ProjectName}.");
            return false;
        }

        if (AuxBanks.ContainsKey(targetProject.ProjectName))
        {
            AuxBanks[targetProject.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.ProjectName, newAuxBank);
        }

        TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Setup Aux MSB Bank for {targetProject.ProjectName}.");

        return true;
    }

    public async Task<bool> SetupMapObjectNames()
    {
        await Task.Yield();


        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Project), "Community Map Object Names");
        var projDir = Path.Combine(Project.ProjectPath, ".smithbox", "Project", "Community Map Object Names");

        if (Directory.Exists(projDir))
        {
            srcDir = projDir;
        }

        MapObjectNameLists = new();

        if (Directory.Exists(srcDir))
        {
            foreach (var file in Directory.EnumerateFiles(srcDir))
            {
                try
                {
                    var filestring = await File.ReadAllTextAsync(file);

                    var item = JsonSerializer.Deserialize(filestring, MapEditorJsonSerializerContext.Default.MapObjectNameMapEntry);

                    if (item == null)
                    {
                        throw new Exception($"[{Project.ProjectName}:Map Editor] JsonConvert returned null.");
                    }
                    else
                    {
                        if(!MapObjectNameLists.ContainsKey(item.Name))
                        {
                            MapObjectNameLists.Add(item.Name, item);
                        }
                    }
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to load {file} for Map Object Name lists.", LogLevel.Error, LogPriority.High, e);
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupMapObjectSelections()
    {
        await Task.Yield();

        MapObjectSelections = new();

        // Information
        var projectFolder = Path.Combine(
            Project.ProjectPath,
            ".smithbox",
            "MSB",
            "Entity Selections");

        var projectFile = Path.Combine(
            projectFolder,
            "Selection Groups.json");

        if (File.Exists(projectFile))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(projectFile);

                try
                {
                    MapObjectSelections = JsonSerializer.Deserialize(filestring, MapEditorJsonSerializerContext.Default.EntitySelectionGroupList);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Map Object Selections: {projectFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Map Object Selections: {projectFile}", LogLevel.Error, LogPriority.High, e);
            }
        }
        else
        {
            if (!Directory.Exists(projectFolder))
            {
                Directory.CreateDirectory(projectFolder);
            }

            string template = "{ \"Resources\": [ ] }";
            try
            {
                var fs = new FileStream(projectFile, FileMode.Create);
                var data = Encoding.ASCII.GetBytes(template);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to write Map Entity Selection Groups: {projectFile}\n{ex}");
            }
        }

        if (MapObjectSelections.Resources == null)
        {
            MapObjectSelections.Resources = new();
        }

        return true;
    }

    public void SaveMapObjectSelections()
    {
        var projectFolder = Path.Combine(
            Project.ProjectPath,
            ".smithbox",
            "MSB",
            "Entity Selections");

        if (!Directory.Exists(projectFolder))
        {
            Directory.CreateDirectory(projectFolder);
        }

        var projectFile = Path.Combine(
            projectFolder,
            "Selection Groups.json");

        string jsonString = JsonSerializer.Serialize(MapObjectSelections, MapEditorJsonSerializerContext.Default.EntitySelectionGroupList);

        try
        {
            var fs = new FileStream(projectFile, FileMode.Create);
            var data = Encoding.ASCII.GetBytes(jsonString);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Dispose();
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog($"Failed to save map object selections: {projectFile}\n{ex}");
        }
    }

    /// <summary>
    /// Setup the MSB information for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMsbInfo()
    {
        await Task.Yield();

        MsbInformation = new();
        MsbEnums = new();
        MsbMasks = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Core.json");

        var projectFolder = Path.Join(Project.ProjectPath, ".smithbox", "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.ProjectType));
        var projectFile = Path.Combine(projectFolder, "Core.json");

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
                    MsbInformation = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FormatResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the MSB information: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the MSB information: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        // Enums
        sourceFile = Path.Combine(sourceFolder, "Enums.json");

        projectFile = Path.Combine(projectFolder, "Enums.json");

        targetFile = sourceFile;

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
                    MsbEnums = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FormatEnum);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the MSB enums: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the MSB enums: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        // Masks
        sourceFile = Path.Combine(sourceFolder, "Masks.json");

        projectFile = Path.Combine(projectFolder, "Masks.json");

        targetFile = sourceFile;

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
                    MsbMasks = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FormatMask);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the MSB masks: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the MSB masks: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }

    /// <summary>
    /// Setup the map spawn states for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMapSpawnStates()
    {
        await Task.Yield();

        MapSpawnStates = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "SpawnStates.json");

        var projectFolder = Path.Join(Project.ProjectPath, ".smithbox", "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.ProjectType));
        var projectFile = Path.Combine(projectFolder, "SpawnStates.json");

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
                    MapSpawnStates = JsonSerializer.Deserialize(filestring, MapEditorJsonSerializerContext.Default.SpawnStateResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Map Spawn States: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Map Spawn States: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }
}
