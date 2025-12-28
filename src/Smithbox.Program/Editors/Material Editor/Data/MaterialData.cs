using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditor;

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

    public MaterialDisplayConfiguration MaterialDisplayConfiguration;

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
            .Where(e => e.Folder.StartsWith("/mtd"))
            .Where(e => e.Extension == "mtdbnd")
            .ToList();

        // DS2 has it as a single .bnd file
        if(Project.ProjectType is ProjectType.DS2 or ProjectType.DS2)
        {
            MTD_Files.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/material"))
            .Where(e => e.Filename == "allmaterialbnd")
            .ToList();
        }

        MATBIN_Files.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/material"))
            .Where(e => e.Extension == "matbinbnd")
            .ToList();

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Material Display Configuration
        Task<bool> matDispTask = SetupMaterialDisplayConfiguration();
        bool matDispTaskResult = await matDispTask;

        if (matDispTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Setup Material Display Configuration.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to setup Material Display Configuration.");
        }

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to fully setup Primary Bank.", LogLevel.Error, LogPriority.High);
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Material Editor] Failed to fully setup Vanilla Bank.", LogLevel.Error, LogPriority.High);
        }

        return true;
    }

    /// <summary>
    /// Setup the material display configuration for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupMaterialDisplayConfiguration()
    {
        await Task.Yield();

        MaterialDisplayConfiguration = new();

        // Information
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "MATERIAL", ProjectUtils.GetGameDirectory(Project.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Display Configuration.json");

        var projectFolder = Path.Join(Project.ProjectPath, ".smithbox", "Assets", "MATERIAL", ProjectUtils.GetGameDirectory(Project.ProjectType));
        var projectFile = Path.Combine(projectFolder, "Display Configuration.json");

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
                    MaterialDisplayConfiguration = JsonSerializer.Deserialize(filestring, MaterialEditorJsonSerializerContext.Default.MaterialDisplayConfiguration);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the Material Display Configuration: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the Material Display Configuration: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }
}
