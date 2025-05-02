using StudioCore;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.MapEditorNS;
using StudioCore.Resources.JSON;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Smithbox.Core.MapEditorNS;

public class MapData
{
    public BaseEditor BaseEditor;
    public Project Project;

    public bool IsSetup;

    public MapBank PrimaryBank;

    public MapMeta Meta;

    public FileDictionary MapFiles;
    public FileDictionary BtabFiles;

    // TODO: may need to add mtd/matbin stuff here for map viewport usage

    /// <summary>
    /// JSON stores for this editor
    /// </summary>
    public EntitySelectionGroupList EntitySelections;
    public SpawnStateResource SpawnStates;
    public FormatMask MaskInformation;

    public MapData(BaseEditor baseEditor, Project projectOwner)
    {
        BaseEditor = baseEditor;
        Project = projectOwner;

        PrimaryBank = new(this, "Primary", Project.FS);

        Setup();
    }

    public async void Setup()
    {
        Meta = new(this);
        Meta.Load();

        MapFiles = new();
        MapFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "msb").ToList();

        var rootFiles = Project.FileDictionary.Entries.Where(e => e.Extension == "msb").ToList();
        var projectFiles = FileDictionaryUtils.GetUniqueFileEntries(Project, "msb");
        MapFiles.Entries = FileDictionaryUtils.GetFinalDictionaryEntries(rootFiles, projectFiles);

        BtabFiles = new();
        BtabFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "btab").ToList();

        rootFiles = Project.FileDictionary.Entries.Where(e => e.Extension == "btab").ToList();
        projectFiles = FileDictionaryUtils.GetUniqueFileEntries(Project, "btab");
        BtabFiles.Entries = FileDictionaryUtils.GetFinalDictionaryEntries(rootFiles, projectFiles);

        // Entity Selection Groups
        EntitySelections = new();

        Task<bool> entitySelectionTask = LoadEntitySelectionGroups();
        bool entitySelectionResult = await entitySelectionTask;

        if (entitySelectionResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Setup Entity Selection Groups.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to load Entity Selection Groups.");
        }

        // Spawn States
        SpawnStates = new();

        Task<bool> spawnStateTask = LoadSpawnStates();
        bool spawnStateResult = await spawnStateTask;

        if (spawnStateResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Setup Spawn States information.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to load Spawn States information.");
        }

        // Mask Information
        MaskInformation = new();

        Task<bool> maskTask = LoadMaskInformation();
        bool maskResult = await maskTask;

        if (maskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Setup Mask information.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to load Mask information.");
        }

        IsSetup = true;
    }

    /// <summary>
    /// For loading the Entity Selection Groups.json on project setup
    /// </summary>
    public async Task<bool> LoadEntitySelectionGroups()
    {
        await Task.Delay(1000);

        var folder = $@"{Project.ProjectPath}\{ProjectUtils.DataFolder}\MSB\";
        var file = Path.Combine(folder, "Entity Selection Groups.json");

        if(!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (!File.Exists(file))
        {
            EntitySelections = new();
            return false;
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                EntitySelections = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.EntitySelectionGroupList);

                if (EntitySelections == null)
                {
                    throw new Exception("[Smithbox:Map Editor] Failed to read Entity Selection Groups.json");
                    return false;
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Entity Selection Groups.json");

                EntitySelections = new EntitySelectionGroupList();
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// For saving the Entity Selection Groups.json on changes
    /// </summary>
    public void SaveEntitySelectionGroups()
    {
        var folder = $@"{Project.ProjectPath}\{ProjectUtils.DataFolder}\MSB\";
        var file = Path.Combine(folder, "Entity Selection Groups.json");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var json = JsonSerializer.Serialize(EntitySelections, SmithboxSerializerContext.Default.EntitySelectionGroupList);

        File.WriteAllText(file, json);
    }

    /// <summary>
    /// For loading the Spawn States.json on project setup
    /// </summary>
    public async Task<bool> LoadSpawnStates()
    {
        await Task.Delay(1000);

        if (Project.ProjectType != ProjectType.DS2)
            return false;

        if (Project.ProjectType != ProjectType.DS2S)
            return false;

        var folder = $@"{AppContext.BaseDirectory}\Assets\MSB\{ProjectUtils.GetGameDirectory(Project)}";
        var file = Path.Combine(folder, "Spawn States.json");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (!File.Exists(file))
        {
            SpawnStates = new();
            return false;
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                SpawnStates = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.SpawnStateResource);

                if (SpawnStates == null)
                {
                    throw new Exception("[Smithbox:Map Editor] Failed to read Spawn States.json");
                    return false;
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Spawn States.json");

                SpawnStates = new SpawnStateResource();
                return false;
            }

            return true;
        }
    }
    /// <summary>
    /// For loading the Format Mask.json on project setup
    /// </summary>
    public async Task<bool> LoadMaskInformation()
    {
        await Task.Delay(1000);

        var folder = $@"{AppContext.BaseDirectory}\Assets\MSB\{ProjectUtils.GetGameDirectory(Project)}";
        var file = Path.Combine(folder, "Masks.json");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (!File.Exists(file))
        {
            MaskInformation = new();
            return false;
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                MaskInformation = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.FormatMask);

                if (MaskInformation == null)
                {
                    throw new Exception("[Smithbox:Map Editor] Failed to read Masks.json");
                    return false;
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Masks.json");

                MaskInformation = new FormatMask();
                return false;
            }

            return true;
        }
    }
}
