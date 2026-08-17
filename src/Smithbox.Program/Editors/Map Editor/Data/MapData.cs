using StudioCore.Editors.ParamEditor;
using StudioCore.Logger;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

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

    public SpeedTreeList SpeedTreeList;
    public GrassList GrassList;

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
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_Primary_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAP_Data_Setup_Primary_Bank_PASS"));
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_Vanilla_Bank_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAP_Data_Setup_Vanilla_Bank_PASS"));
        }

        // META
        Meta = new MsbMeta(Project);

        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        if (!metaTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_MSB_Meta_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAP_Data_Setup_MSB_Meta_PASS"));
        }

        // Map Object Names
        Task<bool> mapObjNamesTask = SetupMapObjectNames();
        bool mapObjNamesTaskResult = await mapObjNamesTask;

        if (!mapObjNamesTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_MSB_Map_Object_Names_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAP_Data_Setup_MSB_Map_Object_Names_PASS"));
        }

        // Map Object Selections
        Task<bool> mapObjSelectionTask = SetupMapObjectSelections();
        bool mapObjSelectionTaskResult = await mapObjSelectionTask;

        if (!mapObjSelectionTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_MSB_Map_Object_Selections_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAP_Data_Setup_MSB_Map_Object_Selections_PASS"));
        }

        // Asset Masks
        Task<bool> assetMaskTask = SetupAssetMasks();
        bool assetMaskTaskResult = await assetMaskTask;

        if (!assetMaskTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_MSB_Asset_Masks_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAP_Data_Setup_MSB_Asset_Masks_PASS"));
        }

        // Spawn States
        Task<bool> spawnStatesTask = SetupSpawnStates();
        bool spawnStatesTaskResult = await spawnStatesTask;

        if (!spawnStatesTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_MSB_Spawn_States_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAP_Data_Setup_MSB_Spawn_States_PASS"));
        }

        // Speed Tree List
        Task<bool> speedTreeListTask = SetupSpeedTreeList();
        bool speedTreeListTaskResult = await speedTreeListTask;

        if (!speedTreeListTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_MSB_Speed_Tree_List_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAP_Data_Setup_MSB_Speed_Tree_List_PASS"));
        }

        // Grass List
        Task<bool> grassListTask = SetupGrassList();
        bool grassListTaskResult = await grassListTask;

        if (!grassListTaskResult)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_MSB_Grass_List_FAIL"));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("MAP_Data_Setup_MSB_Grass_List_PASS"));
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
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_Aux_Bank_FAIL", targetProject.Descriptor.ProjectName));
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

        Smithbox.Log(this, LOC.Get("MAP_Data_Setup_Aux_Bank_PASS", targetProject.Descriptor.ProjectName));

        return true;
    }

    public async Task<bool> SetupMapObjectNames()
    {
        await Task.Yield();

        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Project), "Map Object Names");

        var projDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Map Object Names");

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
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_Map_Object_Name_Lists_FAIL", file), e);
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
                    Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_Failed_Deserialize_Map_Object_Selections", projectFile), e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_Failed_Read_Map_Object_Selections", projectFile), e);
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
                Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_Failed_Write_Map_Object_Selections", projectFile), ex);
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
            Smithbox.LogError(this, LOC.Get("MAP_Data_Setup_Failed_Write_Map_Object_Selections", projectFile), ex);
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
                    Smithbox.LogError(this, 
                        LOC.Get("MAP_Data_Setup_Failed_Deserialize_Spawn_States", file), e);
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
                    Smithbox.LogError(this,
                        LOC.Get("MAP_Data_Setup_Failed_Deserialize_Spawn_States", file), e);
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
                    Smithbox.LogError(this,
                        LOC.Get("MAP_Data_Setup_Failed_Deserialize_Asset_Entry", file), e);
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
                    Smithbox.LogError(this,
                        LOC.Get("MAP_Data_Setup_Failed_Deserialize_Asset_Entry", file), e);
                }
            }
        }

        return true;
    }

    public string GetMapObjectName(string mapID, string mapObjectKey)
    {
        var name = "";
        var nameLists = Project.Handler.MapData.MapObjectNameLists;

        var entries = nameLists.FirstOrDefault(e => e.Key == mapID);
        if (entries.Key != null)
        {
            foreach (var entry in entries.Value.Entries)
            {
                if (entry.ID == mapObjectKey)
                {
                    name = entry.Name;
                }
            }
        }

        return name;
    }

    public void UpdateMapObjectName(string mapID, string mapObjectKey, string mapObjectName)
    {
        var srcDir = Path.Combine(ParamDebugTools.ProjectFolder,
            "src", "Smithbox.Data", "Assets", "MSB",
            ProjectUtils.GetGameDirectory(Project), "Map Object Names");

        var projDir = Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Map Object Names");

        var targetDir = projDir;

        if(CFG.Current.Developer_Enable_Tools)
        {
            targetDir = srcDir;
        }

        if(!Directory.Exists(targetDir))
        {
            Directory.CreateDirectory(targetDir);
        }

        if (Project.Handler.MapData.MapObjectNameLists.ContainsKey(mapID))
        {
            var mapEntry = Project.Handler.MapData.MapObjectNameLists[mapID];

            if (mapEntry.Entries.Any(e => e.ID == mapObjectKey))
            {
                for (int i = 0; i < mapEntry.Entries.Count; i++)
                {
                    var curEntry = mapEntry.Entries[i];

                    if (curEntry.ID == mapObjectKey)
                    {
                        curEntry.Name = mapObjectName;
                    }
                }
            }
            else
            {
                var nameEntry = new MapObjectNameEntry();
                nameEntry.ID = mapObjectKey;
                nameEntry.Name = mapObjectName;

                mapEntry.Entries.Add(nameEntry);
            }
        }
        else
        {
            var nameEntry = new MapObjectNameEntry();
            nameEntry.ID = mapObjectKey;
            nameEntry.Name = mapObjectName;

            var mapEntry = new MapObjectNameMapEntry();
            mapEntry.Name = mapID;
            mapEntry.Entries = new() { nameEntry };

            Project.Handler.MapData.MapObjectNameLists.Add(mapID, mapEntry);
        }

        if (Project.Handler.MapData.MapObjectNameLists.ContainsKey(mapID))
        {
            var mapEntry = Project.Handler.MapData.MapObjectNameLists[mapID];

            if (Directory.Exists(targetDir))
            {
                var targetFile = Path.Combine(targetDir, $"{mapID}.json");

                var options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true,
                    IncludeFields = true
                };

                var jsonString = JsonSerializer.Serialize(mapEntry, typeof(MapObjectNameMapEntry), options);

                File.WriteAllText(targetFile, jsonString);
            }
        }
    }

    public async Task<bool> SetupSpeedTreeList()
    {
        await Task.Yield();

        SpeedTreeList = new();

        var sourcePath = Path.Join(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "SpeedTreeAssets.json");

        if (File.Exists(sourcePath))
        {
            var file = File.ReadAllText(sourcePath);
            try
            {
                var speedTreeList = JsonSerializer.Deserialize(file, MapEditorJsonSerializerContext.Default.SpeedTreeList);

                SpeedTreeList = speedTreeList;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this,
                    LOC.Get("MAP_Data_Setup_Failed_Deserialize_Speed_Tree_List", file), e);
            }
        }

        return true;
    }

    public async Task<bool> SetupGrassList()
    {
        await Task.Yield();

        SpeedTreeList = new();

        var sourcePath = Path.Join(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "GrassAssets.json");

        if (File.Exists(sourcePath))
        {
            var file = File.ReadAllText(sourcePath);
            try
            {
                var grassList = JsonSerializer.Deserialize(file, MapEditorJsonSerializerContext.Default.GrassList);

                GrassList = grassList;
            }
            catch (Exception e)
            {
                Smithbox.LogError(this,
                    LOC.Get("MAP_Data_Setup_Failed_Deserialize_Grass_Tree_List", file), e);
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
