using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Framework.META;
using StudioCore.Formats.JSON;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static StudioCore.Core.ProjectEntry;

namespace StudioCore.Editors.MapEditor.Data;

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

    public FileDictionary MapFiles = new();
    public FileDictionary LightFiles = new();
    public FileDictionary NavmeshFiles = new();
    public FileDictionary LightAtlasFiles = new();
    public FileDictionary CollisionFiles = new();

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
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to fully setup Primary Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to fully setup Vanilla Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        // META
        Meta = new MsbMeta(BaseEditor, Project);

        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        if (!metaTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to setup MSB Meta.", LogLevel.Error, Tasks.LogPriority.High);
        }

        return primaryBankTaskResult && vanillaBankTaskResult;
    }

    public void SetupFileDictionaries()
    {
        // MSB
        MapFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "msb").ToList();

        // BTL
        LightFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "btl").ToList();

        // TODO: need to grab the containers for DS2 btls, which are in a .gibhd

        // NVA
        NavmeshFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "nva").ToList();

        // BTAB
        LightAtlasFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "btab").ToList();

        // Collision
        CollisionFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "hkxbhd").ToList();
    }

    public async Task<bool> SetupAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Task.Yield();

        if (reloadProject)
        {
            await targetProject.Init(silent: true, InitType.MapEditorOnly);
        }
        else
        {
            if (!targetProject.Initialized)
            {
                await targetProject.Init(silent: true, InitType.MapEditorOnly);
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
}
