using StudioCore.Core;
using StudioCore.Editors.MapEditor.Framework.META;
using StudioCore.Formats.JSON;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Data;

/// <summary>
/// Holds the data banks for Maps.
/// Data Flow: Lazy Load
/// </summary>
public class MapData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public FileDictionary MapFiles = new();
    public FileDictionary LightFiles = new();

    public MapBank PrimaryBank;
    public MapBank VanillaBank;
    public Dictionary<string, MapBank> AuxBanks = new();

    public MsbMeta Meta;

    public MapData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        MapFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "msb").ToList();

        // TODO: need to grab the containers for DS2 btls, which are in a BXF
        LightFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "btl").ToList();

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        // META
        Meta = new MsbMeta(BaseEditor, Project);

        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        return true;
    }

    public async Task<bool> SetupAuxBank(ProjectEntry targetProject)
    {
        await Task.Delay(1);

        // If project isn't already loaded, init it
        if (!targetProject.Initialized)
        {
            await targetProject.Init();
        }

        var newAuxBank = new MapBank(targetProject.ProjectName, BaseEditor, Project, targetProject.FS);

        Task<bool> auxBankTask = newAuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (AuxBanks.ContainsKey(targetProject.ProjectName))
        {
            AuxBanks[targetProject.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.ProjectName, newAuxBank);
        }

        if (auxBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Setup Aux MSB Bank for {targetProject.ProjectName}.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Map Editor] Failed to setup Aux MSB Bank for {targetProject.ProjectName}.");
        }

        return true;
    }
}
