using Microsoft.Extensions.Logging;
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

    public TextData(ProjectEntry project)
    {
        Project = project;
    }

    public FileDictionary SetupFmgFilelist()
    {
        var msgbndDictionary = new FileDictionary();
        msgbndDictionary.Entries = Project.Locator.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Folder.StartsWith("/msg"))
            .Where(e => e.Extension == "msgbnd")
            .ToList();


        if (Project.Descriptor.ProjectType == ProjectType.ER)
        {
            msgbndDictionary.Entries = msgbndDictionary.Entries.OrderBy(e => e.Folder).ThenBy(e => e.Filename.Contains("dlc02")).ThenBy(e => e.Filename.Contains("dlc01")).ThenBy(e => e.Filename).ToList();
        }

        var fmgDictionary = new FileDictionary();
        fmgDictionary.Entries = new List<FileDictionaryEntry>();

        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            fmgDictionary.Entries = Project.Locator.FileDictionary.Entries
                .Where(e => e.Archive != "sd")
                .Where(e => e.Folder.StartsWith("/menu/text"))
                .Where(e => e.Extension == "fmg").ToList();
        }

        return ProjectUtils.MergeFileDictionaries(msgbndDictionary, fmgDictionary);
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        FmgFiles = SetupFmgFilelist();

        PrimaryBank = new("Primary", Project, Project.VFS.FS);
        VanillaBank = new("Vanilla", Project, Project.VFS.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddError($"[Text Editor] Failed to fully setup Primary Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddError($"[Text Editor] Failed to fully setup Primary Bank.");
        }

        return true;
    }

    public async Task<bool> LoadAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Task.Yield();

        if (reloadProject)
        {
            await targetProject.Init(silent: true, ProjectInitType.TextEditorOnly);
        }
        else
        {
            if (!targetProject.Initialized)
            {
                await targetProject.Init(silent: true, ProjectInitType.TextEditorOnly);
            }
        }

        var newAuxBank = new TextBank($"{targetProject.Descriptor.ProjectName}",Project, targetProject.VFS.FS);

        // Aux Bank
        Task<bool> auxBankTask = newAuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            TaskLogs.AddError($"[Text Editor] Failed to setup Aux FMG Bank.");
        }

        if (AuxBanks.ContainsKey(targetProject.Descriptor.ProjectName))
        {
            AuxBanks[targetProject.Descriptor.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.Descriptor.ProjectName, newAuxBank);
        }

        TaskLogs.AddLog($"[Text Editor] Setup Aux FMG Bank.");

        return true;
    }

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
