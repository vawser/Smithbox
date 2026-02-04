using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor;

public class GparamData : IDisposable
{
    public ProjectEntry Project;

    public GparamBank PrimaryBank;
    public GparamBank VanillaBank;

    public FormatResource GparamInformation;
    public FormatEnum GparamEnums;

    public GparamData(ProjectEntry project)
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
            TaskLogs.AddError($"[Graphics Param Editor] Failed to fully setup Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddError($"[Graphics Param Editor] Failed to fully setup Primary Bank.");
        }

        // GPARAM Information
        Task<bool> gparamInfoTask = SetupGparamInfo();
        bool gparamInfoResult = await gparamInfoTask;

        if (gparamInfoResult)
        {
            TaskLogs.AddLog($"[Graphics Param Editor] Setup GPARAM information.");
        }
        else
        {
            TaskLogs.AddError($"[Graphics Param Editor] Failed to setup GPARAM information.");
        }

        // GPARAM Enums
        Task<bool> gparamEnumTask = SetupGparamEnums();
        bool gparamEnumResult = await gparamEnumTask;

        if (gparamEnumResult)
        {
            TaskLogs.AddLog($"[Graphics Param Editor] Setup GPARAM enums.");
        }
        else
        {
            TaskLogs.AddError($"[Graphics Param Editor] Failed to setup GPARAM enums.");
        }

        return primaryBankTaskResult && vanillaBankTaskResult;
    }

    public async Task<bool> SetupGparamInfo()
    {
        await Task.Yield();

        GparamInformation = new();
        GparamEnums = new();

        // Information
        var sourceFolder = Path.Join(StudioCore.Common.FileLocations.Assets, "GPARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Core.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "GPARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
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
                    GparamInformation = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FormatResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the GPARAM information: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the GPARAM information: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }


    public async Task<bool> SetupGparamEnums()
    {
        await Task.Yield();

        GparamEnums = new();

        // Enums
        var sourceFolder = Path.Join(StudioCore.Common.FileLocations.Assets, "GPARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Enums.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "GPARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var projectFile = Path.Combine(projectFolder, "Enums.json");

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
                    GparamEnums = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FormatEnum);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"[Smithbox] Failed to deserialize the GPARAM enums: {targetFile}", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog($"[Smithbox] Failed to read the GPARAM enums: {targetFile}", LogLevel.Error, LogPriority.High, e);
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        VanillaBank?.Dispose();

        PrimaryBank = null;
        VanillaBank = null;

        GparamInformation = null;
        GparamEnums = null;
    }
    #endregion
}
