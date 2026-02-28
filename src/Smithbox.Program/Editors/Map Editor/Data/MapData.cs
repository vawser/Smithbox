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
public class MapData : IDisposable
{
    public ProjectEntry Project;

    public MapBank PrimaryBank;
    public MapBank VanillaBank;
    public Dictionary<string, MapBank> AuxBanks = new();

    public MsbMeta Meta;
    public EntitySelectionGroupList MapObjectSelections;

    public Dictionary<string, MapObjectNameMapEntry> MapObjectNameLists = new();
    public FormatResource MsbInformation;
    public FormatEnum MsbEnums;
    public FormatMask MsbMasks;
    public SpawnStateResource MapSpawnStates;

    public MapData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to fully setup Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to fully setup Vanilla Bank.");
        }

        // META
        Meta = new MsbMeta(Project);

        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        if (!metaTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup MSB Meta.");
        }

        // MSB Information
        Task<bool> msbInfoTask = SetupMsbInfo();
        bool msbInfoResult = await msbInfoTask;

        if (!msbInfoResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup MSB information.");
        }

        // Map Object Names
        Task<bool> mapObjNamesTask = SetupMapObjectNames();
        bool mapObjNamesTaskResult = await mapObjNamesTask;

        if (!mapObjNamesTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup Map Object Name lists.");
        }

        // Map Object Selections
        Task<bool> mapObjSelectionTask = SetupMapObjectSelections();
        bool mapObjSelectionTaskResult = await mapObjSelectionTask;

        if (!mapObjSelectionTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup Map Object Selections.");
        }

        // Spawn States (per project) -- DS2 specific
        Task<bool> mapSpawnStatesTask = SetupMapSpawnStates();
        bool mapSpawnStatesResult = await mapSpawnStatesTask;

        if (!mapSpawnStatesResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup Spawn States information.");
        }

        return primaryBankTaskResult && vanillaBankTaskResult;
    }

    public async Task<bool> SetupAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Smithbox.Orchestrator.LoadAuxiliaryProject(targetProject, ProjectInitType.MapEditorOnly, reloadProject);

        var newAuxBank = new MapBank(targetProject.Descriptor.ProjectName, Project, targetProject.VFS.FS);

        Task<bool> auxBankTask = newAuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup Aux MSB Bank for {targetProject.Descriptor.ProjectName}.");
            return false;
        }

        if (AuxBanks.ContainsKey(targetProject.Descriptor.ProjectName))
        {
            AuxBanks[targetProject.Descriptor.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.Descriptor.ProjectName, newAuxBank);
        }

        Smithbox.LogError(this, $"[Map Editor] Setup Aux MSB Bank for {targetProject.Descriptor.ProjectName}.");

        return true;
    }

    public async Task<bool> SetupMapObjectNames()
    {
        await Task.Yield();


        var srcDir = Path.Combine(StudioCore.Common.FileLocations.Assets, "MSB", ProjectUtils.GetGameDirectory(Project), "Community Map Object Names");
        
        var projDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Community Map Object Names");

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

                    if (item != null)
                    {
                        if(!MapObjectNameLists.ContainsKey(item.Name))
                        {
                            MapObjectNameLists.Add(item.Name, item);
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Map Editor] Failed to load {file} for Map Object Name lists.", e);
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
            Project.Descriptor.ProjectPath,
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
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize the Map Object Selections: {projectFile}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Editor] Failed to read the Map Object Selections: {projectFile}", e);
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
                Smithbox.Log(this, $"Failed to write Map Entity Selection Groups: {projectFile}\n{ex}");
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
            Project.Descriptor.ProjectPath,
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
            Smithbox.LogError(this, $"Failed to save map object selections: {projectFile}", ex);
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
        var gameDir = ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType);

        var sourceFolder = Path.Join(StudioCore.Common.FileLocations.Assets, "MSB", gameDir);

        var sourceFile = Path.Combine(sourceFolder, "Core.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "MSB", gameDir);

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
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize the MSB information: {targetFile}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Editor] Failed to read the MSB information: {targetFile}", e);
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
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize the MSB enums: {targetFile}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Editor] Failed to read the MSB enums: {targetFile}", e);
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
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize the MSB masks: {targetFile}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Editor] Failed to read the MSB masks: {targetFile}", e);
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
        var gameDir = ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType);

        var sourceFolder = Path.Join(StudioCore.Common.FileLocations.Assets, "MSB", gameDir);

        var sourceFile = Path.Combine(sourceFolder, "SpawnStates.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "MSB", gameDir);

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
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize the Map Spawn States: {targetFile}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Map Editor] Failed to read the Map Spawn States: {targetFile}", e);
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank.Dispose();
        VanillaBank.Dispose();

        foreach(var entry in AuxBanks)
        {
            entry.Value.Dispose();
        }

        MapObjectNameLists.Clear();

        PrimaryBank = null;
        VanillaBank = null;
        AuxBanks = null;
        MapObjectNameLists = null;

        Meta = null;
        MapObjectSelections = null;
        MsbInformation = null;
        MsbEnums = null;
        MsbMasks = null;
        MapSpawnStates = null;
    }
    #endregion
}
