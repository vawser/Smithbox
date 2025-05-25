using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

/// <summary>
/// Holds the data banks for Materials.
/// Data Flow: Full Load
/// </summary>
public class MaterialData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public FileDictionary MTD_Files = new();
    public FileDictionary MATBIN_Files = new();

    public MaterialBank PrimaryBank;
    public MaterialBank VanillaBank;

    public MaterialData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        MTD_Files.Entries =  Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "mtdbnd")
            .ToList();

        // DS2 has it as a single .bnd file
        if(Project.ProjectType is ProjectType.DS2 or ProjectType.DS2)
        {
            MTD_Files.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Filename == "allmaterialbnd")
            .ToList();
        }

        MATBIN_Files.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "matbinbnd")
            .ToList();

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to fully setup Primary Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to fully setup Vanilla Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        return true;
    }
}
