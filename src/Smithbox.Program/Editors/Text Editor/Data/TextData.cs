using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextData : IDisposable
{
    public ProjectEntry Project;

    public TextBank PrimaryBank;
    public TextBank VanillaBank;
    public Dictionary<string, TextBank> AuxBanks = new();

    public FileDictionary FmgFiles = new();

    public FmgDescriptors FmgDescriptors;

    public TextData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // FMG Descriptors
        Task<bool> descriptorTask = SetupFmgDescriptors();
        bool descriptorTaskResult = await descriptorTask;

        if (descriptorTaskResult)
        {
            Smithbox.Log(this, $"Setup FMG Descriptors.");
        }
        else
        {
            Smithbox.LogError(this, $"Failed to setup FMG Descriptors.");
        }

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            Smithbox.LogError(this, $"[Text Editor] Failed to fully setup Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            Smithbox.LogError(this, $"[Text Editor] Failed to fully setup Primary Bank.");
        }

        return true;
    }

    public async Task<bool> LoadAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Smithbox.Orchestrator.LoadAuxiliaryProject(targetProject, ProjectInitType.TextEditorOnly, reloadProject);

        var newAuxBank = new TextBank($"{targetProject.Descriptor.ProjectName}",Project, targetProject.VFS.FS);

        // Aux Bank
        Task<bool> auxBankTask = newAuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            Smithbox.LogError(this, $"[Text Editor] Failed to setup Aux FMG Bank.");
        }

        if (AuxBanks.ContainsKey(targetProject.Descriptor.ProjectName))
        {
            AuxBanks[targetProject.Descriptor.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.Descriptor.ProjectName, newAuxBank);
        }

        Smithbox.Log(this, $"[Text Editor] Setup Aux FMG Bank.");

        return true;
    }

    #region FMG Descriptors

    public async Task<bool> SetupFmgDescriptors()
    {
        var jsonName = "FMG Descriptor Registry.json";

        await Task.Yield();

        var folder = @$"{StudioCore.Common.FileLocations.Assets}/FMG/{ProjectUtils.GetGameDirectory(Project)}";
        var file = Path.Combine(folder, jsonName);

        if (File.Exists(file))
        {
            try
            {
                var filestring = await File.ReadAllTextAsync(file);

                try
                {
                    FmgDescriptors = JsonSerializer.Deserialize(filestring, TextEditorJsonSerializerContext.Default.FmgDescriptors);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, $"Failed to deserialize FMG descriptor registry: {file}", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, $"Failed to read FMG descriptor registry: {file}", e);
            }
        }

        return true;
    }
    #endregion

    #region Dispose
    public void Dispose()
    {
        PrimaryBank?.Dispose();
        VanillaBank?.Dispose();

        foreach(var entry in AuxBanks)
        {
            entry.Value?.Dispose();
        }

        PrimaryBank = null;
        VanillaBank = null;
        AuxBanks = null;

        FmgFiles = null;
    }
    #endregion
}
