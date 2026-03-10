using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.FileBrowser;
using StudioCore.Editors.ParamEditor;
using StudioCore.Logger;
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

    // User-overridable meta data
    public MsbMeta Meta;

    // User meta data
    public EntitySelectionGroupList MapObjectSelections;
    public Dictionary<string, MapObjectNameMapEntry> MapObjectNameLists = new();

    // ER-specific
    public AssetMasks AssetMasks;

    // DS2-specific
    public SpawnStates SpawnStates;

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
            Smithbox.LogError(this, $"[Map Editor] Failed to setup the Primary Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Editor] Setup the Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup the Vanilla Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Editor] Setup the Vanilla Bank.");
        }

        // META
        Meta = new MsbMeta(Project);

        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        if (!metaTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup the MSB meta.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Editor] Setup the MSB meta.");
        }

        // Map Object Names
        Task<bool> mapObjNamesTask = SetupMapObjectNames();
        bool mapObjNamesTaskResult = await mapObjNamesTask;

        if (!mapObjNamesTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup the Map Object Name lists.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Editor] Setup the the Map Object Name lists.");
        }

        // Map Object Selections
        Task<bool> mapObjSelectionTask = SetupMapObjectSelections();
        bool mapObjSelectionTaskResult = await mapObjSelectionTask;

        if (!mapObjSelectionTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup the Map Object Selection lists.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Editor] Setup the the Map Object Selection lists.");
        }

        // Asset Masks
        Task<bool> assetMaskTask = SetupAssetMasks();
        bool assetMaskTaskResult = await assetMaskTask;

        if (!assetMaskTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup MSB asset masks.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Editor] Setup the MSB asset masks.");
        }

        // Spawn States
        Task<bool> spawnStatesTask = SetupSpawnStates();
        bool spawnStatesTaskResult = await spawnStatesTask;

        if (!spawnStatesTaskResult)
        {
            Smithbox.LogError(this, $"[Map Editor] Failed to setup the Spawn States annotations.");
        }
        else
        {
            Smithbox.Log(this, $"[Map Editor] Setup the the Spawn States annotations.");
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

        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Project), "Community Map Object Names");

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

    public async Task<bool> SetupSpawnStates()
    {
        await Task.Yield();

        SpawnStates = new();

        // Build project-local first, so it takes precedence over the base versions
        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Spawn States");

        if (Path.Exists(projectFolder))
        {
            foreach (var entry in Directory.EnumerateFiles(projectFolder))
            {
                var file = File.ReadAllText(entry);
                try
                {
                    var layout = JsonSerializer.Deserialize(file, MapEditorJsonSerializerContext.Default.SpawnStateEntry);

                    if (!SpawnStates.List.Any(e => e.id == layout.id))
                    {
                        SpawnStates.List.Add(layout);
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize spawn state entry: {file}", LogPriority.High, e);
                }
            }
        }

        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Spawn States");

        if (Path.Exists(sourceFolder))
        {
            foreach (var entry in Directory.EnumerateFiles(sourceFolder))
            {
                var file = File.ReadAllText(entry);
                try
                {
                    var layout = JsonSerializer.Deserialize(file, MapEditorJsonSerializerContext.Default.SpawnStateEntry);

                    if (!SpawnStates.List.Any(e => e.id == layout.id))
                    {
                        SpawnStates.List.Add(layout);
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize spawn state entry: {file}", LogPriority.High, e);
                }
            }
        }

        return true;
    }

    public async Task<bool> SetupAssetMasks()
    {
        await Task.Yield();

        AssetMasks = new();

        // Build project-local first, so it takes precedence over the base versions
        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Asset Masks");

        if (Path.Exists(projectFolder))
        {
            foreach (var entry in Directory.EnumerateFiles(projectFolder))
            {
                var file = File.ReadAllText(entry);
                try
                {
                    var layout = JsonSerializer.Deserialize(file, MapEditorJsonSerializerContext.Default.AssetMaskEntry);

                    if (!AssetMasks.List.Any(e => e.model == layout.model))
                    {
                        AssetMasks.List.Add(layout);
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize asset mask entry: {file}", LogPriority.High, e);
                }
            }
        }

        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Asset Masks");

        if (Path.Exists(sourceFolder))
        {
            foreach (var entry in Directory.EnumerateFiles(sourceFolder))
            {
                var file = File.ReadAllText(entry);
                try
                {
                    var layout = JsonSerializer.Deserialize(file, MapEditorJsonSerializerContext.Default.AssetMaskEntry);

                    if (!AssetMasks.List.Any(e => e.model == layout.model))
                    {
                        AssetMasks.List.Add(layout);
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Map Editor] Failed to deserialize asset mask entry: {file}", LogPriority.High, e);
                }
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

        Meta = null;

        MapObjectSelections = null;
        MapObjectNameLists = null;

        AssetMasks = null;
        SpawnStates = null;
    }
    #endregion
}
