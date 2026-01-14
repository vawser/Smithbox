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
public class MaterialData : IDisposable
{
    public ProjectEntry Project;

    public FileDictionary MTD_Files = new();
    public FileDictionary MATBIN_Files = new();

    public MaterialBank PrimaryBank;
    public MaterialBank VanillaBank;

    public MaterialDisplayConfiguration MaterialDisplayConfiguration;

    public MaterialData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        MTD_Files.Entries =  Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/mtd"))
            .Where(e => e.Extension == "mtdbnd")
            .ToList();

        // DS2 has it as a single .bnd file
        if(Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            MTD_Files.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/material"))
            .Where(e => e.Filename == "allmaterialbnd")
            .ToList();
        }

        MATBIN_Files.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/material"))
            .Where(e => e.Extension == "matbinbnd")
            .ToList();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // Material Display Configuration
        Task<bool> matDispTask = SetupMaterialDisplayConfiguration();
        bool matDispTaskResult = await matDispTask;

        if (matDispTaskResult)
        {
            TaskLogs.AddLog($"[Material Editor] Setup Material Display Configuration.");
        }
        else
        {
            TaskLogs.AddError($"[Material Editor] Failed to setup Material Display Configuration.");
        }

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddError($"[Material Editor] Failed to fully setup Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddError($"[Material Editor] Failed to fully setup Vanilla Bank.");
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
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "MATERIAL", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Display Configuration.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "MATERIAL", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
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

    #region Dispose
    public void Dispose()
    {
        MTD_Files = null;
        MATBIN_Files = null;

        PrimaryBank?.Dispose();
        VanillaBank?.Dispose();

        PrimaryBank = null;
        VanillaBank = null;

        MaterialDisplayConfiguration = null;
    }
    #endregion
}
