using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelData : IDisposable
{
    public ProjectEntry Project;

    public ModelBank PrimaryBank;

    public FormatResource FlverInformation;
    public FormatEnum FlverEnums;

    public ModelData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);

        // FLVER Information
        Task<bool> flverInfoTask = SetupFlverInfo();
        bool flverInfoResult = await flverInfoTask;

        if (!flverInfoResult)
        {
            Smithbox.LogError(this, $"[Model Editor] Failed to setup the FLVER annotations.");
        }
        else
        {
            Smithbox.Log(this, $"[Model Editor] Setup the FLVER annotations.");
        }

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, $"[Model Editor] Failed to setup the Primary Bank.");
        }
        else
        {
            Smithbox.Log(this, $"[Model Editor] Setup the Primary Bank.");
        }

        return primaryBankTaskResult;
    }


    /// <summary>
    /// Setup the FLVER information for this project
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SetupFlverInfo()
    {
        await Task.Yield();

        FlverInformation = new();
        FlverEnums = new();

        // Information
        var sourceFolder = Path.Join(StudioCore.Common.FileLocations.Assets, "FLVER");
        var sourceFile = Path.Combine(sourceFolder, "Core.json");

        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "FLVER");
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
                    FlverInformation = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FormatResource);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Model Editor] Failed to deserialize the FLVER annotations", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Model Editor] Failed to read the FLVER annotations: {targetFile}", e);
            }
        }

        // Enums
        sourceFile = Path.Combine(sourceFolder, "Enums.json");

        projectFile = Path.Combine(projectFolder, "Enums.json");

        targetFile = sourceFile;

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
                    FlverEnums = JsonSerializer.Deserialize(filestring, ProjectJsonSerializerContext.Default.FormatEnum);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"[Model Editor] Failed to deserialize the FLVER enums: {targetFile}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"[Model Editor] Failed to read the FLVER enums: {targetFile}", e);
            }
        }

        return true;
    }

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();

        PrimaryBank = null;

        FlverInformation = null;
        FlverEnums = null;
    }
    #endregion
}
