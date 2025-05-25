using Google.Protobuf.Collections;
using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using StudioCore.EzStateEditorNS;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Core.ProjectEntry;

namespace StudioCore.Editors.TextEditor.Data;

public class TextData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public TextBank PrimaryBank;
    public TextBank VanillaBank;
    public Dictionary<string, TextBank> AuxBanks = new();

    public FileDictionary FmgFiles = new();

    public TextData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        var msgbndDictionary = new FileDictionary();
        msgbndDictionary.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "msgbnd")
            .ToList();

        var fmgDictionary = new FileDictionary();
        fmgDictionary.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "fmg").ToList();

        FmgFiles = ProjectUtils.MergeFileDictionaries(msgbndDictionary, fmgDictionary);

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to fully setup Primary Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to fully setup Primary Bank.", LogLevel.Error, Tasks.LogPriority.High);
        }

        return true;
    }

    public async Task<bool> LoadAuxBank(ProjectEntry targetProject, bool reloadProject)
    {
        await Task.Yield();

        if (reloadProject)
        {
            await targetProject.Init(silent: true, InitType.TextEditorOnly);
        }
        else
        {
            if (!targetProject.Initialized)
            {
                await targetProject.Init(silent: true, InitType.TextEditorOnly);
            }
        }

        var newAuxBank = new TextBank($"{targetProject.ProjectName}", BaseEditor, Project, targetProject.FS);

        // Aux Bank
        Task<bool> auxBankTask = newAuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (!auxBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to setup Aux FMG Bank.");
        }

        if (AuxBanks.ContainsKey(targetProject.ProjectName))
        {
            AuxBanks[targetProject.ProjectName] = newAuxBank;
        }
        else
        {
            AuxBanks.Add(targetProject.ProjectName, newAuxBank);
        }

        TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Setup Aux FMG Bank.");

        return true;
    }
}
