using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.EzStateEditorNS;

/// <summary>
/// Holds the data banks for EzState Scripts
/// Data Flow: Lazy Load
/// </summary>
public class EsdData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public FileDictionary EsdFiles = new();

    public EsdBank PrimaryBank;
    public EsdBank VanillaBank;

    public EsdMeta Meta;

    public EsdData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        Meta = new(BaseEditor, Project);

        var talkDictionary = new FileDictionary();
        talkDictionary.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "talkesdbnd")
            .ToList();

        var looseDictionary = new FileDictionary();
        looseDictionary.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "esd")
            .ToList();

        EsdFiles = ProjectUtils.MergeFileDictionaries(talkDictionary, looseDictionary);

        // Meta
        Task<bool> metaTask = Meta.Setup();
        bool metaTaskResult = await metaTask;

        if (!metaTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to setup ESD Meta.", LogLevel.Error, Tasks.LogPriority.High);
        }

        // Banks
        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to fully setup Primary Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:EzState Script Editor] Failed to fully setup Vanilla Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        return primaryBankTaskResult && vanillaBankTaskResult;
    }
}
