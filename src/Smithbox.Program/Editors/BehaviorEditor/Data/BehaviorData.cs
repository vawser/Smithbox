using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore;
using StudioCore.Core;
using StudioCore.EventScriptEditorNS;
using StudioCore.Formats.JSON;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace BehaviorEditorNS;

public class BehaviorData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public FileDictionary BehaviorFiles = new();

    public BehaviorBank PrimaryBank;
    public BehaviorBank VanillaBank;

    public BehaviorData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        BehaviorFiles.Entries = Project.FileDictionary.Entries
            .Where(e => e.Archive != "sd")
            .Where(e => e.Extension == "behbnd")
            .ToList();

        PrimaryBank = new("Primary", BaseEditor, Project, Project.FS);
        VanillaBank = new("Vanilla", BaseEditor, Project, Project.VanillaFS);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (!primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to fully setup Primary Bank.", LogLevel.Error, StudioCore.Tasks.LogPriority.High);
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (!vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Event Script Editor] Failed to fully setup Vanilla Bank.", LogLevel.Error, StudioCore.Tasks.LogPriority.High);
        }

        return primaryBankTaskResult && vanillaBankTaskResult;
    }
}
